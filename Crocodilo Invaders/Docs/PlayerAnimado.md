# Bombardiro Crocodilo animado

O prefab compartilhado `Assets/Prefabs/Player.prefab` mantém física, fachada,
movimento, combate, vida, input, áudio e upgrades no root. O root ganhou Animator
e `PlayerPresentation`; somente o filho `Visual` contém o SpriteRenderer e treme.
As cenas `PlayerMovementTest` e `StageOneWaves` herdam essa apresentação.

Vida inicial nas partidas normais: **20 HP**, configurados no prefab compartilhado
(ajuste de 18/10/2026; antes, 30 HP). Cartas de vida usam essa nova base.
O kit exclusivo de `GusiniBossTest` continua com 240 HP no Editor.

## Arte e Animator

Foram importados os 26 PNGs individuais nativos de
`Bombardiro_Crocodilo_Animacoes.zip`, em `Assets/Art/Player/Bombardiro`.
Todos usam canvas 160 × 120, PPU 100, pivot Unity `(0.45, 1/3)` — pixel `(72,40)`
medido de baixo —, Point, Full Rect, sem compressão nem mipmaps.
Os cinco JSONs originais estão em `SourceArt/BombardiroAnimations`.
Previews, reviews, paleta e versões x4 não entram em Assets.

| Clip / estado | Durações dos frames (ms) | Loop | Release |
|---|---|---|---|
| PlayerIdle / Idle | 140, 200, 140, 200 | Sim | — |
| PlayerFly / Fly | 100, 80, 110, 80, 90, 80 | Sim | — |
| PlayerShoot / Shoot | 50, 70, 70, 90, 110 | Não | Frame 2, 0,12 s |
| PlayerSpecial / Special | 80, 90, 120, 70, 90, 100, 80 | Não | Frame 3, 0,29 s |
| PlayerHurt / Hurt | 50, 80, 120, 110 | Não | — |

Clips e `Player.controller` ficam em `Assets/Animations/Player`. As curvas trocam
somente `Visual/SpriteRenderer.sprite` nos tempos cumulativos dos JSONs.
A grade de edição de 1000 fps permite precisão de milissegundos; os frames não
foram convertidos para FPS uniforme. Root Motion está desligado.

Parâmetros: bool `Moving`; triggers `Shoot`, `Special`, `Hurt`; float `ShootSpeed`.
Idle é default. `Moving` observa a velocidade real do Rigidbody2D. One-shots
voltam a Idle/Fly conforme essa velocidade, com transições imediatas.
`ShootSpeed` acelera somente Shoot quando necessário para preservar a cadência
das melhorias de ataque; o intervalo base de 0,45 s não mudou.

## Combate e dano

`PlayerCombat` valida o ataque e emite `ShotStarted` ou `SpecialStarted`.
Animation Events chamam a ponte `PlayerPresentation`, e o combate cria o
projétil. `ShotFired`/`SpecialFired` continuam significando criação real, mantendo
o áudio existente sincronizado. Há no máximo um ataque pendente. O especial
consome a carga somente no release; morte/disable cancelam releases pendentes.
Input simultâneo dá prioridade ao especial carregado antes do tiro contínuo.

`Health.Damaged` inicia flash e shake imediatamente. Se houver release pendente,
a pose Hurt aguarda esse release para não eliminar um ataque aceito.
`SpriteHitFlash` reutiliza shader, material e MaterialPropertyBlock existentes;
agora aceita renderer serializado, com fallback root/filho no Awake para manter
inimigos e boss compatíveis. O áudio de dano existente foi preservado.

O shake dura 0,10 s em tempo não escalado: −2, +2, −1 pixels da arte, depois
restaura exatamente a posição base. Novos hits reiniciam o efeito sem somar
offsets; disable/morte também restauram. Apenas `Visual.localPosition` muda.
Nenhuma curva anima transforms, e o shake não escreve na física nem nas origens.

O Visual usa escala uniforme `100/116 ≈ 0,86207` e offset Y `−0,176724`, para
manter o corpo aparente próximo dos antigos 100 × 83 pixels. Root, Rigidbody,
collider, velocidade e seus overrides nas cenas foram preservados.
As origens continuam filhas do root, alinhadas pelos pontos dos JSONs:
ShotOrigin `(0,491379; −0,168103; 0)`; SpecialOrigin `(0,5; −0,168103; 0)`.

## Validação e ajustes

Validação nativa no Editor 6000.6.4f1: importação dos 26 sprites, curvas/timings,
eventos, controller, herança nas duas cenas, referências e scripts do Player.
Play Mode real: Idle/Fly, releases e poses correspondentes, dano durante ataques,
flash do Player/inimigo/boss, shake sem mover física/origens, hits repetidos,
pausa, disable/enable, morte, carga única e tiro contínuo com ataque 3×.
Isso não substitui uma partida completa para avaliar a sensação da nova arte.

Nenhuma configuração manual é necessária. Ajustes futuros opcionais: animação
de morte e avaliação visual da hitbox durante uma partida, sem alterar a física
automaticamente. Os scripts descartáveis de geração e teste foram removidos.
