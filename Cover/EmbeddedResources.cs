using System;
using UnityEngine;

namespace ChillPatcherLite.Cover;

/// <summary>
/// 从嵌入资源加载封面占位图，并带纯色兜底。
/// </summary>
public static class EmbeddedResources
{
    private static Sprite _loadingPlaceholder;
    private static Sprite _defaultPlaceholder;
    private static bool _loadingLoadAttempted;
    private static bool _defaultLoadAttempted;

    public static Sprite LoadingPlaceholder
    {
        get
        {
            if (_loadingPlaceholder == null && !_loadingLoadAttempted)
            {
                _loadingLoadAttempted = true;
                _loadingPlaceholder = LoadEmbeddedSprite(
                    "ChillPatcherLite.Resources.covers.loading.png", "loading");
            }

            if (_loadingPlaceholder == null)
            {
                _loadingPlaceholder = CreateColoredSprite(new Color(0.30f, 0.30f, 0.30f, 1f));
            }

            return _loadingPlaceholder;
        }
    }

    public static Sprite DefaultPlaceholder
    {
        get
        {
            if (_defaultPlaceholder == null && !_defaultLoadAttempted)
            {
                _defaultLoadAttempted = true;
                _defaultPlaceholder = LoadEmbeddedSprite(
                    "ChillPatcherLite.Resources.covers.defaultcover.png", "default cover");
            }

            if (_defaultPlaceholder == null)
            {
                _defaultPlaceholder = CreateColoredSprite(new Color(0.85f, 0.85f, 0.85f, 1f));
            }

            return _defaultPlaceholder;
        }
    }

    private static Sprite LoadEmbeddedSprite(string resourceName, string displayName)
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    Plugin.Log?.LogWarning("Embedded resource not found: " + resourceName);
                    return null;
                }

                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (texture.LoadImage(bytes))
                {
                    var sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f));
                    Plugin.Log?.LogInfo("Loaded embedded " + displayName);
                    return sprite;
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogWarning("Failed to load embedded " + displayName + ": " + ex.Message);
        }

        return null;
    }

    private static Sprite CreateColoredSprite(Color color)
    {
        var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        var pixels = new Color[16];
        for (var i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, 4, 4),
            new Vector2(0.5f, 0.5f));
    }
}
