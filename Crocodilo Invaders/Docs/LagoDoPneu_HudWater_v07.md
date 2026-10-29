# HUD com vida em água — v07

A vida do Sapo do Pneu é um reservatório de água. A corrente percorre toda a largura da moldura e o volume diminui pela altura, com superfície ondulada, reflexos e pequenas bolhas. A moldura ilustrada do lago e o rosto piscando permanecem os mesmos.

Cada perda real de HP libera água pelas duas bordas: jatos curvos se desfazem em gotas com gravidade, enquanto o nível desce gradualmente até a fração de vida atual. Dano maior produz mais gotas e um derramamento mais intenso e duradouro. Receber o mesmo valor de HP novamente não cria outro derramamento. A entrada enche o reservatório sem gerar jatos; após o dano, o fluxo externo para e a corrente interna continua. Pausa congela ondas, nível, jatos e gotas.

`LakeHudWater` desenha a água em um Graphic do uGUI. `LakeHudSpill` usa uma reserva fixa de 64 gotas e malhas de jatos; não cria GameObjects por impacto. `LakeBossBar` recebe o Health e coordena nível, volume perdido e tempo escalado. A arte aprovada continua sem alterações e os efeitos não interceptam comandos.

Play Mode pela entrada real: **59 verificações, zero erros**. A validação cobre corrente em movimento, jatos por dano, nível diminuindo gradualmente, volume proporcional, pausa durante derramamento e encerramento do fluxo, além de combate, cenário e piscadas.

Prévia em `ArtPreviews/LagoDoPneu_HudWater_v07`: captura `hud-spill.png`, gameplay, piscada, vídeo completo e vídeo ampliado da HUD.

Build: `Builds/Windows-LagoDoPneu-HudWater-v07/CrocodiloInvaders.exe`. Pacote: `Builds/Pacotes/CrocodiloInvaders-LagoDoPneu-HudWater-v07-Windows.zip`. Acesso **FASE 2 · LAGO DO PNEU** ou **DEV → BOSS FINAL → SAPO DO PNEU**.
