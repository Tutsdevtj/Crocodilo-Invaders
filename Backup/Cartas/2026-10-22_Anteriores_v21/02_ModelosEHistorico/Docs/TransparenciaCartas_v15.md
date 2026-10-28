# Transparência externa — v15

As 25 frentes e o verso tinham alpha totalmente opaco, inclusive no fundo escuro
externo aos cantos e recortes da moldura. A v15 transforma esse fundo em alpha zero.
Arte, texto, moldura e áreas escuras internas mantêm os pixels RGB originais.
O contorno de tinta de um pixel junto ao metal é preservado.

O gerador `Tools/Art/build_card_alpha_v15.py` trabalha sobre a coleção v14 e o
verso v13, sem redesenhar os assets. A máscara considera apenas pixels escuros
conectados às bordas na faixa externa; o interior da carta é protegido. A aplicação
é atômica por arquivo. `--apply` aplica e `--check` confere os 26 arquivos ativos
e metadados por hash. Manifesto em `SourceArt/CardCollection_v15_Transparency`.

As pranchas `Docs/ArtPreviews/CardCollection_v15/*-checker.png` mostram as áreas
transparentes sobre um fundo quadriculado. PNGs individuais preservam alpha real.
Som, verso desenhado, efeitos e lógica de recompensa permanecem os mesmos.

O teste de Play Mode percorre novamente as 25 cartas e verifica pausa, reroll,
escolha e retorno ao menu: 94 checks passaram com zero erros do jogo. Capturas
das frentes e do verso foram conferidas com o cenário visível nos recortes.
Pacote atual:
`Builds/Pacotes/CrocodiloInvaders-DEV-Cartas-v15-Windows.zip`.

Não reaplicar os geradores v13/v14 nos assets ativos: restauram o fundo opaco.
