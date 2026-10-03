using UnityEngine;

public sealed class CrocodiloLaserView : MonoBehaviour
{
    public bool vertical;
    public CrocodiloVisual beam, trace, warningArea;
    [Min(1)] public float visualThickness = 220;
    [Min(1)] public float damageThickness = 30;
    public float previewPosition = 200;

    public Rect BeamRect(Rect visible, float position, float thickness)
    {
        float center = position + damageThickness / 2;
        return vertical
            ? new Rect(center - thickness / 2, visible.yMin, thickness, visible.height)
            : new Rect(visible.xMin, center - thickness / 2, visible.width, thickness);
    }

    public void Show(int phase, float position, Rect visible, float elapsed)
    {
        beam.Show(phase == 2, elapsed);
        trace.Show(phase == 1, 0);
        warningArea.Show(phase == 1, 0);
        if (phase == 0) return;
        beam.FitRect(BeamRect(visible, position, visualThickness));
        trace.FitRect(BeamRect(visible, position, 7));
        warningArea.FitRect(BeamRect(visible, position, damageThickness));
    }
}
