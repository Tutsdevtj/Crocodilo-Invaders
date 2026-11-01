# Lago do Pneu — correções v26

Trabalho local, sem novos commits/push ou atualização da PR.

- Piranhas residentes: dez peixes nadam continuamente antes da aproximação. Os próximos viram e perseguem a nave, saltando da posição real quando ela chega à superfície; no fundo, nadam até ela e mordem. Sem marca de mira. Subir encerra a perseguição, sem esconder peixes. Saltos de ambientação individuais a cada 10–16 s, afastados da nave.
- Sapeneu: os saltos só aterrissam do meio para a direita. Mantém espaço livre para movimentar a nave.
- Insetos: oito novos quadros de voo para a direita, virada com inclinação e troca de lado. Mosquitos grandes voam durante a espera na fila; só assumem aviso curto quando recebem sua vez, com asas animadas.
- Pneu: posição definida antes de criar o Rigidbody2D, tamanho constante entre mão e voo, levantamento mais visível de 1,05 s. Projétil nasce no último ponto da mão.
- Fúria: nova expressão furiosa registrada sobre a master, corpo tremendo e aviso na HUD por 1,15 s antes da Saperereca saltar. Expressão permanece após a entrada.
- Saperereca: olhos na superfície acompanham o deslocamento e mostram a origem da emergência. Removidos os círculos azuis e aviso por partículas. Preparação de 0,4 s após emergir; três gotas compactas, mesma arte/tamanho e arco dos sapinhos, espaçadas por 0,16 s.
- Besouros: investida mais longa/alta, duas gotas direcionadas durante o salto, retorno à folha antes de abrir o casco. Proteção frontal reduzida para 55% do dano, abertura recebe 1,8x.

Os ataques da horda continuam coordenados: até dois pedidos na fila, apenas um ataque liberado por vez; novas investidas aguardam as gotas terminarem. Arte e testes da selva/Macaquini preservados.

## Testar

Menu FASE 2 · LAGO DO PNEU percorre as nove hordas até o boss. DEV → BOSS FINAL → SAPO DO PNEU entra diretamente no Sapeneu. Shift atravessa os ataques com dash. Descer na água aproxima as piranhas; subir afasta a perseguição.

## Fontes e validação

Novas masters/prompts em SourceArt/LagoDoPneu_Rework_v26. Importador nativo LakeReworkArtTools.Install; olhos recortados diretamente do desenho existente da Saperereca, sem inventar um símbolo de aviso.

Validações nativas LakeReworkBossValidation e LakeReworkJourneyValidation cobrem dano real das piranhas, continuidade do pneu, limite do salto, fúria, emergência/tiros, viradas, fila, besouros e pausa/limpeza. As capturas usam a cena e os controladores reais, com proteção nos trechos de inspeção. O balanceamento depende do teste do jogador.


## Referências técnicas consultadas

Fluxo das skills unity-cli e create-2d-physics, incluindo references/legacy-2d-physics.md, para manter Rigidbody2D e movimentação no passo de física. Documentação oficial: [MovePosition](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/rigidbody2d/moveposition) e [position](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/rigidbody2d/position).

Os oito quadros de virada e a expressão de raiva foram produzidos com a skill imagegen; nenhum PNG de produção foi pintado por scripts.


Validação concluída: 25 verificações do boss/fauna + 10 das criaturas + 75 do percurso completo = 110 verificações, zero erros. A travessia percorreu as nove hordas e nove recompensas até a entrada real do boss. Checkout: 2626 GUIDs válidos. Vídeo nativo combinado em Docs/ArtPreviews/LagoDoPneu_Rework_v26/lago-correcoes-v26.mp4, decodificado integralmente sem erros.

Build Windows-LagoDoPneu-Rework-v26 concluída com sucesso (207224194 bytes de conteúdo informado pela Unity). Pacote TESTE-LOCAL inclui jogo e vídeo; não representa aprovação de dificuldade pelo jogador.
