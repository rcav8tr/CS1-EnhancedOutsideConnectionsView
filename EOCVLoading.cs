using ICities;
using System;
using ColossalFramework.Plugins;
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
                    foreach (PluginManager.PluginInfo mod in PluginManager.instance.GetPluginsInfo())
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
                                        "The Enhanced Outside Connections View mod supersedes the Exclude Mail mod." + Environment.NewLine + Environment.NewLine +
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

                    // initialize user interface
                    if (!EOCVUserInterface.instance.Initialize()) return;

                    // create the Harmony patches
                    if (!HarmonyPatcher.CreatePatches()) return;
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogException(ex);
            }
        }

        public override void OnLevelUnloading()
        {
            // do base processing
            base.OnLevelUnloading();

            try
            {
                try
                {
                    // remove Harmony patches
                    HarmonyPatcher.RemovePatches();
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    // ignore missing Harmony, rethrow all others
                    if (!ex.FileName.ToUpper().Contains("HARMONY"))
                    {
                        throw ex;
                    }
                }

                // deinitialize user interface and snapshots
                EOCVUserInterface.instance.Deinitialize();
                EOCVSnapshots.instance.Deinitialize();
            }
            catch (Exception ex)
            {
                LogUtil.LogException(ex);
            }
        }
    }
}