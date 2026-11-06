# Ícones exclusivos das conquistas — v03

Dez imagens novas, uma para cada conquista do catálogo, geradas individualmente com o `image_gen` integrado. Referências: Croc/nave original, Gusini, banana/Macaquini, Sapeneu e carta mítica reais do projeto. Nenhum ícone utiliza a skin Crocs.

- Fontes imutáveis: `Sources/{id}.png`.
- Prompts completos e referências: `prompts.json`.
- Proveniência das gerações: `generations.json`.
- Exportações usadas pelo jogo: `Assets/Resources/Achievements/Icons_v03/{id}.png`.
- Receita reproduzível: `Tools/Art/prepare_achievement_icons_v03.cjs`.
- Paleta e hashes de entrada/saída: `manifest.json`.
- Prévia: `Docs/ArtPreviews/AchievementIcons_v03/achievement-icons.png`.

Exportação nativa 64×64 RGBA opaca, escala nearest-neighbor, paleta compartilhada, borda discreta. A paleta protege os marrons do punho do Macaquini. Na Unity: Point, Full Rect, 64 PPU, sem compressão/mipmaps. A UI usa os sprites no painel e no popup compacto de 40 px.

| ID | Símbolo |
| --- | --- |
| first_wave | Croc, patente alada e estrela de primeira vitória |
| sport_mode | Nave original em esquiva, rastros ciano e projétil vermelho |
| boss_special | Especial azul acertando o alvo |
| clean_wave | Escudo intacto com emblema de avião |
| gusini_defeated | Gusini de capacete roxo, cachecol e pena |
| macaquini_defeated | Banana aberta, olhos verdes e punho do macaco |
| sapeneu_defeated | Sapeneu no pneu furado |
| all_card_kinds | Leque dos cinco tipos de carta |
| first_mythic | Carta mítica carmesim com estrela |
| last_tooth | Presa de crocodilo e um resto de vida |

`Por Um Dente` continua com interrogação no painel até o desbloqueio. O ícone real só é apresentado depois.

Instalação/validação: `AchievementIconTools.Install`; prévias reais em Play Mode: `AchievementIconPreview.Run`. O instalador também equipa a nave original. A validação garante que todas as cenas do build e todo o conteúdo de Resources estejam sem dependências da skin Crocs, mantendo seus 26 sprites e animações guardados.

Validação final: 16 verificações de Play Mode / zero erros; checkout com 2.717 GUIDs únicos. Build Windows-AchievementIcons-v03 concluído com sucesso (212.479.364 bytes reportados). Inicialização headless verificada por oito segundos sem exceções no log.
