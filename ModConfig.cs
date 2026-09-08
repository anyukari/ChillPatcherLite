using BepInEx.Configuration;

namespace ChillPatcherLite;

/// <summary>
/// 与 ChillPatcher V1.3.4.1 保持相同键名的配置项。
/// 仅保留本 mod 需要的三个开关。
/// </summary>
public static class ModConfig
{
    /// <summary>启用 UI 重排列（将游戏 UI 调整为更接近音乐播放器的布局）。</summary>
    public static ConfigEntry<bool> EnableUIRearrange { get; private set; }

    /// <summary>启用音乐封面显示（把播放列表按钮替换/显示当前歌曲封面）。</summary>
    public static ConfigEntry<bool> EnableAlbumArtDisplay { get; private set; }

    /// <summary>隐藏音乐控制条的底部背景图（UI 重排列附带选项）。</summary>
    public static ConfigEntry<bool> HideBottomBackImage { get; private set; }

    public static void Initialize(ConfigFile config)
    {
        EnableUIRearrange = config.Bind(
            "Features",
            "EnableUIRearrange",
            true,
            "Rearrange main UI buttons layout");

        EnableAlbumArtDisplay = config.Bind(
            "Features",
            "EnableAlbumArtDisplay",
            true,
            "Display album art on playlist toggle button");

        HideBottomBackImage = config.Bind(
            "Features",
            "HideBottomBackImage",
            false,
            "Hide the bottom background image of music control bar");
    }
}
