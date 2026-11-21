using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace MycoMenuLib;

[MycoMod(null, ModFlags.IsClientSide)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger;

    private void Awake()
    {
        Logger = base.Logger;

        try
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }
        catch (Exception exception)
        {
            Logger.LogError($"Failed to apply patches {exception.Message}");
        }

        Logger.LogInfo("MycoMenuLib loaded");
    }
}