# B. Crocodilo — Guia de montagem do rig 2D (Unity)

Pacote: `Croc_Rig_Ready`. Fonte: master aprovado + `Croc_Idle_Modular` aprovado (idle de 6 frames a 6 FPS).
Nada foi redesenhado: todo pixel visível na pose de repouso é o pixel do master. Os pixels novos ficam **escondidos** atrás de outras peças e só aparecem quando um membro dobra.

> Este guia é para o Codex montar o prefab. O `rig_manifest.json` tem todos os números; este texto explica o que eles significam e onde é preciso cuidado.

---

## 1. Resultado da validação

| Verificação | Resultado |
|---|---|
| Repouso montado pelo rig × master | **0 px diferentes**, idêntico pixel a pixel (`previews/original_vs_reconstructed.png`) |
| Montagem usando só o manifesto + PNGs (`tools/verify_manifest.py`) | **0 px diferentes**, e todos os 23 pivôs e posições batem |
| Piscada (overlay) | 0 px diferentes |
| Idle, frames 0 e 5 | 0 px diferentes |
| Idle, frames 1 / 2 / 3 / 4 | **20 / 23 / 23 / 4 px** de contorno diferentes. São diferenças documentadas (seção 8) |
| Alpha dos PNGs | Só 0 ou 255 |
| Arquivos citados no manifesto | Todos existem |
| Teste de flexão (braços e pernas, 20 poses) | Nenhuma ilha de pixels solta e nenhum buraco nas juntas |

---

## 2. Conteúdo do pacote

```text
Croc_Rig_Ready/
  master/croc_platformer_master.png      master aprovado (96×184), sem alteração
  modules/*.png                          23 PNGs independentes, fundo transparente
  idle/                                  6 frames aprovados + piscada + spritesheet (originais, sem alteração)
  previews/
    assembled_rest.png                   personagem montado pelo rig (x4)
    module_exploded_view.png             peças afastadas a partir das articulações (x3)
    modules_sheet_x2.png                 cada PNG isolado, com nome (x2)
    arm_bending_test.gif / _poses.png    braços: estendido / parcial / mais flexionado + ombro p/ trás e p/ frente
    leg_bending_test.gif / _poses.png    pernas: estendida / joelho parcial / joelho mais dobrado + coxa à frente e atrás
    original_vs_reconstructed.png        master × rig × mapa de diferença
    idle_comparison.gif                  idle aprovado × idle pelo rig × diferença (vermelho)
    idle_differences_detail.png          as diferenças restantes do idle ampliadas, com marcação
  rig_manifest.json
  RIG_SETUP_GUIDE.md
  tools/                                 scripts usados para gerar e validar (Python, não é código Unity)
```

Os testes de flexão aparecem ampliados e também em **tamanho real (1:1)**, na faixa inferior dos `_poses.png`.

---

## 3. Convenções de coordenadas

- Canvas: **96×184 px**. Origem no canto **superior esquerdo** e y para baixo, como no `croc_idle.json`.
- Pivô geral do personagem: **(48, 180)**, no centro dos pés. A linha do chão é y = 180, e a última linha de pixels opacos é 179.
- Uma articulação `(x, y)` é o **canto superior esquerdo** do pixel (x, y). É a mesma convenção do pacote do idle, o que mantém tudo em pixels inteiros.
- `unity_pivot` = `((jx − x) / w, (y + h − jy) / h)`, normalizado com a origem **embaixo à esquerda**, como no Sprite Editor.
  - Alguns valores ficam **fora de 0..1** (marcados "fora" na tabela da seção 5): anéis, coxas, tronco, capacete, pálpebra e `torso_side_back`. O pivô deles é a articulação do nó, que fica fora do retângulo da imagem. A Unity aceita isso (Custom Pivot).
  - Se preferir evitar pivôs fora do retângulo, cada sprite também traz `alt_bottom_left_pivot`: pivô (0,0) e `local_position_px` em relação ao nó.
- `local_px` dos nós usa y para baixo. **`local_unity_px` já está com o y invertido**: é ele que vai no `localPosition`, dividido pelo PPU.
- Rotação: **positivo = anti-horário na tela** (eixo Z da Unity).

---

## 4. Hierarquia

Cada nó é um **Transform vazio posicionado na articulação**. Os SpriteRenderers são filhos desse nó, com `localPosition (0,0,0)` e pivô = articulação.

