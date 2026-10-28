# Macaquini — emenda superior do cipó v28

Correção visual da segunda forma solicitada pelo usuário: o cipó pintado junto da mão foi preservado; a linha lisa e fina que completava o trecho superior foi substituída.

- Extensão em malha com textura extraída do próprio cipó aprovado (`Macaquini_v11/half_00.png`), mantendo contorno, cor e densidade de pixels. Textura 32×32, Point, sem mipmaps/compressão, repetição vertical; não estica um único pedaço pela altura inteira.
- Pontas, direções de saída e larguras medidas para 68 poses de balanço, transformação e chute. A curva começa alinhada à ponta pintada, com pequena sobreposição para fechar frestas. Não muda mão, desenho, pivô ou anatomia das poses.
- Topo ancorado na copa durante cada trecho contínuo. Removida a oscilação senoidal artificial da extensão. O macaco move a extremidade inferior; o topo não acompanha o corpo.
- Mesma extensão no preparo/retomada do chute e no controle do pêndulo. O pêndulo esconde a extensão normal para não desenhar dois cipós superiores. Soltura, pausa e fim da luta respeitam a visibilidade existente.

Implementação: `MacaquiniVineRibbon`, integrada às linhas de caminho existentes com `forceRenderingOff`, e `MacaquiniFlightPresentation.FillUpperVine`. A linha mantém os dados/visibilidade usados pelo combate, mas a malha texturizada fornece o desenho. As regras de dano e os movimentos dos ataques não foram alterados.

Recursos: `Assets/Resources/Art/MacaquiniVineV28/vine_bark.png` e `joins.json`. Receita `Tools/Art/prepare_macaquini_vine_v28.py`; manifesto e hashes em `SourceArt/MacaquiniVine_v28`. Conferência de SHA256 confirmou que as 68 imagens de origem permaneceram intactas.

Validação `MacaquiniVineV28Validation.Run`: zero erros, com 554 asserções incluindo amostras repetidas ao longo do balanço, encaixe/tangente, copa fixa, deslocamento, pausa, chute, retomada, pêndulo e limpeza. Capturas em 1200×720, 800×600 e 1920×1080, mais 30 quadros reais em `Docs/ArtPreviews/MacaquiniVine_v28`; prévia `encaixe-em-movimento.gif`. Checkout: 2480 GUIDs válidos. Unity 6000.6.4f1, 27/10/2026.

Build: `MacaquiniVineV28Tools.Build`, saída `Builds/Windows-Macaquini-Cipo-v28/MacaquiniSelva.exe`. Entra pela cena de teste da segunda forma; F4 força o pêndulo, F5 o chute e R reinicia. Áudio restaurado anterior às propostas rejeitadas v26/v27. Sem commit/push.

Entrega concluída: build Windows `Succeeded`, executável aberto para teste local. Log do player em `Logs/JungleStage/Vine28-player.log`.
