using System.Collections.Generic;
using Bulbul;
using BepInEx.Logging;
using ChillPatcherLite.Compatibility;
using ChillPatcherLite.Cover;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace ChillPatcherLite.Patches;

/// <summary>
/// UI 重排列补丁（从 ChillPatcher V1.3.4.1 移植）。
/// 将游戏主界面的图标/音乐控制条移动到接近音乐播放器的布局，
/// 并把播放列表按钮放到音乐控制条上方放大 2 倍显示方形封面。
/// </summary>
[HarmonyPatch]
public static class UIRearrangePatch
{
    private static ManualLogSource _logger;
    private static bool _hasRearranged;

    private static ManualLogSource Logger =>
        _logger ??= BepInEx.Logging.Logger.CreateLogSource("UIRearrange");

    private static bool _monitorCreated;

    /// <summary>TopIcons 按钮间距。</summary>
    public const float TopIconsSpacing = 6f;

    /// <summary>TopIcons 整体缩放比例。</summary>
    public const float TopIconsContainerScale = 0.7f;

    /// <summary>RightIcons 整体缩放比例。</summary>
    public const float RightIconsContainerScale = 0.7f;

    /// <summary>RightIcons 垂直偏移量（正值向下，负值向上）。</summary>
    public const float RightIconsVerticalOffset = 80f;

    /// <summary>TopIcons 垂直偏移量（正值向上），用于与右侧底部按钮拉开距离。</summary>
    public const float TopIconsVerticalOffset = 24f;

    /// <summary>UI_FacilityMusic 整体缩放比例。</summary>
    public const float FacilityMusicScale = 1.0f;

    /// <summary>UI_FacilityMusic 水平偏移（正值向右）。</summary>
    public const float FacilityMusicHorizontalOffset = -80f;

    /// <summary>UI_FacilityMusic 垂直偏移（正值向下）。</summary>
    public const float FacilityMusicVerticalOffset = 0f;

    /// <summary>IconMusicPlaylist_Button 缩放比例（相对于 UI_FacilityMusic）。</summary>
    public const float MusicPlaylistButtonScale = 2.0f;

    /// <summary>IconMusicPlaylist_Button 相对于 UI_FacilityMusic 的水平偏移。</summary>
    public const float MusicPlaylistHorizontalOffset = 40f;

    /// <summary>IconMusicPlaylist_Button 相对于 UI_FacilityMusic 的垂直偏移。</summary>
    public const float MusicPlaylistVerticalOffset = 100f;

    /// <summary>封面图像分辨率（像素），2 倍按钮下建议至少 176+。</summary>
    public const int AlbumArtResolution = 256;

    private static class ButtonPaths
    {
        // 注意：游戏中根节点名字是 "Paremt"（拼写错误但这就是实际名字）
        // 游戏 V1.3.4 后 Canvas 位于 Paremt/PCPlatform/Canvas
        public const string RootPath = "Paremt/PCPlatform/Canvas/UI/MostFrontArea";
        public const string BottomBackImage = "Paremt/PCPlatform/Canvas/UI/BottomBackImage";
        public const string UIFacilityMusic = RootPath + "/UI_FacilityMusic";

        public const string TopIcons = RootPath + "/TopIcons";
        public const string LeftIcons = RootPath + "/LeftIcons";
        public const string CenterIcons = RootPath + "/CenterIcons";
        public const string RightIcons = RootPath + "/RightIcons";

            public const string IconDecoration = "IconDecoration_Button";
            public const string IconEnviroment = "IconEnviroment_Button";
            public const string IconMusicPlaylist = "IconMusicPlaylist_Button";
    }

    /// <summary>Hook 到 FacilityMusic.Setup（UI 初始化完成后重排列）。</summary>
    [HarmonyPostfix]
    [HarmonyPatch(typeof(FacilityMusic), "Setup")]
    public static void FacilityMusic_Setup_Postfix()
    {
        if (!ModConfig.EnableUIRearrange.Value)
        {
            Logger.LogInfo("UI rearrange disabled");
            return;
        }

        UnityRunner.Instance.RunDelayed(0.5f, RearrangeUI);
    }

