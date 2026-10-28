# Cartas Croc v03 — arcade de combate

O usuário rejeitou a v02 e pediu uma revisão mais marcante. A v03 substitui as
molduras planas e sprites pequenos colados por close-ups expressivos do Croc,
cenas de disparo/impacto e molduras industriais com volume, metal, rebites,
desgaste e luzes embutidas. Remove frases pequenas para abrir espaço à arte.

As cinco molduras são: aço/oliva gasto comum; azul com cantoneiras de prata e
luzes ciano raro; armadura violeta com conduítes de energia épica; bronze/ouro
com ombros alados e estrela lendário; metal carmesim com presas marfim mítico.
As cinco cenas representam dano, cadência, vida, movimento e dano especial.
Cada cena é combinada com cada moldura, totalizando 25 frentes. O verso traz
o Croc de capacete com asas militares. Títulos voltam à identificação direta
ATAQUE, RAJADA, BLINDAGEM, PROPULSÃO e ESPECIAL.

Fonte `arcade_masters.png`: atlas gerado por IA, usando o retrato oficial do
projeto como referência de identidade. Direção solicitada: Croc verde-oliva,
focinho largo, capacete militar, olhos amarelos e dentes marfim; pixel art de
arcade, paleta com contraste, luz em planos e molduras industriais sombreadas;
sem joias/fantasia e sem texto gerado. O master foi reformatado para cartas
verticais mantendo personagem e equipamento. As áreas de título e atributos
foram mantidas vazias para tipografia bitmap determinística no exportador.

`manifest.json` registra recortes do atlas, janela da arte, percentuais e hashes.
`master_0.png` a `master_5.png` são fontes de exportação, fora de Assets. Os PNGs
finais ficam nos mesmos caminhos, com 180×268, importação Point, sem mipmaps e
com GUIDs/fileIDs preservados. Catálogo, prefab, regras, probabilidades e valores
permanecem iguais. Originais anteriores em CardsMilitary_v01/originals.

Reexportar:

```powershell
node 'Crocodilo Invaders/Tools/Art/build_croc_cards_v03.cjs'
```

Conferência visual em `Docs/ArtPreviews/CardsCroc_v03`: mesma carta nas cinco
raridades, cinco famílias com verso e todas as 25 cartas. Percentuais/importação
verificados pelo exportador; validação do checkout passou. Sem Play Mode.
