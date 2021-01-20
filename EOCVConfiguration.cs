namespace EnhancedOutsideConnectionsView
{
    /// <summary>
    /// define global (i.e. for this mod but not game specific) configuration properties
    /// </summary>
    /// <remarks>convention for the config file name seems to be the mod name + "Config.xml"</remarks>
    [ConfigurationFileName("EnhancedOutsideConnectionsViewConfig.xml")]
    public class EOCVConfiguration
    {
        // it is important to set default config values in case there is no config file
        public bool ImportGoods    = true;
        public bool ImportForestry = true;
        public bool ImportFarming  = true;
        public bool ImportOre      = true;
        public bool ImportOil      = true;
        public bool ImportMail     = true;

        public bool ExportGoods    = true;
        public bool ExportForestry = true;
        public bool ExportFarming  = true;
        public bool ExportOre      = true;
        public bool ExportOil      = true;
        public bool ExportMail     = true;
        public bool ExportFish     = true;
    }
}