using System;
using System.Linq;

using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Queues;
using Feedback = PepperDash.Essentials.Core.Feedback;
using Thread = Crestron.SimplSharpPro.CrestronThread.Thread;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using System.Collections.Generic;
using PepperDash.Essentials.Devices.Common.Displays;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Core.Logging;


namespace EpsonProjectorEpi
{
    public class EpsonProjector : TwoWayDisplayBase, IHasPowerControlWithFeedback,
        IWarmingCooling, IOnline, IBasicVideoMuteWithFeedback, ICommunicationMonitor, IHasInputs<int>, IBridgeAdvanced, IRoutingSinkWithSwitchingWithInputPort
    {
        private readonly IBasicCommunication _coms;
        private readonly PropsConfig _config;
        private readonly GenericQueue _commandQueue;
        private readonly int _pollTime = new Random().Next(3000, 4000);

        private CTimer _pollTimer;
        private CTimer _lensTimer;

        private const long DefaultWarmUpTimeMs = 1000;
        private const long DefaultCooldownTimeMs = 2000;

        private PowerHandler.PowerStatusEnum _currentPowerStatus;
        private PowerHandler.PowerStatusEnum _requestedPowerStatus;

        private VideoMuteHandler.VideoMuteStatusEnum _currentVideoMuteStatus;
        private VideoMuteHandler.VideoMuteStatusEnum _requestedMuteStatus;

        private VideoFreezeHandler.VideoFreezeStatusEnum _currentVideoFreezeStatus;
        private VideoFreezeHandler.VideoFreezeStatusEnum _requestedFreezeStatus;

        private VideoInputHandler.VideoInputStatusEnum _currentVideoInput;
        private VideoInputHandler.VideoInputStatusEnum _requestedVideoInput;

        public ISelectableItems<int> Inputs { get; private set; }

        private bool _isWarming;
        private bool _isCooling;

        public EpsonProjector(string key, string name, PropsConfig config, IBasicCommunication coms) : base(key, name)
        {
            _coms = coms;
            _config = config;
            if (config.Monitor == null)
                config.Monitor = GetDefaultMonitorConfig();

            CommunicationMonitor = new GenericCommunicationMonitor(this, coms, config.Monitor);
            var gather = new CommunicationGather(coms, "\x0D:");

            _commandQueue = new GenericQueue(key + "-command-queue", 213, Thread.eThreadPriority.MediumPriority, 50);

            SetupInputs();

            VideoMuteIsOn =
                new BoolFeedback("VideoMuteIsOn", () => _currentVideoMuteStatus == VideoMuteHandler.VideoMuteStatusEnum.Muted && PowerIsOnFeedback.BoolValue);

            VideoMuteIsOff =
                new BoolFeedback(() => !VideoMuteIsOn.BoolValue && PowerIsOnFeedback.BoolValue);

            VideoFreezeIsOn =
                new BoolFeedback("VideoFreezeIsOn", () => _currentVideoFreezeStatus == VideoFreezeHandler.VideoFreezeStatusEnum.Frozen && PowerIsOnFeedback.BoolValue);

            VideoFreezeIsOff =
                new BoolFeedback(() => !VideoFreezeIsOn.BoolValue && PowerIsOnFeedback.BoolValue);

            var powerHandler = new PowerHandler(key);
            powerHandler.PowerStatusUpdated += HandlePowerStatusUpdated;

            var muteHandler = new VideoMuteHandler(key);
            muteHandler.VideoMuteStatusUpdated += HandleMuteStatusUpdated;

            var freezeHandler = new VideoFreezeHandler(key);
            freezeHandler.VideoFreezeStatusUpdated += HandleFreezeStatusUpdated;

            var inputHandler = new VideoInputHandler(key);
            inputHandler.VideoInputUpdated += HandleVideoInputUpdated;

            _ = new StringResponseProcessor(gather,
                s =>
                    {
                        powerHandler.ProcessResponse(s);
                        muteHandler.ProcessResponse(s);
                        freezeHandler.ProcessResponse(s);
                        inputHandler.ProcessResponse(s);
                    });

            LampHoursFeedback =
                new LampHoursHandler(key, _commandQueue, gather, PowerIsOnFeedback).LampHoursFeedback;

            SerialNumberFeedback =
                new SerialNumberHandler(key, _commandQueue, gather, PowerIsOnFeedback).SerialNumberFeedback;

            CurrentInputValueFeedback =
                new IntFeedback("CurrentInput",
                    () =>
                        {
                            if (!PowerIsOnFeedback.BoolValue)
                                return 0;

                            return (int) _currentVideoInput;
                        });

            var feedbacks = new FeedbackCollection<Feedback>
                {
                    PowerIsOnFeedback,
                    IsWarmingUpFeedback,
                    IsCoolingDownFeedback,
                    VideoMuteIsOn,
                    VideoMuteIsOff,
                    VideoFreezeIsOn,
                    VideoFreezeIsOff,
                    CurrentInputValueFeedback,
                    new StringFeedback("RequestedPower", () => _requestedPowerStatus.ToString()),
                    new StringFeedback("RequestedMute", () => _requestedMuteStatus.ToString()),
                    new StringFeedback("RequestedFreeze", () => _requestedFreezeStatus.ToString()),
                    new StringFeedback("RequestedInput", () => _requestedVideoInput.ToString()),
                };

            Feedbacks.AddRange(feedbacks);

            CrestronEnvironment.ProgramStatusEventHandler += type =>
                {
                    if (type != eProgramStatusEventType.Stopping)
                        return;

                    if (_pollTimer == null)
                        return;

                    _pollTimer.Stop();
                    _pollTimer.Dispose();
                };

            WarmupTime = (uint) (config.WarmupTimeMs > 0 ? config.WarmupTimeMs : DefaultWarmUpTimeMs);
            CooldownTime = (uint) (config.CooldownTimeMs > 0 ? config.CooldownTimeMs : DefaultCooldownTimeMs);
        }

        private static CommunicationMonitorConfig GetDefaultMonitorConfig()
        {
            return new CommunicationMonitorConfig()
            {
                PollInterval = 30000,
                PollString = Commands.PowerPoll + "\x0D",
                TimeToWarning = 120000,
                TimeToError = 360000,
            };
        }

        private void HandlePowerStatusUpdated(object sender, Events.PowerEventArgs eventArgs)
        {
            if (_currentPowerStatus == eventArgs.Status)
            {
                return;
            }

            _currentPowerStatus = eventArgs.Status;
            _isCooling = _currentPowerStatus == PowerHandler.PowerStatusEnum.PowerCooling;
            _isWarming = _currentPowerStatus == PowerHandler.PowerStatusEnum.PowerWarming;
            ProcessRequestedPowerStatus();
            PowerIsOnFeedback.FireUpdate();
            IsWarmingUpFeedback.FireUpdate();
            IsCoolingDownFeedback.FireUpdate();
            CurrentInputFeedback.FireUpdate();
            CurrentInputValueFeedback.FireUpdate();
        }

        private void HandleMuteStatusUpdated(object sender, Events.VideoMuteEventArgs videoMuteEventArgs)
        {
            _currentVideoMuteStatus = videoMuteEventArgs.Status;
            ProcessRequestedMuteStatus();
            PowerIsOnFeedback.FireUpdate();
            CurrentInputFeedback.FireUpdate();
            CurrentInputValueFeedback.FireUpdate();
            VideoFreezeIsOn.FireUpdate();
            VideoFreezeIsOff.FireUpdate();
            VideoMuteIsOn.FireUpdate();
            VideoMuteIsOff.FireUpdate();
        }

        public override bool CustomActivate()
        {
            Feedbacks.RegisterForConsoleUpdates(this);
            Feedbacks.FireAllFeedbacks();

            _pollTimer = new CTimer(o =>
                {
                    _commandQueue.Enqueue(new Commands.EpsonCommand
                    {
                        Coms = _coms,
                        Message = Commands.PowerPoll,
                    });

                    if (!PowerIsOnFeedback.BoolValue)
                        return;

                    _commandQueue.Enqueue(new Commands.EpsonCommand
                    {
                        Coms = _coms,
                        Message = Commands.SourcePoll,
                    });

                    _commandQueue.Enqueue(new Commands.EpsonCommand
                    {
                        Coms = _coms,
                        Message = Commands.MutePoll,
                    });

                    _commandQueue.Enqueue(new Commands.EpsonCommand
                    {
                        Coms = _coms,
                        Message = Commands.FreezePoll,
                    });
                },
                null,
                5189,
                _pollTime);

            PowerIsOnFeedback.OutputChange += (sender, args) =>
                {
                    this.LogDebug("PowerIsOn changed to {0}", args.BoolValue);

                    if (!args.BoolValue)
                        return;

                    ProcessRequestedVideoInput();
                    ProcessRequestedMuteStatus();
                    ProcessRequestedFreezeStatus();
                };

            CommunicationMonitor.Start();
            return base.CustomActivate();
        }

        private void HandleFreezeStatusUpdated(object sender, Events.VideoFreezeEventArgs videoFreezeEventArgs)
        {
            _currentVideoFreezeStatus = videoFreezeEventArgs.Status;

            ProcessRequestedFreezeStatus();
            Feedbacks.FireAllFeedbacks();
        }

        private void ProcessRequestedPowerStatus()
        {
            if (_requestedPowerStatus != PowerHandler.PowerStatusEnum.None)
            {
                this.LogDebug("ProcessRequestedPowerStatus: Current={0}, Requested={1}", _currentPowerStatus, _requestedPowerStatus);
            }

            switch (_requestedPowerStatus)
            {
                case PowerHandler.PowerStatusEnum.PowerOn:
                    ProcessRequestedPowerOn();
                    break;
                case PowerHandler.PowerStatusEnum.PowerWarming:
                    break;
                case PowerHandler.PowerStatusEnum.PowerCooling:
                    break;
                case PowerHandler.PowerStatusEnum.PowerOff:
                    ProcessRequestedPowerOff();
                    break;
                case PowerHandler.PowerStatusEnum.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Feedbacks.FireAllFeedbacks();
        }

        private void ProcessRequestedPowerOn()
        {
            if (_requestedPowerStatus != PowerHandler.PowerStatusEnum.PowerOn)
                throw new InvalidOperationException("Power on isn't requested");

            switch (_currentPowerStatus)
            {
                case PowerHandler.PowerStatusEnum.PowerOn:
                    _isWarming = false;
                    _requestedPowerStatus = PowerHandler.PowerStatusEnum.None;
                    break;
                case PowerHandler.PowerStatusEnum.PowerWarming:
                    break;
                case PowerHandler.PowerStatusEnum.PowerCooling:
                    break;
                case PowerHandler.PowerStatusEnum.PowerOff:
                    _isWarming = true;
                    _currentPowerStatus = PowerHandler.PowerStatusEnum.PowerWarming;
                    break;

                case PowerHandler.PowerStatusEnum.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _commandQueue.Enqueue(new Commands.EpsonCommand
            {
                Coms = _coms,
                Message = Commands.PowerOn,
            });
        }

        private void ProcessRequestedPowerOff()
        {
            if (_requestedPowerStatus != PowerHandler.PowerStatusEnum.PowerOff)
                throw new InvalidOperationException("Power off isn't requested");

            switch (_currentPowerStatus)
            {
                case PowerHandler.PowerStatusEnum.PowerOn:
                    _isCooling = true;
                    IsCoolingDownFeedback.FireUpdate();
                    _currentPowerStatus = PowerHandler.PowerStatusEnum.PowerCooling;
                    break;
                case PowerHandler.PowerStatusEnum.PowerWarming:
                    break;
                case PowerHandler.PowerStatusEnum.PowerCooling:
                    break;
                case PowerHandler.PowerStatusEnum.PowerOff:
                    _isCooling = false;
                    IsCoolingDownFeedback.FireUpdate();
                    _requestedPowerStatus = PowerHandler.PowerStatusEnum.None;
                    break;
                case PowerHandler.PowerStatusEnum.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _commandQueue.Enqueue(new Commands.EpsonCommand
            {
                Coms = _coms,
                Message = Commands.PowerOff,
            });
        }

        private void ProcessRequestedMuteStatus()
        {
            if (!PowerIsOnFeedback.BoolValue)
                return;

            switch (_requestedMuteStatus)
            {
                case VideoMuteHandler.VideoMuteStatusEnum.Muted:
                    ProcessRequestedMuteOnStatus();
                    break;
                case VideoMuteHandler.VideoMuteStatusEnum.Unmuted:
                    ProcessRequestedMuteOffStatus();
                    break;
                case VideoMuteHandler.VideoMuteStatusEnum.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessRequestedMuteOnStatus()
        {
            if (_requestedMuteStatus != VideoMuteHandler.VideoMuteStatusEnum.Muted)
                throw new InvalidOperationException("Mute on isn't requested");

            if (_requestedMuteStatus == VideoMuteHandler.VideoMuteStatusEnum.None ||
                _currentVideoMuteStatus == VideoMuteHandler.VideoMuteStatusEnum.Muted)
            {
                _requestedMuteStatus = VideoMuteHandler.VideoMuteStatusEnum.None;
            }

            _commandQueue.Enqueue(new Commands.EpsonCommand
            {
                Coms = _coms,
                Message = Commands.MuteOn,
            });
        }

        private void ProcessRequestedMuteOffStatus()
        {
            if (_requestedMuteStatus != VideoMuteHandler.VideoMuteStatusEnum.Unmuted)
                throw new InvalidOperationException("Mute off isn't requested");

            if (_requestedMuteStatus == VideoMuteHandler.VideoMuteStatusEnum.None ||
                _currentVideoMuteStatus == VideoMuteHandler.VideoMuteStatusEnum.Unmuted)
            {
                _requestedMuteStatus = VideoMuteHandler.VideoMuteStatusEnum.None;
            }

            _commandQueue.Enqueue(new Commands.EpsonCommand
            {
                Coms = _coms,
                Message = Commands.MuteOff,
            });
        }

        private void ProcessRequestedFreezeStatus()
        {
            if (!PowerIsOnFeedback.BoolValue)
                return;

            switch (_requestedFreezeStatus)
            {
                case VideoFreezeHandler.VideoFreezeStatusEnum.Frozen:
                    ProcessRequestedFreezeOnStatus();
                    break;
                case VideoFreezeHandler.VideoFreezeStatusEnum.Unfrozen:
                    ProcessRequestedFreezeOffStatus();
                    break;
                case VideoFreezeHandler.VideoFreezeStatusEnum.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ProcessRequestedFreezeOnStatus()
        {
            if (_requestedFreezeStatus != VideoFreezeHandler.VideoFreezeStatusEnum.Frozen)
                throw new InvalidOperationException("Freeze on isn't requested");

            if (_requestedFreezeStatus == VideoFreezeHandler.VideoFreezeStatusEnum.None ||
                _currentVideoFreezeStatus == VideoFreezeHandler.VideoFreezeStatusEnum.Frozen)
            {
                _requestedFreezeStatus = VideoFreezeHandler.VideoFreezeStatusEnum.None;
            }

            _commandQueue.Enqueue(new Commands.EpsonCommand
            {
                Coms = _coms,
                Message = Commands.FreezeOn,
            });
        }

        private void ProcessRequestedFreezeOffStatus()
        {
            if (_requestedFreezeStatus != VideoFreezeHandler.VideoFreezeStatusEnum.Unfrozen)
                throw new InvalidOperationException("Freeze off isn't requested");

            if (_requestedFreezeStatus == VideoFreezeHandler.VideoFreezeStatusEnum.None ||
                _currentVideoFreezeStatus == VideoFreezeHandler.VideoFreezeStatusEnum.Unfrozen)
            {
                _requestedFreezeStatus = VideoFreezeHandler.VideoFreezeStatusEnum.None;
            }

            _commandQueue.Enqueue(new Commands.EpsonCommand
            {
                Coms = _coms,
                Message = Commands.FreezeOff,
            });
        }

        private void ProcessRequestedVideoInput()
        {
            if (!PowerIsOnFeedback.BoolValue)
                return;

            switch (_requestedVideoInput)
            {
                case VideoInputHandler.VideoInputStatusEnum.Hdmi:
                    _commandQueue.Enqueue(new Commands.EpsonCommand
                    {
                        Coms = _coms,
                        Message = Commands.SourceHdmi,
                    });
                    break;
                case VideoInputHandler.VideoInputStatusEnum.Dvi:
                    _commandQueue.Enqueue(new Commands.EpsonCommand
                    {
                        Coms = _coms,
                        Message = Commands.SourceDvi,
                    });
                    break;
                case VideoInputHandler.VideoInputStatusEnum.Computer:
                    _commandQueue.Enqueue(new Commands.EpsonCommand
                    {
                        Coms = _coms,
                        Message = Commands.SourceComputer,
                    });
                    break;
                case VideoInputHandler.VideoInputStatusEnum.Video:
                    _commandQueue.Enqueue(new Commands.EpsonCommand
                    {
                        Coms = _coms,
                        Message = Commands.SourceVideo,
                    });
                    break;
                case VideoInputHandler.VideoInputStatusEnum.Lan:
                    _commandQueue.Enqueue(new Commands.EpsonCommand
                    {
                        Coms = _coms,
                        Message = Commands.SourceLan,
                    });
                    break;
                case VideoInputHandler.VideoInputStatusEnum.HdBaseT:
                    _commandQueue.Enqueue(new Commands.EpsonCommand
                    {
                        Coms = _coms,
                        Message = Commands.SourceHdBaseT,
                    });
                    break;
                case VideoInputHandler.VideoInputStatusEnum.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetupInputs()
        {
            InputPorts.AddRange(new[]
            {
                new RoutingInputPort("Hdmi",
                    eRoutingSignalType.Video,
                    eRoutingPortConnectionType.Hdmi,
                    VideoInputHandler.VideoInputStatusEnum.Hdmi,
                    this) { Port = (int)VideoInputHandler.VideoInputStatusEnum.Hdmi, FeedbackMatchObject = VideoInputHandler.VideoInputStatusEnum.Hdmi },

                new RoutingInputPort("DVI",
                    eRoutingSignalType.Video,
                    eRoutingPortConnectionType.Dvi,
                    VideoInputHandler.VideoInputStatusEnum.Dvi,
                    this) { Port = (int)VideoInputHandler.VideoInputStatusEnum.Dvi, FeedbackMatchObject = VideoInputHandler.VideoInputStatusEnum.Dvi },

                new RoutingInputPort("Computer",
                    eRoutingSignalType.Video,
                    eRoutingPortConnectionType.Vga,
                    VideoInputHandler.VideoInputStatusEnum.Computer,
                    this) { Port = (int)VideoInputHandler.VideoInputStatusEnum.Computer, FeedbackMatchObject = VideoInputHandler.VideoInputStatusEnum.Computer },

                new RoutingInputPort("Video",
                    eRoutingSignalType.Video,
                    eRoutingPortConnectionType.Rgb,
                    VideoInputHandler.VideoInputStatusEnum.Video,
                    this) { Port = (int)VideoInputHandler.VideoInputStatusEnum.Video, FeedbackMatchObject = VideoInputHandler.VideoInputStatusEnum.Video },

                new RoutingInputPort("Lan",
                    eRoutingSignalType.Video,
                    eRoutingPortConnectionType.HdBaseT,
                    VideoInputHandler.VideoInputStatusEnum.Lan,
                    this) { Port = (int)VideoInputHandler.VideoInputStatusEnum.Lan, FeedbackMatchObject = VideoInputHandler.VideoInputStatusEnum.Lan },

                new RoutingInputPort("HDBaseT",
                    eRoutingSignalType.Video,
                    eRoutingPortConnectionType.HdBaseT,
                    VideoInputHandler.VideoInputStatusEnum.HdBaseT,
                    this) { Port = (int)VideoInputHandler.VideoInputStatusEnum.Lan, FeedbackMatchObject = VideoInputHandler.VideoInputStatusEnum.Lan }
            });

            if (_config.ActiveInputs != null && _config.ActiveInputs.Count > 0)
            {
                Inputs = new EpsonInputs
                {
                    Items = new Dictionary<int, ISelectableItem>()
                };

                var activeInputsMap = _config.ActiveInputs.ToDictionary(ai => ai.Key, ai => ai.Name);
                var allInputs = new Dictionary<string, KeyValuePair<int, EpsonInput>>
                {
                    {"Hdmi", new KeyValuePair<int, EpsonInput>(1, new EpsonInput("Hdmi", "Hdmi", this, SetHDMI)) },
                    {"DVI", new KeyValuePair<int, EpsonInput>(2, new EpsonInput("DVI", "DVI", this, SetDVI)) },
                    {"Computer", new KeyValuePair<int, EpsonInput>(3, new EpsonInput("Computer", "Computer", this, SetComputer)) },
                    {"Video", new KeyValuePair<int, EpsonInput>(4, new EpsonInput("Video", "Video", this, SetVideo)) }
                };
                foreach (var activeInput in activeInputsMap)
                {
                    if (allInputs.TryGetValue(activeInput.Key, out var input))
                    {
                        Inputs.Items.Add(input.Key, new EpsonInput(input.Value.Key, activeInput.Value, this, input.Value.Select));
                    }
                }
            }
            else
            {
                Inputs = new EpsonInputs
                {
                    Items = new Dictionary<int, ISelectableItem>
                    {
                        { (int)VideoInputHandler.VideoInputStatusEnum.Hdmi, new EpsonInput("Hdmi", "Hdmi", this, SetHDMI) },
                        { (int)VideoInputHandler.VideoInputStatusEnum.Dvi, new EpsonInput("DVI", "DVI-D", this, SetDVI) },
                        { (int)VideoInputHandler.VideoInputStatusEnum.Computer, new EpsonInput("Computer", "Computer", this, SetComputer) },
                        { (int)VideoInputHandler.VideoInputStatusEnum.HdBaseT,
                            new EpsonInput("HDBaseT", "HDBaseT", this, () => _commandQueue.Enqueue(new Commands.EpsonCommand
                            {
                                Coms = _coms,
                                Message = Commands.SourceHdBaseT,
                            }))
                        },
                    }
                };
            }

        }

        public void SetHDMI()
        {
            _commandQueue.Enqueue(new Commands.EpsonCommand
            {
                Coms = _coms,
                Message = Commands.SourceHdmi,
            });
        }

        public void SetDVI()
        {
            _commandQueue.Enqueue(new Commands.EpsonCommand
            {
                Coms = _coms,
                Message = Commands.SourceDvi,
            });
        }

        public void SetComputer()
        {
            _commandQueue.Enqueue(new Commands.EpsonCommand
            {
                Coms = _coms,
                Message = Commands.SourceComputer,
            });

        }

        public void SetVideo()
        {
            _commandQueue.Enqueue(new Commands.EpsonCommand
            {
                Coms = _coms,
                Message = Commands.SourceVideo,
            });
        }

        private void HandleVideoInputUpdated(object sender, Events.VideoInputEventArgs videoInputEventArgs)
        {
            _currentVideoInput = videoInputEventArgs.Input;
            Inputs.CurrentItem = (int)_currentVideoInput;

            var currentInputPort = InputPorts.FirstOrDefault(
                p => p.FeedbackMatchObject is VideoInputHandler.VideoInputStatusEnum @enum &&
                     @enum == _currentVideoInput);

            CurrentInputPort = currentInputPort;

            if (Inputs.Items.ContainsKey((int) _currentVideoInput))
            {
                foreach (var input in Inputs.Items)
                {
                    input.Value.IsSelected = input.Key.Equals((int) _currentVideoInput);
                }
            }


            //ProcessRequestedVideoInput();
            Feedbacks.FireAllFeedbacks();
        }

        public void VideoMuteToggle()
        {
            switch (_currentVideoMuteStatus)
            {
                case VideoMuteHandler.VideoMuteStatusEnum.Muted:
                    VideoMuteOff();
                    break;
                case VideoMuteHandler.VideoMuteStatusEnum.Unmuted:
                    VideoMuteOn();
                    break;
                case VideoMuteHandler.VideoMuteStatusEnum.None:
                    VideoMuteOn();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void VideoMuteOn()
        {
            if (_requestedPowerStatus != PowerHandler.PowerStatusEnum.PowerOn && !PowerIsOnFeedback.BoolValue)
                return;

            _requestedMuteStatus = VideoMuteHandler.VideoMuteStatusEnum.Muted;
            ProcessRequestedMuteStatus();
            Feedbacks.FireAllFeedbacks();
            _pollTimer.Reset(329, _pollTime);
        }

        public void VideoMuteOff()
        {
            if (_requestedPowerStatus != PowerHandler.PowerStatusEnum.PowerOn && !PowerIsOnFeedback.BoolValue)
                return;

            _requestedMuteStatus = VideoMuteHandler.VideoMuteStatusEnum.Unmuted;
            ProcessRequestedMuteStatus();
            Feedbacks.FireAllFeedbacks();
            _pollTimer.Reset(329, _pollTime);
        }

        public BoolFeedback VideoMuteIsOn { get; private set; }

        public void VideoFreezeToggle()
        {
            switch (_currentVideoFreezeStatus)
            {
                case VideoFreezeHandler.VideoFreezeStatusEnum.Frozen:
                    VideoFreezeOff();
                    break;
                case VideoFreezeHandler.VideoFreezeStatusEnum.Unfrozen:
                    VideoFreezeOn();
                    break;
                case VideoFreezeHandler.VideoFreezeStatusEnum.None:
                    VideoFreezeOn();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void VideoFreezeOn()
        {
            if (_requestedPowerStatus != PowerHandler.PowerStatusEnum.PowerOn && !PowerIsOnFeedback.BoolValue)
                return;

            _requestedFreezeStatus = VideoFreezeHandler.VideoFreezeStatusEnum.Frozen;
            ProcessRequestedFreezeStatus();
            Feedbacks.FireAllFeedbacks();
            _pollTimer.Reset(329, _pollTime);
        }

        public void VideoFreezeOff()
        {
            if (_requestedPowerStatus != PowerHandler.PowerStatusEnum.PowerOn && !PowerIsOnFeedback.BoolValue)
                return;

            _requestedFreezeStatus = VideoFreezeHandler.VideoFreezeStatusEnum.Unfrozen;
            ProcessRequestedFreezeStatus();
            Feedbacks.FireAllFeedbacks();
            _pollTimer.Reset(329, _pollTime);
        }

        public BoolFeedback VideoFreezeIsOn { get; private set; }

        public override void PowerOn()
        {
            _requestedPowerStatus = PowerHandler.PowerStatusEnum.PowerOn;

            ProcessRequestedPowerStatus();
            Feedbacks.FireAllFeedbacks();
            _pollTimer.Reset(329, _pollTime);

            WarmupTimer?.Dispose();
        }

        public override void PowerOff()
        {
            _requestedPowerStatus = PowerHandler.PowerStatusEnum.PowerOff;
            _requestedMuteStatus = VideoMuteHandler.VideoMuteStatusEnum.None;
            _requestedFreezeStatus = VideoFreezeHandler.VideoFreezeStatusEnum.None;
            _requestedVideoInput = VideoInputHandler.VideoInputStatusEnum.None;

            ProcessRequestedPowerStatus();
            Feedbacks.FireAllFeedbacks();
            _pollTimer.Reset(329, _pollTime);

            CooldownTimer?.Dispose();
        }

        public override void PowerToggle()
        {
            switch (_currentPowerStatus)
            {
                case PowerHandler.PowerStatusEnum.PowerOn:
                    PowerOff();
                    break;

                case PowerHandler.PowerStatusEnum.PowerWarming:
                    break;

                case PowerHandler.PowerStatusEnum.PowerCooling:
                    break;

                case PowerHandler.PowerStatusEnum.PowerOff:
                    PowerOn();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            Feedbacks.FireAllFeedbacks();
        }

        public override void ExecuteSwitch(object inputSelector)
        {
            try
            {
                var input = Convert.ToInt32(inputSelector);

                var inputToSwitch = (VideoInputHandler.VideoInputStatusEnum)input;
                if (inputToSwitch == VideoInputHandler.VideoInputStatusEnum.None)
                    return;

                _requestedVideoInput = inputToSwitch;

                PowerOn();
                VideoMuteOff();
                VideoFreezeOff();
                ProcessRequestedVideoInput();
                _pollTimer.Reset(438, _pollTime);
            }
            catch (Exception ex)
            {
                this.LogError("Error executing switch: {message}", ex.Message);
                this.LogDebug(ex, "Stack Trace: ");
            }
            finally
            {
                Feedbacks.FireAllFeedbacks();
            }
        }

        /// <summary>
        /// Does what it says
        /// </summary>
        public void StartLensMoveRepeat(ELensFunction func)
        {
            if (_lensTimer == null)
            {
                _lensTimer = new CTimer(o => LensFunction(func), null, 0, 250);
            }
        }

        /// <summary>
        /// Does what it says
        /// </summary>
        public void StopLensMoveRepeat()
        {
            if (_lensTimer != null)
            {
                _lensTimer.Stop();
                _lensTimer = null;
            }
        }

        public void LensFunction(ELensFunction function)
        {
            string message;
            switch (function)
            {
                case ELensFunction.ZoomPlus: message = Commands.ZoomInc; break;
                case ELensFunction.ZoomMinus: message = Commands.ZoomDec; break;
                case ELensFunction.FocusPlus: message = Commands.FocusInc; break;
                case ELensFunction.FocusMinus: message = Commands.FocusDec; break;
                case ELensFunction.HShiftPlus: message = Commands.HLensInc; break;
                case ELensFunction.HShiftMinus: message = Commands.HLensDec; break;
                case ELensFunction.VShiftPlus: message = Commands.VLensInc; break;
                case ELensFunction.VShiftMinus: message = Commands.VLensDec; break;
                default: message = null; break;

            }
            if (!string.IsNullOrEmpty(message))
            {
                _commandQueue.Enqueue(new Commands.EpsonCommand
                {
                    Coms = _coms,
                    Message = message
                });
            }
        }
        public void LensPositionRecall(ushort memory)
        {
            if (memory > 0 && memory <= 10)
            {
                _commandQueue.Enqueue(new Commands.EpsonCommand
                {
                    Coms = _coms,
                    Message = string.Format("POPLP {0}", Convert.ToByte(memory))
                });
            }
        }

    public void LinkToApi(Crestron.SimplSharpPro.DeviceSupport.BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
    {
      LinkDisplayToApi(this, trilist, joinStart, joinMapKey, bridge);
    }

        public BoolFeedback VideoMuteIsOff { get; private set; }
        public BoolFeedback VideoFreezeIsOff { get; private set; }
        public StatusMonitorBase CommunicationMonitor { get; private set; }
        public IntFeedback LampHoursFeedback { get; private set; }
        public StringFeedback SerialNumberFeedback { get; private set; }
        public IntFeedback CurrentInputValueFeedback { get; private set; }
        public event EventHandler ItemsUpdated;
        public event EventHandler CurrentItemChanged;

        public BoolFeedback IsOnline
        {
            get { return CommunicationMonitor.IsOnlineFeedback; }
        }

        public Dictionary<int, ISelectableItem> Items
        {
            get => _items;
            set
            {
                _items = value;
                ItemsUpdated?.Invoke(this, EventArgs.Empty);
            }
        }
        private Dictionary<int, ISelectableItem> _items;
        public int CurrentItem
        {
            get => CurrentInputValueFeedback.IntValue;
            set
            {
                if (Items.ContainsKey(value))
                {
                    _requestedVideoInput = (VideoInputHandler.VideoInputStatusEnum) value;
                    Items[value].Select();
                    CurrentItemChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        protected override Func<bool> IsCoolingDownFeedbackFunc
        {
            get { return () => _isCooling; }
        }

        protected override Func<bool> IsWarmingUpFeedbackFunc
        {
            get { return () => _isWarming; }
        }

        protected override Func<string> CurrentInputFeedbackFunc => () => CurrentInputValueFeedback.IntValue.ToString();

        protected override Func<bool> PowerIsOnFeedbackFunc => () => _currentPowerStatus == PowerHandler.PowerStatusEnum.PowerOn;
    }
}