```text
CharacterRoot                (pivô 48,180)
└── Hips                     articulação 46,94    local (−2, +86)    sprites: hips, pelvis_under
    ├── Torso                46,86                (0, +8)            torso, torso_side_back
    │   ├── NeckHead         49,58                (+3, +28)          head, helmet, eyelid_closed
    │   ├── ArmBackUpper     65,61  ombro tras.   (+19, +25)         arm_back_upper, ring_elbow_back
    │   │   └── ArmBackLower 68,83  cotovelo      (+3, −22)          arm_back_lower
    │   │       └── HandBack 72,102 pulso         (+4, −19)          hand_back
    │   └── ArmFrontUpper    21,60  ombro frente  (−25, +26)         arm_front_upper, ring_elbow_front
    │       └── ArmFrontLower 15,82 cotovelo      (−6, −22)          arm_front_lower
    │           └── HandFront 17,103 pulso        (+2, −21)          hand_front
    ├── LegBackUpper         56,100 quadril tras. (+10, −6)          leg_back_upper, ring_knee_back
    │   └── LegBackLower     58,131 joelho        (+2, −31)          leg_back_lower
    │       └── BootBack     59,158 tornozelo     (+1, −27)          boot_back
    └── LegFrontUpper        36,100 quadril frente (−10, −6)         leg_front_upper, ring_knee_front
        └── LegFrontLower    31,131 joelho        (−5, −31)          leg_front_lower
            └── BootFront    26,158 tornozelo     (−5, −27)          boot_front
```

Os valores entre parênteses são `local_unity_px` (x, y para cima) em relação ao pai. Para o `localPosition`, divida pelo PPU.

### Como os pivôs foram medidos

Os pivôs foram medidos na arte, não estimados:

- **Ombros:** centro da cabeça do ombro, onde a curva da manga encontra o tronco.
- **Cotovelos:** centro da dobra da manga, na linha em que o volume do braço muda de direção.
- **Pulsos:** centro do punho da manga, logo acima da luva.
- **Quadris:** centro de cada coxa, logo abaixo da aba da jaqueta.
- **Joelhos:** centro do volume do joelho na calça.
- **Tornozelos:** centro do cano da bota, dentro da barra da calça.
- **Pescoço:** base do pescoço, no meio da gola.
- **Torso:** gira na cintura (46,86), acima do cinto. Assim o cinto e a aba ficam no Hips e a cintura não "quebra".

---

## 5. Ordem de renderização (sorting order)

Use **um único Sorting Layer** e `sortingOrder` = índice abaixo. Quanto maior o índice, mais à frente.

| # | sprite | nó | tamanho | x,y no canvas | unity_pivot | papel |
|---|---|---|---|---|---|---|
| 0 | pelvis_under | Hips | 39×25 | 27,94 | 0.4872, 1.0000 |pelve/assento escondido (novo)|
| 1 | boot_back | BootBack | 31×27 | 49,152 | 0.3226, 0.7778 |bota de trás|
| 2 | ring_knee_back | LegBackUpper | 18×10 | 50,131 | 0.3333, 4.1000 (fora) |anel do joelho (novo)|
| 3 | leg_back_lower | LegBackLower | 22×29 | 47,123 | 0.5000, 0.7241 |canela de trás|
| 4 | leg_back_upper | LegBackUpper | 21×39 | 47,101 | 0.4286, 1.0256 (fora) |coxa de trás|
| 5 | boot_front | BootFront | 20×29 | 17,151 | 0.4500, 0.7586 |bota da frente|
| 6 | ring_knee_front | LegFrontUpper | 21×11 | 21,131 | 0.7143, 3.8182 (fora) |anel do joelho (novo)|
| 7 | leg_front_lower | LegFrontLower | 26×29 | 16,122 | 0.5769, 0.6897 |canela da frente|
| 8 | leg_front_upper | LegFrontUpper | 27×40 | 20,101 | 0.5926, 1.0250 (fora) |coxa da frente|
| 9 | hips | Hips | 40×19 | 26,83 | 0.5000, 0.4211 |cinto + aba da jaqueta|
| 10 | torso_side_back | Torso | 4×29 | 61,56 | -3.7500, -0.0345 (fora) |contorno lateral do tronco (novo)|
| 11 | ring_elbow_back | ArmBackUpper | 10×4 | 64,86 | 0.1000, 7.2500 (fora) |anel do cotovelo (novo)|
| 12 | arm_back_lower | ArmBackLower | 15×37 | 63,78 | 0.3333, 0.8649 |antebraço de trás|
| 13 | arm_back_upper | ArmBackUpper | 17×33 | 57,56 | 0.4706, 0.8485 |braço de trás|
| 14 | hand_back | HandBack | 11×18 | 68,102 | 0.3636, 1.0000 |luva de trás|
| 15 | torso | Torso | 43×40 | 21,45 | 0.5814, -0.0250 (fora) |tronco|
| 16 | head | NeckHead | 49×37 | 26,23 | 0.4694, 0.0541 |cabeça|
| 17 | helmet | NeckHead | 39×18 | 26,8 | 0.5897, -1.7778 (fora) |capacete|
| 18 | eyelid_closed | NeckHead | 10×9 | 39,28 | 1.0000, -2.3333 (fora) |piscada: desligado, exceto no frame de piscada|
| 19 | ring_elbow_front | ArmFrontUpper | 17×9 | 7,82 | 0.8235, 3.4444 (fora) |anel do cotovelo (novo)|
| 20 | arm_front_lower | ArmFrontLower | 17×32 | 7,75 | 0.4706, 0.7812 |antebraço da frente|
| 21 | arm_front_upper | ArmFrontUpper | 24×41 | 8,49 | 0.5417, 0.7317 |braço da frente|
| 22 | hand_front | HandFront | 17×18 | 10,103 | 0.4118, 1.0000 |luva da frente|

