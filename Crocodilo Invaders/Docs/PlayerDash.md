# Dash do Player

**Controle:** direção de movimento + Shift esquerdo ou direito. Sem direção no
momento do pedido, usa a última direção não-zero; antes do primeiro movimento,
usa direita. A direção é normalizada e fixada no início do dash.

Configuração inicial no `PlayerDash` do prefab compartilhado `Player`:

| Propriedade | Base |
|---|---|
| Dash Distance | 1,5 unidade de mundo |
| Dash Duration | 0,15 s |
| Cooldown | 1,0 s, contado desde o início |
| Invulnerability Duration | 0,11 s |

Na cena, a velocidade normal é 5 unidades/s; o dash deriva 10 unidades/s de
distância/duração. Todos os valores ficam no Inspector. A distância é de mundo,
independente da escala do sprite e das melhorias de velocidade normal.

## Responsabilidades

- `PlayerInputReader`: clona/habilita/desabilita/descarta a action `Gameplay/Dash`;
  `WasPressedThisFrame` pede `Player.TryDash()`. Segurar Shift não repete o dash.
- `Player`: expõe `Dash` e `TryDash`; bloqueio de gameplay, disable e morte
  cancelam o dash. As cenas herdam componente e input pelo prefab.
- `PlayerDash`: direção, duração, cooldown e i-frames. Configura um override de
  velocidade; não acessa Rigidbody, teclado, Animator, HUD ou efeitos.
- `PlayerMovement`: continua sendo o único componente do Player que escreve em
  `Rigidbody2D.linearVelocity`. O override ganha do input normal até ser limpo.
- `Health`: invulnerabilidade genérica, desligada por padrão. `TakeDamage`
  retorna false sem eventos quando invulnerável; cura continua independente.

O dash calcula o override antes do `FixedUpdate` de movimento. O último passo
usa uma fração da velocidade quando necessário, preservando a distância de
0,15 s mesmo com física a cada 0,02 s. A conclusão é observada no passo seguinte;
a posição nunca é teleportada. Paredes podem reduzir o deslocamento real.
As quatro paredes existentes foram preservadas, sem mudanças globais na física.

Duração, i-frames e cooldown usam tempo escalado: pausa congela todos eles.
Cancelar limpa override e invulnerabilidade, mantendo o cooldown já iniciado.
Novos inputs atualizam o movimento normal, mas não redirecionam o dash em curso.

## Dano esquivado

`Projectile` consome um contato com alvo invulnerável, sem emitir `Hit`, aplicar
dano ou produzir feedback de acerto. Isso impede o projétil de ficar sobre o
Player e cobrar dano depois. `BossLaser` registra o contato esquivado no mesmo
conjunto usado para limitar o feixe a um acerto por alvo; `OnTriggerStay2D` não
reaplica dano ao terminar o i-frame. Permanecer no mesmo feixe esquivado é seguro
para aquela ativação. Contato com inimigo já usa apenas `OnTriggerEnter2D`;
sair e entrar novamente representa um novo contato.

## API para melhorias futuras

`BaseDashDistance`/`DashDistance`, `BaseDashDuration`/`DashDuration`,
`BaseCooldown`/`Cooldown` e
`BaseInvulnerabilityDuration`/`InvulnerabilityDuration` distinguem os valores.
`SetUpgradeMultipliers(distance, cooldown, invulnerability)` recebe multiplicadores
totais relativos à base, sem acumular multiplicações a cada chamada. Exemplo:
`SetUpgradeMultipliers(1.2f, .8f, 1.25f)` dá +20% de distância, −20% na duração do
cooldown e +25% no i-frame. Invulnerabilidade é limitada à duração configurada do
dash. Os modificadores valem no próximo dash; não alteram um dash em andamento.

`IsDashing`, `IsReady`, `CooldownRemaining` e `CooldownFraction` permitem consulta.
A fração é 1 ao iniciar o cooldown e 0 ao completar. Não foram criadas cartas,
HUD, cargas extras, animações, sons ou VFX de dash.

Nenhuma configuração manual é necessária no Unity. A integração mantém a
apresentação animada do Bombardiro e seus releases de tiro/especial.

## Validação executada

Editor 6000.6.4f1: actions/bindings antigos preservados, referência estável de Dash,
herança nas duas cenas, collider, apresentação, scripts e referências do Player.
Play Mode real com o prefab, os overrides de escala/velocidade da cena e cópias
das quatro paredes reais: cardinais/diagonal com distância 1,5; fallbacks;
cooldown e Shift segurado; retorno ao movimento normal; pausa pelo PauseController;
i-frames e fim da proteção; projétil consumido e laser sem dano atrasado; dano
normal de laser e vulnerabilidade padrão de inimigo/boss; bloqueio de gameplay,
disable/enable e morte; melhorias existentes e API de multiplicadores; tiro e
especial animados junto ao dash; ambos os Shifts através do Input System.
Scripts descartáveis de geração e validação foram removidos. Não foi executada
uma partida completa para avaliar o balanceamento subjetivo do dash.
