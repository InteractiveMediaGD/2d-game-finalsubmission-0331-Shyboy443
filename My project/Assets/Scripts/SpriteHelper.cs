using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates basic sprites at runtime for game objects.
/// </summary>
public static class SpriteHelper
{
    private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

    public static Sprite Square
    {
        get
        {
            if (!Cache.ContainsKey("square"))
            {
                Texture2D tex = new Texture2D(4, 4);
                Color[] pixels = new Color[16];
                for (int i = 0; i < 16; i++) pixels[i] = Color.white;
                tex.SetPixels(pixels);
                tex.filterMode = FilterMode.Point;
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.Apply();
                Cache["square"] = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
            }
            return Cache["square"];
        }
    }

    public static Sprite Circle
    {
        get
        {
            if (!Cache.ContainsKey("circle"))
            {
                int size = 64;
                Texture2D tex = new Texture2D(size, size);
                float center = size * 0.5f;
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(center, center));
                        tex.SetPixel(x, y, dist < center ? Color.white : Color.clear);
                    }
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.Apply();
                Cache["circle"] = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            }
            return Cache["circle"];
        }
    }

    public static Sprite Diamond
    {
        get
        {
            if (!Cache.ContainsKey("diamond"))
            {
                int size = 64;
                Texture2D tex = new Texture2D(size, size);
                float center = size * 0.5f;
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float dx = Mathf.Abs(x + 0.5f - center) / center;
                        float dy = Mathf.Abs(y + 0.5f - center) / center;
                        tex.SetPixel(x, y, (dx + dy) < 1f ? Color.white : Color.clear);
                    }
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.Apply();
                Cache["diamond"] = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            }
            return Cache["diamond"];
        }
    }

    public static Sprite RoundedRect
    {
        get
        {
            if (!Cache.ContainsKey("roundedRect"))
            {
                const int size = 128;
                const float radius = 24f;
                const float edgeSoftness = 1.6f;

                Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;

                Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
                Vector2 halfSize = new Vector2(size * 0.5f - radius, size * 0.5f - radius);

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        Vector2 point = new Vector2(Mathf.Abs(x + 0.5f - center.x), Mathf.Abs(y + 0.5f - center.y));
                        Vector2 delta = new Vector2(
                            Mathf.Max(point.x - halfSize.x, 0f),
                            Mathf.Max(point.y - halfSize.y, 0f));

                        float signedDistance = delta.magnitude - radius;
                        float alpha = signedDistance <= 0f
                            ? 1f
                            : Mathf.Clamp01(1f - signedDistance / edgeSoftness);

                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                }

                tex.Apply();
                Cache["roundedRect"] = Sprite.Create(
                    tex,
                    new Rect(0f, 0f, size, size),
                    new Vector2(0.5f, 0.5f),
                    size,
                    0,
                    SpriteMeshType.FullRect,
                    new Vector4(radius, radius, radius, radius));
            }

            return Cache["roundedRect"];
        }
    }
}
