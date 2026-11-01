# Moscas fixas e linguada rápida — v10 local

**Não fazer commits, push ou atualizar a PR até o usuário dizer que está finalizado.**

Durante a luta há cinco moscas em pontos específicos do mapa e uma única mosca dedicada à posição atrás da nave. A de trás é reposicionada no início de cada sequência; depois o alvo fica no ponto, sem perseguir a esquiva. Os cinco pontos podem ser ajustados no prefab em `stationPositions`, em coordenadas normalizadas da arena.

Todas batem asas no lugar. Somente a escolhida se agita e acelera a batida das asas: esse movimento é o aviso do próximo golpe. As demais mantêm seus pontos, sem círculos de mira ou avisos extras. O sapo seleciona um alvo por vez, captura a mosca e passa para a próxima. As moscas capturadas reaparecem no mesmo ponto após 2,3 segundos, reutilizando os mesmos seis objetos.

## Ritmo

- Aviso de movimento por 0,46 segundo, seguido de 0,16 segundo de estabilização. A boca fica fechada durante esse aviso.
- A boca abre já com a língua saindo, no mesmo passo da física. Extensão de 0,13 segundo, breve contato de 0,04 segundo e recolhimento de 0,19 segundo.
- Intervalo entre golpes próximo de 1,2 segundo com o dash padrão, preservando a oportunidade de reutilizá-lo.
- Sequência natural de três alvos; na fúria, quatro. Só uma mosca se agita por vez, e há apenas uma seleção da mosca de trás por sequência.
- O boss começa pela linguada, alterna com saltos e gotas, e aguarda as gotas terminarem antes de iniciar outro grupo de linguadas.

O dano continua sendo 2 HP por golpe, sem repetir no recolhimento. O dash nativo funciona. Pausa congela moscas, seleção e língua; derrota do jogador ou do boss remove todo o campo. A língua inteira texturizada e o encaixe da v09 permanecem.

## Implementação e revisão

`LakeTongueAttack` mantém o campo de seis moscas e um plano de alvos distintos. `LakeTongueFly` separa a batida de asas ociosa da agitação do aviso. `SapoDoPneuCombat` organiza o ciclo mais rápido. `LakeTongueFieldTools.Apply` salva os cinco pontos pelo Editor nativo.

Prévia real: `ArtPreviews/LagoDoPneu_FlyTargets_v10`. Build local: `Builds/Windows-LagoDoPneu-FlyTargets-v10/CrocodiloInvaders.exe`. Acesso **DEV → BOSS FINAL → SAPO DO PNEU** ou **FASE 2 · LAGO DO PNEU**.

Validação nativa pela entrada real: **44 verificações, zero erros**. Inclui seis objetos reutilizados, um único alvo agitado, cinco pontos fixos, boca fechada durante aviso, disparo no mesmo passo da abertura, intervalo acelerado com dash disponível, dano único, variantes de três/quatro alvos, saltos/gotas e limpeza nas duas derrotas.

Build Windows concluída e inicialização de 10 segundos sem exceções. Pacote local com integridade conferida: `Builds/Pacotes/CrocodiloInvaders-LagoDoPneu-FlyTargets-v10-TESTE-LOCAL-Windows.zip`. Vídeo: `ArtPreviews/LagoDoPneu_FlyTargets_v10/lago-fly-targets-v10.mp4`.

HEAD permanece em `f16c05e`, índice vazio, nenhum commit, push ou atualização de PR.
