# Macaquini HUD v15 — encaixe do retrato e preenchimento

Correção incremental do prefab existente, preservando as artes e os vínculos da v14.

- Os retratos das duas formas ficam atrás das molduras. Uma abertura elíptica na geometria da imagem UI revela o rosto sem apagar ou redesenhar os pixels da moldura.
- O medidor passa a preencher da esquerda para a direita, encaixado no trilho em 337 × 18 unidades de UI.
- Preenchimento retangular nativo, sem a textura arredondada de botão. Faixas discretas de luz e sombra dão volume à vida verde da banana e âmbar do macaco.
- Rastro de dano curto e apagado, sem a faixa amarela que parecia vida extra.
- A transformação, a pausa, a recarga e os retratos originais seguem preservados.

Instalação incremental: `MacaquiniHudV15Tools.Install`. Teste: `Builds/Windows-Macaquini-HUD-v15/MacaquiniSelva.exe`. J tira 45 HP; K transforma; I ativa proteção; R reinicia.

Artes e prompts originais: `SourceArt/MacaquiniHUD_v14`. Esta correção usa geometria e componentes nativos de UI, sem regenerar o personagem nem as molduras. Evidências reais: `Docs/ArtPreviews/MacaquiniHUD_v15`.

Validação: 66 verificações de luta, preenchimento, ordem dos retratos, abertura da moldura, contenção do rosto, pausa, transformação e derrota passaram sem erros. Checkout: 2377 GUIDs válidos.

A geometria usa coordenadas em pixels do retângulo do sprite, conforme a API [Sprite.OverrideGeometry](https://docs.unity.cn/Documentation/ScriptReference/Sprite.OverrideGeometry.html).
