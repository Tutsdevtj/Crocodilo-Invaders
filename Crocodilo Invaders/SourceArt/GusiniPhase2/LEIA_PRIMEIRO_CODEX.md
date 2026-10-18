# Gusini — Fase 2: pacote para implementação (Unity)

Este pacote reúne tudo o que é novo da Fase 2 do boss Gusini em *Crocodilo Invaders*.

**Mudança de direção:** o Gusini agora tem **só 2 fases**, e a **Fase 3 foi descartada**. A Fase 2 é a forma corrompida pelo parasita alien, e a transformação F1 → F2 acontece em campo, sem o boss sair da tela.

Cada pasta tem **notas (`.md`)** e um **JSON, que é a fonte da verdade** para frames, durações, eventos, pivôs e hitboxes. Leia as notas de cada pasta antes de implementar aquela parte.

## Ordem sugerida
| # | Pasta | O quê | Fonte da verdade |
|---|---|---|---|
| 1 | `Gusini_Transform_F1_F2/` | Transformação F1 → F2 (4,1 s, 30 frames). Começa quando a barra da F1 chega a 0 | `gusini_transform_f1_f2.json` |
| 2 | `BossBar_Gusini_F2/` | Barra de vida da F2 (verde, corrompida). Mesmo prefab da barra da F1: troca os sprites na transição | `bossbar_f2_layout.json` |
| 3 | `Gusini_P2_Ataque1_EnergyShot/` | Rajada Corrompida: 3 bolas de energia gigantes, uma a cada 0,3 s, cada uma em zigue-zague vertical | `gusini_p2_energy_shot.json` |
| 4 | `Gusini_P2_Ataque2_OvoCorrompido/` | Ovo corrompido de paraquedas que estoura em 5 gemas ácidas. **O ovo NÃO pode ser abatido** | `gusini_p2_egg_drop.json` |
| 5 | `Gusini_P2_Ataque3_Esquadrilha/` | Esquadrilha corrompida: gansinhos que **podem ser abatidos** (2 tiros), com mergulhadores abaixo de 60% de vida | `gusini_p2_squadron_call.json` |
| 6 | `Gusini_P2_Ataque4_Tentaculos_v2/` | Ataque exclusivo, os Tentáculos: 3 golpes em faixas (uma faixa segura por golpe) e depois a resistência, com núcleo exposto (dano ×2 por 2,0 s) | `gusini_p2_tentacles.json` |

## Convenções (valem para tudo)
- **Canvas do boss:** **256×196** em todos os clips.
  - Pivô **(120,118)** px = Unity **(0.4688, 0.398)**, o mesmo da `gusini_p1_idle`.
  - Basta trocar o clip no mesmo SpriteRenderer.
- **Importação:** pixel art com Filter Mode **Point**, Compression **None** e alpha só 0/255.
  - Respeite o PPU indicado em cada nota: assets nativos ou x2.
- **Eventos:** os Animation Events disparam **no início do frame** indicado.
  - As durações são **variáveis por frame**: use os `duration_ms` do JSON.
- **Pose base da F2:** o **último frame da transformação** (frame 29).
  - Todos os ataques da F2 começam e terminam nessa pose (0 px de diferença), então as transições entre clips não pulam.
- **Movimento:**
  - O **shake e o movimento do corpo inteiro** (bob, recuo, afundar) ficam a cargo da **Unity**.
  - As spritesheets só têm a animação interna: cabeça, bico, canhão, tentáculos.
- **Arquivos separados:** projéteis e FX são separados do boss, nas pastas `fx/`, `lane/`, `lane_tentacle/`, `gansinho/` etc.
  - Na pasta `parts/` de cada ataque, cada frame tem o sprite e o FX em camadas separadas.
- **Previews (`previews/`):** são só referência visual.
  - Movimentos de projétil, zigue-zague e trajetórias nas previews são ilustrativos; os valores sugeridos estão nos JSONs (`suggested_gameplay` / `suggested_sequence`).
- **`tools/`:** scripts Python que geraram a arte. Não fazem parte do jogo e não precisam ser importados.

## Pontos de atenção
- **Idle da F2:** não existe um clip animado próprio.
  - Use o **frame 29 da transformação (pose base)** como idle estático, com bob e flutuação feitos na Unity.
  - Se quiser um idle animado, peça a arte antes de improvisar.
- **Tentáculos:** use **só a v2** (`Gusini_P2_Ataque4_Tentaculos_v2`). A v1 foi descartada e não está neste pacote.
  - O tentáculo de faixa tem 8 frames de 600×128, com pivô (600,64) = (1.0, 0.5). Coloque o pivô na **borda direita da tela, no centro da faixa**.
  - O hitbox de cada frame está no JSON. Os espinhos ficam fora do hitbox de propósito.
- **Personagem:** o Gusini **deve continuar recuperável** (ele vira NPC lojista depois). Não adicione efeitos que o deixem grotesco.
- **Fora deste pacote:** áudio da F2 e o padrão de alternância e seleção dos ataques (a ordem e os pesos ficam no código).
  - Os ataques 1 a 3 podem ser sorteados.
  - Sugestão: o ataque 4 (Tentáculos) é o ataque exclusivo de pressão. Use-o 1 vez a cada ciclo dos outros três.
- **`Extras_mesma_rodada/`:** traz o **HUD do player v2**, a **barra de vida da F1** e o **background "Céu de Batalha"**, feitos na mesma rodada.
  - Não são da Fase 2, mas são novos. Implemente-os se ainda não estiverem no projeto.
  - A barra da F2 reaproveita o prefab da barra da F1.
