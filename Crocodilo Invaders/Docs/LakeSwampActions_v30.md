# Lago do Pneu — ações v30
31/10/2026. Trabalho local, sem commit/push ou atualização da PR.

- Mosquitos pequenos e grandes morrem esmagados: corpo achatado, impacto curto e sangue vermelho em pixels que se espalha e desaparece. O efeito só nasce na morte, não tem hitbox/vida e mantém a pontuação original. Congela na pausa e termina durante as cartas.
- Mosquitos pequenos passam a perseguir a posição da nave com aceleração e virada amortecidas. Velocidade de 1,4 unidade/s, componente vertical limitada a 0,8; sprites originais e registro dos quadros preservados.
- Sapos e besouros são anunciados por vitórias-régias vazias vindo pela água. Depois da folha chegar, aguardam 0,4 s e saltam de trás da vegetação em arcos de 0,9/1 s. Só ficam vulneráveis, recebem hitbox e começam a preparar ataques depois de pousar; os pés acompanham a folha. A folha nasce já na água, eliminando um quadro na origem do mundo.
- Piranhas saltam até 1,65–2,01 unidades acima da água. A trajetória calcula a passagem pela posição avisada durante a descida. Saltos ambientes a cada 7–11 s (antes 10–16); grupos ofensivos a cada 1,85 s (antes 2,3), com o aviso de 0,65 s preservado. Mordida continua de 1 HP, intervalo compartilhado de 0,65 s. Elas continuam presentes no lago, sem círculo ou teleporte.

Efeitos usam sprites existentes e partículas quadradas geradas pelo código. Sem novas imagens rasterizadas.

Validação nativa na fase real: LakeSwampActionsValidation cobre folha vazia/entrada/pouso, perseguição nos dois sentidos, acerto não fatal, chinelada única, pausa, limpeza e ataques após o pouso. LakePiranhaHighLeapValidation cobre altura, frequência, dano real, pausa e retirada. Percurso completo exercita as nove hordas, cartas e entrada do boss. Capturas/probes usam proteção nos trechos de inspeção e retiram-na nos testes de dano.

Prévia: Docs/ArtPreviews/LakeSwampActions_v30/lago-acoes-v30.mp4. Testar pelo menu FASE 2 · LAGO DO PNEU; DEV → BOSS FINAL → SAPO DO PNEU para conferir as piranhas diretamente no boss. Balanceamento aguarda o jogador experimentar.

Fluxo Unity CLI e create-2d-physics/references/legacy-2d-physics.md: movimento em Rigidbody2D/FixedUpdate e varredura LakeHit existentes preservados.


Resultados: 22 verificações de atores/efeitos + 10 de piranhas + 75 do percurso completo = 107 verificações, zero erros. Nove hordas e nove recompensas chegaram ao boss. Salto de piranha medido em 1,83 unidade acima da água; intervalo ambiente medido em 8,34 s. Checkout com 2628 GUIDs válidos. Vídeos reais inspecionados e decodificados sem erros. A cópia auxiliar do teste completo foi arquivada em Logs/LagoDoPneu/v30/Full/CaptureHelper; as duas validações específicas permanecem em Assets/Editor.

Build Windows-LagoDoPneu-Actions-v30 concluída com sucesso: 207226994 bytes de conteúdo informado pela Unity. Pacote local com jogo e vídeo.
