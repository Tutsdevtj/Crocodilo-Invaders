# Background "Céu de Batalha": notas para a Unity

**Conceito:** um céu azul amplo com uma guerra aérea acontecendo ao longe.
- Um cruzador alien gigante, avariado e enevoado, domina o fundo, sem ser boss.
- Mini dogfights em silhueta deixam contrails finos.
- O horizonte mostra mar e ilhas com uma base militar.

O sol synthwave foi **removido**. Nada aqui é um elemento circular central.

Fonte da verdade: **`background_layers.json`**. Lá estão tamanhos, posições, ordem, velocidades, sortingOrder e os pontos de avaria.

## 1. Escala e import
- **Arte nativa:** a janela tem **600×360 px** e é exibida em x2. Use **PPU 50**: a janela vira **12×7.2 unidades**, a mesma escala do `background.png` atual (1200 px a PPU 100).
- **Import:** Filter **Point**, Compression **None**, sem mipmaps, Mesh Type **Full Rect**.
- **Camadas repetidas:** têm **800 px (16 unidades)** de largura e **emendam sem costura**. Use com o `ScrollingLayer` que já existe, no modo `RepeatingLayer` (2 segmentos bastam; o script cria mais se precisar).
- **Encaixe na câmera:** coloque todas as camadas dentro de um pai `BattleSky`. Esse pai recebe o mesmo encaixe que o `BackgroundFit` faz hoje, para tudo escalar junto.
  - Camadas de 360 px de altura: alinhe o **topo** ao topo da janela.
  - `horizon_far` e `horizon_near`: alinhe a **base** à base da janela.

## 2. Camadas (de trás para frente)
| Ordem | Arquivo | Função | Repete | Velocidade sugerida (un/s) | sortingOrder |
|---|---|---|---|---|---|
| L0 | `L0_sky_base` | céu em faixas, **opaco**; não rola | — | 0 | -100 |
| L1 | `L1_alien_mothership_layer` | cruzador alien gigante, enevoado, com rombos | não (sprite único 340×112) | 0.06, ou 0 se preso à câmera | -96 |
| — | FX da nave-mãe | explosões, clarões, fumaça, falhas e detritos | — | acompanham a L1 | -95 |
| L2 | `L2_far_clouds` | estratos altos e banco de nuvens distante; **cobre a barriga da nave** | sim | 0.25 | -92 |
| L3 | `L3_distant_battle_layer` | mini batalha estática: silhuetas, contrails, traçantes, flak | sim | 0.4 | -88 |
| L4 | `L4_mid_clouds` | cúmulos médios | sim | 0.9 | -84 |
| L5 | `L5_horizon_far_layer` | montanhas azuladas, mar com brilhos, 2 fumaças na costa | sim | 1.2 | -76 |
| L6 | `L6_near_clouds` | fragmentos no topo e bruma baixa sobre o mar | sim | 2.2 | -72 |
| L7 | `L7_horizon_near_layer` | ilhas: base militar (hangar, torre, radar, antiaérea), farol, canhão destruído | sim | 4.0 | -68 |

Os elementos de gameplay devem ficar em sortingOrder **≥ 0**.

**Diferenças em relação à ordem sugerida no seu pedido, e por quê:**
- A **nave-mãe** fica atrás das nuvens distantes (L2), porque assim parece mais longe e maior.
- A **mini batalha** fica na frente da nave: os caças estão mais perto que o cruzador.
- As **nuvens próximas** ficam entre as montanhas e as ilhas, como bruma baixa. Na frente de tudo, elas brigariam com o gameplay.

**O que substituir na cena atual:**
- **Sai:** `AnimatedSun`, `background.png` e `ScrollingGround`.
- **Opcional:** o `Cloud` atual (`cloud.png`) pode ficar como nuvem extra bem rápida, mas ele tem outro estilo.

## 3. FX da nave-mãe (pasta `fx/`)
Os pontos de avaria estão em `mothership.damage_points_*` no JSON, em px a partir do canto superior esquerdo do sprite e também em unidades locais a partir do centro:
- `breach_top`: rombo no dorso
- `prow_hole`: buraco na proa
- `rear`: popa
- `dome`: domo
- `fin3_stump`: barbatana quebrada

