using System;
using System.Reflection;
using UnityEngine;

namespace ChillPatcherLite.Compatibility;

/// <summary>
/// 让 ChillMusicInformationSync 进入“ChillPatcher 共存模式”。
///
/// 该同步 mod 通过判断原版类型是否存在来决定封面注入方式：
/// - 共存模式：直接把封面写入 IconMusicPlaylist_Button 的两个 Image；
/// - 非共存模式：在 CenterIcons 另建一张独立 Mod_SongCover 图片。
///
/// 我们不想冒用原 ChillPatcher 的程序集名/类型名，
/// 因此改为在启动后把它的私有缓存字段 _chillPatcherDetected 置为 true。
/// </summary>
internal static class ChillMusicSyncCoop
{
    private const string SyncManagerType =
        "ChillMusicInformationSync.UISync.MusicCoverMananger, ChillMusicInformationSync";

    private static bool _done;
    private static bool _warned;
    private static float _startTime;

    /// <summary>应由 Unity 主线程每帧/每 tick 调用，直到成功或超时。</summary>
    public static void TryEnable()
    {
        if (_done)
            return;

        if (_startTime <= 0f)
            _startTime = Time.realtimeSinceStartup;

        if (Time.realtimeSinceStartup - _startTime > 30f)
        {
            Plugin.Log?.LogWarning("[Compatibility] ChillMusicInformationSync cover manager not found after 30s, " +
                                   "skip cooperation mode.");
            _done = true;
            return;
        }

        try
        {
            var type = Type.GetType(SyncManagerType, false);
            if (type == null)
                return;

            var field = type.GetField(
                "_chillPatcherDetected",
                BindingFlags.NonPublic | BindingFlags.Static);

            if (field == null)
            {
                if (!_warned)
                {
                    _warned = true;
                    Plugin.Log?.LogWarning("[Compatibility] Sync mod's _chillPatcherDetected field not found; " +
                                           "its version may have changed.");
                }
                _done = true;
                return;
            }

            field.SetValue(null, true);
            _done = true;
            Plugin.Log?.LogInfo("[Compatibility] ChillMusicInformationSync cooperation mode enabled " +
                                "(cover will be injected into the big button).");
        }
        catch (Exception ex)
        {
            if (!_warned)
            {
                _warned = true;
                Plugin.Log?.LogWarning("[Compatibility] Failed to enable ChillMusicInformationSync cooperation: " +
                                       ex.Message);
            }
            _done = true;
        }
    }
}
