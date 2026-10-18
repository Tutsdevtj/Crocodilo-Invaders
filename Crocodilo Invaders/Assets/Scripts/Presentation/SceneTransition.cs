using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Troca de cena reutilizável: fade, encerra diálogo, salva o resultado pendente e carrega uma única vez.
// Destino ausente do build não chama SceneManager: mostra o aviso de desenvolvimento sobre o fade.
public sealed class SceneTransition : MonoBehaviour
{
    public enum State { Idle, FadingOut, Loading, DestinationUnavailable }
    [SerializeField] private CanvasGroup overlay;
    [SerializeField] private TMP_Text message;
    [SerializeField] private TMP_Text hint;
    [SerializeField] private DialogueRunner dialogue;
    [SerializeField, Min(.01f)] private float fadeDuration = 1.2f;
    [SerializeField, Min(0f)] private float messageDelay = .4f;
    [SerializeField, Min(.01f)] private float messageFade = .5f;

    public State Current { get; private set; }
    public string Destination { get; private set; }
    public event Action<State> StateChanged;

    private void Awake() => Hide();

    public static bool IsAvailable(string scene) => !string.IsNullOrEmpty(scene) && Application.CanStreamedLevelBeLoaded(scene);

    public bool Go(string scene, string unavailableMessage)
    {
        if (Current != State.Idle || !isActiveAndEnabled) return false;
        Destination = scene;
        StartCoroutine(Run(scene, unavailableMessage));
        return true;
    }

    private IEnumerator Run(string scene, string unavailableMessage)
    {
        Set(State.FadingOut);
        if (dialogue != null) dialogue.Cancel();
        overlay.gameObject.SetActive(true);
        message.alpha = hint.alpha = 0f;
        for (float elapsed = 0f; elapsed < fadeDuration; elapsed += Time.unscaledDeltaTime)
        {
            overlay.alpha = Mathf.SmoothStep(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        overlay.alpha = 1f;
        // Resultado da run vai para o ranking antes de sair da cena, uma vez por ID.
        LeaderboardSession.SaveCompletedRun();
        if (IsAvailable(scene))
        {
            Set(State.Loading);
            Time.timeScale = 1f;
            AudioListener.pause = false;
            SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            yield break;
        }
        Debug.Log($"Cena '{scene}' ainda não está no build; capítulo concluído aguardando o destino.", this);
        Set(State.DestinationUnavailable);
        message.text = unavailableMessage;
        for (float elapsed = 0f; elapsed < messageDelay; elapsed += Time.unscaledDeltaTime) yield return null;
        for (float elapsed = 0f; elapsed < messageFade; elapsed += Time.unscaledDeltaTime)
        {
            message.alpha = hint.alpha = Mathf.Clamp01(elapsed / messageFade);
            yield return null;
        }
        message.alpha = 1f;
        hint.alpha = .7f;
    }

    private void Set(State state)
    {
        Current = state;
        StateChanged?.Invoke(state);
    }

    private void Hide()
    {
        if (overlay == null) return;
        overlay.alpha = 0f;
        overlay.gameObject.SetActive(false);
    }
}