| FX | Arquivos | Tamanho | Tempo | Uso |
|---|---|---|---|---|
| explosões (`alien_mothership_fx_explosions`) | `fx_ms_explosion_0-5` | 17×17, pivot no centro | 100 ms/frame, uma vez | 1 explosão a cada 1.2–2.5 s, em um ponto de avaria aleatório |
| clarão | `fx_ms_flash_0-2` | 9×9 | 50 ms/frame | junto com o início da explosão |
| fumaça (`alien_mothership_fx_smoke`) | `fx_ms_smoke_0-7` | 22×32, pivot (0.18, 0.06) na base | 110 ms/frame, **loop** | contínua em `breach_top`, `rear` e `fin3_stump`, com defasagem de 3 frames entre elas |
| falha alien | `fx_ms_glitch_0-3` | 7×5 | 70 ms/frame | cintila no domo e nas veias a cada 0.8–1.5 s |
| detritos | `fx_ms_debris_0-3` | 3×3 | 80 ms/frame, loop | 2–3 por explosão, caindo em arco, somem em ~1.2 s |

Tudo isso é discreto e abafado pela distância. O GIF `05_nave_mae_fx_x3.gif` mostra o rodízio.

## 4. Mini batalha animada (opcional, pasta `sprites/`)
A camada **L3 já é uma batalha estática completa**. Para dar vida, adicione poucos sprites soltos por cima dela, com sortingOrder -87:
- **Naves:** `battle_human_fighter`, `battle_human_bomber`, `battle_alien_dart`, `battle_alien_saucer`.
- **Tiros:** `battle_tracer`, `battle_alien_shot`.
- **Efeitos:** `battle_explosion_0-3` (100 ms), `battle_flak_0-2` (150 ms).

Regras para não roubar a atenção:
- no máximo **3–4 naves vivas** ao mesmo tempo;
- velocidade de **0.3–0.6 un/s**, além do parallax da L3;
- tinja os sprites com uma cor próxima do céu (ex.: `#9FB8D8`);
- traçantes de 3 px **nunca** brancos puros;
- evite trajetórias retas cruzando o centro junto com os tiros do jogo.

O `04_parallax_sim.gif` mostra 2 duplas animadas, mais flak e mini explosões.

## 5. Leitura de gameplay
Alguns cuidados para o fundo não competir com o jogo:
- **Contraste:** o fundo não tem contorno preto em nenhum lugar, e os valores são médios e claros. Os sprites de jogo (contorno escuro, cores saturadas) ficam sempre acima. Veja `10_teste_valores_cinza.png`.
- **Nave-mãe:** recebeu ~30% de névoa atmosférica, para os inimigos que passam na frente dela continuarem saltando. O verde alien dela é apagado, então não se confunde com os tiros verdes do player.
- **Contrails e silhuetas:** são de baixo contraste e ficam fora da faixa central.
- **Textos:** "Pontos" e "Inimigos" podem passar sobre a bruma clara do rodapé. Recomendo um contorno ou sombra de 2 px nesses textos. É uma mudança de UI, que eu não fiz.

## 6. Arquivos
```
Background_CeuDeBatalha/
  layers/        L0…L7 em PNG nativo (L0 opaco; demais transparentes)
  layers_x2/     as mesmas em x2 (se preferir PPU 100)
  fx/, fx_x2/    FX da nave-mãe
  sprites/, sprites_x2/   peças da mini batalha animada
  previews/      01 composição final · 02 camadas separadas · 03 empilhamento · 04 parallax (GIF)
                 05 FX da nave (GIF) · 06 folha de FX/sprites · 07/08/09 contexto antes × depois · 10 teste de valores
  background_layers.json
  tools/         bg_core.py, bg_ship.py, bg_world.py, bg_compose.py, bg_previews.py, bg_docs.py
```

**Observação:** a tela "antes" foi **reconstruída** a partir de `background.png`, `ground.png`, `cloud.png` e do sol, que está em `SkyEnvironment` (y = 3.95). Nada foi alterado no projeto Unity.
