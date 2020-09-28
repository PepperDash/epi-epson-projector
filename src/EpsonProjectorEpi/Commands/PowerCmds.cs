namespace EpsonProjectorEpi.Commands
{
    public class PowerOnCmd : IEpsonCmd
    {
        #region IEpsonCmd Members

        public string CmdString
        {
            get { return "PWR ON\r"; }
        }

        #endregion
    }

    public class PowerOffCmd : IEpsonCmd
    {
        #region IEpsonCmd Members

        public string CmdString
        {
            get { return "PWR OFF\r"; }
        }

        #endregion
    }
}