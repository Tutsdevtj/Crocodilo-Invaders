# Patrulheiro, interceptor e artilheiro

Os três usam `Enemy`, `Health`, `EnemyMovement`, `EnemyPresentation`,
`SpriteHitFlash` e `DamageAppearance`. Os ataques recebem um alvo de gameplay;
não leem input e não dependem da HUD.

| Tipo | Vida | Movimento e ataque |
| --- | --- | --- |
| Patrulheiro | 4 | Comportamento anterior: patrulha e tiro mirando a posição do player. |
| Interceptor | 3 | Patrulha rápida; anuncia a trajetória por 0,7 s; atravessa a arena a 10 unidades/s; recupera por 0,4 s e retorna. Contato causa 2 de dano. |
| Artilheiro | 10 | Patrulha lenta; anuncia a mira por 0,9 s; dispara cinco projéteis vermelhos em leque de 48°, a 3,6 unidades/s. |

A investida e o leque travam o destino no início do aviso. Mover-se depois do aviso
permite escapar: não há perseguição durante esses ataques. Acertos desaceleram também
a investida. Os ataques usam o relógio do gameplay e param com a pausa e com o fim da partida.

## Arquivos e responsabilidades

- `EnemyAttack`: base pequena compartilhada pelos três ataques reais. Guarda o alvo,
  verifica se a entidade pode atacar e expõe eventos de disparo e aviso.
- `EnemyCombat`: preserva o tiro do patrulheiro.
- `InterceptorCombat`: decide quando avisar, investir, recuperar e retornar.
- `ArtilleryCombat`: prepara e dispara o leque. Usa o `Projectile` existente.
- `EnemyMovement`: executa patrulha, parada e investida com `Rigidbody2D`. Não decide
  quando atacar nem usa input.
- `EnemyAttackPresentation`: desenha os avisos e troca a animação do motor na investida.
- `EnemyWave`: prefab/quantidade dos grupos, intervalo e limite de vivos; `EnemySpawner` executa a fila;
  `WaveController` continua responsável pela progressão.

Os prefabs novos são `Assets/Prefabs/Enemies/Interceptor.prefab` e
`Assets/Prefabs/Enemies/Artillery.prefab`. O patrulheiro continua no `Enemy.prefab`.

## Ajustes e arte

Velocidades, vida, dano de contato e intervalos são configurados no Inspector dos
prefabs. A área da investida deve acompanhar a área jogável quando as paredes mudarem.
O aviso usa o mesmo destino da movimentação; no artilheiro, `Fan Angle` da apresentação
deve acompanhar `Fan Angle` do ataque.

As naves são Ship1 (verde) e Ship3 (cinza/vermelha) do pack gratuito CraftPix.
PNG original preservado, importação Sprite, Point, sem mipmaps nem compressão.
Casco, motor normal/turbo e explosão são separados em animações da Unity;
não é preciso instalar Spriter. O motor turbo só é usado pelo interceptor.

Escurecimento, flash, fumaça, som de dano e explosão são reaproveitados. As rachaduras
desenhadas para a cúpula do patrulheiro não foram aplicadas nesses cascos diferentes.
A fumaça do interceptor começa em 34% (um HP de três); a do artilheiro em 25%.
Todos continuam dando 800 pontos por abate. A composição atual tem nove hordas,
com interceptor a partir da onda 3 e artilheiro a partir da onda 6.

É um primeiro balanceamento jogável; tempos, quantidade e vida devem ser ajustados
após jogar com os três tipos juntos. O boss é a onda 10 e chama reforços de interceptores.
