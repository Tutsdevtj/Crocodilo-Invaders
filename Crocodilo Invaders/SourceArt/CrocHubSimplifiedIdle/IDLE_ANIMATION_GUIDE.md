# B. Crocodilo (HUB) — Guia do IDLE e da montagem na Unity

## 1. O ciclo

O ciclo tem **6 quadros a 6 FPS**, ou seja, 1 segundo por respiração. Ele é montado só com os módulos do master e deslocamentos de 1 px. Nenhum quadro foi redesenhado.

| Quadro | Nome | Cabeça | Tronco | Braços | Quadril / pernas |
|---|---|---|---|---|---|
| 00 | repouso | 0 | 0 | 0 | 0 |
| 01 | inspira | 0 | −1 | −1 | 0 |
| 02 | topo | −1 | −1 | −1 | 0 |
| 03 | segura | −1 | −1 | −1 | 0 |
| 04 | expira | −1 | 0 | 0 | 0 |
| 05 | assenta | 0 | 0 | 0 | 0 |

Os valores estão em px no canvas, com y para baixo: −1 = sobe 1 px.

- **O tronco e os ombros sobem juntos** ao inspirar. Os braços acompanham o ombro, então não há desencaixe.
- **A cabeça chega 1 quadro depois** e **desce 1 quadro depois**. Esse atraso dá peso e um ar tranquilo e firme, em vez de um boneco subindo inteiro.
- **Quadril, pernas e botas não se mexem.** O personagem respira, não pula.
- O 00 e o 05 são iguais de propósito. O repouso fica mais tempo na tela e o fechamento do loop (05 → 00) é imperceptível.

## 2. Piscada (independente da respiração)

| Item | Valor |
|---|---|
| Overlays | `eyelid_half` e `eyelid_closed`, filhos do nó `Head` (sobem e descem com a cabeça) |
| Sequência | meio fechado 60 ms → fechado 100 ms → meio fechado 60 ms |
| Intervalo | aleatório entre 3 e 6 s, sem ligação com o quadro do idle |
| Na pose de repouso | os dois overlays ficam desativados |

`previews/idle_zoom.gif` e `previews/idle_actual_scale.gif` mostram 5 ciclos com uma piscada no 3º.

## 3. Montagem na Unity (para o Codex)

1. **Importação:** importe `modules/*.png` com Point (no filter), Compression None, Mesh Type Full Rect, PPU 100 (provisório, o mesmo do projeto) e pivô Custom = `unity_pivot` do manifesto.
2. **Hierarquia:** crie os nós vazios na ordem de `hierarchy`, com `localPosition = local_units_ppu100`:

   ```text
   CrocHub (pivô 24,71 = chão)
   └── Hips (24,47)          sprite: hips
       ├── LegBack (28,51)   leg_back
       ├── LegFront (16,51)  leg_front
       └── Torso (24,41)     torso
           ├── ArmBack (35,35)   arm_back
           ├── Head (25,33)      head, eyelid_half, eyelid_closed
           └── ArmFront (10,33)  arm_front
   ```

3. **SpriteRenderers:** um filho por sprite, com `localPosition (0,0,0)` e `sortingOrder` conforme `render_order`:

   ```text
   leg_back(0) → leg_front(1) → hips(2) → arm_back(3) → torso(4) → head(5)
   → eyelid_half(6) → eyelid_closed(7) → arm_front(8)
   ```

   Use um `SortingGroup` no `CrocHub`.
4. **Idle:** aplique `idle.frames[].node_local_offset_unity_px` como `localPosition.y` (em px, dividido pelo PPU) dos nós `Torso`, `Head`, `ArmFront` e `ArmBack`. Os filhos compensam o pai: quando o tronco sobe e a cabeça não, `Head` recebe −1.
5. **Alternativa por sprite:** os quadros prontos estão em `idle/croc_idle_00..05.png`. É o mesmo resultado, num canvas idêntico de 48×72, com o pivô no chão em (24,71) = `unity_pivot (0.5, 0.0139)`.

## 4. Verificações feitas

- **Remontagem pelos módulos × master:** 0 px de diferença (`previews/modular_reconstructed.png`).
- **Frestas entre peças:** nenhuma nos quadros. As 3–4 mudanças restantes de silhueta são as bordas do capacete, do focinho e da luva subindo 1 px, que é movimento legítimo e não buraco.
- **Pés:** botas no mesmo lugar em todos os quadros (o quadril e as pernas nunca se movem).
- **Cabeça:** é sempre a mesma peça, sem deformação. Capacete, bolsos e fivela não mudam de forma.
- **Fundos de teste:** claro neutro e um recorte do cenário da Base Improvisada (`idle_contact_sheet.png`), além de branco, preto, xadrez e máscara de alfa (`transparency_check.png`).
- **Escala real:** a leitura 1:1 está na faixa inferior do contact sheet e em `idle_actual_scale.gif`.

## 5. Preparação para a corrida (não implementada)

- **Pernas** já separadas (frente e trás), com pivô no quadril.
- **Braços** inteiros, com pivô no ombro. Se a corrida precisar de cotovelo, dá para dividir na linha 44 (punho da manga).
- **Quadril** é peça própria, para balanço e inclinação.
- **Tronco** é peça própria, para inclinar à frente.
- **Subpinturas atuais** cobrem 1 px de deslocamento. A corrida vai precisar de áreas ocultas maiores no quadril e nos ombros, que serão feitas na próxima tarefa.
