# Travessia do Lago do Pneu v22

Arte gerada com a ferramenta integrada `image_gen`, a partir da captura aprovada do lago (`Docs/ArtPreviews/SapeneuCururu_Difficulty_v21/aimed-volley.png`) como referência de estilo e paleta. Masters preservados com alfa original, sem edição raster externa.

- `enemies-master.png`: quatro linhas (mosquito, libélula, sapinho, besouro), quatro poses por linha. Cópia runtime: `Assets/Resources/LakeJourneyV22/enemies.png`.
- `props-master.png`: tronco, pneu abandonado, taboas e flores de lótus. Cópia runtime: `Assets/Resources/LakeJourneyV22/props.png`.
- Gota reutiliza o asset aprovado `Assets/Art/LakeStage/gota-lago-v03.png`, copiado para o mesmo Resources.

`LakeJourneyTools.Install` cria recortes nativos registrados como subassets de `art.asset`, sem reescrever pixels. Importação Point, FullRect, alfa, sem compressão ou mipmaps. As poses compartilham recorte por linha e escala no mundo; adereços usam regiões distintas para preservar o tronco largo.

Prompts completos acompanham estes masters em `prompts.txt`.
