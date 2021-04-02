using CitiesHarmony.API;
using ICities;

namespace EnhancedOutsideConnectionsView
{
    public class EOCV : IUserMod
    {
        // required name and description of this mod
        public string Name => "Enhanced Outside Connections View";
        public string Description => "Show or hide resources on the Outside Connections info view";

        public void OnEnabled()
        {
            // check Harmony
            HarmonyHelper.EnsureHarmonyInstalled();
        }
    }
}