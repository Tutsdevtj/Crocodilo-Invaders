using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public sealed class AudioSliderFeedback : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler
{
    [SerializeField] private RectTransform handle;
    [SerializeField] private Outline highlight;
    private Slider slider;
    private bool selected;
    private float focus;
    private void Awake() => slider = GetComponent<Slider>();
    private void Update()
    {
        focus = Mathf.MoveTowards(focus, selected && slider.IsInteractable() ? 1f : 0f, Time.unscaledDeltaTime * 10f);
        handle.localScale = Vector3.one * (1f + focus * .16f);
        highlight.effectColor = new Color(1f, .77f, .3f, focus * .75f);
    }
    public void OnSelect(BaseEventData data) => selected = true;
    public void OnDeselect(BaseEventData data) => selected = false;
    public void OnPointerEnter(PointerEventData data)
    {
        if (slider.IsInteractable() && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(gameObject);
    }
    private void OnDisable() { selected = false; focus = 0f; if (handle != null) handle.localScale = Vector3.one; }
}
