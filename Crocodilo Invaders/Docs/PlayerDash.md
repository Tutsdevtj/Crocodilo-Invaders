# Dash evasivo e VFX do B. Crocodilo

Controle: direção + Shift esquerdo/direito. Segurar Shift não repete. Direção normalizada e fixada no início; sem entrada, usa a última direção válida (inicialmente direita). O input analógico conserva seu vetor real; somente a arte é arredondada para a orientação de 45° mais próxima.

## Valores e Inspector

No componente `PlayerDash` de `Assets/Prefabs/Player.prefab`:

| Campo | Base |
| --- | --- |
| Base Dash Distance | 2,2 unidades de mundo |
| Base Dash Duration | 0,16 s |
| Base Cooldown | 1,1 s desde o início |
| Base Invulnerability Duration | 0,13 s |
| Speed Curve | pesos 2,0 no início, 0,8 em 65%, 0,2 no fim |

Os valores são iniciais para playtest. `Speed Curve` é editável: eixo X = progresso 0–1; Y = peso de velocidade. O perfil padrão tem impulso imediato, queda progressiva e desaceleração curta no fim. A velocidade média é 13,75 unidades/s e o pico inicial fica perto de 24 unidades/s; o controle normal volta no primeiro passo após o movimento.

`SetUpgradeMultipliers(distance, cooldown, invulnerability)` continua recebendo totais relativos às bases. A duração limita os i-frames. Valores são capturados ao começar: mudar um upgrade não altera o dash já em execução. A cena `GusiniBossTest` mantém o kit de teste anterior (distância ×1,3, cooldown ×0,4, proteção ×1,4): nela, são 2,86 unidades, 0,16 s, cooldown 0,44 s e proteção limitada a 0,16 s.

## Física e distância

`PlayerMovement` continua sendo o único escritor de `Rigidbody2D.linearVelocity`. O dash fornece um override, sem teleporte e sem escrever Transform.

Ao iniciar, `PlayerDash` calcula a integral acumulada da curva em 256 intervalos, elimina pesos negativos/não finitos e normaliza a área para 1. Uma curva vazia/sem área usa distribuição uniforme. Cada passo aplica:

`velocidade = direção × distância × (integral(fim) − integral(início)) / fixedDeltaTime`

O último passo usa a fração restante. As diferenças telescopam até a distância inteira, mesmo com duração não múltipla do passo, FPS variável ou outra curva. Obstáculos podem reduzir a distância normalmente.

Teste real encontrou tunneling com o modo Discrete: distância melhorada para 4,4 unidades, física a 30 Hz e obstáculo sólido de 0,03 unidade. Por isso **somente o Rigidbody2D do player** usa `Continuous` agora. Paredes, colliders, velocidade normal e configurações globais da física foram preservados.

## Vida, eventos e sincronização

Eventos reais do componente:

- `DashStarted(Vector2)`: somente após `TryDash` ter sucesso; ignição, preparação do rastro, primeiro snapshot e indicador.
- `InvulnerabilityEnded`: retira somente o contorno; não corta o movimento ou os efeitos residuais.
- `DashEnded`: conclusão natural após a física aplicar o último deslocamento; puff final e fade do rastro.
- `DashCancelled`: interrupção; mantém o cooldown já iniciado.

`Direction`, `ActiveDuration`, `HasInvulnerability`, `IsDashing`, `IsReady`, `CooldownRemaining` e `CooldownFraction` permitem observação sem duplicar estado de gameplay.

O dash usa `Health.SetInvulnerable(this, true/false)`: cada dono libera apenas sua proteção. A API antiga `Health.SetInvulnerable(bool)` continua sendo a proteção geral; liberar o dash não a cancela, nem cancela outro dono. Desativar Health limpa as fontes. O contorno acompanha **os i-frames do dash**, e não fica aceso porque outra proteção ainda está ativa.

`Projectile` e `BossLaser` não foram alterados: contatos esquivados são consumidos/registrados e não cobram dano ao terminar a proteção. `Player`, input e combate preservam seus fluxos, inclusive releases de tiros por Animation Events.

## Assets definitivos do pacote

Fonte: `Dash_VFX.zip`, recebido em 18/10/2026. Foram copiados, sem alteração, apenas **22 PNGs** para `Assets/Art/Player/DashVFX`:

- `dash_ignition`: 4 frames horizontais + 4 diagonais, 20 ms/frame, duração 80 ms.
- `dash_speed_streak`: 3 horizontais + 3 diagonais, 30 ms/frame, loop.
- `dash_end`: 4 horizontais + 4 diagonais, 25 ms/frame, duração 100 ms.

Point, sem compressão/mipmaps, Full Rect, PPU 100 igual ao player, transparência original. Pivôs: ignição (36,36); fim (32,32); rastro H (128,56) no canvas 176×112; rastro D (118,118) em coordenadas Unity no canvas 168×168 — corresponde a (118,50) medido do topo. Foram usados os pixels exatos, sem erro de arredondamento dos valores normalizados.

Guia original, manifesto e hashes SHA-256 estão em `SourceArt/DashVFX`. Nenhuma imagem nova foi criada. Sheets, previews e ferramentas de geração não entram em Assets; referências/capturas locais ficam em `Logs/DashVFX`.

| Direção | Variante | Apresentação |
| --- | --- | --- |
| Direita | h | sem transformação |
| Esquerda | h | flipX |
| Cima | h | +90° |
| Baixo | h | −90° |
| Cima/direita | d | sem transformação |
| Cima/esquerda | d | flipX |
| Baixo/direita | d | flipY |
| Baixo/esquerda | d | flipX + flipY |

