﻿using DaLion.Common;
using DaLion.Common.Attributes;
using HarmonyLib;

namespace DaLion.Stardew.Professions.Framework.Patches;

/// <summary>Wildcard prefix patch for on-demand debugging.</summary>
[UsedImplicitly, DebugOnly]
internal class DebugPatch : DaLion.Common.Harmony.HarmonyPatch
{
    /// <summary>Construct an instance.</summary>
    internal DebugPatch()
    {
        //Target = RequireMethod<>(nameof(.));
    }

    #region harmony patches

    [HarmonyPrefix]
    private static bool DebugPrefix(object __instance)
    {
        Log.D("DebugPatch called!");


        return false; // don't run original logic
    }

    #endregion harmony patches
}