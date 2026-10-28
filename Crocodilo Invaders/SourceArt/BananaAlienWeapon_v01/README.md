# Arma alienígena de banana — arte separada v01

Solicitada em 20/10/2026 como uma arma que o macaco usará posteriormente: formato de banana, casca abre na frente, banana é disparada, arma recua e volta fechando a casca para repetir o movimento.

Design: casca amarela curva, fendas e luzes verdes, colar/empunhadura roxos e talo marrom atrás. Disparo para a **esquerda**, mesma direção geral do boss. Nenhum personagem ou mão foi incorporado à arma. O projétil é a fruta clara e curva, independente da casca que permanece presa à arma.

## Arquivos finais

- `weapon_idle.png`: arma fechada, PNG transparente 112 × 80 pixels nativos.
- `frames/weapon_00.png` a `weapon_11.png`: 12 posições do ciclo, canvas comum 112 × 80.
- `projectile/banana.png`: banana disparada, PNG separado 24 × 12.
- `spritesheet_6x2.png`: atlas 672 × 160; células 112 × 80, leitura por linhas.
- `animation.json`: ordem, tempos, evento de tiro, deslocamentos de recoil e pontos de encaixe.
- `palette.json`: paleta máxima de 20 cores mais transparência; os exports usam 19 cores opacas.
- `sources/` e `prompts.txt`: fontes de image_gen e prompts completos.
- `qa_report.json`: hashes, quantidade de frames, dimensões, cores e verificação de repetição.

Os exports têm pixels uniformes e alpha binário. As folhas geradas são fontes em resolução maior; usar os PNGs nativos para integração, com filtro Point, sem mipmaps/compressão. PPU 40 é uma sugestão para comparação com o boss em pixel art, ainda sem importação no Editor.

## Ciclo de 1 segundo

1. Fechada e pronta.
2. Ponta da casca começa a separar.
3. Abertura parcial.
4. Abertura completa, evento de disparo no índice 3 (320 ms).
5–7. Recoil para trás, até quatro pixels à direita; boca ainda aberta.
8–11. Retorno gradual e fechamento das três abas.
12. Retorna exatamente à pose inicial. Este último frame é uma cópia intencional do primeiro para a emenda do loop.

A prévia animada compõe o projétil por cima do sprite da arma; ele **não está gravado dentro dos frames da arma**. Cada novo ciclo emite uma banana. A prévia demonstra o comportamento solicitado, sem criar lógica de recarga, combate ou movimento de projéteis no jogo.

## Futuro encaixe na mão

Ponto de pegada nominal: **(72,50)** a partir do topo esquerdo. Pivô normalizado equivalente: **(72/112, 1−50/80)**. Os frames de recoil informam o deslocamento visual e a posição atual da pegada em `hand_socket_px`; ao encaixar posteriormente, a animação da mão pode acompanhar esse recuo. Saída de tiro nominal em (32,34), também informada por frame. O projétil deve nascer uma vez no evento do índice 3, voltado para a esquerda.

Prévia em `Docs/ArtPreviews/BananaAlienWeapon_v01/banana_weapon_fire_x4.gif`, ampliada exatamente 4×; arma sem projétil em `weapon_only_native.gif`; atlas e idle ampliados na mesma pasta. Verificados os exports, paleta, alpha, emenda inicial/final e inspeção visual do tiro e atlas. O GIF pode consolidar quadros visualmente idênticos mantendo o tempo total. Ferramenta de reconstrução: `Tools/Art/package_banana_alien_weapon.cjs`.

Arte e prévia prontas para revisão. Arma ainda não encaixada no macaco, sem prefab, Animator ou lógica de combate.
