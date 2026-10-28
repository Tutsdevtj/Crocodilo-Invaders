# Fase 2 da campanha — diagnóstico e proposta

Data: 22/10/2026. Diagnóstico histórico anterior à confirmação do usuário. A direção foi confirmada: nova fase com Macaquini e bichos da selva. A implementação atual está documentada em [Fase 2 — Selva](Fase2Selva.md). As propostas e pendências abaixo registram o momento da inspeção.

## Pedido e interpretação

[F] Pedido: conferir o boss existente e implementar outro boss com outros bichos, uma fase 2.
[?] Confirmar se fase 2 significa capítulo seguinte da campanha e quais bichos devem compor a fase. Pergunta enviada durante a inspeção.
[P] Direção possível: capítulo de selva, Macaquini/Banana como boss, novos inimigos animais. Esta escolha ainda não foi aprovada para este pedido.

Os três guias fornecidos são referências de produção. O gerador de prompts descreve um papel de outra tarefa: não transforma o pedido atual em uma solicitação de prompt. Regras de arte, eventos, reuso e evidências orientam a implementação. Este diagnóstico não presume aprovação nova dos estudos existentes.

## Estado confirmado no código e nos dados

- Projeto ativo: `Crocodilo Invaders/`, Unity 6000.6.4f1. A pasta `unity/` é legado.
- `Gusini.prefab`: boss atual, 258 HP na primeira forma. `GusiniPhaseTwo` prepara 320 HP na segunda forma.
- F1 (`GusiniCombat`): rajada, esquadrilha e ovos. F2 (`GusiniPhaseTwo`): energia, ovos, esquadrilha e tentáculos.
- O esgotamento da F1 inicia transformação; a run não termina ali. A derrota final integra recuperação, diálogos e retorno à base.
- `WaveController` configura hordas via `EnemyWavePool`/`EnemyWave`, oferece cartas entre hordas e instancia o `bossPrefab` ao final. Não precisa de outro controlador de ondas para um capítulo novo.
- `Boss` seleciona um `IBossCombatController`, controla entrada, invulnerabilidade, encerramento e pontuação. Este é o ponto existente para um controlador de boss diferente.
- `ChapterDefinition`, `ChapterFlow` e `CampaignProgress` já permitem destino e marcos de conclusão por capítulo. Hoje existe `Assets/Data/Flow/Chapter1.asset`.
- `BossHUD.Bind` exige `GusiniCombat`; apenas trocar o prefab deixaria outro boss sem barra. A apresentação precisa aceitar o novo boss sem reutilizar indevidamente o retrato do Gusini.
- `RunCutscenes.PrepareBoss` exige `GusiniCombat`; `PlayBossFinale` exige `GusiniDefeat`. Diálogos, transformação e recuperação não são genéricos.
- Inimigos comuns atuais: patrulheiro, interceptor e artilheiro. Novos bichos precisam de arte, prefabs e composições próprios, reaproveitando `Enemy`, `Health`, movimento, spawner, projéteis e pontuação quando aplicáveis.

Fontes: scripts em `Assets/Scripts/{Controllers,Entities,Presentation,Persistence}`, prefab Gusini e documentos `Docs/Boss.md`, `Docs/GusiniFase2.md`, `Docs/BananaPossessora.md`, `Docs/Ondas.md` e `Docs/Inimigos.md`. Valores são leitura do estado atual, não resultado de novo playtest.

## Material do segundo boss já existente

