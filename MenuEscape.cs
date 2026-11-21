namespace MycoMenuLib;


public static class MenuEscape
{
    public static void RegisterPendingBlocker(System.Func<bool> tryBlock) =>
        EscapeGuard.RegisterPendingBlocker(tryBlock);
}
