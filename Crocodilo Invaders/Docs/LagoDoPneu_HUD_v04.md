> Versão visual rejeitada pelo usuário; substituída pela v05. Mantida apenas como registro local.

# HUD do Sapo do Pneu — v04

A fase 2 usa uma apresentação própria para o boss: moldura de borracha com sulcos, medalhão de pneu com o sprite aprovado, detalhes de musgo, taboa e vitória-régia. O nome permanece legível em uma placa escura.

A vida aparece como água turquesa com doze reflexos em movimento. A barra entra deslizando e carregando, reage ao dano com espuma clara e pequeno tremor, e mantém uma trilha de dano antes de esvaziar. Abaixo de 40% de vida, acompanha a fúria do combate com água âmbar pulsante e a indicação `FURIA DO PANTANO`.

`LakeBossBar` recebe os valores do `Health` por meio do `BossHUD`/`BossBarFX` existente. `LakeHudFrame` desenha geometria em pixels no uGUI com `CanvasRenderer`; a identidade visual não depende de novas texturas ou altera a arte aprovada. A composição conserva a ancoragem inferior direita e não intercepta comandos. Tempo escalado mantém as animações congeladas durante a pausa. Os outros bosses continuam com suas apresentações existentes.

## Verificação

- Play Mode pela entrada real DEV da fase: **46 verificações, zero erros**. Inclui criação da HUD, renderizador da moldura, reflexos, nome, HP, estado de fúria e pausa, além do combate completo.
- Regressão do menu e dos demais bosses: **44 verificações, zero erros**.
- Capturas reais: `ArtPreviews/LagoDoPneu_HUD_v04/hud-normal.png` e `gameplay.png`.
- Vídeo real de 12 segundos: `ArtPreviews/LagoDoPneu_HUD_v04/lago-hud-v04.mp4`.

Build: `Builds/Windows-LagoDoPneu-HUD-v04/CrocodiloInvaders.exe`. Acesso: **DEV → BOSS FINAL → SAPO DO PNEU** ou **FASE 2 · LAGO DO PNEU**. Espaço dispara, Shift usa dash, Esc pausa.
