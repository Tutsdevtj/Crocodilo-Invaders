# HUD do Sapo do Pneu — rosto no pneu

A HUD usa somente a cabeça do sapo dentro de um pneu de borracha com sulcos. O reservatório de vida se conecta ao medalhão por madeira molhada, musgo, taboas, vitórias-régias e pequenas ondas, seguindo a paleta do Lago do Pneu. A fonte continua sendo a fonte pixelada do projeto.

A água turquesa recebe doze reflexos animados. A vida carrega na entrada e responde ao dano com espuma, tremor curto e trilha clara. Na fúria, a água pulsa em tons de aqua e o nome ganha um alerta quente. As animações usam tempo escalado e congelam na pausa. O HP vem do componente Health existente.

Arte gerada pela ferramenta integrada image_gen com o sprite aprovado do sapo e uma captura da fase como referências. A master RGBA foi preservada sem edição em `SourceArt/LagoDoPneu_HUD_v05/hud-frame-master.png`, junto do prompt completo. O runtime carrega a cópia de produção `Assets/Resources/LakeHudV05/hud-frame.png`, importada com filtro Point, sem compressão ou mipmaps. O posicionamento da água e do nome é feito no uGUI, sem texto gravado na imagem.

Prévia real: `ArtPreviews/LagoDoPneu_HUD_v05/hud-normal.png`, `gameplay.png` e `lago-hud-v05.mp4`.

Acesso: **FASE 2 · LAGO DO PNEU** ou **DEV → BOSS FINAL → SAPO DO PNEU**. Espaço dispara, Shift usa dash, Esc pausa.

Validação nativa da HUD: 46 verificações de Play Mode sem erros; regressão DEV dos demais bosses: 44 sem erros. Build Windows v05 concluída pelo Unity.
