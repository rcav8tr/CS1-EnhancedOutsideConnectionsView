using CitiesHarmony.API;
using ICities;

namespace EnhancedOutsideConnectionsView
{
    public class EOCV : IUserMod
    {
        // required name and description of this mod
        public string Name => "Enhanced Outside Connections View";
        public string Description => "Show or hide resources, show resource values, and graph resources over time";

        /// <summary>
        /// make sure Harnoy is installed
        /// </summary>
        public void OnEnabled()
        {
            HarmonyHelper.EnsureHarmonyInstalled();
        }
    }
}