# HUD Macaquini — v14

Barra própria da selva em `Assets/Prefabs/UI/BossHUD.prefab`, usada na campanha e na arena de teste. O HUD do Gusini continua independente.

- Forma I: moldura de madeira, bambu, folhas e cipós; seiva verde e retrato da banana infectada com jetpack.
- Forma II: outra moldura, com espinhos, madeira partida, casca rasgada e tons de cobre; seiva âmbar e retrato do macaco furioso gritando.
- Ambos os retratos medem 104 × 104 unidades de UI e são recortes dos sprites existentes. A segunda imagem vem de `Macaquini_v13/transform_29.png`; o personagem não foi redesenhado.
- A troca acompanha o progresso protegido da transformação, entre 24% e 73%, com sobreposição suave das duas imagens e das molduras. Não reinicia a entrada da interface.
- O preenchimento acompanha `Health.Fraction`, com rastro de dano, indicação vermelha abaixo de 30%, pausa e ocultação na derrota. A barra fica vazia durante o rasgo e recarrega só ao começar a segunda forma.

Arte: `SourceArt/MacaquiniHUD_v14`, geração pelo modo integrado `image_gen.imagegen`; exportação reproduzível em `Tools/Art/prepare_macaquini_hud_v14.cjs`. Instalação por API nativa Unity: `MacaquiniHudV14Tools.Install`.

Teste jogável: `Builds/Windows-Macaquini-HUD-v14/MacaquiniSelva.exe`. Abre na arena do Macaquini com save de teste separado. J tira 45 HP; K esgota a forma; I liga proteção; R reinicia; ESC pausa. F1/F2/F3 forçam ataques.

Evidências e gravação da luta em `Docs/ArtPreviews/MacaquiniHUD_v14`. Validação nativa verifica as duas formas, preenchimento de vida, transição intermediária, pausa, recarga e derrota, além dos ataques, animações e objetos descartados. Regressões separadas do Gusini e da campanha são registradas em `Logs/MacaquiniHUD_v14_*.log`.

Resultado: 62 verificações na luta do Macaquini, 49 na regressão do Gusini e 93 na campanha passaram, totalizando 204. Integridade do checkout: 2375 GUIDs válidos. Capturas verificadas em 800×600, 1200×720 e 1920×1080.
