# Revisão completa das molduras — v14

## Causa e correção

A composição anterior substituía somente o retângulo de arte de 132×160 pixels
em uma base de 180×268. A abertura da moldura era maior. As tiras laterais e
horizontais continuavam contendo a ilustração antiga, causando a aparência de
duas cartas sobrepostas. Corrigir a tipografia não alterava essas tiras.

As 25 cartas agora recompõem toda a abertura a partir de uma única ilustração
v12, com um contorno chanfrado contínuo por raridade. A imagem foi ajustada à
abertura completa; nenhum fragmento da imagem antiga é reaproveitado sob o aro.
Nas míticas, somente o recorte existente de equipamento pode passar por cima
da moldura; ele recebe o mesmo ajuste geométrico para continuar alinhado à arte.

Textos v13, expressões, molduras externas, valores, versos, sons e efeitos de
raridade permanecem. Os limites da correção e as cinco paletas estão no gerador.

## Auditoria das 25 cartas

- Ataque: comum, rara, épica, lendária e mítica revisadas.
- Rajada: comum, rara, épica, lendária e mítica revisadas.
- Blindagem: comum, rara, épica, lendária e mítica revisadas.
- Propulsão: comum, rara, épica, lendária e mítica revisadas.
- Especial: comum, rara, épica, lendária e mítica revisadas.

As cinco pranchas finais em `Docs/ArtPreviews/CardCollection_v14` foram conferidas
visualmente. O gerador verifica pixel a pixel que o interior usa apenas a nova
ilustração, o aro usa somente a paleta nova e as mudanças ficam limitadas à
abertura/contorno ou ao equipamento mítico. Camadas de texto e metadados dos
sprites são conferidos por hash. Histórico v13 preservado.

`CardEffectsPlayValidation.Run` também percorre nove ofertas no modo Ciclo das
25, verifica a imagem real de cada slot contra o catálogo, registra as 25
combinações e captura todas as ofertas em `Logs/CardEffects/all_cards_*.png`.
Isso complementa os testes de efeitos, sons, verso, pausa, reroll, escolha e menu;
ofertas aleatórias de cada raridade, sozinhas, não garantiam cobrir a coleção.

Resultado: 94 checks em Play Mode passaram com zero erros do jogo. Cobertura
confirmada das 25 combinações de tipo/raridade, além das verificações de fonte,
moldura, hashes e metadados. Capturas adicionais mantêm 800×600 e 1920×1080.

## Reprodução

Gerador atual: `Tools/Art/build_card_frames_v14.py`.

- Sem argumentos: prepara a revisão, sem escrever nos assets ativos.
- `--apply`: aplica as 25 frentes preservando os metadados.
- `--check`: verifica os arquivos ativos, textos e metadados sem mutação.

Fontes e manifesto em `SourceArt/CardCollection_v14_FrameRepair`. Não reaplicar
os geradores v12/v13 na coleção atual: eles restauram as molduras antigas.
Comparações em `antes-depois-rara.png` e `borda-direita-zoom.png` na pasta da prévia.

Pacote Windows: `Builds/Pacotes/CrocodiloInvaders-DEV-Cartas-v14-Windows.zip`.
