# Cartas Croc v02 — 20/10/2026

O usuário rejeitou a v01: ícones com aparência genérica de IA e molduras sem
diferença suficiente entre raridades. Esta revisão substitui o atlas gerado por
composições dos sprites reais do jogo. Não usa os ícones gerados da v01.

Famílias: Dente de Aço (dano), Chuva de Bala (cadência), Casco Duro (vida),
Pé na Tábua (movimento), Bafo Atômico (especial). Nomes e frases dão personalidade
às cartas; não representam novas mecânicas. A linha de atributo e o percentual
continuam explícitos. Valores, catálogo, probabilidades e gameplay inalterados.

Cinco molduras desenhadas separadamente em pixels:

- Comum: chapa cinza/oliva, cantos cortados, parafusos e desgaste.
- Rara: chassi azul, quatro cantoneiras reforçadas, trilhos ciano.
- Épica: laterais recortadas, circuitos, losangos e cristais roxos.
- Lendária: asas, colunas e estrelas de latão/dourado.
- Mítica: carcaça irregular vermelha, dentes marfim voltados para dentro e coroa.

As diferenças são de silhueta, largura, construção e ornamentação, além da cor.
Verso como identificação militar do Croc usando seu retrato original. Os sprites
fonte são listados em `manifest.json`; permanecem nos caminhos existentes de
Assets, sem duplicação de arquivos fonte nesta pasta. A composição usa recortes
de transparência e escala Point, sem redesenhar os sprites originais. As cartas
exportadas têm 180×268, com os GUIDs/fileIDs e caminhos anteriores preservados.

Reexportar:

```powershell
node 'Crocodilo Invaders/Tools/Art/build_croc_cards_v02.cjs'
```

O exportador v01 continua como histórico e fornece a fonte bitmap compartilhada;
não executá-lo para reconstruir a versão atual. Os 26 PNGs anteriores à primeira
revisão continuam arquivados em `SourceArt/CardsMilitary_v01/originals`.

Prévias em `Docs/ArtPreviews/CardsCroc_v02`: mesma carta nas cinco raridades,
cinco famílias com verso, prancha das 25 cartas. Validação de percentuais contra
catálogo e dimensões realizada pelo exportador, além da conferência visual.
Sem validação no Editor/Play Mode nesta revisão.
