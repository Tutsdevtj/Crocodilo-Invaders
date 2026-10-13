using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public sealed class PlayerNameView : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private TMP_InputField input;
    [SerializeField] private TMP_Text message;
    [SerializeField] private Button startButton;
    [SerializeField] private Button cancelButton;
    private Action<string> confirmed;
    private Action closed;
    private bool submitArmed;
    private int openedFrame;
    public bool IsVisible => content.activeSelf;

    private void Awake()
    {
        content.SetActive(false);
        input.characterLimit = LeaderboardStore.NameLimit;
        input.onSubmit.AddListener(_ => { if (submitArmed) Confirm(); });
        input.onValueChanged.AddListener(_ => message.text = "ATÉ 16 CARACTERES • ENTER PARA INICIAR");
        input.onValidateInput += ValidateCharacter;
        startButton.onClick.AddListener(Confirm);
        cancelButton.onClick.AddListener(Close);
    }

    private char ValidateCharacter(string text, int index, char character) =>
        char.IsLetterOrDigit(character) || character == ' ' || character == '-' || character == '_' || character == '.' ? character : '\0';

    public void Open(Action<string> onConfirmed, Action onClosed)
    {
        if (IsVisible) return;
        confirmed = onConfirmed;
        closed = onClosed;
        submitArmed = false;
        openedFrame = Time.frameCount;
        content.SetActive(true);
        input.SetTextWithoutNotify(LeaderboardSession.Store.LastPlayerName);
        message.text = "ATÉ 16 CARACTERES • ENTER PARA INICIAR";
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(input.gameObject);
        input.ActivateInputField();
        input.Select();
    }

    private void Update()
    {
        if (!IsVisible || submitArmed || Time.frameCount <= openedFrame) return;
        var keyboard = Keyboard.current;
        submitArmed = keyboard == null || (!keyboard.enterKey.isPressed && !keyboard.numpadEnterKey.isPressed);
    }

    public void Confirm()
    {
        if (!IsVisible) return;
        string name = LeaderboardStore.NormalizeName(input.text);
        if (name.Length == 0)
        {
            message.text = "INFORME O NOME DO JOGADOR";
            input.ActivateInputField();
            return;
        }
        confirmed?.Invoke(name);
    }

    public void ShowError(string error) { message.text = error; input.ActivateInputField(); }
    public void Hide() { content.SetActive(false); confirmed = null; closed = null; }
    public void Close()
    {
        if (!IsVisible) return;
        Action callback = closed;
        Hide();
        callback?.Invoke();
    }
}
