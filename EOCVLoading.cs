using ICities;
using UnityEngine;
using System;
using Harmony;
using ColossalFramework.Plugins;
using ColossalFramework;
using ColossalFramework.UI;

namespace EnhancedOutsideConnectionsView
{
    /// <summary>
    /// handle game loading and unloading
    /// </summary>
    /// <remarks>A new instance of EOCVLoading is NOT created when loading a game from the Pause Menu.</remarks>
    public class EOCVLoading : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            // do base processing
            base.OnLevelLoaded(mode);

            try
            {
                // check for new or loaded game
                if (mode == LoadMode.NewGame || mode == LoadMode.NewGameFromScenario || mode == LoadMode.LoadGame)
                {
                    // determine if Exclude Mail mod is enabled
                    foreach (PluginManager.PluginInfo mod in Singleton<PluginManager>.instance.GetPluginsInfo())
                    {
                        // ignore builtin mods and camera script
                        if (!mod.isBuiltin && !mod.isCameraScript)
                        {
                            // check against the Exclude Mail workshop ID
                            if (mod.publishedFileID.AsUInt64 == 2093019121)
                            {
                                if (mod.isEnabled)
                                {
                                    // create dialog panel
                                    ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
                                    panel.SetMessage(
                                        "Enhanced Outside Connections View", 
                                        "The Enhanced Outside Connections View mod supersedes the Exclude Mail mod.  \n\n" +
                                        "Please unsubscribe from the Exclude Mail mod.",
                                        false);

                                    // do not initialize this mod
                                    return;
                                }

                                // found it, but not enabled
                                break;
                            }
                        }
                    }

                    // initialize Harmony
                    EOCV.Harmony = HarmonyInstance.Create("com.github.rcav8tr.EnhancedOutsideConnectionsView");
                    if (EOCV.Harmony == null)
                    {
                        Debug.LogError("Unable to create Harmony instance.");
                        return;
                    }

                    // initialize user interface
                    if (!EOCVUserInterface.Initialize()) return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public override void OnLevelUnloading()
        {
            // do base processing
            base.OnLevelUnloading();

            try
            {
                // remove Harmony patches
                if (EOCV.Harmony != null)
                {
                    EOCV.Harmony.UnpatchAll();
                    EOCV.Harmony = null;
                }

                // deinitialize user interface
                EOCVUserInterface.Deinitialize();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}