# Feedback de acerto

## O que orientou a escolha

A revisão **Designing Game Feel. A Survey**, de Pichlmair e Johansen, organiza práticas de game feel a partir de mais de 200 fontes. Ela descreve o uso combinado de efeitos breves para comunicar eventos: flash de cor, efeitos no impacto, som e reação ao golpe. O objetivo aqui é tornar o acerto fácil de perceber, mesmo olhando para outro ponto da tela.

Fonte: https://arxiv.org/html/2011.09201v1 — seções III-B e IV-B.

**What Features Influence Impact Feel?**, de Lin e colaboradores, examina comentários de jogadores e compara recursos de feedback em jogos de ação. Discute flash para destacar o alvo, coerência entre som e imagem e diferenças entre ataques leves e pesados. Seus resultados são uma referência de design, não uma prova de que uma duração específica funciona para nosso shooter; o estudo trata principalmente de jogos de luta, beat ’em ups e ARPGs.

Fonte: https://arxiv.org/html/2208.06155v1 — seções III-C e IV.

Minha aplicação dessas referências ao Crocodilo: um acerto deve ter uma resposta visual imediata e uma pequena reação mecânica. O especial deve ser reconhecível por um efeito diferente. Os tempos abaixo são valores iniciais de ajuste, escolhidos considerando o intervalo atual de 0,2 segundo entre tiros. A qualidade subjetiva ainda precisa ser avaliada jogando.

## Comportamento implementado

| Camada | Ajuste inicial | Motivo |
|---|---|---|
| Flash branco | Branco por 0,045 s, retorno à textura em 0,065 s | Confirmar o acerto e devolver rapidamente a leitura da nave |
| Lentidão | 60% da velocidade por 0,10 s, recuperação suave em 0,12 s | Dar peso ao tiro e facilitar a continuidade da mira |
| Impacto normal | Os 8 frames originais de `hit_effect`, a 25 fps | Mostrar o ponto do contato, mantendo a arte do jogo |
| Impacto especial | Os 7 frames originais de `sp_hit_effect`, com 0,06 s por frame | Dar identidade ao ataque especial sem ocupar a tela por muito tempo |
| Som | Som original de dano, acionado pelo evento de dano existente | Sincronizar a confirmação sonora com o acerto |

Um novo acerto reinicia flash e lentidão. A redução de velocidade não se multiplica a cada tiro. A velocidade base, os pontos de patrulha e a cadência de ataque da nave permanecem os mesmos. A morte encerra o movimento e continua usando a animação e o som existentes.

Esta etapa usa reação local ao golpe. Congelar o jogo inteiro em cada acerto atrapalharia a cadência do tiro contínuo e tornaria o feedback dependente de um relógio global. Tremor de câmera e efeitos maiores podem ser avaliados depois para eventos fortes, como morte de boss.

## Responsabilidades e arquivos

- `Enemy`: conecta o evento de dano à reação do movimento.
- `EnemyMovement`: aplica e recupera o multiplicador temporário de velocidade, tanto na entrada quanto na patrulha.
- `SpriteHitFlash`: observa o dano e controla apenas o branco do sprite.
- `SpriteHitFlash.shader` e seu material: tornam os pixels visíveis brancos, preservando a transparência. Usar apenas `SpriteRenderer.color = Color.white` não produziria esse efeito sobre uma textura colorida.
- `Projectile`: comunica um acerto aceito e sua posição, depois da aplicação de dano.
- `ProjectileImpact`: observa o acerto e instancia o prefab visual correspondente.
- `OneShotEffect`: remove o efeito depois da animação.
- Prefabs `Enemy`, `NormalProjectile` e `SpecialProjectile`: já recebem os componentes e referências necessários. As instâncias existentes na cena herdam as alterações.

O flash usa `MaterialPropertyBlock`: cada nave tem sua intensidade, compartilhando um material. O projeto atual usa o renderizador Built-in; numa eventual migração para URP, o shader e o uso desses blocos devem ser revisados para o novo pipeline.

Documentação: https://docs.unity3d.com/6000.0/Documentation/ScriptReference/MaterialPropertyBlock.html

