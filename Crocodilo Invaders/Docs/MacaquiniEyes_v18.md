# Macaquini — olhos totalmente verdes v18

Corrigida a área branca dos olhos nos 12 quadros da transformação em que o macaco vira o rosto ou olha para cima (28 a 39). A esclera e os brilhos neutros agora são verdes; contornos pretos e íris original permanecem. Foram revisados 34 quadros da transformação e 24 do cipó. O ciclo do cipó já não apresentava essa área branca.

Correção localizada de paleta, sem redesenhar o personagem: preserva peito claro da v17, poses, pixels fora dos olhos, alfa, dimensões, pivôs e 80 PPU. Fontes anteriores permanecem intactas. Novos sprites em `Assets/Art/JungleStage/Macaquini_v18`.

Referência de cor via modo integrado `image_gen.imagegen`, com prompt em `SourceArt/MacaquiniEyes_v18/prompt.json`. O arquivo `processing.json` registra os índices dos pixels corrigidos e hashes. Exportação reproduzível: `Tools/Art/prepare_macaquini_eyes_v18.cjs`; instalação nativa: `MacaquiniEyesV18Tools.Install`.

Comparação: `Docs/ArtPreviews/MacaquiniEyes_v18/olhos-antes-depois.png` (antes em cima, depois embaixo). Gravação da luta e validação no mesmo diretório. Teste: `Builds/Windows-Macaquini-Olhos-v18/MacaquiniSelva.exe`; K transforma, I ativa proteção, R reinicia.
