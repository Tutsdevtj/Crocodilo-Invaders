# Macaquini — ataques e efeitos da primeira forma v25

Revisão solicitada após jogar v24: ataques estranhos e efeitos vermelhos feios. Os voadores animados da v24 permanecem integrados.

- **Rajada móvel:** deslocamento vertical a 2,3 u/s (antes 0,7), Fire nativo a 1,45×, dois tiros diretos e um leque final de três bananas a ±18°. A mira é travada novamente antes de cada tiro, com 0,28 s de aviso nos seguintes; cada projétil mantém sua trajetória depois de sair. Velocidade da banana aumentada em 10% na primeira forma, rastro curto dourado/verde e clarão menor. A velocidade do Animator volta a 1 no fim, cancelamento, troca de ataque e transformação.
- **Rasante:** remove a faixa vermelha que causava dano em toda a rota. Agora há pequenos marcadores dourados por 0,9 s, deslocamento físico a 10,5 u/s e cinco traços curtos de vento marfim/verde. Dano de 1 apenas quando o corpo realmente atravessa a nave (varredura com raio 0,72), uma vez por rasante. Aviso e rastro não dão dano. Preserva o rasante acertando os próprios ajudantes.
- **Chuva de bananas (antigo `ExhaustGates`):** três frutas sobem em arco e caem por faixas fixas separadas em 2,1 u. Espera de 1,3/1,55/1,8 s antes das quedas, 5,4 u/s, 1 de dano por contato. Pequenos marcadores mostram as colunas travadas. Lançamento e aviso são inofensivos; nenhuma faixa vermelha ou dano em toda a coluna.
- **Ritmo:** retorno da primeira forma usa um destino, sem a parada intermediária, e intervalo de 0,7 s antes do próximo ciclo. Recuperação e padrões da segunda forma mantidos.
- **Acabamento:** brilho das rachaduras agora é dourado discreto; faíscas menores, mais curtas e menos numerosas. Marcadores e vento são pequenos sprites de pixels gerados no código; a fruta reutiliza a arte existente da banana. Sem novas dependências ou alteração estrutural de prefab/cena.

Código novo: `MacaquiniDashAttack`, `MacaquiniFallingBanana`, `MacaquiniCombatPixels`. O antigo `MacaquiniHeatHazard` permanece como código histórico, sem ser instanciado pelos padrões atuais. Pausa, fim da batalha e transformação limpam/congelam as ameaças pelo ciclo de vida existente.

Validação `MacaquiniBananaV25Validation.Run`: **58 verificações em Play Mode, zero erros**, em 27/10/2026, Unity 6000.6.4f1. Inclui mira renovada, três Animation Events, leque, cadência, ausência de dano antecipado no rasante, dano único por contato, três frutas, escalonamento, pausa, limpeza, asas dos dois voadores, pontuação, esporos e transformação completa. Capturas reais e relatório em `Docs/ArtPreviews/MacaquiniBanana_v25`; log `Logs/JungleStage/Banana25.log`. Os testes comprovam comportamento; balanceamento e preferência visual continuam sujeitos ao teste humano.

Build: `MacaquiniBananaV25Tools.Build`, saída `Builds/Windows-Macaquini-Combate-v25/MacaquiniSelva.exe`. F1 rajada, F2 rasante, F3 chuva de bananas, F8 ajudantes, I proteção, H cura, R reiniciar. O pedido atual prefere abrir o executável local, sem precisar baixar ZIP.

Entrega: build Windows `Succeeded`, checkout com 2472 GUIDs válidos, diff das alterações desta revisão sem erros de whitespace. Executável v25 aberto para teste direto; log do player em `Logs/JungleStage/Banana25-player.log`. Sem commit/push.