As texturas originais em `Resources/Art` permanecem disponíveis para o port com `GUI.DrawTexture`. O efeito normal ganhou cópias como sprites em `Art/Effects`; o especial usa os arquivos já existentes em `Art/Player/Projectiles/sp_hit_effect`, importados como sprites. A infraestrutura de importação do port não foi alterada.

## Como ajustar

No prefab `Enemy`, altere **Hit Speed Multiplier**, **Hit Slow Duration** e **Hit Recovery Duration** no `EnemyMovement`. Para uma reação mais leve, aumente o multiplicador para 0,75. Para um impacto mais pesado, reduza-o, mantendo uma duração curta.

No mesmo prefab, ajuste **Hold Duration** e **Fade Duration** no `SpriteHitFlash`. Eles devem terminar antes do próximo tiro normal para que cada acerto continue legível.

Os prefabs em `Prefabs/Effects` controlam escala, animação e duração dos impactos. A duração de `OneShotEffect` acompanha o tempo da animação, com 0,02 s de margem; ao mudar a velocidade do clip, ajuste também essa duração.

## Fumaça de avaria

O prefab `Enemy` também tem um filho `DamageSmoke`, ancorado no casco. `EnemyPresentation` observa `HealthChanged` e mostra a fumaça quando a nave está viva e com 25% de vida ou menos (1 dos 4 HP atuais). Cura acima desse limite e morte fazem o efeito desaparecer suavemente. Desativar a apresentação limpa o efeito imediatamente.

A folha `Art/Effects/EnemySmoke/fire_smoke.png` foi copiada do arquivo fornecido pelo usuário. Seus 32 frames são recortados pelo importador em uma grade de 8 colunas e 4 linhas. Cada célula tem 128 × 256 pixels; os 40 pixels inferiores são excluídos para remover a base laranja. A imagem original não precisa ser editada. A animação usa 24 fps em loop, com início aleatório por nave.

O prefab reutilizável `Prefabs/Effects/DamageSmoke.prefab` contém apenas `SpriteRenderer` e `Animator`. A fumaça é desenhada atrás do casco, com pivô na base e inclinação de 18°. No `EnemyPresentation`, **Critical Health Threshold**, **Smoke Opacity** e **Smoke Fade Duration** ajustam o limite, a transparência e a duração da transição. O filho no prefab `Enemy` permite ajustar o ponto de saída.

## Escurecimento e rachaduras

`DamageAppearance` observa a vida e cuida apenas da cor do casco e da rachadura. A proporção usada é `CurrentHealth / MaxHealth`; nenhum limite depende de uma quantidade absoluta de HP.

No prefab `Enemy`, os valores iniciais são:

- **Darkening Start Health = 0,75**: começa a escurecer abaixo de 75% de vida. Quanto menor a porcentagem, maior o escurecimento.
- **Maximum Darkening = 0,45**: define a intensidade máxima. A transparência original do corpo é preservada.
- **Cracks Health Threshold = 0,50**: mostra a rachadura com 50% de vida ou menos.
- **Cracks Size Limit**: limita largura e altura da arte no domo, mantendo sua proporção.

Cada nave sorteia uma das duas artes em `Art/Enemies/Damage` uma única vez ao nascer. Um único `SpriteRenderer` no filho `DamageOverlay` exibe a arte escolhida; as duas nunca são exibidas juntas. Cura e novos acertos não refazem o sorteio. A variação visual usa um gerador separado e não muda os sorteios de spawn ou patrulha.

Os PNGs fornecidos permanecem intactos; as margens transparentes são recortadas pelo importador de sprites. A animação `Animations/Enemies/Flight.anim` ganhou curvas de posição para `DamageOverlay`, alinhadas ao domo em cada frame. Os sprites e tempos originais de voo foram mantidos. Esse nome de filho deve ser preservado para as curvas continuarem encontrando seu alvo.

O overlay compartilha o material do casco e acompanha seu flash de acerto. Cura acima do limite esconde a rachadura e recupera a cor proporcionalmente à nova vida. Morte remove o overlay e restaura o tint original para a animação de morte. A fumaça continua usando seu limite independente de 25%.

Validação em uma cópia isolada da Unity: naves com 4, 20 e 100 HP, escurecimento por porcentagem, escolha única das duas variantes, cura sem novo sorteio, flash compartilhado, alinhamento animado, pausa, desativação/reativação e morte.
