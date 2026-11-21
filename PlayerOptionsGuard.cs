using System;
using HarmonyLib;

namespace MycoMenuLib;

/// <summary>
/// Dirty hack to bypass preference system used by some UI
/// </summary>
public static class ModConfigIds
{
    public const string Prefix = "MycoMenu.";

    internal static bool IsManaged(string id)
        => id != null && id.StartsWith(Prefix, StringComparison.Ordinal);
}

[HarmonyPatch(typeof(PlayerOptions.ConfigDictionary), "Item", MethodType.Getter)]
internal static class PlayerOptionsConfigGetPatch
{
    private static bool Prefix(string key, ref object __result)
    {
        if (!ModConfigIds.IsManaged(key)) return true;

        __result = null;
        return false;
    }
}

[HarmonyPatch(typeof(PlayerOptions.ConfigDictionary), "Item", MethodType.Setter)]
internal static class PlayerOptionsConfigSetPatch
{
    private static bool Prefix(string key) => !ModConfigIds.IsManaged(key);
}
