using Harmony;
using UnityEngine;
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
        public static void CreateUpdatePanelPatch()
        {
            // get the original UpdatePanel method
            MethodInfo original = typeof(OutsideConnectionsInfoViewPanel).GetMethod("UpdatePanel", BindingFlags.Instance | BindingFlags.NonPublic);
            if (original == null)
            {
                Debug.LogError($"Unable to find OutsideConnectionsInfoViewPanel.UpdatePanel method.");
                return;
            }

            // find the UpdatePanelPostfix method
            MethodInfo postfix = typeof(OCIVPPatch).GetMethod("UpdatePanelPostfix", BindingFlags.Static | BindingFlags.Public);
            if (postfix == null)
            {
                Debug.LogError($"Unable to find OCIVPPatch.UpdatePanelPostfix method.");
                return;
            }

            // create the patch
            EOCV.Harmony.Patch(original, null, new HarmonyMethod(postfix), null);
        }

        /// <summary>
        /// update everything after base processing
        /// base processing is always allowed to execute because it does a few other things that are needed
        /// </summary>
        public static void UpdatePanelPostfix()
        {
            // update the panel
            EOCVUserInterface.UpdatePanel();
        }

    }
}
