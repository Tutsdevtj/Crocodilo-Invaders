# Expressões por raridade v12 — 21/10/2026

Progressão de expressão nas cinco famílias: comum confiante, raro irritado com mandíbula cerrada, épico furioso em rosnado, lendário quase insano num berro, mítico no auge em gargalhada aberta. Os dez extremos comum/mítico são preservados integralmente; quinze faces intermediárias revisadas.

Cada família possui uma faixa-fonte de três expressões `*_target.png` (raro/épico/lendário, da esquerda para direita) e uma faixa editada com a ferramenta image_gen integrada `*_edited.png`. Prompt exato em `prompt.txt`.

O compositor aplica somente polígonos de rosto/mandíbula/pescoço localizados em `manifest.json`. Para os raros aplica principalmente sobrancelhas/olhos e mantém a boca corrigida da v11 para evitar o retorno da dentição duplicada. Nos épicos/lendários há uma só abertura bucal com mandíbula superior/inferior e dentes pontudos. Todo pixel fora do polígono fica idêntico à v11.

Prévia: `../../Docs/ArtPreviews/CardCollection_v12/cartas-expressoes-progressivas.html`. Exportações PNG, camadas e pranchas no mesmo diretório. Ferramentas `Tools/Art/build_card_collection_v12.cjs` e `Tools/Art/verify_card_collection_v12.cjs`.

25 camadas de texto e máscaras de acabamento preservadas, assim como shader v10/v11 e míticas carmesim. Não modifica Assets/runtime Unity; coleção preparada para revisão e futura integração.
