using System;
using UnityEngine;

// Conteúdo exclusivo de um boss na apresentação PERIGO!: título, peças nas âncoras do guia (§7),
// partícula própria e o som do alerta. Faixa, PERIGO!, impacto, speedlines e tempos são genéricos.
[CreateAssetMenu(menuName = "Crocodilo/UI/Boss Alert Theme")]
public sealed class BossAlertTheme : ScriptableObject
{
    [Serializable]
    public sealed class Particle
    {
        public Sprite normal, dissolve1, dissolve2;
    }

    [Header("Título")]
    public Sprite title;
    [Tooltip("Um quadro (33 ms) no impacto.")] public Sprite titleFlash;
    public Vector2Int titlePosition = new Vector2Int(66, 34);
    [Tooltip("Largura do título + 26; mínimo 170.")] public int bandLength = 236;

    [Header("Faixa (vazio = genérica do BossAlertBanner)")]
    [Tooltip("Variante recolorida da faixa genérica para outro boss (contorno navy fixo).")]
    public Sprite bandHead;
    public Sprite bandMid;
    public Sprite bandEnd;

    [Header("Peças exclusivas")]
    [Tooltip("Anexo da frente da faixa (Goosini: ponta-bico).")] public Sprite frontAttachment;
    public Vector2Int frontPosition = new Vector2Int(30, 40);
    [Tooltip("Elemento acima e à direita (Goosini: pena longa).")] public Sprite topAccessory;
    public Vector2Int topPosition = new Vector2Int(150, 0);
    [Tooltip("Elemento que tremula no fim da faixa (Goosini: cachecol).")] public Sprite[] trailFrames = new Sprite[0];
    [Min(1f)] public float trailFrameMs = 90f;
    [Tooltip("Somado a (faixa.x + comprimento, faixa.y).")] public Vector2Int trailOffset = new Vector2Int(2, 12);

    [Header("Partículas (giro = ordem da lista)")]
    public Particle[] particles = new Particle[0];
    [Tooltip("Penas atravessando a tela entre a saída da onda e a entrada do banner.")] public bool transitionParticles = true;

    [Header("Som do alerta")]
    public AudioClip alertSound;
    [Range(0f, 1f)] public float alertVolume = .7f;
    [Tooltip("Início do som relativo ao t = 0 do banner. Negativo começa durante a transição de penas.")]
    public float alertSoundOffsetMs = -150f;
}
