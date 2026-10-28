# Cartas militares v01 — 20/10/2026

Substituição das 25 frentes e do verso por pedido do usuário: remover espadas,
arcos e estética de fantasia e alinhar tudo ao estilo do jogo.

Direção: militar cartunesco, oliva, marfim, azul-marinho e dourado. Munição/canhão
para dano, metralhadoras e traçantes para cadência, placas de blindagem e cápsula
de saúde para vida máxima, propulsor para movimento, esfera verde carregada para
dano especial. Verso com emblema alado do Croc de capacete e olhos amarelos.
Cinco raridades identificadas por texto, cor e 1–5 marcas de patente. Os títulos
das famílias são visuais; os atributos, valores e chances do catálogo não mudam.

`equipment_atlas_generated.png` é a fonte gerada por IA (3×2 células de 512 px).
Referências fornecidas à geração: `Assets/Art/Portraits/Crocodilo/crocodilo_master_neutral.png`
para personagem/pixel art e `Assets/Art/Leaderboard/Panel.png` para a interface.
Especificação: seis ícones centrados, fundo navy, silhuetas legíveis, equipamento
oliva e efeitos ciano/verde/amarelo, contornos escuros, poucos tons, sem texto,
armas medievais, desfoque ou acabamento pintado. A fonte gerada não é asset runtime.

`icon_0.png` a `icon_5.png`: exportações nativas 64×64, 32 cores por ícone,
exibidas em escala inteira 2× dentro das cartas de 180×268. Moldura e tipografia
bitmap 5×7 são compostas deterministicamente pelo exportador. Transparência
binária nas bordas; filtro Point e sem mipmaps nas importações. Metadados mantêm
os GUIDs e o fileID do sprite existentes, alterando só o retângulo para a tela
inteira; prefab, animações, sons e referências do catálogo são preservados.

`originals/` preserva os 26 PNGs anteriores sem modificações. Não importar esta
pasta em Assets. `manifest.json` registra atributos, valores, cores e hashes.
Prévias ficam em `Docs/ArtPreviews/CardsMilitary_v01/`.

Reexportar na raiz do repositório:

```powershell
node 'Crocodilo Invaders/Tools/Art/build_military_cards.cjs'
```

O exportador verifica os 25 valores contra o catálogo, as dimensões e os recortes
dos 26 sprites. Conferência visual feita nas pranchas exportadas. Não houve
validação em Play Mode nesta revisão (sem Editor/Pipeline acessível).