    private static void RearrangeUI()
    {
        if (_hasRearranged)
        {
            Logger.LogInfo("UI already rearranged, skip");
            return;
        }

        try
        {
            Logger.LogInfo("Starting UI rearrange...");

            var topIcons = GameObject.Find(ButtonPaths.TopIcons);
            var leftIcons = GameObject.Find(ButtonPaths.LeftIcons);
            var centerIcons = GameObject.Find(ButtonPaths.CenterIcons);
            var rightIcons = GameObject.Find(ButtonPaths.RightIcons);

            if (topIcons == null)
            {
                // 打印实际 UI 结构帮助适配新版游戏
                var mostFront = GameObject.Find("Paremt/PCPlatform/Canvas");
                if (mostFront == null)
                    mostFront = GameObject.Find("Paremt/PCPlatform/Canvas/UI/MostFrontArea");
                if (mostFront == null)
                    mostFront = GameObject.Find("Paremt/PCPlatform/Canvas/UI");
                if (mostFront == null)
                    mostFront = GameObject.Find("Paremt");

                if (mostFront != null)
                {
                    Logger.LogInfo("[UIRearrange] Found container: " + mostFront.name +
                                   ", path: " + mostFront.GetGameObjectPath());
                    for (var i = 0; i < mostFront.transform.childCount; i++)
                    {
                        var child = mostFront.transform.GetChild(i);
                        Logger.LogInfo("[UIRearrange]   [" + i + "] " + child.name +
                                       " (active=" + child.gameObject.activeSelf + ")");
                    }
                }

                Logger.LogError("Cannot find TopIcons container, UI structure changed");
                return;
            }

            // 2. LeftIcons 下所有启用子项移到 TopIcons
            MoveAllActiveChildrenToParent(leftIcons, topIcons);

            // 3. CenterIcons 的装饰/环境按钮移到 TopIcons
            MoveButtonToParent(centerIcons, ButtonPaths.IconDecoration, topIcons);
            MoveButtonToParent(centerIcons, ButtonPaths.IconEnviroment, topIcons);

            // 4. 把音乐控制条 UI_FacilityMusic 移到 LeftIcons 位置
            MoveFacilityMusicToLeftIcons(leftIcons);

            // 5. 调整 TopIcons 布局
            AdjustTopIconsLayout(topIcons);

            var topIconsRect = topIcons.GetComponent<RectTransform>();
            if (topIconsRect != null)
            {
                var topPos = topIconsRect.anchoredPosition;
                topPos.y += TopIconsVerticalOffset; // UI 坐标系 Y 向上为正
                topIconsRect.anchoredPosition = topPos;
            }

            // 6. 调整 RightIcons 缩放与偏移
            if (rightIcons != null)
            {
                rightIcons.transform.localScale =
                    new Vector3(RightIconsContainerScale, RightIconsContainerScale, 1f);

                var rightIconsRect = rightIcons.GetComponent<RectTransform>();
                if (rightIconsRect != null)
                {
                    var pos = rightIconsRect.anchoredPosition;
                    pos.y -= RightIconsVerticalOffset; // UI 坐标系 Y 向上为正
                    rightIconsRect.anchoredPosition = pos;
                }
            }

            // 7. 可选隐藏底部背景图
            if (ModConfig.HideBottomBackImage.Value)
            {
                var bottomBackImage = GameObject.Find(ButtonPaths.BottomBackImage);
                if (bottomBackImage != null)
                {
                    var images = bottomBackImage.GetComponentsInChildren<Image>(true);
                    foreach (var img in images)
                        img.enabled = false;

                    var canvasGroup = bottomBackImage.GetComponent<CanvasGroup>();
                    if (canvasGroup != null)
                    {
                        canvasGroup.alpha = 0f;
                        canvasGroup.blocksRaycasts = false;
                        canvasGroup.interactable = false;
                    }
                }
            }

            // 8. 把播放列表按钮移到音乐控制条上方并放大
            MoveMusicPlaylistButton(centerIcons);

            _hasRearranged = true;
            Logger.LogInfo("UI rearrange complete");
        }
        catch (System.Exception ex)
        {
            Logger.LogError("UI rearrange failed: " + ex.Message + "\n" + ex.StackTrace);
        }
    }

