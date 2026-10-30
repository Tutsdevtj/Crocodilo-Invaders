# Travessia do Lago do Pneu v22

A entrada **FASE 2 · LAGO DO PNEU** do menu inicia agora nove hordas próprias do lago, com as cartas reais entre ondas, antes da apresentação e da luta contra Sapeneu Cururu. O teste direto do boss pelo menu DEV continua disponível.

## Percurso

| Ondas | Região | Introdução e pressão |
|---|---|---|
| 1–3 | Entrada do Brejo | Mosquitos em trajetórias curvas, depois libélulas com aviso e investida de mira travada. Vegetação mais distante e flores sobre a água. |
| 4–6 | Lago Fechado | Sapinhos em troncos nas margens cospem gotas balísticas. A quinta onda ativa a defesa das piranhas na linha da água. A margem ganha taboas. |
| 7–9 | Território do Cururu | Besouros resistentes de frente expõem a barriga depois da investida. Combinações dos quatro papéis, pneus abandonados e aparições breves do Cururu ao fundo. |
| 10 | Sapeneu Cururu | Corrente e deslocamento desaceleram, uma mosca atravessa e um coaxo precede a abertura exclusiva já integrada. |

Contagens-base por onda: **6, 8, 10, 10, 12, 14, 14, 16, 18** inimigos. Cada uma das nove hordas tem três composições sorteáveis, adicionando 0–2 mosquitos sem antecipar tipos novos. Os grupos pequenos se alternam para combinar papéis. Limite de vivos cresce de quatro a sete; intervalo de spawn reduz gradualmente de 2,3–3,1 s para 1,55–2,25 s.

## Inimigos

- **Mosquito de enxame:** 2 HP, 250 pontos; aproximação e trajetória curva em alturas diferentes, dano de contato.
- **Libélula caçadora:** 4 HP, 600 pontos; aviso de 0,8 s com asas abertas, investida de 0,5 s mirando a posição travada e retorno à margem.
- **Sapinho cuspidor:** 5 HP, 650 pontos; apoiado em tronco, infla a garganta antes de cuspir duas gotas (três no trecho final). A trajetória mira a posição avisada, inclusive no alto.
- **Besouro de casco:** 8 HP, 1000 pontos; casco recebe 25% do dano pela frente. Depois de uma investida curta, expõe a barriga por 1,35 s e recebe dano x1,8. Ataques pelo lado de trás recebem dano normal.

Contato e gotas causam 1 HP com intervalo compartilhado de 0,7 s. Usam a proteção nativa da nave e do dash. O diretor permite até duas preparações, segue a ordem em que foram anunciadas e só libera uma investida/rajada de cada vez; gotas em voo também seguram o próximo golpe. Intervalo mínimo entre liberações: 1,18 s.

Revisão visual: removidos os círculos que marcavam o destino dos ataques da fase e da rajada da Saperereca. Os avisos ficam nas poses, cores e animações dos inimigos; as bolhas na superfície continuam indicando a emergência da sapa.

## Arte e ambiente

Folha inédita com 16 poses dos quatro inimigos e atlas de tronco, pneu, taboas e flores gerados por `image_gen`. Masters, prompts e hashes ficam em `SourceArt/LagoDoPneu_Journey_v22`; os PNGs consumidos pelo jogo ficam em `Assets/Resources/LakeJourneyV22`. Importação e recortes são nativos do Unity, com Point, FullRect e sem compressão ou mipmaps. Nenhum pixel foi reprocessado por ferramenta externa.

Doze adereços acompanham a margem, com composição mais densa conforme a região. São decorativos, sem colisão. As seis folhas pequenas, pássaros, peixes, água e vegetação frontal existentes continuam ativos. Os dois apoios grandes rejeitados permanecem removidos.

## Integração e testes

`LakeJourneyTools.Install` cria quatro prefabs em `Assets/Prefabs/Enemies/LakeV22`, importa os sprites e configura somente `StageTwoLake`. Usa `Enemy`, `EnemySpawner`, `RunScore`, `WaveController` e `CardRewardController` existentes. Não altera ondas de Gusini/Macaquini nem a física do jogador. Física 2D legada com `Rigidbody2D` e contato varrido `LakeHit`.

`LakeJourneyPlayValidation.RunFromMenu` percorre o loading de produção, nove hordas e nove escolhas de carta até a entrada do boss; observa os ataques antes de eliminar atores para avançar o teste. O combate é acelerado na validação e o jogador recebe proteção de teste. Isso confere comportamento e fluxo, sem substituir avaliação humana de balanceamento.

Evidências locais: `Logs/LagoDoPneu/v22`, capturas e vídeo em `Docs/ArtPreviews/LagoDoPneu_Journey_v22`. Restore anterior à integração em `Logs/LagoDoPneu/v22/restore`.

Validação nativa: **75 verificações do percurso + 11 de contato, sem erros**. O teste de contato confere gota consumida durante dash sem dano, 1 HP sem proteção, redução do casco pela frente, dano normal atrás, mira travada, abertura do besouro, duas gotas do sapinho e cancelamento na derrota. Checkout: 2604 GUIDs válidos. A revisão final também corrige a divisão das linhas do atlas para separar os chifres do besouro da base dos sapinhos.

Controles: setas, Espaço para tiro normal, C para especial e Shift para dash. Nas cartas, mouse ou navegação de teclado; Esc pausa.

Build local: `Builds/Windows-LagoDoPneu-Journey-v22/CrocodiloInvaders.exe`. O pacote completo fica em `Builds/Pacotes/CrocodiloInvaders-LagoDoPneu-Travessia-v22-TESTE-LOCAL-Windows.zip`. Extrair todos os arquivos e usar **FASE 2 · LAGO DO PNEU** no menu para jogar a travessia, ou o menu DEV para testar apenas o boss. Esta entrega permanece local, sem novo commit ou push.
