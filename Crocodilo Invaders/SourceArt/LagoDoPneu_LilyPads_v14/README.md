# Vitórias-régias do lago v14

Seis sprites gerados com a ferramenta integrada image_gen, usando a captura atual do lago como referência de estilo e paleta. Prompt completo em `prompt.txt`.

`lilypads-master.png` preserva a imagem RGBA original gerada. `Assets/Art/LakeStage/LilyPadsV14/lilypads.png` é uma cópia idêntica, com seis recortes nativos no Unity:

1. `lilypad_small_notched`: folha pequena com recorte triangular.
2. `lilypad_large_rim`: vitória-régia grande com borda elevada.
3. `lilypad_nibbled`: folha irregular com marcas na borda.
4. `lilypad_pink_bud`: folha com botão de flor rosa.
5. `lilypad_pink_bloom`: folha com flor rosa aberta.
6. `lilypad_double_cluster`: duas folhas sobrepostas.

Perspectiva baixa, silhuetas horizontais, verdes oliva/musgo e pequenos reflexos turquesa. Fundo transparente. Importação Point, sem mipmaps/compressão, 256 PPU e pivô (0,5; 0,12), próximo ao contato inferior com a água. A folha grande recebe uma região de recorte mais larga para preservar sua borda inteira.

Importador: `LakeLilyPadImport.Install`, usando ISpriteEditorDataProvider com checagem de capacidades e IDs estáveis. Não modifica os pixels nem edita `.meta` manualmente.

Assets preparados para distribuição sobre a água; esta entrega não altera a disposição da cena nem adiciona colisão. Para o esquema atual do lago, uma ordem Default/-175 coloca as folhas acima dos reflexos da superfície e abaixo dos personagens. O mato frontal e a HUD mantêm suas camadas.

Protótipo local. Não commitar, fazer push ou atualizar PR até o usuário finalizar.
