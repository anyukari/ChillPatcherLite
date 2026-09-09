using System;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using Bulbul;
using ChillPatcherLite.Cover;
using ChillPatcherLite.Compatibility;
using ChillPatcherLite.Patches;
using HarmonyLib;

namespace ChillPatcherLite;

[BepInPlugin(Guid, Name, Version)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string Guid = "com.chillpatcherlite.plugin";
    public const string Name = "ChillPatcherLite";
    public const string Version = "1.0.7";

    internal static ManualLogSource Log;

    private Harmony _harmony;

    private void Awake()
    {
        Log = Logger;

        ModConfig.Initialize(Config);
        DefaultCoverProvider.Initialize();
        UnityRunner.EnsureInstance();
        UIRearrangePatch.EnsureMonitor();
        ChillMusicSyncCoop.TryEnable();

        try
        {
            _harmony = new Harmony(Guid);
            ApplyPatches(_harmony);
        }
        catch (Exception ex)
        {
            Logger.LogError(Name + " patch failed: " + ex);
        }

        Logger.LogInfo(Name + " v" + Version + " loaded. UI Rearrange: " +
                       ModConfig.EnableUIRearrange.Value +
                       ", Album Art Display: " + ModConfig.EnableAlbumArtDisplay.Value);
    }

    private void ApplyPatches(Harmony harmony)
    {
        // 1. UI 重排列：FacilityMusic.Setup 后置
        var facilitySetup = AccessTools.Method(typeof(FacilityMusic), "Setup");
        var rearrangePostfix = AccessTools.Method(
            typeof(UIRearrangePatch), "FacilityMusic_Setup_Postfix");

        if (facilitySetup == null)
        {
            Logger.LogError("[Patch] target not found: FacilityMusic.Setup");
        }
        else if (rearrangePostfix == null)
        {
            Logger.LogError("[Patch] postfix not found: UIRearrangePatch.FacilityMusic_Setup_Postfix");
        }
        else
        {
            harmony.Patch(facilitySetup, postfix: new HarmonyMethod(rearrangePostfix));
            Logger.LogInfo("[Patch] applied postfix -> FacilityMusic.Setup");
        }

        // 2. 大封面：MusicUI 的显式接口实现 Bulbul.IMusicListUI.Setup 后置
        var musicUiSetup = AccessTools.Method(typeof(MusicUI), "Bulbul.IMusicListUI.Setup");
        var albumArtPostfix = AccessTools.Method(typeof(MusicUIAlbumArtPatch), "Setup_Postfix");

        if (musicUiSetup == null)
        {
            Logger.LogError("[Patch] target not found: MusicUI.Bulbul.IMusicListUI.Setup");
        }
        else if (albumArtPostfix == null)
        {
            Logger.LogError("[Patch] postfix not found: MusicUIAlbumArtPatch.Setup_Postfix");
        }
        else
        {
            harmony.Patch(musicUiSetup, postfix: new HarmonyMethod(albumArtPostfix));
            Logger.LogInfo("[Patch] applied postfix -> MusicUI.Bulbul.IMusicListUI.Setup");
        }

        try
        {
            var patched = harmony.GetPatchedMethods()
                .Select(m => m.DeclaringType?.Name + "." + m.Name)
                .ToList();
            Logger.LogInfo("[Patch] currently patched by this plugin: " +
                           string.Join(", ", patched));
        }
        catch (Exception ex)
        {
            Logger.LogWarning("[Patch] failed to list patched methods: " + ex.Message);
        }
    }

    private void OnDestroy()
    {
        try
        {
            _harmony?.UnpatchSelf();
        }
        catch (Exception ex)
        {
            Log?.LogWarning("Harmony cleanup failed: " + ex);
        }
    }
}
