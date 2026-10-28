# Míticas carmesim v10 — 21/10/2026

Revisão das cinco míticas: carmesim, metal escuro e marfim no equipamento; Croc verde e capacete oliva. Equipamentos principais maiores e menos peças secundárias, cápsulas, faíscas e explosões. Boca aberta e expressão insana mantidas. Apenas um pequeno recorte de ombros pode ultrapassar o aro da arte.

Holografia vermelha prismática na arte e escovada na moldura, com intensidades diferentes. Menor intensidade no personagem; placas de texto protegidas. Acabamentos continuam dinâmicos na prévia.

`mythic_edit_target.png`: atlas de edição com as cinco artes anteriores. `mythic_red_atlas.png`: atlas editado pela ferramenta image_gen integrada. `prompt.txt`: instruções usadas. Pastas por família contêm as novas artes nativas. Manifesto com hashes das 25 frentes.

Exportações e prévia: `../../Docs/ArtPreviews/CardCollection_v10/`. Prévia principal: `cartas-miticas-vermelhas.html`. Prancha das míticas: `mythic_families.png`. PNGs de carta contêm arte/textos; a holografia é renderizada na prévia e nas pranchas `rarities_foil.png`.

Reprodução: `node Tools/Art/build_card_collection_v10.cjs`, depois `node Tools/Art/verify_card_collection_v10.cjs`. QA passou para WebGL, troca de família, iluminação, foil desligado, regiões protegidas e larguras 736/360px. As 20 cartas de outras raridades e todas as 25 camadas de texto permanecem idênticas à v09 por hash.

v09 e modelo aprovado v08 preservados. Design/exportação; esta revisão não altera Assets nem o runtime Unity.
