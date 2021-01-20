using Harmony;
using ICities;

namespace EnhancedOutsideConnectionsView
{
    public class EOCV : IUserMod
    {
        // required name and description of this mod
        public string Name => "Enhanced Outside Connections View";
        public string Description => "Show or hide resources on the Outside Connections info view";

        // Harmony instance
        public static HarmonyInstance Harmony;
    }
}