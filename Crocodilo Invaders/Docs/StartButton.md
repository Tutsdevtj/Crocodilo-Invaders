# Botão Iniciar

O popup usa `Assets/Art/Leaderboard/Start.png`, editado a partir do PNG enviado
pelo usuário com a ferramenta integrada de edição de imagens. O losango foi
movido para a direita, com o texto INICIAR à esquerda. O fundo externo preserva
transparência real; o sprite exclui margens transparentes. O filtro Point e
`preserveAspect` mantêm a proporção e o estilo pixelado. A altura é de 77 unidades,
igual à de Voltar, e a largura acompanha a proporção original do sprite.

`Crocodilo → Update Start Button` importa e atualiza apenas esse botão no prefab
PlayerName. A rotina de criação completa do leaderboard também usa o novo asset.

Prompt usado na edição integrada:

> Edit the provided image into a finished transparent PNG UI button for the Unity pixel-art game Crocodilo Invaders. Preserve its wide horizontal stepped gold outer frame, cyan inner border, dark navy blue filled interior, block pixel art visual style and exact cream-white text INICIAR. Change only the arrangement of the interior content: place the word INICIAR on the left/middle and move the single gold diamond icon with its white star/cross highlight to the RIGHT of the word, evenly spaced. Do NOT mirror any letters. The text must read exactly INICIAR, I N I C I A R. Keep the text and diamond vertically centered with balanced inner padding. The diamond should have about the same height as the capital letters. Keep the overall button proportions close to the source, about 3.25:1 width:height for the actual button excluding transparent margins. Preserve crisp pixelated edges and the same colors. Entire button fully visible, minimal transparent padding outside its silhouette. Background outside the button is truly transparent alpha, interior remains opaque dark navy. No extra text, icons or decorations. This is a precise rearrangement of the supplied button, not a redesign.
