using HarmonyLib;

namespace MycoMenuLib;

[HarmonyPatch(typeof(Menu), "OnCancel")]
internal static class MenuEscapePatch
{
    private static bool Prefix() =>
        !EscapeGuard.ShouldBlock && !EscapeGuard.ConsumePendingBlocker;
}

[HarmonyPatch(typeof(Menu), "OnClosePressed")]
internal static class MenuClosePressedPatch
{
    private static bool Prefix() =>
        !EscapeGuard.ShouldBlock && !EscapeGuard.ConsumePendingBlocker;
}

[HarmonyPatch(typeof(WindowSystem), "TryEscape")]
internal static class WindowEscapePatch
{
    private static bool Prefix() =>
        !EscapeGuard.ShouldBlock && !EscapeGuard.ConsumePendingBlocker;
}
