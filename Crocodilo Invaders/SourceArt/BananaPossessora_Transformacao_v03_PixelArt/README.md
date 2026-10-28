# Banana possessora — transformação em pixel art, v03

Correção solicitada em 20/10/2026: a v02 tinha acabamento de ilustração; o usuário pediu pixel art de fato. Os 24 desenhos foram redesenhados em image_gen com silhuetas em degraus, formas compactas e áreas de sombra simplificadas, usando o sprite nativo do Goosini como referência. A sequência foi preservada: força e raiva aumentam, arranca/arremessa a ponta da banana e rasga a casca pelo meio. A ponta não reaparece na forma final.

## Especificação final

- 24 PNGs diferentes em `frames/`, **128 × 128 pixels nativos**.
- Paleta fixa compartilhada de **20 cores opacas mais transparência**, em `palette.json`.
- Alpha somente 0 ou 255; sem pixels suavizados, gradientes contínuos, dithering ou interpolação.
- Posições inteiras e pés alinhados; pivô comum (64,120), a partir do canto superior esquerdo.
- Duração 4,22 s, mesmos tempos e etapas da v02; `animation.json` contém ordem e tempos.
- `spritesheet_8x3.png`: atlas 1024 × 384, células 128 × 128, leitura por linhas.

`sources/` contém as quatro folhas do redesenho, anteriores à exportação nativa e à unificação de paleta. `prompts.txt` preserva os quatro prompts completos. **Os sprites finais são os PNGs nativos de `frames/`, não as folhas-fonte ampliadas.** A exportação faz amostragem por vizinho mais próximo e associação a uma paleta fixa; não acrescenta novos desenhos à sequência.

Prévias em `Docs/ArtPreviews/BananaPossessora_Transformacao_v03_PixelArt/`:

- `transformacao_native128.gif`: animação em resolução nativa.
- `transformacao_pixelart_x4.gif`: ampliação exata 4× (512 × 512), sem suavização; cada pixel nativo corresponde a um bloco 4 × 4.
- `contact_sheet_x3.png`: atlas ampliado exatamente 3×.
- `preview.html`: player independente com avanço por quadro, velocidade e repetição opcional; usa os PNGs nativos incorporados.

O valor sugerido de PPU mudou para 40 para compensar a resolução menor ao comparar com a versão anterior. É uma sugestão no manifesto, não uma alteração nos importadores ou prefabs existentes.

Reconstrução com `Tools/Art/package_banana_pixel_v03.cjs`. Verificação: 24 hashes diferentes, 128 × 128 em todos os quadros, somente a paleta comum, alpha binário, GIF com 24 frames; inspeção visual do sprite nativo e atlas. As versões anteriores, poses e cipó foram preservados. Sem integração no combate, Animator ou cenas nesta entrega.
