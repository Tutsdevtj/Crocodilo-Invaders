# Gusini F2 — Ataque 3: Esquadrilha Corrompida

O Gusini chama a esquadrilha com um **grasnado distorcido**: ondas verdes serrilhadas saem da ponta do bico, com falha, e o olho do módulo acende. Antes de qualquer gansinho entrar, **marcadores verdes piscam na borda direita**, nas faixas por onde cada um vai entrar. Os gansinhos chegam **corrompidos**: olho e lente verdes, veias no corpo e bico verde por dentro. Eles **podem ser abatidos** com 2 tiros.

- **Boss:** recomposto das **peças aprovadas da F2**, com os **mesmos tempos aprovados da chamada da F1**.
- **Canvas e pivô:** **256×196**, pivô **(120,118)** = Unity (0.4688, 0.398).
- **Início e fim:** F0 = F6 = pose base da F2.
- **Gansinho:** **mesmo canvas (36×28), pivô (19,14), hitbox (26×17) e animação de voo** do gansinho aprovado. Só a pintura muda, nas variantes A e B.

## 1. Chamada do boss (não-loop · 7 frames · 870 ms)
| F | ms | Início | O que acontece | Evento |
|---|---|---|---|---|
| 0 | 70 | 0 | Pose base F2 | `telegraph_start` |
| 1 | 100 | 70 | Estufa o peito, olho do módulo acende | |
| 2 | 110 | 170 | **Grasnado começa**: cabeça sobe, bico abre, 1ª onda | `call_start` |
| 3 | 150 | 280 | Grasnado sustentado: 2 ondas + falha | |
| 4 | 150 | 430 | Grasnado sustentado: 3 ondas | |
| 5 | 180 | 580 | **COMANDO**: cabeça avança, bico escancarado | `squadron_spawn` |
| 6 | 110 | 760 | Recuperação (= F0) | `recover_start` |

## 2. Aviso longo (o ponto que você pediu)
| ms | O que acontece |
|---|---|
| 170 | `call_start`: grasnado + ondas verdes. **Som de aviso aqui.** |
| **300** | **Marcadores de borda** (`fx_edge_warning_0/1`, piscando a 120 ms), um por gansinho, colados na borda direita, na altura por onde cada um vai entrar |
| 580 | `squadron_spawn`: os gansinhos nascem **fora da tela**, à direita |
| **~1200** | O 1º gansinho cruza a borda e o marcador dele some. Os de trás cruzam depois, e os marcadores deles ficam até lá |

São **~1,0 s** de aviso visual na borda e **~1,2 s** de aviso total antes do primeiro gansinho aparecer.

## 3. Gansinho corrompido (pasta `gansinho/`)
| Arquivo | Uso |
|---|---|
| `gansinho_ca_0-3`, `gansinho_cb_0-3` | voo em loop, 80/70/80/70 ms (variantes A e B, como na F1) |
| `gansinho_c_beak_0-2` | camada do bico no mesmo pivô, para o canto: estados 0→1→2→1 (60/60/120/70 ms); `OnQuack` no estado 2 |
| `gansinho_ca/cb_flash_green_0-3` | **aviso do mergulhador**: alternar com o voo normal a cada 80 ms por **400 ms**, parado |
| `gansinho_ca/cb_hit_white_0-3` | flash de dano (60 ms) quando leva o 1º tiro |
| `gansinho_c_sheet.png` | folha A e B |

**FX (pasta `fx/`):**
- `fx_gansinho_poof_0-4` (40×40): abate com flash, penas voando e faíscas verdes. Tempos 60/60/70/80/90 ms.
- `fx_edge_warning_0-1` (26×24): marcador de borda.
- `fx_dive_streak_0-2` (24×12): rastro atrás do mergulhador, 60 ms.

**Mergulhador (abaixo de 60% de vida, slots 1 e 4 do V):**
1. Ao chegar a 70% da largura da tela, ele para.
2. Pisca verde por 400 ms.
3. Mergulha **em linha reta** até onde o player estava naquele instante, sem perseguir. Use o frame 2 do voo (asa para baixo) e o `fx_dive_streak` atrás.

**Voo "menos certinho" (na Unity):**
- **Formação:** V mais aberto que na F1.
- **Ondulação:** cada gansinho ondula ±8 px num período de 0,8 s, defasado 1,1 rad do vizinho.
- **Velocidade:** 120–140 px/s.
- **Canto:** os grasnados são defasados como no coral da F1.

Os valores estão em `suggested_gameplay` no JSON.

## 4. Arquivos
```
frames/  gusini_p2_squadron_call_sheet.png (+_x2)  gusini_p2_squadron_call.json
parts/   sprite e fx separados por frame do boss
gansinho/  fx/  tools/
previews/  gusini_p2_squadron_call_x3.gif · gusini_p2_squadron_call_em_tela.gif · gansinho_corrompido_voo_canto_x6.gif
           gansinho_mergulhador_x4.gif · gansinho_dano_abate_x6.gif · folha_gansinho_fx_x5.png · comparacao_gansinho_f1_f2_x8.png
```

## Autorrevisão
- **Boss:** canvas e pivô fixos e F0 = F6 = pose base. Só a cabeça e o bico se movem (até 2 px) e o corpo sobe 1 px no grasnado. As ondas saem da ponta do bico.
- **Gansinho:** a silhueta, o canvas, o pivô e a animação são idênticos aos aprovados.
- **Penas eriçadas cortadas:** testei pontas espetadas na barriga, mas no tamanho nativo (36 px) elas viravam ruído e pareciam pernas. Pela regra do contexto, a corrupção do gansinho ficou em **olho e lente verdes, veias e bico verde**, que leem bem no tamanho real.
- Alpha só 0/255, sem anti-aliasing.
