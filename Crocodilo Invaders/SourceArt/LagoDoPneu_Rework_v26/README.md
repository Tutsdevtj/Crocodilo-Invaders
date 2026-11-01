# Fontes visuais — Lago do Pneu v26

Produção com image_gen em 30/10/2026. As masters e prompts completos estão nesta pasta:
- right-wings-master.png / right-wings-prompt.txt: oito cels de voo voltado à direita, quatro do mosquito pequeno e quatro do inseto grande. Referência: Assets/Resources/LakeJourneyV22/enemies.png.
- angry-boss-master.png / angry-boss-prompt.txt: Sapeneu furioso, com a tela, posição e corpo registrados sobre SourceArt/SapoDoPneu_Master_v01/master.png.

As cópias usadas em produção ficam em Assets/Resources/LakeReworkV26. LakeReworkArtTools.Install usa recortes nativos Sprite.Create e Point, sem mipmaps ou compressão, preservando as imagens originais. O frame de raiva herda retângulo, pivô e pixels por unidade da sprite anterior. Os olhos da Saperereca são um recorte do frame existente em SapererecaV12, com pivô na linha da água.

A animação de virada inclina o inseto por 0,3 s e troca para os novos desenhos olhando à direita. As asas continuam animadas enquanto ele espera na fila.

O primeiro resultado de tentativa de virada não foi usado porque mantinha parte dos insetos olhando à esquerda. A master usada nesta pasta é a segunda versão, com os oito desenhos voltados à direita.


**Atualização v27:** o usuário preferiu a suavidade anterior. Os oito cels de voo para a direita desta pasta foram retirados da animação ativa; LakeJourneyEnemy voltou a usar as sprites originais de LakeJourneyV22 espelhadas. A master furiosa continua ativa. Fontes v26 preservadas como histórico.