Os valores exatos (com mais casas decimais) estão em `rig_manifest.json → sprites.*.unity_pivot`. **Use sempre o manifesto, não esta tabela.**

Ordens que não podem mudar:

- `ring_*` fica **logo abaixo** da peça de baixo da mesma junta. Se ficar acima, aparece no repouso.
- `pelvis_under` fica abaixo de **todas** as peças das pernas.
- O braço de cima fica **acima** do antebraço (`arm_*_upper` > `arm_*_lower`), porque o disco do cotovelo pertence ao braço de cima. Nas pernas é o contrário do que se esperaria: a coxa fica acima da canela, pelo mesmo motivo.
- `torso_side_back` fica abaixo do braço de trás inteiro, embora seja filho do Torso.

---

## 6. Passo a passo para montar o prefab (para o Codex)

1. **Importar `modules/*.png`:**
   - Texture Type: Sprite (2D and UI);
   - Sprite Mode: Single;
   - **Filter Mode: Point (no filter)**;
   - **Compression: None**;
   - Mesh Type: Full Rect, para que o pivô fora do retângulo não seja recortado;
   - PPU igual ao do idle atual.
2. **Definir o pivô de cada sprite:** Custom = `unity_pivot` do manifesto. Alternativa: pivô Bottom Left e o SpriteRenderer com `localPosition = alt_bottom_left_pivot.local_position_px / PPU`, com o y já para cima.
3. **Criar os nós** na ordem de `nodes` (cada pai antes dos filhos). Use `localPosition = local_unity_px / PPU` e rotação 0.
4. **Criar um SpriteRenderer filho por sprite**, dentro do nó indicado em `sprites.*.node`. Use `localPosition (0,0,0)` (ou a alternativa do passo 2) e `sortingOrder = sorting_order`.
5. **Deixar `eyelid_closed` desativado.** Ele só é ligado no frame de piscada.
6. **Conferir:** com todas as rotações em 0, o prefab deve ficar idêntico a `master/croc_platformer_master.png`. Para checar fora da Unity, rode `tools/verify_manifest.py`, que monta usando só o manifesto.
7. **Recomendado:** usar Pixel Perfect Camera (URP 2D ou o pacote 2D Pixel Perfect) com o mesmo PPU, e posições de Transform em múltiplos de 1/PPU.

---

## 7. Como as articulações funcionam (áreas ocultas)

Não é só um corte. Cada junta tem uma estrutura de encaixe:

