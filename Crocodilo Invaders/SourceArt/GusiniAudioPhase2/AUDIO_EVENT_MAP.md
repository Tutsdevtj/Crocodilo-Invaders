# AUDIO_EVENT_MAP.md — integração e sincronismo

Este documento usa os tempos e eventos REAIS extraídos dos JSONs presentes no pacote de assets enviado. **Timecodes são relativos ao início de cada clip**; entidades independentes (ovo, gansinho, tentáculo de faixa) têm seus próprios relógios.

## Transformação F1 → F2 — total 4100 ms
| Evento | Momento | Arquivo correspondente | Observação |
|---|---:|---|---|
| final_hit_frame | 0 ms | TR01 FinalHit | Barra F1 zera; pare tiros ativos |
| start_stagger_frame | 160 ms | TR02 EngineStall | Morte aparente; caixa(s) de cura são gameplay, sem som novo aqui |
| apparent_defeat_hold_frame | 690 ms | *sem efeito* | **Pausa intencional**. Não colocar whoosh/loop novo até o 1º sinal. Pode reduzir engine e music usando mixer Unity |
| first_sign_frame | 1330 ms | TR03 FirstPulse | O 1º brilho verde surpreende |
| corruption_start_frame | 1510 ms | TR04 CorruptionSurge | Cobre mutação/flashes rápidos |
| transformation_peak_frame | 2530 ms | TR05 MutationPeak | Pico/clarão da transformação |
| phase2_reveal_frame | 2820 ms | OP07 BossBarTakeover | **Opcional**: entra a barra F2. Evitar excesso de camadas |
| return_pose_frame | 3230 ms | TR06 RevealCry | Ganso resiste / ruge distorcido |
| phase2_ready_frame | 3800 ms | OP01 DamagedEngine_Loop | A IA só começa depois do final do clip, em 4100 ms |

## Ataque 1 — Rajada Corrompida (boss clip 1530 ms)
| Evento | Momento | SFX |
|---|---:|---|
| telegraph_start | 0 ms | EN01 Charge |
| charge_full | 360 ms | OP02 ChargeFull (opcional) |
| fire_1 | 490 ms | EN02 Fire |
| fire_2 | 790 ms | EN02 Fire |
| fire_3 | 1090 ms | EN02 Fire |
| ball_pop | na colisão / dissipação de **cada bola** | EN03 BallPop |

**Crítico:** três tiros com 300 ms entre si, não usar um som contínuo de 1,5 s contendo três disparos; assim o áudio continua certo se timing for alterado.

## Ataque 2 — Ovo Corrompido (boss clip 710 ms)
| Evento | Momento | SFX |
|---|---:|---|
| hatch_open | 180 ms | OP03 HatchCorrupt (ou reutilizar som F1) |
| egg_drop | 400 ms | OP04 EggDrop (ou reutilizar F1) |
| Egg Deploy start | cerca de **250 ms depois** do spawn do ovo (valor sugerido de gameplay) | EG01 ParachutePop |
| Pulses | a partir do início da descida: 0,20 / 0,55 / 0,82 / 1,00 s | EG02 PulseTick, variar levemente pitch/volume |
| Crack1 | após descida; duração ~200 ms | EG03 EggCrack |
| Crack2 | em sequência; duração ~160 ms | EG03 EggCrack (variação leve) |
| OnEggBurst | início do Burst de ~120 ms | EG04 EggBurst, **um áudio** para cinco gemas |

**Crítico:** Ovo **não pode ser abatido**. ParachutePop = aviso principal; não confundir seus 210 ms de deploy com 210 ms após o boss começar.

## Ataque 3 — Esquadrilha Corrompida (boss clip 870 ms)
| Evento | Momento | SFX |
|---|---:|---|
| call_start | 170 ms | SQ01 CorruptCall |
| edge_markers_on | ~300 ms (gameplay sugerido) | SQ02 EdgeMark (1x para todos os marcadores) |
| squadron_spawn | 580 ms, gansinhos nascem **fora da tela** | sem som obrigatório |
| first goose visible | ~1200 ms (sugestão) | OP05 Flyby opcional |
| OnQuack | quando boca atinge OPEN, +120 ms após sing começar | SQ03 / SQ04 / SQ05 honks em vozes diversas |
| diver_flash_start | 400 ms antes de mergulho, para os gansos mergulhadores | SQ06 DiverWarning |
| diver_launch | após os 400 ms de aviso | SQ07 DiveWhoosh |
| goose_defeated | HP gansinho = 0 | SQ08 GoosePoof |

**Crítico:** voo do corpo e canto do bico são independentes. Não bake coral inteiro em WAV; Unity controla offsets por gansinho. Mergulho não persegue player após travar alvo.

## Ataque 4 — Tentáculos v2 (exclusivo)
| Evento | Momento | SFX |
|---|---:|---|
| start / carapace crack | início da entrada; módulo começa a rachar | TE01 ModuleCrack |
| tentacles_emerge | +190 ms do start | TE02 Emerge |
| warn_loop_start | no início de cada um dos três avisos de **900 ms** | TE03 LaneWarning |
| lash_hit | depois de cada aviso | TE04 Lash (3x, com variação mínima) |
| lane_strike | **+160 ms** após instanciar tentáculo de faixa | TE05 TipCrack (uma vez para as duas faixas simultâneas) |
| lane retract | **+320 ms** no clip lane_tentacle de 530 ms | OP06 Retract opcional |
| resist_start | quando terminam três golpes | TE06 ResistCry |
| weakpoint_open | +370 ms no clip resist | TE07 WeakpointOpen; manter exposto **2000 ms** |
| weakpoint_close | depois dos 2000 ms de exposição | TE08 WeakpointClose |

**Crítico:** a sequência `warn` da arte (frames 0/1) é repetida até completar **900 ms**. O `lash_hit` NÃO deve ocorrer 300 ms após começar o aviso só porque o frame 2 seria esse no clip não-loop. Os 900 ms prevalecem. Depois de abrir o núcleo, frames 4/5 da resistência ficam em loop por 2 s; não fechar 240 ms depois. Não gerar aviso sonoro por frame nem misturar 3 chicotadas num único WAV.

## Prioridade de mixagem e concorrência

- Telégrafos importantes (parachute pop, tentacle warning, diver warning) mais legíveis que motores/ornamentos.
- Sons repetidos (HONKs, tiro triplo, três tentáculos) devem ter ganho moderado, limiter de vozes por família e modulação leve de pitch.
- Boss bar e som do transform peak podem coincidir: use OP07 baixo ou descarte.
- Ao pausar/morrer/encerrar luta, desligar engine loops e áudios de entidades órfãs com cleanup existente.
- UI não dispara áudios de gameplay; scripts de gameplay/public events conduzem os gatilhos.
- **Nunca** sobrescrever os SFX aprovados da F1 sem decisão explícita; substituir apenas os pontos da F2.
