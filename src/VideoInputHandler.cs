using System;
using PepperDash.Core;
using PepperDash.Core.Logging;

namespace EpsonProjectorEpi
{
    public class VideoInputHandler : IKeyed
    {
        public const string SearchString = "SOURCE=";
        public const string VideoInputHdmi = "SOURCE=30";
        public const string VideoInputDvi = "SOURCE=A0";
        public const string VideoInputComputer = "SOURCE=10";
        public const string VideoInputVideo = "SOURCE=45";
        public const string VideoInputLan = "SOURCE=63";
        public const string VideoInputHdBaseT = "SOURCE=80";

        public enum VideoInputStatusEnum
        {
            None = 0, Hdmi = 1, Dvi = 2, Computer = 3, Video = 4, Lan = 5, HdBaseT = 6
        }

        public event EventHandler<Events.VideoInputEventArgs> VideoInputUpdated;

        public VideoInputHandler(string key)
        {
            Key = key;
        }

        public void ProcessResponse(string response)
        {
            if (!response.Contains(SearchString))
                return;

            if (response.Contains(VideoInputHdmi))
            {
                OnInputUpdated(new Events.VideoInputEventArgs
                    {
                        Input = VideoInputStatusEnum.Hdmi,
                    });

                return;
            }

            if (response.Contains(VideoInputDvi))
            {
                OnInputUpdated(new Events.VideoInputEventArgs
                    {
                        Input = VideoInputStatusEnum.Dvi,
                    });

                return;
            }

            if (response.Contains(VideoInputComputer))
            {
                OnInputUpdated(new Events.VideoInputEventArgs
                    {
                        Input = VideoInputStatusEnum.Computer,
                    });

                return;
            }

            if (response.Contains(VideoInputVideo))
            {
                OnInputUpdated(new Events.VideoInputEventArgs
                    {
                        Input = VideoInputStatusEnum.Video,
                    });

                return;
            }

            if (response.Contains(VideoInputLan))
            {
                OnInputUpdated(new Events.VideoInputEventArgs
                    {
                        Input = VideoInputStatusEnum.Lan,
                    });

                return;
            }

            if (response.Contains(VideoInputHdBaseT))
            {
                OnInputUpdated(new Events.VideoInputEventArgs
                {
                    Input = VideoInputStatusEnum.HdBaseT,
                });

                return;
            }

            this.LogWarning("Received an unknown video input response: {response}", response);
        }

        private void OnInputUpdated(Events.VideoInputEventArgs args)
        {
            var handler = VideoInputUpdated;
            if (handler == null)
                return;

            handler.Invoke(this, args);
        }

        public string Key { get; private set; }
    }
}