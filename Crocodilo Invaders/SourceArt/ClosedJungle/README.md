# Floresta fechada — conceito v01

Data: 19/10/2026. Primeira proposta visual, ainda não aprovada nem integrada à Unity.

Imagem: `../../Docs/ArtPreviews/ClosedJungle/closed-jungle-concept-v01.png`.
Gerada com a ferramenta integrada image_gen; prompt completo em `generation-prompt.txt`.
Não é spritesheet, conjunto de camadas separadas ou imagem garantidamente repetível.

## Direção do cenário

Arena lateral para o combate aéreo de Crocodilo Invaders, dentro de uma floresta tropical fechada. Copa e troncos próximos emolduram a arena; fundo verde-azulado menos contrastado preserva a leitura do avião, do boss e dos projéteis. Raízes, pedras e samambaias concentram-se na base. Cipós pendem de galhos visíveis, com espaço central para manobras.

O conceito usa a identidade cartunesca em pixel art do projeto. A grade e a resolução nativa deverão ser definidas na produção dos assets, sem importar esta composição ampliada como se fosse um pack nativo.

## Próxima montagem proposta

- Fundo opaco: névoa e silhuetas de floresta distante.
- Parallax distante: troncos e copas frias, pouco contraste.
- Plano médio: vegetação com mais definição.
- Moldura próxima: copa, troncos laterais, raízes e chão.
- Cipós decorativos: sprites separados, atrás da área de combate.
- Cipós de interação/ataque: futuros objetos próprios, com origem, aviso e alcance associados à lógica do boss; não confundir com decoração.
- Luz e partículas: efeitos discretos, sem prejudicar a leitura dos ataques.

Na produção, seguir BattleSky: PNGs de runtime separados das fontes e previews, filtro Point, sem mipmaps/compressão, PPU coerente com a resolução nativa escolhida, sorting explícito e pausa respeitada. Não presumir que esta imagem possui camadas ocultas ou bordas sem emenda. Não adicionar colliders ao cenário decorativo.

## Boss e história — sugestões, não decisões finais

Nome de trabalho: **Chimpanizi Bananini — Fúria da Selva**. Preserva a identidade original fornecida pelo usuário; subtítulo ainda é sugestão.

Premissa fornecida: um chimpanzé come uma banana contaminada por uma bactéria deixada pelos alienígenas, é consumido pela transformação e passa a destruir a floresta; Croc precisa salvá-lo. Forma inicial desejada: banana com braços e pernas de chimpanzé. Movimenta-se pendurado em cipós e lança cipós contra Croc, com avisos e sincronização de ataques semelhantes ao Gusini.

Refinamento sugerido: a fruta é uma isca alienígena. A bactéria cria uma simbiose descontrolada que envolve o chimpanzé numa carapaça de banana e provoca surtos de fúria. Ele ainda resiste por breves instantes, dando a Croc motivos para tentar salvá-lo. A contaminação começa a afetar os cipós próximos. Aparência final, fases, cura e desfecho permanecem em aberto.

Nesta etapa foi criado somente o conceito do ambiente. Nenhuma cena, prefab, regra de combate ou progressão foi alterada.