    private static void MoveFacilityMusicToLeftIcons(GameObject leftIcons)
    {
        if (leftIcons == null)
        {
            Logger.LogWarning("LeftIcons is null, cannot move UI_FacilityMusic");
            return;
        }

        var facilityMusic = GameObject.Find(ButtonPaths.UIFacilityMusic);
        if (facilityMusic == null)
        {
            Logger.LogWarning("Cannot find UI_FacilityMusic");
            return;
        }

        var leftIconsRect = leftIcons.GetComponent<RectTransform>();
        var facilityMusicRect = facilityMusic.GetComponent<RectTransform>();

        if (leftIconsRect == null || facilityMusicRect == null)
        {
            Logger.LogError("Cannot get RectTransform");
            return;
        }

        var leftIconsPosition = leftIconsRect.anchoredPosition;
        var leftIconsAnchorMin = leftIconsRect.anchorMin;
        var leftIconsAnchorMax = leftIconsRect.anchorMax;

        var leftIconsParent = leftIcons.transform.parent;
        facilityMusic.transform.SetParent(leftIconsParent, false);

        facilityMusicRect.anchorMin = new Vector2(0, leftIconsAnchorMin.y);
        facilityMusicRect.anchorMax = new Vector2(0, leftIconsAnchorMax.y);
        facilityMusicRect.pivot = new Vector2(0, 0.5f);

        facilityMusicRect.anchoredPosition = new Vector2(
            leftIconsPosition.x + FacilityMusicHorizontalOffset,
            leftIconsPosition.y - FacilityMusicVerticalOffset);

        facilityMusic.transform.localScale =
            new Vector3(FacilityMusicScale, FacilityMusicScale, 1f);

        // 原 LeftIcons 内容已移走，隐藏容器
        leftIcons.SetActive(false);
    }

    private static void MoveMusicPlaylistButton(GameObject centerIcons)
    {
        if (centerIcons == null)
        {
            Logger.LogWarning("CenterIcons is null, cannot move IconMusicPlaylist_Button");
            return;
        }

        var musicPlaylistButton = centerIcons.transform.Find(ButtonPaths.IconMusicPlaylist);
        if (musicPlaylistButton == null)
        {
            Logger.LogWarning("Cannot find IconMusicPlaylist_Button in CenterIcons");
            return;
        }

        var facilityMusic = GameObject.Find(ButtonPaths.UIFacilityMusic);
        if (facilityMusic == null)
        {
            Logger.LogWarning("Cannot find UI_FacilityMusic");
            return;
        }

        var buttonRect = musicPlaylistButton.GetComponent<RectTransform>();
        var originalSize = buttonRect != null ? buttonRect.sizeDelta : Vector2.zero;

        var facilityMusicRect = facilityMusic.GetComponent<RectTransform>();
        if (buttonRect == null || facilityMusicRect == null)
        {
            Logger.LogError("Cannot get RectTransform");
            return;
        }

        // 用包裹容器应用缩放，避免按钮自身的悬浮动画覆盖缩放
        var wrapperGo = new GameObject("MusicPlaylistButtonWrapper");
        var wrapperRect = wrapperGo.AddComponent<RectTransform>();
        wrapperRect.SetParent(facilityMusic.transform, false);

        wrapperRect.anchorMin = new Vector2(0f, 1f);
        wrapperRect.anchorMax = new Vector2(0f, 1f);
        wrapperRect.pivot = new Vector2(0.5f, 0.5f);
        wrapperRect.sizeDelta = originalSize * MusicPlaylistButtonScale;
        wrapperGo.transform.localScale =
            new Vector3(MusicPlaylistButtonScale, MusicPlaylistButtonScale, 1f);

        float buttonHalfWidth = (originalSize.x * MusicPlaylistButtonScale) / 2f;
        wrapperRect.anchoredPosition = new Vector2(
            MusicPlaylistHorizontalOffset + buttonHalfWidth,
            MusicPlaylistVerticalOffset);

        musicPlaylistButton.SetParent(wrapperGo.transform, false);

        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = Vector2.zero;
        buttonRect.sizeDelta = originalSize;
        musicPlaylistButton.localScale = Vector3.one;

        Logger.LogInfo("Moved IconMusicPlaylist_Button to UI_FacilityMusic (2x)");

        SetupSquareDefaultCover(musicPlaylistButton);
    }

    private static void SetupSquareDefaultCover(Transform buttonTransform)
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

            if (MusicUIAlbumArtPatch.HasAlbumArtSet)
            {
                Logger.LogInfo("Album art already set, skip default cover setup");
                return;
            }

            var deactiveImageTransform = buttonTransform.Find("IconDeactivemage");
            var activeImageTransform = buttonTransform.Find("IconActiveImage");

            if (deactiveImageTransform == null || activeImageTransform == null)
            {
                Logger.LogWarning("Cannot find button icon Image components");
                return;
            }

            var deactiveImage = deactiveImageTransform.GetComponent<Image>();
            var activeImage = activeImageTransform.GetComponent<Image>();

            if (deactiveImage == null || activeImage == null)
            {
                Logger.LogWarning("Cannot get Image components");
                return;
            }