- `SourceArt/BananaPossessora_Fase1_Master_v09_Modular/`: master modular indicada como atual em `Docs/BananaPossessora.md`. Há exports estáticos em `Assets/Art/Boss/BananaPossessora/Fase1/`.
- `SourceArt/BananaPossessora_Transformacao_v03_PixelArt/`: transformação em 24 desenhos nativos de 128 × 128, 4,22 s, conforme documentação. Revisões anteriores são histórico e não fonte automática.
- `SourceArt/BananaAlienWeapon_v04_Organic/`: arma orgânica e projétil separados; versões tecnológicas anteriores não são a direção atual.
- `SourceArt/BananaPhase1_VinesShoot_v01/animation.json`: 24 quadros de 160 × 160, duração 4080 ms; disparo no índice 15, a 2660 ms; projétil separado de 64 × 32.
- Esse mesmo JSON declara `composition_includes_vines: true` e `world_motion_baked_into_frames: true`. Antes de usar cipós independentes e trajetória física, separar as camadas e resolver continuidade/pivôs; copiar a animação inteira para um Transform móvel duplicaria a movimentação.
- Não há prefab de combate da Banana/Macaquini entre os bosses atuais. Arte disponível não equivale a boss pronto.
- `SourceArt/ClosedJungle/README.md`: conceito de floresta ainda não aprovado nem integrado; não é um pack de parallax pronto.

Identidade histórica registrada: banana infectada aprisiona o macaco; Croc deve libertá-lo. Casca amarela, detalhes verdes, forma inicial com rosto do macaco oculto. Isso vale para retomar esse personagem; não define automaticamente o novo boss solicitado agora.

## Proposta para tornar a próxima integração revisável

1. Definir identidade: nova fase da campanha, boss e bichos. Se retomar Macaquini, confirmar uso do material atual; se escolher outro animal, iniciar pela direção visual desse personagem.
2. Montar fatia jogável isolada: cena de desenvolvimento com uma horda curta de novos inimigos e o boss; acesso ao boss sem atravessar nove hordas. Seleção de ataque, vida, hitboxes e pausa disponíveis para validação.
3. Reusar `WaveController`, `EnemyWave`, `EnemyWavePool`, cartas, `Boss`, `Health`, `Projectile`, pausa e pontuação. Criar somente controlador e apresentação específicos que ainda não existam.
4. Resolver HUD e diálogos por dados/referências do novo boss. Preservar comportamento do Gusini e não copiar sua identidade visual ou sua recuperação para outro personagem.
5. Configurar capítulo e acesso à fase em fluxo existente, com identificador e flags próprios. Não alterar o capítulo 1 para fingir que a fase 2 já existe. Seleção via HUB continua sendo trabalho de integração a definir.
6. Expandir as hordas e ajustar equilíbrio depois do teste isolado. Quantidade de hordas, HP, ataques, transformações e desfecho do novo boss continuam em aberto.

[P] Se a direção for selva, possíveis inimigos para avaliação: morcegos em formação e vespas que anunciam investida. São sugestões; ainda não foram encontrados como assets de runtime nem aprovados para produção. Não presumir que devem ser antropomórficos.

## Critérios de validação da implementação futura

- Compilação na versão correta, validação de referências e ausência de Missing Script.
- Ataques com aviso, origem/evento definidos e caminho seguro, usando dimensões e velocidades reais.
- Pausa interrompe projéteis, timers e animações; retomada não acumula disparos.
- Morte do player, morte do boss, desativação e troca de cena limpam ameaças e eventos.
- Pontuação e conclusão acontecem uma vez; o capítulo 2 não registra marcos do Gusini.
- HUD correta ao entrar, receber dano e reativar; sem interferir na barra do Gusini.
- Regressão do capítulo 1, cartas e retorno à base. Evidências distinguem validação automatizada de avaliação humana da dificuldade.

## O que foi feito nesta inspeção

Lidos os guias, `agent.md`, documentação e arquivos reais de boss/ondas/campanha; conferidos Git e processos Unity. Existem alterações locais anteriores em fontes/configurações e no `agent.md`; foram preservadas.

`unity status --format json` não encontrou instância acessível pelo Pipeline. Os processos Unity encontrados pertencem ao MCP de outro projeto; não foi aberto nem encerrado um Editor. Nenhum teste de Play Mode, compilação, nova arte ou alteração de gameplay foi executado nesta inspeção. Nenhum commit foi criado.
