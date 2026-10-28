# Macaquini — ajudantes da primeira forma v23

A forma com jetpack recebe duas espécies pequenas da selva infectada. O primeiro padrão continua solo. No segundo padrão entram dois morcegos; rodadas posteriores misturam dois morcegos e uma perereca. Limite de três ajudantes e oito segundos entre ondas, sem reposição de indivíduos abatidos. A implementação é instalada por `MacaquiniCombat.Awake` apenas em bosses com `MacaquiniFlightPresentation`; não exige remontar o prefab nem altera os combos da segunda forma.

- **Morcego emboscador:** 2 HP, 150 pontos. Aviso de 0,85 s, destino travado, bote reto de 0,42 s e recuperação vulnerável de 1,05 s. Um bote por indivíduo, sem dano ao encostar fora do ataque. Intervalo entre botes acompanha a recarga real do dash.
- **Perereca cuspidora:** 3 HP, 250 pontos. Desce num cipó, infla o papo por 0,95 s e cospe uma bolha. Um disparo por indivíduo. A bolha viaja por 1 s, cresce, avisa quatro direções e solta quatro sementes retas a 3,8 u/s. Possui 1 HP: tiro do jogador cancela a explosão. Máximo de duas bolhas simultâneas.
- **Interação:** o rasante visível `JetDash` elimina ajudantes e neutraliza bolhas ao atravessá-los. Isso usa o mesmo caminho de vida e pontuação; não duplica abates.
- **Coordenação:** a próxima manobra do boss aguarda um bote já anunciado ou a resolução da bolha. A bolha pode amadurecer durante a rajada, mas não explode durante descarga/rasante. Não há perseguição depois do aviso.
- **Ciclo de vida:** tempo escalado; pausa congela movimento/avisos. `StopCombat`, transformação, morte, desativação e saída da cena removem a raiz de ameaças.

Arte: `Assets/Resources/MacaquiniHelpersV23`, seis PNGs 128×112, PPU 64, Point, alfa binário, sem mipmaps/compressão. Fontes e prompts: `SourceArt/MacaquiniHelpers_v23`. Prévia: `Docs/ArtPreviews/MacaquiniHelpers_v23/modelos-x3.png`. Importador específico, sem mudar configurações globais.

Validação repetível: `MacaquiniHelpersV23Validation.Run`. Compila e roda a cena de jetpack em Play Mode; confere integração natural, assets, limite, pausa, contato de bote, pontuação, tiro real contra bolha, quatro sementes, dano aliado do boss, transformação e limpeza. Resultados em `Docs/ArtPreviews/MacaquiniHelpers_v23/validation.json` e `Logs/JungleStage/Helpers23.log`. As capturas não equivalem a aprovação artística humana nem a balanceamento jogado manualmente.

Build de teste: `MacaquiniHelpersV23Tools.Build`, saída `Builds/Windows-Macaquini-Ajudantes-v23/MacaquiniSelva.exe`. Usa a cena existente `MacaquiniJetpackTest`, entrando na primeira forma.

Validação de 27/10/2026: **26 verificações em Play Mode, zero erros**, no Unity 6000.6.4f1 local. Inclui dano real da semente, tiro real cancelando bolha e transformação completa. Conferência estática: 2448 GUIDs válidos. F8 chama os ajudantes na cena de teste; F1/F2/F3 forçam rajada/rasante/descarga, I alterna proteção, H cura, R reinicia. O teste existente mantém 240 HP para facilitar a observação; não é a vida da campanha.

Entrega Windows concluída com `BuildResult.Succeeded`; inicialização do executável conferida em segundo plano sem exceções no log (`Helpers23-startup.log`, dispositivo gráfico nulo, sem avaliação visual nesse smoke test). ZIP completo em `Builds/Pacotes/Macaquini-Fase1-Ajudantes-v23-Windows.zip`: 189 arquivos, 113561437 bytes. CRC e conteúdo de cada arquivo conferidos por SHA256 contra a pasta de build. SHA256 do ZIP: `45251d4b67aee5f24b7130ee62af548e07997068336693c2fec0b8c1d4c765b1`. Log final de testes/build: `Logs/JungleStage/Helpers23-final.log`. Sem commit/push nesta tarefa.
