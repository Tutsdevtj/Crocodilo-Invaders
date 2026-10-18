# Crocodilo da base — um frame de 64 × 64

Registro histórico: os assets deste estudo foram retirados de Assets em
18/10/2026, junto da implementação descartada da base. Não é o personagem atual.

Redesenho baseado no retrato fornecido pelo usuário: cabeça de crocodilo
verde escura, focinho largo, olhos âmbar, mandíbula clara e capacete oliva
arredondado. Corpo inteiro, postura relaxada, camisa de serviço com mangas
curtas, bolsos, cinto, calça e coturnos. Sem arma.

## Arquivos finais

- `Assets/Art/Characters/CrocodiloBase64/crocodilo-base-64-pose.png`: PNG
  transparente de **64 × 64 pixels**, um único frame.
- `SourceArt/CrocodiloBase64/crocodilo-base-64-pose.aseprite`: fonte editável,
  **64 × 64**, uma camada e um frame, paleta indexed.
- `Docs/ArtPreviews/crocodilo-base-64-pose-x8.png`: apresentação ampliada
  por nearest-neighbour a **512 × 512**, sobre fundo claro.
- `Docs/ArtPreviews/crocodilo-base-64-pose-transparent-x8.png`: apresentação
  ampliada com transparência preservada.
- `Docs/ArtPreviews/crocodilo-base-64-validation.txt`: dimensões, contagem
  de frames, cores e transparência conferidas no Aseprite.

## Processo

Foi utilizada a ferramenta integrada ImageGen, com o retrato como referência
de identidade. O resultado de geração não era um PNG nativo de 64 × 64; ele
foi salvo como conceito e finalizado no Aseprite com redução por nearest-neighbour,
alpha binário e quantização sem dithering. A leitura da exportação final foi
conferida em ampliação por pixels.

- Conceito original: `SourceArt/CrocodiloBase64/crocodilo-base-64-concept.png`.
- Prompt completo: `SourceArt/CrocodiloBase64/prompt.txt`.
- Finalização reproduzível: `Tools/Art/finalize_crocodilo_base64.lua`.

A arte final tem **30 cores opacas**, **um frame**, **64 × 64 pixels** e
transparência sem pixels semitransparentes. A figura ocupa o retângulo
`(8,3,46,60)`, inteiramente dentro do canvas.

O `.meta` do PNG configura Sprite Single, Point, sem mipmaps, compressão
desativada, 64 pixels por unidade e pivot `(0.5,0.015625)` junto à linha do chão.
Não foi criado AnimationClip nem alterado o prefab do jogador.
