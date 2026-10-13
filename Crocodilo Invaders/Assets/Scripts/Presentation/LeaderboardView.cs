using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class LeaderboardView : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private TMP_Text[] positions;
    [SerializeField] private TMP_Text[] names;
    [SerializeField] private TMP_Text[] scores;
    [SerializeField] private TMP_Text pagination;
    [SerializeField] private TMP_Text emptyMessage;
    [SerializeField] private TMP_Text status;
    [SerializeField] private Button previous;
    [SerializeField] private Button next;
    [SerializeField] private Button close;
    private Action closed;
    public bool IsVisible => content.activeSelf;
    public int Page { get; private set; }
    public int PageCount => Mathf.Max(1, (LeaderboardSession.Store.Entries.Count + names.Length - 1) / names.Length);

    private void Awake()
    {
        content.SetActive(false);
        previous.onClick.AddListener(() => SetPage(Page - 1));
        next.onClick.AddListener(() => SetPage(Page + 1));
        close.onClick.AddListener(Close);
    }

    public void Open(Action onClosed)
    {
        if (IsVisible) return;
        closed = onClosed;
        LeaderboardSession.SaveCompletedRun();
        content.SetActive(true);
        SetPage(0);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(close.gameObject);
    }

    public void SetPage(int page)
    {
        Page = Mathf.Clamp(page, 0, PageCount - 1);
        var entries = LeaderboardSession.Store.Entries;
        for (int row = 0; row < names.Length; row++)
        {
            int index = Page * names.Length + row;
            bool populated = index < entries.Count;
            positions[row].text = populated ? (index + 1).ToString() : "";
            names[row].text = populated ? entries[index].playerName.ToUpperInvariant() : "";
            scores[row].text = populated ? entries[index].points.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")) : "";
            Color color = index == 0 ? new Color(1f, .8f, .15f) : index == 1 ? new Color(.83f, .91f, 1f)
                : index == 2 ? new Color(1f, .65f, .4f) : new Color(.97f, .96f, .9f);
            positions[row].color = names[row].color = scores[row].color = color;
        }
        pagination.text = (Page + 1) + " / " + PageCount;
        previous.interactable = Page > 0;
        next.interactable = Page < PageCount - 1;
        emptyMessage.gameObject.SetActive(entries.Count == 0);
        status.text = LeaderboardSession.Store.Error ?? "RANKING LOCAL";
        if (EventSystem.current != null)
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if ((selected == next.gameObject && !next.interactable) || (selected == previous.gameObject && !previous.interactable))
                EventSystem.current.SetSelectedGameObject(close.gameObject);
        }
    }

    public void Close()
    {
        if (!IsVisible) return;
        content.SetActive(false);
        Action callback = closed;
        closed = null;
        callback?.Invoke();
    }
}
