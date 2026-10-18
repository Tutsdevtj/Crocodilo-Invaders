using System;
using TMPro;
using UnityEngine;

// Falantes e expressões aprovadas. A posição do retrato fica no prefab da UI, não aqui.
[CreateAssetMenu(menuName = "Crocodilo/Dialogue Cast")]
public sealed class DialogueCast : ScriptableObject
{
    // None: falante sem retrato (ex.: RÁDIO); os retratos em cena só escurecem.
    public enum Side { Left, Right, None }

    [Serializable]
    public sealed class Expression
    {
        public string id;
        public Sprite portrait;
    }

    [Serializable]
    public sealed class Speaker
    {
        public string id;
        public string displayName;
        [Tooltip("Tarja da placa de nome.")] public Color accent = Color.white;
        public Side side;
        [Tooltip("Espelha o retrato quando o lado escolhido contraria a direção da arte.")] public bool flipX;
        [Range(.5f, 2f)] public float typingPitch = 1f;
        [Tooltip("Opcional: fonte própria da placa (ex.: acentos legíveis no RÁDIO). Vazio usa a da UI.")] public TMP_FontAsset nameFont;
        [Min(0f)] public float nameFontSize;
        public Expression[] expressions;
    }

    [SerializeField] private Speaker[] speakers;
    public Speaker[] Speakers => speakers;

    public Speaker Find(string id)
    {
        if (speakers == null || string.IsNullOrEmpty(id)) return null;
        foreach (Speaker speaker in speakers)
            if (speaker != null && speaker.id == id) return speaker;
        return null;
    }

    public static Sprite Portrait(Speaker speaker, string expressionId)
    {
        if (speaker?.expressions == null || speaker.expressions.Length == 0) return null;
        foreach (Expression expression in speaker.expressions)
            if (expression != null && expression.id == expressionId) return expression.portrait;
        return speaker.expressions[0].portrait;
    }

    public static bool HasExpression(Speaker speaker, string expressionId)
    {
        if (speaker?.expressions == null) return false;
        foreach (Expression expression in speaker.expressions)
            if (expression != null && expression.id == expressionId && expression.portrait != null) return true;
        return false;
    }
}
