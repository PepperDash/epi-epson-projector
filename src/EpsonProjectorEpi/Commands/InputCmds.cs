namespace EpsonProjectorEpi.Commands
{
    public class SourceInputComputerCmd : IEpsonCmd
    {
        #region IEpsonCmd Members

        public string CmdString
        {
            get { return "SOURCE 11"; }
        }

        #endregion
    }

    public class SourceInputDviCmd : IEpsonCmd
    {
        #region IEpsonCmd Members

        public string CmdString
        {
            get { return "SOURCE A0"; }
        }

        #endregion
    }

    public class SourceInputHdmiCmd : IEpsonCmd
    {
        #region IEpsonCmd Members

        public string CmdString
        {
            get { return "SOURCE 30"; }
        }

        #endregion
    }

    public class SourceInputVideoCmd : IEpsonCmd
    {
        #region IEpsonCmd Members

        public string CmdString
        {
            get { return "SOURCE 45"; }
        }

        #endregion
    }
}