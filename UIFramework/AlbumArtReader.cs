using System;
using UnityEngine;

namespace ChillPatcherLite.UIFramework;

/// <summary>
/// 封面图像处理工具类：圆形/方形 Sprite 创建（从 ChillPatcher V1.3.4.1 移植）。
/// </summary>
public static class AlbumArtReader
{
    public static Sprite CreateCircularSprite(Texture2D source, int resolution = 88)
    {
        if (source == null)
            return null;

        try
        {
            var circularTexture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);

            float radius = resolution / 2f;
            var center = new Vector2(radius, radius);

            var scaledColors = GetScaledPixels(source, resolution, resolution);

            for (var y = 0; y < resolution; y++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var pos = new Vector2(x + 0.5f, y + 0.5f);
                    float distance = Vector2.Distance(pos, center);

                    Color color = scaledColors[y * resolution + x];

                    if (distance > radius - 1)
                    {
                        float alpha = Mathf.Clamp01(radius - distance);
                        color.a *= alpha;
                    }

                    if (distance > radius)
                    {
                        color = Color.clear;
                    }

                    circularTexture.SetPixel(x, y, color);
                }
            }

            circularTexture.Apply();

            return Sprite.Create(
                circularTexture,
                new Rect(0, 0, resolution, resolution),
                new Vector2(0.5f, 0.5f),
                100f);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError("Create circular cover failed: " + ex.Message);
            return null;
        }
    }

    public static Sprite CreateSquareSprite(Texture2D source, int resolution = 256)
    {
        if (source == null)
            return null;

        try
        {
            if (source.width == resolution && source.height == resolution)
            {
                return Sprite.Create(
                    source,
                    new Rect(0, 0, resolution, resolution),
                    new Vector2(0.5f, 0.5f),
                    100f);
            }

            var scaledTexture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            var scaledColors = GetScaledPixels(source, resolution, resolution);
            scaledTexture.SetPixels(scaledColors);
            scaledTexture.Apply();

            return Sprite.Create(
                scaledTexture,
                new Rect(0, 0, resolution, resolution),
                new Vector2(0.5f, 0.5f),
                100f);
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError("Create square cover failed: " + ex.Message);
            return null;
        }
    }

    private static Color[] GetScaledPixels(Texture2D source, int targetWidth, int targetHeight)
    {
        var result = new Color[targetWidth * targetHeight];

        float xRatio = (float)source.width / targetWidth;
        float yRatio = (float)source.height / targetHeight;

        for (var y = 0; y < targetHeight; y++)
        {
            for (var x = 0; x < targetWidth; x++)
            {
                int sourceX = Mathf.FloorToInt(x * xRatio);
                int sourceY = Mathf.FloorToInt(y * yRatio);

                sourceX = Mathf.Clamp(sourceX, 0, source.width - 1);
                sourceY = Mathf.Clamp(sourceY, 0, source.height - 1);

                result[y * targetWidth + x] = source.GetPixel(sourceX, sourceY);
            }
        }

        return result;
    }
}
