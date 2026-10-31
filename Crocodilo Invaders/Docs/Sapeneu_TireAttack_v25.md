# Sapeneu Cururu: pneu voador

Novo ataque integrado ao boss da fase 2. Ele ergue um pneu molhado na mão por 0,85 s e o lança girando na posição do jogador no começo do aviso. A trajetória fica fixa, permitindo sair da linha do golpe. Sem círculos de mira.

- Pneu com arte própria, largura de 1,15 unidade e giro de 840°/s.
- Velocidade 10,5 unidades/s; na fúria, 12,5.
- Dano de 2 HP por passagem; atravessar com dash evita o dano.
- Contato com a água gera splash; saída da arena remove o projétil.
- Preparação e voo param na pausa; fúria e fim de combate cancelam o ataque.
- O golpe integra o ciclo do boss e aguarda os ataques anteriores terminarem. Não dispara junto com a rajada da Saperereca.

Para testar: FASE 2 · LAGO DO PNEU, ou DEV → BOSS FINAL → SAPO DO PNEU para chegar diretamente à luta. O pneu aparece depois da língua e do salto iniciais. Controles usuais de movimento, tiro e Shift para dash.

Arte e prompt em `SourceArt/Sapeneu_TireAttack_v25`; importador `LakeTireArtTools.Install`; validação nativa `LakeTirePlayValidation.RunFromMenu`. O teste usa proteção somente nos trechos de inspeção e a remove para testar dano e dash reais. O balanceamento aguarda teste do jogador.

Validação nativa pelo menu real: 27 verificações, zero erros; inclui ciclo natural após língua/salto, mira fixa, giro, pausa, dano único, dash real, interrupção pela fúria, velocidade da fúria, coordenação com Saperereca e limpeza. Verificação do checkout: 2615 GUIDs válidos.

Trabalho local, sem novo commit ou atualização da PR.
