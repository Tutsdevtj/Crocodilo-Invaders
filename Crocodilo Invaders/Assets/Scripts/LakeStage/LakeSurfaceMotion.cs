using UnityEngine;

// Quiet travelling crests stay on the water, below the combat corridor.
public sealed class LakeSurfaceMotion : MonoBehaviour
{
    private LakeBackdrop lake;
    private LineRenderer[] crests;
    public int CrestCount => crests != null ? crests.Length : 0;
    private void Start()
    {
        lake = GetComponent<LakeBackdrop>();
        if (lake == null || lake.WaterFXMaterial == null) { enabled = false; return; }
        crests = new LineRenderer[22];
        for (int i = 0; i < crests.Length; i++)
        {
            var line = new GameObject("TravellingWaterCrest_" + i).AddComponent<LineRenderer>();
            line.transform.SetParent(transform, false); line.useWorldSpace = true;
            line.sharedMaterial = lake.WaterFXMaterial; line.positionCount = 9;
            line.startWidth = line.endWidth = i % 3 == 0 ? .025f : .014f;
            line.sortingOrder = -171; crests[i] = line;
        }
    }
    private void LateUpdate()
    {
        if (crests == null || lake.GameplayCamera == null) return;
        var camera = lake.GameplayCamera; float time = lake.MotionClock;
        float width = camera.orthographicSize * 2 * camera.aspect;
        float left = camera.transform.position.x - width * .5f;
        for (int i = 0; i < crests.Length; i++)
        {
            float phase = Mathf.Repeat(time * (.10f + i % 4 * .012f) + i * .617f, 1);
            float lane = Mathf.Repeat(i * .381966f + phase * .17f, 1);
            float x = left + lane * width;
            float y = lake.WaterY - .055f - (i % 7) * .15f;
            float length = (.3f + (i % 5) * .16f) * Mathf.Sin(phase * Mathf.PI);
            float alpha = Mathf.Pow(Mathf.Sin(phase * Mathf.PI), 2) * (i % 3 == 0 ? .5f : .28f);
            var line = crests[i];
            for (int p = 0; p < 9; p++)
            {
                float u = p / 8f;
                line.SetPosition(p, new Vector3(x + (u - .5f) * length,
                    y + Mathf.Sin(u * Mathf.PI * 2 + time * 1.3f + i) * .012f, 0));
            }
            line.startColor = line.endColor = new Color(.32f, .7f, .71f, alpha);
        }
    }
}
