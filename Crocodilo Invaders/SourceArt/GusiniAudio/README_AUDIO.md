# Gusini Fase 1 — Áudio · Guia para o Codex (Unity)

> **STATUS: WAVs finalizados.** Pacote produzido em 17/10/2026 com ElevenLabs Sound Effects:
> **20 efeitos / 24 WAVs**, incluindo 3 variantes de tiro e 3 de dano.
> Os arquivos em `Phase1/…` estão em WAV PCM 24-bit / 48 kHz; somente o flyby é estéreo.
> O manifest está em `FINAL` e todas as entradas estão em `final`.
>
> **Avaliação auditiva pendente:** a seleção foi técnica, por transientes, duração, clipping e espectro;
> não foi feita escuta crítica. Ouça `_previews/OUVIR_TODOS.wav` e teste o mix na Unity.
> A ordem e o tempo de cada som estão em `_previews/ORDEM_DOS_SONS.tsv`.
> Os dois candidatos comparados por efeito estão em `_candidates/<id>/`; tiro e dano têm um terceiro para variantes.
> A interface do ElevenLabs gera quatro opções por lote e não oferece seletor de duas nesta tela.
> Os prompts foram compactados apenas para caber no limite de 450 caracteres.
> A duração mínima prática usada foi 0.5 s; o acabamento ajusta os tempos exatos abaixo.
>
> `SELECAO_E_QA.md` documenta as escolhas; `selection_and_qa.json` registra as edições;
> `VALIDACAO_FINAL.json` registra a verificação técnica. `tools/finish_elevenlabs.py` reproduz o acabamento.
> Nenhum efeito substituto foi sintetizado localmente. Os opcionais permanecem desligados por padrão.

Fonte da verdade: **`gusini_phase1_audio.json`**. Os tempos vêm dos JSONs das animações do pacote `Gusini_Boss`.

---

## 1. Formato
- WAV · 48 kHz · PCM 24-bit · **mono**. A **única** exceção estéreo é `SFX_Gusini_Squadron_Flyby_01.wav`.
- Sem reverb embutido, com headroom: o pico fica entre −1.5 e −6 dBFS, conforme a prioridade. O mix final é feito na Unity.
- Import Settings: **Decompress On Load** para one-shots curtos; **Compressed In Memory (Vorbis q≈0.7)** para os dois loops; Load In Background desligado.

## 2. Mixer (AudioMixer `Gusini_SFX`)
Grupos sugeridos e ordem de importância:

| Grupo | Clips | Observação |
|---|---|---|
| `Danger` | Cannon, Egg_Burst, Egg_Crack_* | o que machuca soa primeiro |
| `Telegraph` | Squadron_Whistle | precisa atravessar a música |
| `Goose` | Goose_Honk_*, Squadron_Flyby | não pode competir com a música |
| `Mechanism` | Shoot_Cock, Egg_Hatch*, Egg_Drop, ParachuteDeploy | sempre abaixo do tiro |
| `Ambient` | Engine_Idle_Loop, ParachuteFlutter_Loop | bem baixo |
| `Damage` | Hit_01..03 | curto, com limite de repetição |

O `suggested_volume` do manifest já reflete essa hierarquia. Use como `AudioSource.volume`, ou como ganho do grupo.

## 3. Onde cada som toca

### 3.1 Boss: Animation Events

**Eventos novos** (somar aos eventos de `IMPLEMENTACAO_CODEX.md`): `OnShootWindup`, `OnHatchLatch`, `OnHatchOpen` e `OnHatchClose`.

| Clip de animação | Evento | Tempo no clip | Frame | Toca |
|---|---|---|---|---|
| `gusini_p1_shoot` | `OnShootWindup()` | **0.08 s** | 1 | `SFX_Gusini_Shoot_Cock_01` |
| `gusini_p1_shoot` | `OnFire()` | **0.18 s** | 2 | `SFX_Gusini_Shoot_Cannon_01..03` (round-robin) |
| `gusini_p1_squadron_call` | `OnSquadronCallStart()` | **0.17 s** | 2 | `SFX_Gusini_Squadron_Whistle_01` (dura ≤ 410 ms e termina antes do spawn) |
| `gusini_p1_squadron_call` | `OnSquadronSpawn()` | **0.58 s** | 5 | `SFX_Gusini_Squadron_Flyby_01` no **SquadronRoot**; `CommandWhoosh` só se habilitado |
| `gusini_p1_egg_drop` | `OnHatchLatch()` | **0.08 s** | 1 (latch) | `SFX_Gusini_Egg_HatchLatch_01` |
| `gusini_p1_egg_drop` | `OnHatchOpen()` | **0.18 s** | 2 (open1) | `SFX_Gusini_Egg_HatchOpen_01` (cobre open1→open2→open) |
| `gusini_p1_egg_drop` | `OnEggDrop()` | **0.40 s** | 5 (drop) | `SFX_Gusini_Egg_Drop_01` |
| `gusini_p1_egg_drop` | `OnHatchClose()` | **0.49 s** | 6 (close1) | `SFX_Gusini_Egg_HatchClose_01` (o "clack" cai em ~130 ms, no frame 8) |

### 3.2 Gansinho: camada `Beak`, clip `Sing`
| Clip | Evento | Tempo | Toca |
|---|---|---|---|
| `Sing` (bico 0→1→2→1) | `OnQuack()` | **0.12 s** (frame 2 = bico ABERTO) | `SFX_Gusini_Goose_Honk_{Low,Mid,High}_01` |

- O som tem o transient em t≈0. O evento fica no frame em que o bico **chega** a aberto.
- **Coral:** a voz é definida por slot da formação. Padrão sugerido: líder `Mid`; 2º par `Low` + `High`; 3º par `Mid` + `High`.
- Defasagem do `Sing` por slot: `0, 140, 140, 280, 280 ms` (`gansinho.json → coral_example_offsets_ms`).
- O voo **não** é defasado.

