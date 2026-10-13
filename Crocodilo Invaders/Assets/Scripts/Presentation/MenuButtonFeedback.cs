using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public sealed class MenuButtonFeedback : MonoBehaviour, ISelectHandler, IDeselectHandler,
    IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, ISubmitHandler
{
    [SerializeField] private RectTransform artwork;
    [SerializeField] private RectTransform leftDiamond;
    [SerializeField] private RectTransform rightDiamond;
    [SerializeField] private CanvasGroup diamonds;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip focusSound;
    [SerializeField] private AudioClip pressSound;
    [SerializeField, Min(1f)] private float selectedScale = 1.06f;
    [SerializeField, Min(.01f)] private float responseDuration = .12f;

    private Button button;
    private Vector2 artworkPosition;
    private Vector2 leftPosition;
    private Vector2 rightPosition;
    private float focus;
    private float pressUntil;
    private bool pressed;
    private bool selected;

    private void Awake()
    {
        button = GetComponent<Button>();
        artworkPosition = artwork.anchoredPosition;
        leftPosition = leftDiamond.anchoredPosition;
        rightPosition = rightDiamond.anchoredPosition;
    }

    private void OnEnable() => button.onClick.AddListener(PlayPressSound);
    private void OnDisable()
    {
        button.onClick.RemoveListener(PlayPressSound);
        selected = pressed = false;
        focus = pressUntil = 0f;
        artwork.localScale = Vector3.one;
        artwork.anchoredPosition = artworkPosition;
        diamonds.alpha = 0f;
    }

    private void Update()
    {
        float blend = 1f - Mathf.Exp(-4f * Time.unscaledDeltaTime / responseDuration);
        focus = Mathf.Lerp(focus, selected ? 1f : 0f, blend);
        float scale = Mathf.Lerp(1f, selectedScale, focus);
        if (pressed || Time.unscaledTime < pressUntil) scale *= .94f;
        artwork.localScale = Vector3.one * scale;
        artwork.anchoredPosition = artworkPosition + Vector2.up * (2f * focus);
        diamonds.alpha = focus;
        float sway = Mathf.Sin(Time.unscaledTime * 3f) * 2f * focus;
        leftDiamond.anchoredPosition = leftPosition + Vector2.left * sway;
        rightDiamond.anchoredPosition = rightPosition + Vector2.right * sway;
    }

    public void OnSelect(BaseEventData eventData)
    {
        selected = true;
        if (audioSource != null && focusSound != null) audioSource.PlayOneShot(focusSound, .35f);
    }

    public void OnDeselect(BaseEventData eventData) => selected = pressed = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.IsInteractable() && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(gameObject, eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && button.IsInteractable()) pressed = true;
    }

    public void OnPointerUp(PointerEventData eventData) => pressed = false;

    public void OnSubmit(BaseEventData eventData) => pressUntil = Time.unscaledTime + .1f;

    private void PlayPressSound()
    {
        if (audioSource != null && pressSound != null) audioSource.PlayOneShot(pressSound, .55f);
    }
}
