using CitiesHarmony.API;
using HarmonyLib;
using System.Reflection;
using System;

namespace EnhancedOutsideConnectionsView
{
    /// <summary>
    /// Harmony patching
    /// </summary>
    internal class HarmonyPatcher
    {
        private const string HarmonyId = "com.github.rcav8tr.EnhancedOutsideConnectionsView";

        /// <summary>
        /// create Harmony patches
        /// </summary>
        public static bool CreatePatches()
        {
            // check Harmony
            if (!HarmonyHelper.IsHarmonyInstalled)
            {
                ColossalFramework.UI.UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Missing Dependency",
                    "The Enhanced Outside Connections View mod requires the 'Harmony (Mod Dependency)' mod." + Environment.NewLine + Environment.NewLine + 
                    "Please subscribe to the 'Harmony (Mod Dependency)' mod and restart the game.", error: false);
                return false;
            }

            // create the patches
            if (!BuildingAIPatch.CreateGetColorPatches()) return false;
            if (!VehicleAIPatch.CreateGetColorPatches()) return false;
            if (!OCIVPPatch.CreateUpdatePanelPatch()) return false;

            // success
            return true;
        }

        /// <summary>
        /// create a prefix patch (i.e. called before the base processing)
        /// </summary>
        /// <param name="originalType">type that contains the method to be patched</param>
        /// <param name="originalMethodName">name of the method to be patched</param>
        /// <param name="bindingFlags">bindings flags of the method to be patched</param>
        /// <param name="prefixType">type that contains the prefix method</param>
        /// <param name="prefixMethodName">name of the prefix method</param>
        /// <returns>success status</returns>
        public static bool CreatePrefixPatch(Type originalType, string originalMethodName, BindingFlags bindingFlags, Type prefixType, string prefixMethodName)
        {
            // get the original method
            MethodInfo originalMethod = originalType.GetMethod(originalMethodName, bindingFlags);
            if (originalMethod == null)
            {
                LogUtil.LogError($"Unable to find original method {originalType.Name}.{originalMethodName}.");
                return false;
            }

            // get the prefix method
            MethodInfo prefixMethod = prefixType.GetMethod(prefixMethodName, BindingFlags.Static | BindingFlags.Public);
            if (prefixMethod == null)
            {
                LogUtil.LogError($"Unable to find patch prefix method {prefixType.Name}.{prefixMethodName}.");
                return false;
            }

            // create the patch
            new Harmony(HarmonyId).Patch(originalMethod, new HarmonyMethod(prefixMethod), null);

            // success
            return true;
        }

        /// <summary>
        /// create a prefix patch for a vehicle AI (i.e. called before the base processing)
        /// </summary>
        /// <param name="originalType">type that contains the method to be patched</param>
        /// <param name="originalMethodName">name of the method to be patched</param>
        /// <param name="prefixType">type that contains the prefix method</param>
        /// <param name="prefixMethodName">name of the prefix method</param>
        /// <returns>success status</returns>
        public static bool CreatePrefixPatchVehicleAI(Type originalType, string originalMethodName, Type prefixType, string prefixMethodName)
        {
            // get the original vehicle AI method
            // There is a GetColor routine in the derived AI classes which has Vehicle as a ref parameter.
            // There is a GetColor routine in the base class VehicleAI which has VehicleParked as a ref parameter.
            // The GetColor in the derived class is the one to be patched, so need to pass type Vehicle as a ref parameter.
            MethodInfo originalMethod = originalType.GetMethod(originalMethodName, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) });
            if (originalMethod == null)
            {
                LogUtil.LogError($"Unable to find original method {originalType.Name}.{originalMethodName}.");
                return false;
            }

            // get the prefix method
            MethodInfo prefixMethod = prefixType.GetMethod(prefixMethodName, BindingFlags.Static | BindingFlags.Public);
            if (prefixMethod == null)
            {
                LogUtil.LogError($"Unable to find patch prefix method {prefixType.Name}.{prefixMethodName}.");
                return false;
            }

            // create the patch
            new Harmony(HarmonyId).Patch(originalMethod, new HarmonyMethod(prefixMethod), null);

            // success
            return true;
        }

        /// <summary>
        /// create a postfix patch (i.e. called after the base processing)
        /// </summary>
        /// <param name="originalType">type that contains the method to be patched</param>
        /// <param name="originalMethodName">name of the AI method to be patched</param>
        /// <param name="bindingFlags">bindings flags of the AI method to be patched</param>
        /// <param name="postfixType">type that contains the postfix method</param>
        /// <param name="postfixMethodName">name of the postfix method</param>
        /// <returns>success status</returns>
        public static bool CreatePostfixPatch(Type originalType, string originalMethodName, BindingFlags bindingFlags, Type postfixType, string postfixMethodName)
        {
            // get the original method
            MethodInfo originalMethod = originalType.GetMethod(originalMethodName, bindingFlags);
            if (originalMethod == null)
            {
                LogUtil.LogError($"Unable to find original method {originalType.Name}.{originalMethodName}.");
                return false;
            }

            // get the postfix method
            MethodInfo postfixMethod = postfixType.GetMethod(postfixMethodName, BindingFlags.Static | BindingFlags.Public);
            if (postfixMethod == null)
            {
                LogUtil.LogError($"Unable to find patch postfix method {postfixType.Name}.{postfixMethodName}.");
                return false;
            }

            // create the patch
            new Harmony(HarmonyId).Patch(originalMethod, null, new HarmonyMethod(postfixMethod));

            // success
            return true;
        }
        
        /// <summary>
        /// remove Harmony patches
        /// </summary>
        public static void RemovePatches()
        {
            try
            {
                if (HarmonyHelper.IsHarmonyInstalled)
                {
                    new Harmony(HarmonyId).UnpatchAll(HarmonyId);
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogException(ex);
            }
        }
    }
}
