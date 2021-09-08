using System.Reflection;

namespace EnhancedOutsideConnectionsView
{
    /// <summary>
    /// Harmony patching for OutsideConnectionsInfoViewPanel (OCIVP)
    /// </summary>
    public class OCIVPPatch
    {
        /// <summary>
        /// create patch for OutsideConnectionsInfoViewPanel.UpdatePanel
        /// </summary>
        public static bool CreateUpdatePanelPatch()
        {
            // patch with the postfix routine
            return HarmonyPatcher.CreatePostfixPatch(typeof(OutsideConnectionsInfoViewPanel), "UpdatePanel", BindingFlags.Instance | BindingFlags.NonPublic, typeof(OCIVPPatch), "OutsideConnectionsInfoViewPanelUpdatePanel");
        }

        /// <summary>
        /// update everything after base processing
        /// base processing is always allowed to execute because it does a few other things that are needed
        /// </summary>
        public static void OutsideConnectionsInfoViewPanelUpdatePanel()
        {
            // update the panel
            EOCVUserInterface.instance.UpdatePanel();
        }

    }
}
