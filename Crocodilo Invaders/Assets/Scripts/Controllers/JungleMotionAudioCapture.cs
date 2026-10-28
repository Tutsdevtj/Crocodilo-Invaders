#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Temporary listener filter, instantiated only by the motion validation. Captures game output, not OS audio.
public sealed class JungleMotionAudioCapture : MonoBehaviour
{
    private readonly List<float> samples = new List<float>();
    private readonly object gate = new object();
    private volatile bool recording;
    private int sampleRate, channelCount = 2;
    public void Begin() { sampleRate = AudioSettings.outputSampleRate; lock (gate) samples.Clear(); recording = true; }
    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (!recording) return;
        lock (gate) { channelCount = channels; if (samples.Count < sampleRate * 60 * channels) samples.AddRange(data); }
    }
    public void Save(string path)
    {
        recording = false; float[] data; lock (gate) data = samples.ToArray();
        using (var writer = new BinaryWriter(File.Create(path)))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + data.Length * 2);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16); writer.Write((short)1); writer.Write((short)channelCount);
            writer.Write(sampleRate); writer.Write(sampleRate * channelCount * 2); writer.Write((short)(channelCount * 2)); writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(data.Length * 2);
            foreach (float value in data) writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(value, -1, 1) * 32767));
        }
    }
}
#endif
