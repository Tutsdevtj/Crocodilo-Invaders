using System;
using UnityEngine;

// Roteiro editável no Inspector: falas, expressões e pausas não ficam no código.
[CreateAssetMenu(menuName = "Crocodilo/Dialogue Sequence")]
public sealed class DialogueSequence : ScriptableObject
{
    public enum PauseStyle { Silent, RadioStatic }
    public enum LineEffect { None, Interference }

    [Serializable]
    public sealed class Line
    {
        public string speakerId;
        [Tooltip("Vazio usa o nome do elenco.")] public string speakerName;
        [TextArea(2, 4)] public string text;
        public string expressionId;
        [Min(0f)] public float pauseBefore;
        public PauseStyle pauseStyle;
        [Min(0f)] public float pauseAfter;
        [Tooltip("Substitui o som de digitação desta fala.")] public AudioClip soundOverride;
        [Tooltip("Som único no início da fala, depois da pausa anterior.")] public AudioClip startSound;
        [Range(0f, 1f)] public float startSoundVolume = .5f;
        public LineEffect effect;
        [Tooltip("Interferência: retrato que pisca por cima da expressão atual.")] public string glimpseSpeakerId;
        public string glimpseExpressionId;
        [Tooltip("Marco opcional da cena (ex.: radio, farewell), observado pelo orquestrador.")] public string beat;
    }

    [SerializeField] private DialogueCast cast;
    [SerializeField, Min(0f), Tooltip("Espera antes de abrir a caixa.")] private float openDelay = .3f;
    [SerializeField, Min(0f), Tooltip("Espera após a última fala, antes de fechar.")] private float closeDelay = .2f;
    [SerializeField] private Line[] lines;

    public DialogueCast Cast => cast;
    public float OpenDelay => openDelay;
    public float CloseDelay => closeDelay;
    public Line[] Lines => lines;
    public int Count => lines?.Length ?? 0;

    public bool Validate(out string error)
    {
        error = null;
        if (cast == null) error = "Elenco ausente.";
        else if (Count == 0) error = "Sequência sem falas.";
        else
        {
            for (int i = 0; i < lines.Length && error == null; i++)
            {
                Line line = lines[i];
                DialogueCast.Speaker speaker = line == null ? null : cast.Find(line.speakerId);
                if (line == null || string.IsNullOrWhiteSpace(line.text)) error = $"Fala {i + 1}: texto vazio.";
                else if (speaker == null) error = $"Fala {i + 1}: falante '{line.speakerId}' não está no elenco.";
                else if (speaker.side != DialogueCast.Side.None && !DialogueCast.HasExpression(speaker, line.expressionId))
                    error = $"Fala {i + 1}: expressão '{line.expressionId}' não existe para {speaker.id}.";
                else if (line.effect == LineEffect.Interference && !string.IsNullOrEmpty(line.glimpseSpeakerId)
                    && !DialogueCast.HasExpression(cast.Find(line.glimpseSpeakerId), line.glimpseExpressionId))
                    error = $"Fala {i + 1}: retrato da interferência ausente.";
            }
        }
        return error == null;
    }
}
