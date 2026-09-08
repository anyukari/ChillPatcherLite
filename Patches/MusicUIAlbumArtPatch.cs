using System;
using System.IO;
using System.Reflection;
using Bulbul;
using ChillPatcherLite.Cover;
using ChillPatcherLite.UIFramework;
using HarmonyLib;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace ChillPatcherLite.Patches;

/// <summary>
/// 音乐封面显示补丁（从 ChillPatcher V1.3.4.1 移植）。
/// 把播放列表切换按钮的图标替换为当前播放音乐的封面，按钮功能保持不变。
/// 启用 UI 重排列时使用方形大封面，否则沿用圆形小封面。
/// </summary>
[HarmonyPatch(typeof(MusicUI))]
public static class MusicUIAlbumArtPatch
{
    private static readonly FieldInfo FacilityOpenButtonField =
        typeof(MusicUI).GetField("_facilityOpenButton", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly FieldInfo FacilityMusicField =
        typeof(MusicUI).GetField("_facilityMusic", BindingFlags.NonPublic | BindingFlags.Instance);

    private static Sprite _originalDeactiveIcon;
    private static Sprite _originalActiveIcon;
    private static Sprite _currentAlbumArtSprite;
    private static string _currentAudioPath;
    private static Image _iconDeactiveImage;
    private static Image _iconActiveImage;
    private static Component _facilityOpenButton;
    private static MusicUI _initializedMusicUi;
    private static MusicUI _lastPolledMusicUi;
    private static bool _warnedForCurrentInstance;

    private static IDisposable _musicPlaySubscription;
    private static string _pendingMusicUuid;

    /// <summary>是否已经设置了封面（UI 重排列补丁据此判断是否需要放默认封面）。</summary>
    public static bool HasAlbumArtSet => _currentAlbumArtSprite != null;

    static MusicUIAlbumArtPatch()
    {
        if (FacilityOpenButtonField == null)
            Plugin.Log?.LogError("[MusicUIAlbumArtPatch] Failed to find _facilityOpenButton field");

        if (FacilityMusicField == null)
            Plugin.Log?.LogError("[MusicUIAlbumArtPatch] Failed to find _facilityMusic field");
    }

    [HarmonyPostfix]
    [HarmonyPatch("Bulbul.IMusicListUI.Setup")]
    public static void Setup_Postfix(MusicUI __instance)
    {
        if (!ModConfig.EnableAlbumArtDisplay.Value)
        {
            Plugin.Log?.LogDebug("[MusicUIAlbumArtPatch] Album art display is disabled");
            return;
        }

        PollInitialize();
    }

    /// <summary>
    /// 轮询入口：不依赖 MusicUI.Setup 回调，找到已初始化的 MusicUI 后
    /// 自动挂载封面显示。
    /// </summary>
    public static void PollInitialize()
    {
        if (!ModConfig.EnableAlbumArtDisplay.Value)
            return;

        var musicUi = UnityEngine.Object.FindObjectOfType<MusicUI>();
        if (musicUi == null)
            return;

        if (_initializedMusicUi != null && _initializedMusicUi == musicUi)
            return;

        if (_lastPolledMusicUi != musicUi)
        {
            _lastPolledMusicUi = musicUi;
            _warnedForCurrentInstance = false;
        }

        try
        {
            _facilityOpenButton = FacilityOpenButtonField?.GetValue(musicUi) as Component;
            if (_facilityOpenButton == null)
            {
                WarnOnce("[MusicUIAlbumArtPatch] Failed to get _facilityOpenButton");
                return;
            }

            var facilityMusic = FacilityMusicField?.GetValue(musicUi) as FacilityMusic;
            if (facilityMusic == null)
            {
                WarnOnce("[MusicUIAlbumArtPatch] Failed to get _facilityMusic");
                return;
            }

            var buttonTransform = _facilityOpenButton.transform;
            var deactiveImageTransform = buttonTransform.Find("IconDeactivemage");
            var activeImageTransform = buttonTransform.Find("IconActiveImage");

            if (deactiveImageTransform == null || activeImageTransform == null)
            {
                WarnOnce("[MusicUIAlbumArtPatch] Failed to find icon images");
                return;
            }

            _iconDeactiveImage = deactiveImageTransform.GetComponent<Image>();
            _iconActiveImage = activeImageTransform.GetComponent<Image>();

            if (_iconDeactiveImage == null || _iconActiveImage == null)
            {
                WarnOnce("[MusicUIAlbumArtPatch] Failed to get Image components");
                return;
            }

            if (_originalDeactiveIcon == null)
                _originalDeactiveIcon = _iconDeactiveImage.sprite;
            if (_originalActiveIcon == null)
                _originalActiveIcon = _iconActiveImage.sprite;

            // UI 重排列开启时使用方形模式（去掉圆形遮罩）
            if (ModConfig.EnableUIRearrange.Value)
                SetupSquareMode(buttonTransform);

            _musicPlaySubscription?.Dispose();
            _musicPlaySubscription = facilityMusic.MusicService.OnPlayMusic.Subscribe(UpdateAlbumArt);

            CoverService.Instance.OnMusicCoverLoaded -= OnMusicCoverLoaded;
            CoverService.Instance.OnMusicCoverLoaded += OnMusicCoverLoaded;

            if (facilityMusic.PlayingMusic != null)
                UpdateAlbumArt(facilityMusic.PlayingMusic);

            _initializedMusicUi = musicUi;
            Plugin.Log?.LogInfo("[MusicUIAlbumArtPatch] Album art display initialized");
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError("[MusicUIAlbumArtPatch] Error in Setup_Postfix: " + ex.Message +
                                 "\n" + ex.StackTrace);
        }
    }

    private static void WarnOnce(string message)
    {
        if (_warnedForCurrentInstance)
            return;
        _warnedForCurrentInstance = true;
        Plugin.Log?.LogWarning(message);
    }

    private static void OnMusicCoverLoaded(string uuid, Sprite cover)
    {
        if (uuid != _pendingMusicUuid || cover == null)
            return;

        bool useSquareMode = ModConfig.EnableUIRearrange.Value;

        if (!useSquareMode && cover.texture != null)
        {
            var circularSprite = AlbumArtReader.CreateCircularSprite(cover.texture, 88);
            if (circularSprite != null)
                cover = circularSprite;
        }

        ApplyAlbumArt(cover, uuid, "");
        _pendingMusicUuid = null;
    }

    private static void SetupSquareMode(Transform buttonTransform)
    {
        try
        {
            var maskObj = buttonTransform.Find("Mask");
            if (maskObj != null)
            {
                var mask = maskObj.GetComponent<Mask>();
                if (mask != null)
                    mask.enabled = false;

                var maskImage = maskObj.GetComponent<Image>();
                if (maskImage != null)
                    maskImage.enabled = false;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogWarning("[MusicUIAlbumArtPatch] Error setting up square mode: " + ex.Message);
        }
    }

    private static void UpdateAlbumArt(GameAudioInfo audioInfo)
    {
        if (!ModConfig.EnableAlbumArtDisplay.Value)
            return;

        if (_iconDeactiveImage == null || _iconActiveImage == null)
            return;

        bool useSquareMode = ModConfig.EnableUIRearrange.Value;

        // 游戏原生曲目（非导入/非自定义模块，且 PathType=Normal）使用分类封面
        if (IsNativeGameTrack(audioInfo) &&
            TryApplyGameCover(audioInfo, useSquareMode))
        {
            return;
        }

        // 带 UUID 的歌曲（在线模块/导入歌曲）走 CoverService 回退链路
        if (!string.IsNullOrEmpty(audioInfo.UUID))
        {
            _pendingMusicUuid = audioInfo.UUID;

            var sprite = CoverService.Instance.GetMusicCoverOrPlaceholder(audioInfo.UUID);

            if (!useSquareMode &&
                sprite != CoverService.Instance.LoadingPlaceholder &&
                sprite != null &&
                sprite.texture != null)
            {
                var circularSprite = AlbumArtReader.CreateCircularSprite(sprite.texture, 88);
                if (circularSprite != null)
                    sprite = circularSprite;
            }

            _iconDeactiveImage.sprite = sprite;
            _iconActiveImage.sprite = sprite;
            _currentAlbumArtSprite = sprite;
            _currentAudioPath = audioInfo.UUID;
            return;
        }

        // 本地 PC 文件：优先读取同目录 cover/folder/front/album/artwork 图片
        if (audioInfo.PathType == AudioMode.LocalPc && !string.IsNullOrEmpty(audioInfo.LocalPath))
        {
            _pendingMusicUuid = null;
            var albumArtTexture = TryLoadAlbumCover(audioInfo.LocalPath);

            if (albumArtTexture != null)
            {
                int resolution = useSquareMode ? UIRearrangePatch.AlbumArtResolution : 88;
                Sprite albumArtSprite = useSquareMode
                    ? AlbumArtReader.CreateSquareSprite(albumArtTexture, resolution)
                    : AlbumArtReader.CreateCircularSprite(albumArtTexture, resolution);

                if (albumArtSprite != null)
                {
                    ApplyAlbumArt(albumArtSprite, audioInfo.LocalPath, audioInfo.Title);
                    return;
                }

                UnityEngine.Object.Destroy(albumArtTexture);
            }

            TryUseDefaultCover(useSquareMode, true);
            return;
        }

        // 其余情况使用默认封面
        _pendingMusicUuid = null;
        TryUseDefaultCover(useSquareMode, false);
    }

    /// <summary>是否为游戏自带曲目（Tag 只落在原生位，不含自定义模块高位）。</summary>
    private static bool IsNativeGameTrack(GameAudioInfo audioInfo)
    {
        if (audioInfo.PathType != AudioMode.Normal)
            return false;
        if (!string.IsNullOrEmpty(audioInfo.LocalPath))
            return false;

        long raw = (long)audioInfo.Tag;

        // 自定义模块标签使用位 5+；原生歌曲只使用位 0-4
        if ((raw & ~31L) != 0)
            return false;

        return (raw & 7L) != 0; // Original=1 | Special=2 | Other=4
    }

    private static bool TryApplyGameCover(GameAudioInfo audioInfo, bool useSquareMode)
    {
        int tagIndex = GetGameCoverIndex((long)audioInfo.Tag);
        if (tagIndex == 0)
            return false;

        var baseCover = CoverService.Instance.GetGameCover(tagIndex);
        if (baseCover == null || baseCover.texture == null)
            return false;

        int resolution = useSquareMode ? UIRearrangePatch.AlbumArtResolution : 88;
        Sprite cover = useSquareMode
            ? AlbumArtReader.CreateSquareSprite(baseCover.texture, resolution)
            : AlbumArtReader.CreateCircularSprite(baseCover.texture, resolution);

        if (cover == null)
            return false;

        _pendingMusicUuid = null;
        ApplyAlbumArt(cover, "game:" + tagIndex, audioInfo.Title);
        return true;
    }

    private static int GetGameCoverIndex(long rawTag)
    {
        if ((rawTag & 1L) != 0) return 1; // Original
        if ((rawTag & 2L) != 0) return 2; // Special
        if ((rawTag & 4L) != 0) return 4; // Other
        return 0;
    }

    private static void ApplyAlbumArt(Sprite sprite, string cacheKey, string title)
    {
        var loadingPlaceholder = CoverService.Instance.LoadingPlaceholder;

        if (_currentAlbumArtSprite != null &&
            !CoverService.Instance.IsStaticCover(_currentAlbumArtSprite) &&
            _currentAlbumArtSprite != loadingPlaceholder)
        {
            UnityEngine.Object.Destroy(_currentAlbumArtSprite.texture);
            UnityEngine.Object.Destroy(_currentAlbumArtSprite);
        }

        _currentAlbumArtSprite = sprite;
        _currentAudioPath = cacheKey;
        _iconDeactiveImage.sprite = sprite;
        _iconActiveImage.sprite = sprite;

        if (!string.IsNullOrEmpty(title))
            Plugin.Log?.LogInfo("[MusicUIAlbumArtPatch] Updated album art for: " + title);
    }

    private static Texture2D TryLoadAlbumCover(string audioFilePath)
    {
        try
        {
            var directory = Path.GetDirectoryName(audioFilePath);
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return null;

            string[] coverNames = { "cover", "folder", "front", "album", "artwork" };
            string[] extensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

            foreach (var name in coverNames)
            {
                foreach (var ext in extensions)
                {
                    var coverPath = Path.Combine(directory, name + ext);
                    if (!File.Exists(coverPath))
                        continue;

                    var bytes = File.ReadAllBytes(coverPath);
                    var texture = new Texture2D(2, 2);
                    if (ImageConversion.LoadImage(texture, bytes))
                    {
                        Plugin.Log?.LogInfo("[MusicUIAlbumArtPatch] Loaded album cover from: " + coverPath);
                        return texture;
                    }

                    UnityEngine.Object.Destroy(texture);
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogWarning("[MusicUIAlbumArtPatch] Error loading album cover: " + ex.Message);
        }

        return null;
    }

    private static void TryUseDefaultCover(bool useSquareMode, bool isLocalImport)
    {
        try
        {
            Sprite sprite = isLocalImport
                ? CoverService.Instance.GetLocalMusicCover()
                : CoverService.Instance.GetDefaultMusicCover();

            if (sprite == null)
            {
                RestoreOriginalIcon();
                return;
            }

            if (_currentAlbumArtSprite != null &&
                !CoverService.Instance.IsStaticCover(_currentAlbumArtSprite) &&
                _currentAlbumArtSprite != CoverService.Instance.LoadingPlaceholder)
            {
                UnityEngine.Object.Destroy(_currentAlbumArtSprite.texture);
                UnityEngine.Object.Destroy(_currentAlbumArtSprite);
            }

            _currentAlbumArtSprite = sprite;
            _currentAudioPath = null;
            _iconDeactiveImage.sprite = sprite;
            _iconActiveImage.sprite = sprite;
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogWarning("[MusicUIAlbumArtPatch] Error using default cover: " + ex.Message);
            RestoreOriginalIcon();
        }
    }

    private static void RestoreOriginalIcon()
    {
        if (_iconDeactiveImage != null && _originalDeactiveIcon != null)
            _iconDeactiveImage.sprite = _originalDeactiveIcon;

        if (_iconActiveImage != null && _originalActiveIcon != null)
            _iconActiveImage.sprite = _originalActiveIcon;

        if (_currentAlbumArtSprite != null &&
            !CoverService.Instance.IsStaticCover(_currentAlbumArtSprite) &&
            _currentAlbumArtSprite != CoverService.Instance.LoadingPlaceholder)
        {
            UnityEngine.Object.Destroy(_currentAlbumArtSprite.texture);
            UnityEngine.Object.Destroy(_currentAlbumArtSprite);
        }

        _currentAlbumArtSprite = null;
        _currentAudioPath = null;
    }
}
