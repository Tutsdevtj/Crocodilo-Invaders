# Segunda forma: ritmo e desafio v30

Implementado em 27/10/2026 no projeto ativo `Crocodilo Invaders`.

Aproximações e recuos usam trajetórias suaves, com pequenos arcos e preparação em movimento. Cruzados encadeados mudam o lado de aproximação. A guarda continua entre rodadas, evitando a troca brusca de pose. A recuperação termina perto do jogador em vez de atravessar a arena.

A sacola agora tem seis combinações. Acima de 45% de vida, sequências de até três golpes; com 45% ou menos, até quatro, avisos menores e recuperação de 0,40 s em vez de 0,55 s. Finalizações aparecem a cada três combos, ou dois quando ferido. O intervalo externo entre rodadas cai para 0,08 s. O pêndulo fica mais frequente quando ferido.

O boss intercala socos entre investidas dos ajudantes. A saída inofensiva do ajudante ocorre durante a preparação seguinte. Continua existindo uma única janela perigosa por vez e o intervalo mínimo de 1,14 s, ou recarga real da esquiva + 0,04 s. Dano por golpe preservado. Primeira forma, cipós aprovados e áudio restaurado preservados.

## Validação

- Baseline anterior: 83 verificações nativas, zero erros (`Logs/JungleStage/Flow30-baseline.log`).
- Nova versão: 20 verificações nativas, zero erros, oito rodadas, seis combinações observadas, nenhuma sobreposição de dano. Menor intervalo observado entre ameaças: 1,205 s. Inclui contato real, dano único, esquiva real escapando da mira travada, pausa, mudança de flanco, quatro golpes, alternância dos ajudantes, cancelamento e execução pelo controlador normal.
- Resultado estruturado: `Docs/ArtPreviews/MacaquiniFlow_v30/validation.json`; log `Logs/JungleStage/Flow30.log`.
- Build Windows concluída: `Builds/Windows-Macaquini-Combate-Fluido-v30/MacaquiniSelva.exe`.
- Checkout: 2482 GUIDs válidos; diff dos scripts de combate sem erros de whitespace.

Física existente mantida: Rigidbody2D cinemático e consultas de cápsula ao longo do deslocamento. Referência local consultada: skill `create-2d-physics/references/legacy-2d-physics.md`. APIs oficiais consultadas: [Rigidbody2D.position](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Rigidbody2D-position.html) e [Physics2D.OverlapCapsule](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Physics2D.OverlapCapsule.html).

Restore point dos três scripts anteriores em `Logs/JungleStage/Flow30Before`. Ferramentas: `MacaquiniFlowV30Validation.RunAndBuild` e `MacaquiniFlowV30Tools.Build`. A build abre o teste com transformação automática e 24 HP; F6 força combo, F7 ajudantes, R reinicia. Sem commit/push.
