﻿using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Queues;
using Feedback = PepperDash.Essentials.Core.Feedback;
using Thread = Crestron.SimplSharpPro.CrestronThread.Thread;

namespace EpsonProjectorEpi
{
    public class EpsonProjector : EssentialsBridgeableDevice, IRoutingSinkWithSwitching, IHasPowerControlWithFeedback,
        IWarmingCooling, IOnline, IBasicVideoMuteWithFeedback, ICommunicationMonitor, IHasFeedback
    {
        private readonly IBasicCommunication _coms;
        private readonly GenericQueue _commandQueue;
        private CTimer _pollTimer;
        private const int _pollTime = 6000;

        private PowerHandler.PowerStatusEnum _currentPowerStatus;
        private PowerHandler.PowerStatusEnum _requestedPowerStatus;

        private VideoMuteHandler.VideoMuteStatusEnum _currentVideoMuteStatus;
        private VideoMuteHandler.VideoMuteStatusEnum _requestedMuteStatus;

        private VideoInputHandler.VideoInputStatusEnum _currentVideoInput;
        private VideoInputHandler.VideoInputStatusEnum _requestedVideoInput;

        public EpsonProjector(string key, string name, PropsConfig config, IBasicCommunication coms) : base(key, name)
        {
            _coms = coms;
            if (config.Monitor == null)
                config.Monitor = GetDefaultMonitorConfig();

            CommunicationMonitor = new GenericCommunicationMonitor(this, coms, config.Monitor);
            CommunicationMonitor.Stop();
            var gather = new CommunicationGather(coms, "\x0D:");

            var socket = coms as ISocketStatus;
            if (socket != null)
            {
                // This instance uses IP control
                socket.ConnectionChange += socket_ConnectionChange;
            }

            _commandQueue = new GenericQueue(key + "-command-queue", 213, Thread.eThreadPriority.MediumPriority, 50);

            InputPorts = new RoutingPortCollection<RoutingInputPort>
                {
                    new RoutingInputPort("Hdmi",
                        eRoutingSignalType.Video,
                        eRoutingPortConnectionType.Hdmi,
                        VideoInputHandler.VideoInputStatusEnum.Hdmi,
                        this) {Port = (int) VideoInputHandler.VideoInputStatusEnum.Hdmi},
                    new RoutingInputPort("DVI",
                        eRoutingSignalType.Video,
                        eRoutingPortConnectionType.Dvi,
                        VideoInputHandler.VideoInputStatusEnum.Dvi,
                        this) {Port = (int) VideoInputHandler.VideoInputStatusEnum.Dvi},
                    new RoutingInputPort("Computer",
                        eRoutingSignalType.Video,
                        eRoutingPortConnectionType.Vga,
                        VideoInputHandler.VideoInputStatusEnum.Computer,
                        this) {Port = (int) VideoInputHandler.VideoInputStatusEnum.Computer},
                    new RoutingInputPort("Video",
                        eRoutingSignalType.Video,
                        eRoutingPortConnectionType.Rgb,
                        VideoInputHandler.VideoInputStatusEnum.Video,
                        this) {Port = (int) VideoInputHandler.VideoInputStatusEnum.Video},
                };

            PowerIsOnFeedback =
                new BoolFeedback("PowerIsOn", () => _currentPowerStatus == PowerHandler.PowerStatusEnum.PowerOn);

            IsWarmingUpFeedback =
                new BoolFeedback("IsWarmingUp", () => _currentPowerStatus == PowerHandler.PowerStatusEnum.PowerWarming);

            IsCoolingDownFeedback =
                new BoolFeedback("IsCoolingDown", () => _currentPowerStatus == PowerHandler.PowerStatusEnum.PowerCooling);

            PowerIsOffFeedback =
                new BoolFeedback(() => _currentPowerStatus == PowerHandler.PowerStatusEnum.PowerOff);

            VideoMuteIsOn =
                new BoolFeedback("VideoMuteIsOn",
                    () =>
                        _currentVideoMuteStatus == VideoMuteHandler.VideoMuteStatusEnum.Muted &&
                        PowerIsOnFeedback.BoolValue);

            VideoMuteIsOff = new BoolFeedback(() => !VideoMuteIsOn.BoolValue &&
                        PowerIsOnFeedback.BoolValue);

            
            var powerHandler = new PowerHandler(key);
            powerHandler.PowerStatusUpdated += HandlePowerStatusUpdated;

            var muteHandler = new VideoMuteHandler(key);
            muteHandler.VideoMuteStatusUpdated += HandleMuteStatusUpdated;

            var inputHandler = new VideoInputHandler(key);
            inputHandler.VideoInputUpdated += HandleVideoInputUpdated;

            new StringResponseProcessor(gather,
                s =>
                    {
                        powerHandler.ProcessResponse(s);
                        muteHandler.ProcessResponse(s);
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

            Feedbacks = new FeedbackCollection<Feedback>
                {
                    PowerIsOnFeedback,
                    IsWarmingUpFeedback,
                    IsCoolingDownFeedback,
                    PowerIsOffFeedback,
                    VideoMuteIsOn,
                    VideoMuteIsOff,
                    CurrentInputValueFeedback,
                    new StringFeedback("RequestedPower", () => _requestedPowerStatus.ToString()),
                    new StringFeedback("RequestedMute", () => _requestedMuteStatus.ToString()),
                    new StringFeedback("RequestedInput", () => _requestedVideoInput.ToString()),
                };

            CrestronEnvironment.ProgramStatusEventHandler += type =>
                {
                    if (type != eProgramStatusEventType.Stopping)
                        return;

                    if (_pollTimer == null)
                        return;

                    _pollTimer.Stop();
                    _pollTimer.Dispose();
                };
        }

        private void socket_ConnectionChange(object sender, GenericSocketStatusChageEventArgs e)
        {
            if (e.Client.IsConnected)
            {
                Debug.Console(2, this, "Connected, sending ESCVPnet Command");
                var cmd = new byte[] {0x45, 0x53, 0x43, 0x2F, 0x56, 0x50, 0x2E, 0x6E, 0x65, 0x74, 0x10, 0x03, 0x00, 0x00, 0x00, 0x00};
                _coms.SendBytes(cmd);
                _pollTimer.Reset(329, _pollTime);
                CommunicationMonitor.Start();
            }
            else
            {
                Debug.Console(2, this, "Disconnected");
                _pollTimer.Stop();
                CommunicationMonitor.Stop();
            }

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

        public override bool CustomActivate()
        {
            if (_coms != null)
                _coms.Connect();

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
                },
                null,
                5189,
                _pollTime);

            IsWarmingUpFeedback.OutputChange += (sender, args) =>
            {
                if (!args.BoolValue)
                    return;

                ProcessRequestedMuteStatus();
            };

            PowerIsOnFeedback.OutputChange += (sender, args) =>
                {
                    if (!args.BoolValue)
                        return;

                    ProcessRequestedVideoInput();
                    ProcessRequestedMuteStatus();
                };
            var socket = _coms as ISocketStatus;
            if (socket == null)
            {
                CommunicationMonitor.Start();
            }
            return base.CustomActivate();
        }

        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new JoinMap(joinStart);
            if (bridge != null)
                bridge.AddJoinMap(Key, joinMap);

            Bridge.LinkToApi(this, trilist, joinMap);
        }

        private void HandlePowerStatusUpdated(object sender, Events.PowerEventArgs eventArgs)
        {
            _currentPowerStatus = eventArgs.Status;
            ProcessRequestedPowerStatus();
            Feedbacks.FireAllFeedbacks();
        }

        private void HandleMuteStatusUpdated(object sender, Events.VideoMuteEventArgs videoMuteEventArgs)
        {
            _currentVideoMuteStatus = videoMuteEventArgs.Status;

            ProcessRequestedMuteStatus();
            Feedbacks.FireAllFeedbacks();
        }

        private void ProcessRequestedPowerStatus()
        {
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
        }

        private void ProcessRequestedPowerOn()
        {
            if (_requestedPowerStatus != PowerHandler.PowerStatusEnum.PowerOn)
                throw new InvalidOperationException("Power on isn't requested");

            switch (_currentPowerStatus)
            {
                case PowerHandler.PowerStatusEnum.PowerOn:
                    _requestedPowerStatus = PowerHandler.PowerStatusEnum.None;
                    break;
                case PowerHandler.PowerStatusEnum.PowerWarming:
                    _requestedPowerStatus = PowerHandler.PowerStatusEnum.None;
                    break;
                case PowerHandler.PowerStatusEnum.PowerCooling:
                    break;
                case PowerHandler.PowerStatusEnum.PowerOff:
                    //_currentPowerStatus = PowerHandler.PowerStatusEnum.PowerWarming;
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
                    _currentPowerStatus = PowerHandler.PowerStatusEnum.PowerCooling;
                    break;
                case PowerHandler.PowerStatusEnum.PowerWarming:
                    break;
                case PowerHandler.PowerStatusEnum.PowerCooling:
                    _requestedPowerStatus = PowerHandler.PowerStatusEnum.None;
                    break;
                case PowerHandler.PowerStatusEnum.PowerOff:
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
            if (!PowerIsOnFeedback.BoolValue && !IsWarmingUpFeedback.BoolValue)
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

        private void ProcessRequestedVideoInput()
        {
            if (!PowerIsOnFeedback.BoolValue)
                return;

            if (_requestedVideoInput == _currentVideoInput)
                _requestedVideoInput = VideoInputHandler.VideoInputStatusEnum.None;

            switch (_requestedVideoInput)
            {
                case VideoInputHandler.VideoInputStatusEnum.None:
                    break;
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

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleVideoInputUpdated(object sender, Events.VideoInputEventArgs videoInputEventArgs)
        {
            _currentVideoInput = videoInputEventArgs.Input;
            ProcessRequestedVideoInput();
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
            if (_requestedPowerStatus != PowerHandler.PowerStatusEnum.PowerOn && !PowerIsOnFeedback.BoolValue && _requestedPowerStatus != PowerHandler.PowerStatusEnum.PowerWarming && !IsWarmingUpFeedback.BoolValue)
                return;

            _requestedMuteStatus = VideoMuteHandler.VideoMuteStatusEnum.Muted;
            ProcessRequestedMuteStatus();
            Feedbacks.FireAllFeedbacks();
            _pollTimer.Reset(329, _pollTime);
        }

        public void VideoMuteOff()
        {
            if (_requestedPowerStatus != PowerHandler.PowerStatusEnum.PowerOn && !PowerIsOnFeedback.BoolValue || !VideoMuteIsOn.BoolValue) 
                return;

            _requestedMuteStatus = VideoMuteHandler.VideoMuteStatusEnum.Unmuted;
            ProcessRequestedMuteStatus();
            Feedbacks.FireAllFeedbacks();
            _pollTimer.Reset(329, _pollTime);
        }

        public BoolFeedback VideoMuteIsOn { get; private set; }

        public void PowerOn()
        {
            _requestedPowerStatus = PowerHandler.PowerStatusEnum.PowerOn;
            ProcessRequestedPowerStatus();
            Feedbacks.FireAllFeedbacks();
            _pollTimer.Reset(329, _pollTime);
        }

        public void PowerOff()
        {
            _requestedPowerStatus = PowerHandler.PowerStatusEnum.PowerOff;
            _requestedMuteStatus = VideoMuteHandler.VideoMuteStatusEnum.None;
            _requestedVideoInput = VideoInputHandler.VideoInputStatusEnum.None;

            ProcessRequestedPowerStatus();
            Feedbacks.FireAllFeedbacks();
            _pollTimer.Reset(329, _pollTime);
        }

        public void PowerToggle()
        {
            switch (_requestedPowerStatus)
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

        public void ExecuteSwitch(object inputSelector)
        {
            try
            {
                var input = Convert.ToInt32(inputSelector);

                var inputToSwitch = (VideoInputHandler.VideoInputStatusEnum) input;
                if (inputToSwitch == VideoInputHandler.VideoInputStatusEnum.None)
                    return;

                _requestedVideoInput = inputToSwitch;

                PowerOn();
                VideoMuteOff();
                ProcessRequestedVideoInput();
                _pollTimer.Reset(438, _pollTime);
            }
            catch (Exception ex)
            {
                Debug.Console(1, this, "Error executing switch : {0}{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                Feedbacks.FireAllFeedbacks();
            }
        }

        public BoolFeedback PowerIsOnFeedback { get; private set; }
        public BoolFeedback PowerIsOffFeedback { get; private set; }
        public BoolFeedback VideoMuteIsOff { get; private set; }
        public StatusMonitorBase CommunicationMonitor { get; private set; }
        public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }
        public BoolFeedback IsWarmingUpFeedback { get; private set; }
        public BoolFeedback IsCoolingDownFeedback { get; private set; }
        public FeedbackCollection<Feedback> Feedbacks { get; private set; }
        public IntFeedback LampHoursFeedback { get; private set; }
        public StringFeedback SerialNumberFeedback { get; private set; }
        public IntFeedback CurrentInputValueFeedback { get; private set; }

        public string CurrentSourceInfoKey { get; set; }
        public SourceListItem CurrentSourceInfo { get; set; }
        public event SourceInfoChangeHandler CurrentSourceChange;

        public BoolFeedback IsOnline
        {
            get { return CommunicationMonitor.IsOnlineFeedback; }
        }
    }
}