- **Cotovelos e joelhos ("junta de bola"):**
  - A peça de cima (braço ou coxa) é dona de um **disco** em volta da articulação. O raio é cerca de metade da largura do membro: cotovelo da frente 7 px, de trás 5 px, joelho da frente 9 px, de trás 8 px.
  - A peça de baixo carrega, **escondidos sob o disco**, os pixels reais do master daquela região. Quando o antebraço ou a canela gira, o que aparece por baixo é a própria manga ou calça do master, e não um esticamento.
  - Os **anéis** (`ring_*`) são meio anel de contorno, com raio = disco + 1. Ficam abaixo da peça de baixo e só aparecem no lado de fora da dobra, fechando o contorno do cotovelo ou do joelho.
- **Pulsos:** o antebraço tem o **punho da manga continuado por baixo da luva**, com a textura da manga. Ao girar a mão, não abre buraco.
- **Tornozelos:** cada bota tem o **cano continuado por baixo da barra da calça** (7 linhas, com a textura da bota).
- **Quadril:**
  - As coxas continuam **por baixo da aba da jaqueta**, com a textura da calça.
  - `pelvis_under` é um assento/virilha escondido atrás de todas as peças das pernas. Ao abrir as pernas, aparece calça, e não céu. Assim a coxa não se solta do quadril.
- **Cintura:** o Torso gira em (46,86). O tronco tem a lateral continuada **por baixo do braço da frente** (textura da jaqueta) e a aba está no Hips. A cintura não parece cortada.
- **Ombros:**
  - O master só mostra a metade de fora da manga de trás. Por isso, a **metade de dentro foi desenhada** com 4 px de manga sólida nas cores da manga, com contorno, escondida atrás do tronco. Ao balançar, o braço de trás mantém volume.
  - `torso_side_back` é o contorno da lateral direita do tronco. Ele aparece quando o braço de trás sai da frente.
- **Pescoço:** a cabeça e a gola já se sobrepõem no master. Para os ±6° sugeridos, não foi preciso preenchimento extra.

### Espaços negativos

A fenda entre o braço e o corpo continua **transparente** em todas as peças. Nenhum preenchimento oculto fica fora da silhueta de repouso: o código só pinta onde há outra peça **na frente** no repouso.

Pedacinhos de pintura escondida que ficariam soltos ao dobrar foram removidos. Isso inclui sobras antigas de 3 px do idle, além de alguns contornos que pertenciam à peça errada. Esses contornos foram passados para a peça certa (`reassigned_visible_pixels` no manifesto):

- contorno da luva da frente, que estava na perna → mão da frente (6 px);
- contorno da calça, que estava no antebraço de trás → coxa de trás (12 + 5 px).

---

## 8. Idle no rig e diferenças documentadas

O idle é feito só com deslocamentos de 1 px, a 6 FPS (166 ms por frame). O filho compensa o pai para ficar parado.

| frame | Torso | NeckHead | HandFront / HandBack |
|---|---|---|---|
| 0 | 0 | 0 | 0 |
| 1 | +1 | −1 (fica parado) | −1 (ficam paradas) |
| 2 | +1 | 0 | 0 |
| 3 | +1 | 0 | 0 |
| 4 | 0 | +1 | +1 |
| 5 | 0 | 0 | 0 |

Os valores são `localPosition.y` em px (y para cima), a dividir pelo PPU. Também estão em `rig_manifest.json → idle.frames[].local_offset_unity_y`.

Piscada: ligar `eyelid_closed` por 1 frame (~120 ms) a cada 3 a 5 loops.

### Diferenças em relação ao idle aprovado: 20 / 23 / 23 / 4 px nos frames 1–4

Veja `previews/idle_differences_detail.png` (ampliado, com marcação) e `previews/idle_comparison.gif`.

- **O que são:** só pixels de **contorno**, 1 px cada:
  - na borda do punho da frente;
  - na fenda entre o braço da frente e a aba;
  - no contorno da calça ao lado da luva de trás.
- **O que não muda:** a cor, a silhueta, o rosto, o capacete, os bolsos, o cinto e as luvas são idênticos.
- **Por que acontecem:** no pacote antigo, alguns contornos estavam na peça errada. Isso não aparecia em 1 px de idle, mas viraria defeito no rig:
  1. **Contorno da calça dentro do antebraço de trás.** No idle antigo ele subia junto com o braço. No rig ele é da coxa e fica parado. Se ficasse no antebraço, ao balançar o braço um pedaço de contorno da calça "voaria" junto com a mão.
  2. **Contorno da luva da frente dentro da perna.** No idle antigo ele ficava parado enquanto a mão subia. No rig ele acompanha a mão. Na perna, ficaria flutuando ao lado da coxa quando a mão se movesse.
  3. **Contorno da aba/coxa dentro do braço da frente** (fenda do braço). No rig ele pertence ao corpo, para a aba não perder o contorno quando o braço sai da frente.
  4. **Subpintura antiga das coxas sob as mãos.** Eram sobras de 3 px que só apareciam no idle. No rig, elas ficariam soltas fora da silhueta quando a mão sai. Foram removidas, e por isso somem alguns pixels de contorno nos frames em que a mão sobe.
