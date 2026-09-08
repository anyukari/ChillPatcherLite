using System;
using System.Collections;
using UnityEngine;

namespace ChillPatcherLite;

/// <summary>
/// 轻量协程调度器，用于在 Unity 主线程上延迟执行 UI 操作。
/// </summary>
public sealed class UnityRunner : MonoBehaviour
{
    private static UnityRunner _instance;

    public static UnityRunner Instance
    {
        get
        {
            EnsureInstance();
            return _instance;
        }
    }

    public static void EnsureInstance()
    {
        if (_instance != null)
            return;

        var go = new GameObject("ChillPatcherLite_Runner");
        go.hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<UnityRunner>();
    }

    public void RunDelayed(float seconds, Action action)
    {
        StartCoroutine(Delayed(seconds, action));
    }

    public void RunNextFrame(Action action)
    {
        StartCoroutine(NextFrame(action));
    }

    private static IEnumerator Delayed(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }

    private static IEnumerator NextFrame(Action action)
    {
        yield return null;
        action?.Invoke();
    }
}
