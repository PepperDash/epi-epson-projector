using System;

using Crestron.SimplSharp;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Queues;
using Feedback = PepperDash.Essentials.Core.Feedback;
using Thread = Crestron.SimplSharpPro.CrestronThread.Thread;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using System.Collections.Generic;
using Crestron.SimplSharpPro.DM;


namespace EpsonProjectorEpi
{
    public class EpsonProjector : PepperDash.Essentials.Devices.Common.Displays.TwoWayDisplayBase, IHasPowerControlWithFeedback,
        IWarmingCooling, IOnline, IBasicVideoMuteWithFeedback, ICommunicationMonitor, IHasFeedback, IHasInputs<int>
    {
        private readonly IBasicCommunication _coms;
        private readonly GenericQueue _commandQueue;
        private CTimer _pollTimer;
		private CTimer _LensTimer; 
        private const int _pollTime = 6000;

        private PowerHandler.PowerStatusEnum _currentPowerStatus;
        private PowerHandler.PowerStatusEnum _requestedPowerStatus;

        private VideoMuteHandler.VideoMuteStatusEnum _currentVideoMuteStatus;
        private VideoMuteHandler.VideoMuteStatusEnum _requestedMuteStatus;

        private VideoInputHandler.VideoInputStatusEnum _currentVideoInput;
        private VideoInputHandler.VideoInputStatusEnum _requestedVideoInput;

        public ISelectableItems<int> Inputs { get; private set; }



        public EpsonProjector(string key, string name, PropsConfig config, IBasicCommunication coms) : base(key, name)
        {
            _coms = coms;
            if (config.Monitor == null)
                config.Monitor = GetDefaultMonitorConfig();

            CommunicationMonitor = new GenericCommunicationMonitor(this, coms, config.Monitor);
            var gather = new CommunicationGather(coms, "\x0D:");

            _commandQueue = new GenericQueue(key + "-command-queue", 213, Thread.eThreadPriority.MediumPriority, 50);

            

            SetupInputs();
               
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
            //inputHandler.VideoInputUpdated += HandleVideoInputUpdated;

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
                case VideoInputHandler.VideoInputStatusEnum.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetupInputs()
        {

            Inputs = new EpsonInputs
            {
                Items = new Dictionary<int, ISelectableItem>
                    {
                        {1, new EpsonInput("Hdmi", "Hdmi", this, SetHDMI) },
                        {2, new EpsonInput("DVI", "DVI", this, SetDVI) },
                        {3, new EpsonInput("Computer", "Computer", this, SetComputer) },
                        {4, new EpsonInput("Video", "Video", this, SetVideo) }
                    }
            };
        }

        public void SetHDMI()
        {
            ExecuteSwitch(VideoInputHandler.VideoInputStatusEnum.Hdmi);
        }

        public void SetDVI()
        {
            ExecuteSwitch(VideoInputHandler.VideoInputStatusEnum.Dvi);
        }

        public void SetComputer()
        {
            ExecuteSwitch(VideoInputHandler.VideoInputStatusEnum.Computer);

        }

        public void SetVideo()
        {
            ExecuteSwitch(VideoInputHandler.VideoInputStatusEnum.Video);
        }
        private void HandleVideoInputUpdated(object sender, Events.VideoInputEventArgs videoInputEventArgs)
        {
            _currentVideoInput = videoInputEventArgs.Input;
            Inputs.CurrentItem = (int)_currentVideoInput;
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

        public override void PowerOn()
        {
            _requestedPowerStatus = PowerHandler.PowerStatusEnum.PowerOn;
            ProcessRequestedPowerStatus();
            Feedbacks.FireAllFeedbacks();
            _pollTimer.Reset(329, _pollTime);
        }

        public override void PowerOff()
        {
            _requestedPowerStatus = PowerHandler.PowerStatusEnum.PowerOff;
            _requestedMuteStatus = VideoMuteHandler.VideoMuteStatusEnum.None;
            _requestedVideoInput = VideoInputHandler.VideoInputStatusEnum.None;

            ProcessRequestedPowerStatus();
            Feedbacks.FireAllFeedbacks();
            _pollTimer.Reset(329, _pollTime);
        }

        public override void PowerToggle()
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

        public override void ExecuteSwitch(object inputSelector)
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

		/// <summary>
		/// Does what it says
		/// </summary>
		public void StartLensMoveRepeat(eLensFunction func)
		{
			if (_LensTimer == null)
			{
				_LensTimer = new CTimer(o => LensFunction(func), null, 0, 250);
			}
		}

		/// <summary>
		/// Does what it says
		/// </summary>
		public void StopLensMoveRepeat()
		{
			if (_LensTimer != null)
			{
				_LensTimer.Stop();
				_LensTimer = null;
			}
		}

		public void LensFunction(eLensFunction function)
		{
			string message; 
			switch (function)
			{
				case eLensFunction.ZoomPlus:  message = Commands.ZoomInc; break;
				case eLensFunction.ZoomMinus: message = Commands.ZoomDec; break;
				case eLensFunction.FocusPlus: message = Commands.FocusInc; break;
				case eLensFunction.FocusMinus: message = Commands.FocusDec; break;
				case eLensFunction.HShiftPlus: message = Commands.HLensInc; break;
				case eLensFunction.HShiftMinus: message = Commands.HLensDec; break;
				case eLensFunction.VShiftPlus: message = Commands.VLensInc; break;
				case eLensFunction.VShiftMinus: message = Commands.VLensDec; break;
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

        

        

       

        public BoolFeedback PowerIsOnFeedback { get; private set; }
        public BoolFeedback PowerIsOffFeedback { get; private set; }
        public BoolFeedback VideoMuteIsOff { get; private set; }
        public StatusMonitorBase CommunicationMonitor { get; private set; }
        //public RoutingPortCollection<RoutingInputPort> InputPorts { get; private set; }
        public ISelectableItems<EpsonInput> InputPorts { get; private set; }
        public BoolFeedback IsWarmingUpFeedback { get; private set; }
        public BoolFeedback IsCoolingDownFeedback { get; private set; }
        public FeedbackCollection<Feedback> Feedbacks { get; private set; }
        public IntFeedback LampHoursFeedback { get; private set; }
        public StringFeedback SerialNumberFeedback { get; private set; }
        public IntFeedback CurrentInputValueFeedback { get; private set; }

        public string CurrentSourceInfoKey { get; set; }
        public SourceListItem CurrentSourceInfo { get; set; }
        public event SourceInfoChangeHandler CurrentSourceChange;
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
                    _requestedVideoInput = (VideoInputHandler.VideoInputStatusEnum)value;
                    Items[value].Select();
                    CurrentItemChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        protected override Func<string> CurrentInputFeedbackFunc => () => CurrentInputValueFeedback.IntValue.ToString();

        protected override Func<bool> PowerIsOnFeedbackFunc => () => PowerIsOnFeedback.BoolValue;

        protected override Func<bool> IsCoolingDownFeedbackFunc => () => IsCoolingDownFeedback.BoolValue;

        protected override Func<bool> IsWarmingUpFeedbackFunc => () => IsWarmingUpFeedback.BoolValue;
    }
    public enum eLensFunction
	{
		ZoomPlus,
		ZoomMinus,
		ZoomStop,
		FocusPlus,
		FocusMinus,
		FocusStop,
		HShiftPlus,
		HShiftMinus,
		HShiftStop,
		VShiftPlus,
		VShiftMinus,
		VShiftStop,
		Home
	}
	public enum eRemoteControls
	{
		Menu,
		Esc,
		CursorUp,
		CursorDown,
		CursorLeft,
		CursorRight,
		CursorEnter
	}
}