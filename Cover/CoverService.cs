using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChillPatcherLite.Cover;

/// <summary>
/// 轻量封面服务。
/// 本移植版不包含 ChillPatcher 的网络音乐模块系统：
/// - 本地 PC 歌曲由 MusicUI 补丁直接读取同目录 cover/folder/front 等图片；
/// - 带 UUID 的外来/在线歌曲没有模块提供封面时，会回退到默认封面。
/// 默认封面与本地封面均内嵌在本 DLL。
/// </summary>
public class CoverService
{
    private static CoverService _instance;

    public static CoverService Instance => _instance ??= new CoverService();

    private readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

    /// <summary>歌曲封面加载完成事件，参数：(uuid, cover)。</summary>
    public event Action<string, Sprite> OnMusicCoverLoaded;

    public Sprite LoadingPlaceholder => EmbeddedResources.LoadingPlaceholder;

    public Sprite GetDefaultMusicCover()
    {
        return DefaultCoverProvider.DefaultMusicCover ?? EmbeddedResources.DefaultPlaceholder;
    }

    public Sprite GetLocalMusicCover()
    {
        return DefaultCoverProvider.LocalMusicCover ?? GetDefaultMusicCover();
    }

    /// <summary>游戏原生分类封面（Original/Special/Other）。</summary>
    public Sprite GetGameCover(int audioTag)
    {
        return DefaultCoverProvider.GetGameCover(audioTag) ?? GetDefaultMusicCover();
    }

    public bool IsStaticCover(Sprite sprite)
    {
        return DefaultCoverProvider.IsStaticCover(sprite);
    }

    /// <summary>
    /// 返回占位图，并在下一帧用默认封面触发 OnMusicCoverLoaded。
    /// 保留原版“先占位、后回退默认封面”的表现。
    /// </summary>
    public Sprite GetMusicCoverOrPlaceholder(string uuid)
    {
        if (string.IsNullOrEmpty(uuid))
            return GetDefaultMusicCover();

        if (_spriteCache.TryGetValue(uuid, out var cached) && cached != null)
            return cached;

        UnityRunner.Instance.RunNextFrame(() =>
        {
            var fallback = GetDefaultMusicCover();
            _spriteCache[uuid] = fallback;
            OnMusicCoverLoaded?.Invoke(uuid, fallback);
        });

        return LoadingPlaceholder;
    }
}