### 3.3 Entidade Ovo (por script, na troca de estado)
| Estado ao entrar | Toca |
|---|---|
| `Deploy` | `SFX_Gusini_Egg_ParachuteDeploy_01`, com o pop em ~210 ms, quando entra o paraquedas aberto |
| `Descent` | `SFX_Gusini_Egg_ParachuteFlutter_Loop_01` (**opcional, desligado por padrão**) |
| `Crack1` | `SFX_Gusini_Egg_Crack_01` |
| `Crack2` | `SFX_Gusini_Egg_Crack_02` |
| `Burst` (`OnEggBurst`) | `SFX_Gusini_Egg_Burst_01`, mais `YolkWhoosh_01` **só se habilitado**, e **um** só para as 3 gemas |

### 3.4 Contínuo e dano
| Quando | Toca |
|---|---|
| Boss ativo (OnEnable → OnDisable) | `SFX_Gusini_Engine_Idle_Loop_01` (loop, volume ≈ 0.18) |
| `Gusini.OnDamaged()` | `SFX_Gusini_Hit_01..03` (round-robin) |

## 4. Loops
| Clip | Comprimento | Observação |
|---|---|---|
| `SFX_Gusini_Engine_Idle_Loop_01` | 2.400 s (= 4 voltas do idle de 600 ms) | crossfade já embutido; `loop = true` |
| `SFX_Gusini_Egg_ParachuteFlutter_Loop_01` | 1.280 s (= 4 ciclos do paraquedas de 320 ms) | opcional; parar ao sair de `Descent` |

Todo o resto é one-shot.

## 5. Variantes, pitch aleatório e simultaneidade

| Clip / grupo | Variantes | Pitch aleatório | Regra de simultaneidade |
|---|---|---|---|
| Shoot_Cannon | 3 (round-robin, sem repetir a anterior) | 0.95–1.05 | no máx. **3 vozes**; nunca a mesma variante 2× seguidas |
| Shoot_Cock | 1 | 0.97–1.03 | em rajada, **só no 1º tiro** |
| Squadron_Whistle | 1 | **nenhum** (é o telegraph) | **1 instância**; cortar se o ataque for interrompido |
| Squadron_Flyby | 1 | 0.97–1.03 | 1 por formação (no SquadronRoot), nunca 1 por gansinho |
| Goose_Honk_* | 1 por registro | 0.98–1.02 (pouco, para manter a "afinação" do coral) | até 5 (o coral); **no máx. 1 honk por gansinho por vez** |
| Egg_Hatch*, Egg_Drop | 1 | 0.95–1.05 | 1 por ovo |
| Egg_ParachuteDeploy | 1 | 0.96–1.04 | 1 por ovo |
| Egg_Crack_01 / _02 | 1 cada | 0.97–1.03 | 1 por ovo; **nunca os dois juntos** |
| Egg_Burst | 1 | 0.97–1.03 | se 2 ovos estourarem no mesmo frame, tocar 1 só com volume +2 dB |
| Egg_YolkWhoosh (opc.) | 1 | 0.95–1.05 | **1 por burst**, nunca 3 |
| Engine_Idle_Loop | 1 | fixo (opcional: 1.00→1.05 durante ataques) | 1 instância |
| Hit | 3 (round-robin) | 0.92–1.08 | **cooldown 70 ms**; no máx. 2 vozes |

## 6. Opcionais (desligados por padrão)
`Squadron_CommandWhoosh`, `Egg_ParachuteFlutter_Loop` e `Egg_YolkWhoosh` são **opcionais**. No manifest estão com `optional: true` e `enabled_by_default: false`. Exponha um toggle no config e **não** toque nenhum deles se o clip estiver ausente.

## 7. Contrato de código (sugestão)
- `GusiniAudioConfig` (ScriptableObject): uma entrada por `id` do manifest, com `AudioClip[] variants`, `volume`, `pitchMin/Max`, `maxVoices`, `cooldown`, `enabled`.
- Um único `GusiniAudio.Play(id, position)`. Os Animation Events chamam métodos do boss que delegam para ele.
- Posicional: o jogo é 2D. Use `spatialBlend = 0`, com pan leve opcional por X de tela. Não use 3D com rolloff.

## 8. Como regenerar ou trocar os WAVs (para o artista ou produtor)
1. Gere os candidatos com uma ferramenta **dedicada a SFX** usando `SFX_PROMPTS.md`: um efeito por vez e ≥ 3 candidatos cada. Salve em `_candidates/<id>/`.
2. `python3 tools/process_candidates.py analyze`: gera `_candidates/REPORT.md`, que aponta clipping, silêncio inicial, duração fora do evento e excesso de agudo ou sub-grave.
3. **Ouça** e escolha.
4. `python3 tools/process_candidates.py finalize shoot_cannon=a.wav,b.wav,c.wav squadron_whistle=w3.wav …`. O processador:
   - corta o silêncio inicial, mantendo 2 ms antes do transient;
   - limita a duração à do evento visual;
   - aplica micro-fades só quando necessário;
   - faz o crossfade dos loops;
   - normaliza o pico com headroom;
   - grava 48 kHz / 24-bit nos nomes e pastas corretos;
   - atualiza o manifest.
5. `python3 tools/process_candidates.py previews --boss ../Gusini_Boss`: gera em `_previews/` os MP4 dos sons **sobre as animações reais** (tiro ×3, esquadrilha com o coral, ovo completo), mais o coral ×20 e os hits ×15, para testar fadiga.

Requisitos: Python 3, numpy, scipy e ffmpeg.
