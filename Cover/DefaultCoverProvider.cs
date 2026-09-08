using System;
using UnityEngine;

namespace ChillPatcherLite.Cover;

/// <summary>
/// 默认/本地封面加载器（资源嵌入在 DLL 中）。
/// </summary>
public static class DefaultCoverProvider
{
    private static bool _initialized;

    public static Sprite DefaultMusicCover { get; private set; }

    public static Sprite LocalMusicCover { get; private set; }

    private static Sprite _gameCoverOriginal;
    private static Sprite _gameCoverSpecial;
    private static Sprite _gameCoverOther;

    public static void Initialize()
    {
        if (_initialized)
            return;
        _initialized = true;

        DefaultMusicCover = LoadEmbeddedSprite(
            "ChillPatcherLite.Resources.covers.defaultcover.png", "default music cover");
        LocalMusicCover = LoadEmbeddedSprite(
            "ChillPatcherLite.Resources.covers.localcover.jpg", "local music cover");
        _gameCoverOriginal = LoadEmbeddedSprite(
            "ChillPatcherLite.Resources.covers.gamecover1.jpg", "game cover 1");
        _gameCoverSpecial = LoadEmbeddedSprite(
            "ChillPatcherLite.Resources.covers.gamecover2.jpg", "game cover 2");
        _gameCoverOther = LoadEmbeddedSprite(
            "ChillPatcherLite.Resources.covers.gamecover3.png", "game cover 3");

        Plugin.Log?.LogInfo("Default covers loaded");
    }

    /// <summary>游戏原生曲目的分类封面：Original=1, Special=2, Other=4。</summary>
    public static Sprite GetGameCover(int audioTag)
    {
        return audioTag switch
        {
            1 => _gameCoverOriginal,
            2 => _gameCoverSpecial,
            4 => _gameCoverOther,
            _ => DefaultMusicCover
        };
    }

    /// <summary>判断 Sprite 是否为本插件缓存的静态封面（不应销毁）。</summary>
    public static bool IsStaticCover(Sprite sprite)
    {
        return sprite != null &&
               (sprite == DefaultMusicCover ||
                sprite == LocalMusicCover ||
                sprite == _gameCoverOriginal ||
                sprite == _gameCoverSpecial ||
                sprite == _gameCoverOther);
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

                var texture = new Texture2D(2, 2);
                if (texture.LoadImage(bytes))
                {
                    return Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f));
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError("Failed to load embedded " + displayName + ": " + ex.Message);
        }

        return null;
    }
}