- **Teste:** dá para chegar a 0 px recolocando esses contornos nas peças antigas. O teste foi feito, e com isso aparecem pedaços soltos nos testes de flexão. A subpintura que fica sob a peça que a cobre e se move junto (tronco sob a cabeça, manga sob a luva) foi mantida, porque não aparece ao dobrar.
- **Alternativa:** se você preferir o idle 100% idêntico, a Unity pode tocar os frames originais de `idle/` por troca de sprite e usar o rig só para as outras animações.

---

## 9. Limites de rotação sugeridos

Use ângulos inteiros. Fora desses limites, a peça ainda funciona, mas a sobreposição deixa de cobrir tudo.

| nó | mín | máx | observação |
|---|---|---|---|
| ArmUpper (ombro) | −60° | +60° | Acima disso a cabeça do ombro mostra o corte |
| ArmLower (cotovelo) | 0° | +110° | Só dobra para a frente (positivo = anti-horário). Negativo hiperestende o cotovelo |
| Hand | −20° | +20° | |
| LegUpper (quadril) | −45° | +45° | |
| LegLower (joelho) | −110° | 0° | Só dobra para trás (negativo = horário) |
| Boot | −30° | +25° | |
| Torso | −8° | +8° | |
| NeckHead | −6° | +6° | |

---

## 10. Cuidados (pixel art na Unity)

- **Rotação com filtro Point:** fora de 0°/90°, a Unity amostra os pixels por vizinho mais próximo. O contorno pode ficar "pontilhado" ou serrilhado em algumas linhas. Isso é inerente a girar pixel art e **não** é defeito dos PNGs; os testes de flexão foram renderizados exatamente assim, para mostrar o resultado real.
  - Para minimizar: Pixel Perfect Camera, ângulos inteiros e preferência por ângulos de pose com valores "limpos" (por exemplo 15°, 30°, 45°, 90°) nas poses-chave.
- **Não usar** Bilinear, mipmaps, compressão nem Sprite Skin ou deformação de malha nestas peças.
- **Não mudar a ordem relativa** das peças `ring_*`, `pelvis_under` e `torso_side_back` (seção 5).
- O braço e a perna de trás estão **atrás** do tronco e da perna da frente. Em uma animação que troque o lado do corpo, troque o sortingOrder pelo conjunto inteiro (braço + anel + antebraço + mão), nunca por peça avulsa.

---

## 11. Mapeamento dos módulos antigos → novos

| antigo (Croc_Idle_Modular) | novo |
|---|---|
| arm_front | arm_front_upper + arm_front_lower (+ ring_elbow_front) |
| arm_back | arm_back_upper + arm_back_lower (+ ring_elbow_back) |
| leg_front | leg_front_upper + leg_front_lower (+ ring_knee_front) |
| leg_back | leg_back_upper + leg_back_lower (+ ring_knee_back) |
| hips | hips + pelvis_under (novo) |
| torso | torso + torso_side_back (novo) |
| hand_front, hand_back, boot_front, boot_back, head, helmet, eyelid_closed | mesmo nome |

Os nomes dos arquivos perderam o prefixo `croc_idle_` (ex.: `modules/head.png`).

---

## 12. Ferramentas (`tools/`)

Scripts Python usados para gerar e validar o pacote. Não são código de jogo.

| arquivo | função |
|---|---|
| `rigprep.py` | Divide as peças e cria as áreas ocultas. Confere repouso == master |
| `rigtest.py` | Simula a hierarquia de Transforms com amostragem Point. Gera as comparações de repouso e idle |
| `bendtest.py` | Gera os testes de flexão |
| `export_rig.py` | Exporta os PNGs, o manifesto e as previews |
| `idle_diff_detail.py` | Gera o detalhe das diferenças do idle |
| `verify_manifest.py` | Monta o personagem usando só o manifesto e compara com o master |
