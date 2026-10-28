# Correção localizada das bocas v11 — 21/10/2026

Revisadas as 25 cartas da coleção v10. Defeito confirmado em Rajada rara, Rajada épica e Propulsão rara: dentição de sorriso humano sobreposta à mandíbula e forma bege com dentes abaixo dela, parecendo uma segunda boca.

Três edições locais feitas com a ferramenta image_gen integrada; instruções exatas em `prompt.txt`. Arquivos `*_target.png` são recortes ampliados das bases v10; `*_edited.png` são as respostas originais da ferramenta. `*_after.png` mostra o recorte final na escala de conferência.

O compositor aplica apenas os polígonos de boca/pescoço descritos no manifesto e verifica que nenhum pixel fora dessas regiões foi alterado. Não utiliza a imagem regenerada inteira. Mantém focinho, olhos, capacete, equipamento, molduras e cenário originais fora das áreas de correção.

Exportações/camadas/PNG/pranchas/QA em `../../Docs/ArtPreviews/CardCollection_v11`. Prévia principal: `cartas-bocas-corrigidas.html`. Scripts: `Tools/Art/build_card_collection_v11.cjs` e `Tools/Art/verify_card_collection_v11.cjs`.

22 cartas preservadas integralmente; todas as 25 camadas de texto e materiais são idênticas à v10 por hash. Shader/foil da v10 preservado. Míticas vermelhas intactas. Fontes anteriores preservadas. Sem mudança em Assets/runtime Unity.