            var defaultSprite = CoverService.Instance.GetDefaultMusicCover();
            if (defaultSprite != null)
            {
                deactiveImage.sprite = defaultSprite;
                activeImage.sprite = defaultSprite;
                Logger.LogInfo("Default cover set");
            }
            else
            {
                Logger.LogWarning("Cannot load default cover");
            }
        }
        catch (System.Exception ex)
        {
            Logger.LogError("Set square default cover failed: " + ex.Message);
        }
    }

    private static void MoveAllActiveChildrenToParent(GameObject sourceContainer, GameObject targetContainer)
    {
        if (sourceContainer == null || targetContainer == null)
        {
            Logger.LogWarning("Container is null, cannot move children");
            return;
        }

        var childrenToMove = new List<Transform>();
        for (var i = 0; i < sourceContainer.transform.childCount; i++)
        {
            var child = sourceContainer.transform.GetChild(i);
            if (child.gameObject.activeSelf)
                childrenToMove.Add(child);
        }

        foreach (var child in childrenToMove)
        {
            var rectTransform = child.GetComponent<RectTransform>();
            var originalSize = rectTransform != null ? rectTransform.sizeDelta : Vector2.zero;
            child.SetParent(targetContainer.transform, false);
            if (rectTransform != null)
                rectTransform.sizeDelta = originalSize;
        }

        Logger.LogInfo("Moved " + childrenToMove.Count + " children from " +
                       sourceContainer.name + " to " + targetContainer.name);
    }

    private static void MoveButtonToParent(GameObject sourceContainer, string buttonName, GameObject targetContainer)
    {
        if (sourceContainer == null || targetContainer == null)
            return;

        var button = sourceContainer.transform.Find(buttonName);
        if (button == null)
        {
            Logger.LogWarning("Cannot find button: " + buttonName);
            return;
        }

        var rectTransform = button.GetComponent<RectTransform>();
        var originalSize = rectTransform != null ? rectTransform.sizeDelta : Vector2.zero;

        button.SetParent(targetContainer.transform, false);

        if (rectTransform != null)
            rectTransform.sizeDelta = originalSize;
    }

    private static void AdjustTopIconsLayout(GameObject topIcons)
    {
        var rectTransform = topIcons.GetComponent<RectTransform>();
        if (rectTransform == null)
            return;

        var layoutGroup = topIcons.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.spacing = TopIconsSpacing;
        }
        else
        {
            layoutGroup = topIcons.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.spacing = TopIconsSpacing;

            var sizeFitter = topIcons.GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
                sizeFitter = topIcons.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        topIcons.transform.localScale =
            new Vector3(TopIconsContainerScale, TopIconsContainerScale, 1f);

        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    /// <summary>
    /// 轮询入口：不依赖 FacilityMusic.Setup 回调。
    /// UI 容器出现后自动执行重排列，场景切换后容器消失会自动重置。
    /// </summary>
    public static void PollRearrange()
    {
        if (!ModConfig.EnableUIRearrange.Value)
            return;

        var topIcons = GameObject.Find(ButtonPaths.TopIcons);

        if (_hasRearranged)
        {
            // 场景切换后旧容器被销毁，允许下一次对新 UI 重新排列
            if (topIcons == null)
                _hasRearranged = false;
            return;
        }

        var facilityMusic = GameObject.Find(ButtonPaths.UIFacilityMusic);
        if (topIcons == null || facilityMusic == null)
            return;

        Logger.LogInfo("[Poll] UI containers ready, applying rearrange");
        RearrangeUI();
    }

    public static void EnsureMonitor()
    {
        if (_monitorCreated)
            return;
        _monitorCreated = true;

        var go = new GameObject("ChillPatcherLite_UIRearrangeMonitor");
        go.hideFlags = HideFlags.HideAndDontSave;
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.AddComponent<UIRearrangeMonitor>();
    }
}

/// <summary>
/// 低频轮询（0.5s），直到 UI 容器出现；场景切换后自动重新等待。
/// </summary>
public sealed class UIRearrangeMonitor : MonoBehaviour
{
    private float _nextCheck;

    private void Update()
    {
        if (Time.realtimeSinceStartup < _nextCheck)
            return;
        _nextCheck = Time.realtimeSinceStartup + 0.5f;

        try
        {
            UIRearrangePatch.PollRearrange();
            MusicUIAlbumArtPatch.PollInitialize();
            ChillMusicSyncCoop.TryEnable();
        }
        catch (System.Exception ex)
        {
            Plugin.Log?.LogWarning("[UIRearrangeMonitor] tick failed: " + ex.Message);
        }
    }
}

internal static class GameObjectExtensions
{
    public static string GetGameObjectPath(this GameObject obj)
    {
        string path = obj.name;
        var parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }
}