## Apresentação

`PlayerDashVFX` observa os eventos, sem escrever movimento, hitboxes ou vida. Referências estão ligadas no prefab real do player. No Inspector, permite ajustar o intervalo/vida dos afterimages e o áudio do dash.

Centro visual: pixel (72,59) do canvas original 160×120, convertido considerando o pivô verdadeiro da sprite. Traseira = centro − direção × raio (56 px horizontal, 47 vertical, 52 diagonal), respeitando a escala do Visual.

- Ignição: um disparo na traseira inicial, em mundo; fica para trás.
- Rastro: filho do Visual, pivô no centro da aeronave; aparece após 20 ms, segue a nave e sai em 40 ms ao terminar.
- Fim: em mundo, na traseira da posição física final.
- Afterimages: pool fixo de seis snapshots; aproximadamente a cada 30 ms, amostrados nos passos da física. Copiam sprite atual, composição, flips, escala e pose. Compensam a interpolação do Rigidbody para registrar posições realmente percorridas, sem inventar caminhos em frames lentos. Não há Rigidbody, collider ou lógica de combate nas cópias.
- Vida dos snapshots: 130 ms; alpha 0,70 → 0. Cores #AAEAFF, #46C4FA, #247CDC por idade; borda #1848A0 de um pixel, brilho superior e 38% inferiores da silhueta um tom abaixo. A faixa inferior é medida pelo alpha original de cada sprite no Editor; os PNGs não são modificados.
- Contorno: `SpriteHitFlash.shader` recebeu propriedades opcionais de dash, desligadas por padrão. Quatro vizinhos do alpha formam um pixel ciano/branco, alternando a cada 40 ms, sem esconder/piscar a aeronave. `MaterialPropertyBlock` preserva o flash de dano e dispensa um material por uso. Outros consumidores do shader continuam iguais.
- Sorting relativo à nave: ignição −3, rastro −2, afterimages −1, nave 0, fim +1. Tiros inimigos existentes (ordens 6, 10 e superiores) ficam acima.

O pool é criado uma vez por player, numa raiz pertencente à cena; não há Instantiate/Destroy por afterimage ou por dash. Há dois slots de ignição e dois de encerramento. Cancelamento manual faz puff/fade; morte, bloqueio de gameplay/cutscene ou desativação limpam tudo imediatamente. Destroy/unload eliminam a raiz mesmo se a ordem de destruição dos objetos mudar. Todos os relógios usam tempo escalado e congelam na pausa.

## Áudio

Não foi encontrado SFX de dash no projeto. `PlayerDashVFX.Dash Sound` fica vazio e configurável, com volume separado. Quando receber um clip, o início bem-sucedido usa uma fonte 2D roteada a Effects pelo sistema existente, respeitando pausa/limpeza. Nenhum áudio foi gerado, substituído ou usado como placeholder.

## Ferramentas e verificação

- `PlayerDashTools.Install`: importa os 22 originais, configura pivôs/material/metadados de silhueta e aplica o prefab. É mutação, não simples check.
- `PlayerDashTools.Validate`: hashes, importação, pivôs, valores e referências.
- `PlayerDashPlayValidation.Run`: PlayerLoop e Physics2D reais com o prefab; cena isolada sem ranking/campanha.
- `PlayerDashPlayValidation.RunVisual`: parte de input, apresentação, capturas e cleanup.

Cobertura: oito direções, 15/30/60/144 FPS, física 30/50/120 Hz, curvas alternativas/área zero, direção analógica, upgrades, pico/cauda, cooldown, proteção independente, i-frames, cancelamento, pausa/retomada, bloqueio/disable/morte, obstáculo fino melhorado e quatro paredes reais da arena, projétil/laser sem dano atrasado, tiro animado, ambos os Shifts e segurar tecla, oito orientações visuais, pivôs, ignição fixa, afterimages, reutilização do pool e unload. As configurações temporárias do Input System usadas pelo teclado virtual são restauradas; nenhum binding foi alterado.

Capturas em Play Mode usam o Céu de Batalha e um leque real de projéteis do Gusini para conferir legibilidade. Logs/capturas ficam em `Logs/DashVFX` e `Logs/PlayerDash*.log`, fora do versionamento. A validação automática e a inspeção visual não substituem o playtest subjetivo da sensação do dash numa run completa.

Resultado na Unity 6000.6.4f1: **284 verificações na bateria completa passaram**, sem erros de compilação, shader ou execução. Uma rodada adicional de input/apresentação passou em **63 verificações**, incluindo segurar Espaço durante o dash, ambos os Shifts e as oito capturas. O checkout validou 1600 GUIDs de assets. Logs finais: `Logs/PlayerDashComplete.log` e `Logs/PlayerDashVisualFinal.log`. Os 22 PNGs continuam idênticos aos originais por SHA-256. Git diff revisado; nenhum commit feito.

Após integrar o WAV do usuário: 69 verificações de input/apresentação/áudio passaram em Play Mode (Logs/PlayerDashAudio.log), incluindo referência real, silêncio antes do input, roteamento Effects, início da reprodução no dash aceito, rejeição durante cooldown e parada ao desativar. Checkout: 1602 GUIDs. WAV confirmado idêntico ao fornecido por SHA-256. Sem escuta crítica ou commit.
