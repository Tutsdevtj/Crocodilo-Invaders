# Linguada com moscas — versão local em teste

**Pedido do usuário: não fazer commits nem publicar este trabalho até ele dizer que está finalizado.** O desenvolvimento parte de `f16c05e`; o histórico e a PR permanecem nessa versão durante os testes locais.

Uma mosca aparece atrás da nave, no lado oposto ao sapo. Ela bate asas e acompanha o jogador durante 0,85 segundo. Depois trava a posição e continua voando com um pequeno movimento local por mais 0,55 segundo, enquanto o sapo abre a boca. Esse é o aviso: não há laser de mira ou projétil escondido.

O sapo lança a língua em 0,18 segundo, captura a mosca e recolhe em 0,26 segundo. A colisão usa apenas a língua visível durante a extensão e causa 2 HP uma vez por golpe. O recolhimento não acerta de novo. Sair da linha após a trava evita o dano; a invulnerabilidade do dash também funciona. A mosca não causa dano, não bloqueia tiros e não tem colisão.

A variação dupla cria duas moscas separadas. A segunda continua acompanhando a nave até ganhar seu próprio aviso e trava, depois da primeira linguada. O intervalo entre os golpes respeita a recarga do dash. Na luta, o sapo alterna saltos e linguadas, com a variante dupla nas alternâncias seguintes e na fúria. O ataque espera todas as gotas perigosas terminarem.

A boca aberta usa uma nova pose com o corpo e o pneu registrados. O reflexo acompanha a troca de pose; piscadas são suspensas enquanto a boca está aberta e retomam em seguida. Pausa congela moscas, língua e HUD. Encerrar a luta recolhe a língua e elimina todas as moscas.

## Assets e implementação

- Fontes e três prompts: `SourceArt/LagoDoPneu_Tongue_v08`.
- Runtime: `Assets/Resources/LakeTongueV08/art.asset`, com seis quadros de voo, pose e ponta da língua.
- Controle: `LakeTongueAttack`, `LakeTongueFly` e integração em `SapoDoPneuCombat`.
- Importação nativa: `LakeTongueArtTools.Install`. O instalador completo do lago também a chama.

## Validação local

45 verificações do ataque pela entrada real DEV, sem erros: mosca atrás da nave, seguimento, asas, aviso inofensivo, abertura da boca, trava, esquiva, 2 HP por golpe, ausência de dano no recolhimento, duas moscas/dois golpes, recarga do dash, invulnerabilidade, pausa, alternância com saltos/gotas e limpeza na derrota do boss ou do jogador.

Regressão das 59 verificações anteriores da fase: zero erros. Total: **104 verificações sem erros**. A pose e a origem da língua também foram verificadas com alvos à direita e à esquerda, incluindo restauração da orientação e golpe fatal durante a extensão.

Prévia: `ArtPreviews/LagoDoPneu_Tongue_v08/lago-tongue-v08.mp4`, `fly-warning.png`, `tongue-strike.png` e `two-flies.png`.

Build local: `Builds/Windows-LagoDoPneu-Tongue-v08/CrocodiloInvaders.exe`. Acesso **DEV → BOSS FINAL → SAPO DO PNEU** ou **FASE 2 · LAGO DO PNEU**. Espaço dispara, Shift usa dash, Esc pausa.

Referências técnicas consultadas: skill `create-2d-physics`, arquivo `references/legacy-2d-physics.md`, API Unity 6000.6 [Physics2D.OverlapCapsule](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Physics2D.OverlapCapsule.html) e [LineRenderer](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/linerenderer).

Build Windows gerada pelo Unity e inicialização de 10 segundos sem exceções. Pacote local: `Builds/Pacotes/CrocodiloInvaders-LagoDoPneu-Tongue-v08-TESTE-LOCAL-Windows.zip`. História Git permanece em `f16c05e`, com índice vazio. Nenhum commit, push ou alteração de PR foi feito para este protótipo.
