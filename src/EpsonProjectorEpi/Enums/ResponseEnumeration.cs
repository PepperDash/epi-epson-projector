namespace EpsonProjectorEpi.Enums
{
    public abstract class ResponseEnumeration<T, TState> : Enumeration<T> 
        where T : ResponseEnumeration<T, TState>
    {
        public TState ResponseState { get; protected set; }
        public string ResponseString { get; protected set; }

        public static string SearchString { get; protected set; }

        protected ResponseEnumeration(int value, string name)
            : base (value, name)
        {
            
        }
    }

    public class PowerResponseEnum : ResponseEnumeration<PowerResponseEnum, ProjectorPower>
    {
        static PowerResponseEnum()
        {
            SearchString = "PWR";
        }

        PowerResponseEnum(int value, string name)
            : base (value, name)
        {
            
        }

        public static readonly PowerResponseEnum PowerOff = new PowerResponseEnum(0, "Power Off")
        {
            ResponseState = ProjectorPower.PowerOff,
            ResponseString = "PWR=00"
        };

        public static readonly PowerResponseEnum PowerOn = new PowerResponseEnum(1, "Power On")
        {
            ResponseState = ProjectorPower.PowerOn,
            ResponseString = "PWR=01"
        };

        public static readonly PowerResponseEnum PowerWarming = new PowerResponseEnum(2, "Warming")
        {
            ResponseState = ProjectorPower.Warming,
            ResponseString = "PWR=02"
        };

        public static readonly PowerResponseEnum PowerCooling = new PowerResponseEnum(3, "Cooling")
        {
            ResponseState = ProjectorPower.Cooling,
            ResponseString = "PWR=03"
        };

        public static readonly PowerResponseEnum PowerStandby = new PowerResponseEnum(4, "Standby")
        {
            ResponseState = ProjectorPower.PowerOff,
            ResponseString = "PWR=04"
        };

        public static readonly PowerResponseEnum AbnormalStandby = new PowerResponseEnum(5, "Standby")
        {
            ResponseState = ProjectorPower.PowerOff,
            ResponseString = "PWR=05"
        };
    }

    public class InputResponseEnum : ResponseEnumeration<InputResponseEnum, ProjectorInput>
    {
        static InputResponseEnum()
        {
            SearchString = "SOURCE";
        }

        InputResponseEnum(int value, string name)
            : base(value, name)
        {

        }

        public static readonly InputResponseEnum Hdmi = new InputResponseEnum(1, "Hdmi")
        {
            ResponseState = ProjectorInput.Hdmi,
            ResponseString = "SOURCE=A0"
        };

        public static readonly InputResponseEnum Dvi = new InputResponseEnum(2, "Dvi")
        {
            ResponseState = ProjectorInput.Dvi,
            ResponseString = "SOURCE=30"
        };

        public static readonly InputResponseEnum Computer = new InputResponseEnum(3, "Computer")
        {
            ResponseState = ProjectorInput.Computer,
            ResponseString = "SOURCE=11"
        };

        public static readonly InputResponseEnum Video = new InputResponseEnum(4, "Video")
        {
            ResponseState = ProjectorInput.Video,
            ResponseString = "SOURCE=45"
        };
    }

    public class MuteResponseEnum : ResponseEnumeration<MuteResponseEnum, ProjectorMute>
    {
        static MuteResponseEnum()
        {
            SearchString = "MUTE";
        }

        MuteResponseEnum(int value, string name)
            : base(value, name)
        {

        }

        public static readonly MuteResponseEnum MuteOn = new MuteResponseEnum(1, "Mute On")
        {
            ResponseState = ProjectorMute.MuteOn,
            ResponseString = "MUTE=ON"
        };

        public static readonly MuteResponseEnum MuteOff = new MuteResponseEnum(0, "Mute Off")
        {
            ResponseState = ProjectorMute.MuteOff,
            ResponseString = "MUTE=OFF"
        };
    }
}