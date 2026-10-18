using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Crocodilo/Gusini Phase Two Assets")]
public sealed class GusiniPhaseTwoAssets : ScriptableObject
{
    [Serializable] public struct Entry { public string key; public Sprite sprite; }
    public Entry[] sprites;
    private Dictionary<string, Sprite> cache;
    public Sprite Get(string key)
    {
        if (cache == null)
        {
            cache = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            foreach (var entry in sprites) cache[entry.key] = entry.sprite;
        }
        return cache.TryGetValue(key, out var value) ? value : null;
    }

    public Sprite[] Frames(string prefix, int count)
    {
        var frames = new Sprite[count];
        for (int i = 0; i < count; i++) frames[i] = Get(prefix + i);
        return frames;
    }
}
