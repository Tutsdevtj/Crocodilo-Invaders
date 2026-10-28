# Menu DEV

O botão DEV no menu principal (ou F8) abre:

- Cartas: geração, raridade, progresso após uma onda e início do teste existente.
- Boss final:
  - Boss 1 · Gusini: Fase 1 / Fase 2.
  - Boss 2 · Macaquini: Fase 1 / Fase 2.
- Base: entrada direta na Base Improvisada.

Esc ou Voltar retorna um nível; na raiz, fecha o painel. F8 fecha em qualquer nível.
Os atalhos usam as arenas de campanha e o loading existente. Fase 2 esgota a primeira
forma quando o boss está pronto e reproduz a transformação nativa. Os diálogos são
pulados no teste. Retry conserva a fase escolhida; retornar ao menu encerra a sessão.
Não prepara ranking nem registra os marcos de conclusão de capítulo.

Arquivos principais: `DeveloperMenuView`, `MainMenuController`,
`GameplayLoadingController` e `DevelopmentBossSession`.

Validação nativa: `DeveloperMenuValidation.Run`, 44 verificações, zero erros;
navegação, layout, configurações de cartas, quatro atalhos de boss, limpeza de
sessão e carregamento da base. Resultado em `Logs/DeveloperMenu/validation.json`.
Build Windows: `DeveloperMenuValidation.Build`, com MainMenu como primeira cena,
em `Builds/Windows-Dev-Menu-v23/CrocodiloInvaders.exe`.
