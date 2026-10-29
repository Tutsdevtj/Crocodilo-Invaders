# Fontes das piscadas

- `sapo-closed-master.png`: edição da master aprovada `Assets/Art/LakeStage/sapo-master.png`, com olhos fechados.
- `hud-closed-master.png`: edição da moldura `SourceArt/LagoDoPneu_HUD_v05/hud-frame-master.png`, com olhos fechados no retrato.
- `prompt-sapo.txt` e `prompt-hud.txt`: prompts completos usados na ferramenta integrada image_gen.

As imagens RGBA são preservadas integralmente. Cópias idênticas em `Assets/Resources/LakeExpressionsV06` são importadas por `LakeHudArtTools.ImportExpressions` com Point, sem compressão ou mipmaps. O runtime cria sprites somente das regiões dos olhos, com coordenadas normalizadas, sem reexportar nem modificar as imagens geradas. O desenho aberto aprovado permanece como base.
