# Build Windows e publicação automática

## Pacote local DEV — 22/10/2026

Build atual: `Builds/Windows-DEV/CrocodiloInvaders.exe`.
Pacote atual: `Builds/Pacotes/CrocodiloInvaders-DEV-BrilhoSemMusica-v25-Windows.rar`.
RAR5 validado por CRC/conteúdo; SHA-256 em `SHA256SUMS-v25.txt`.
Extraia a pasta inteira e abra o executável. DEV/F8 no menu abre o teste;
confirme Enter no loading e use R para rerolls infinitos no DEV.

Inclui as 25 cartas v21 de sistemas instalados na nave e nove sons originais.
Lendárias e míticas têm somente cintilações discretas, sem música de fundo.
Um brilho por oferta e ganho dividido entre cartas altas evitam acumular volume.
Os pacotes de áudio externos adicionados foram removidos. Procedência no arquivo
`Creditos/Audio/ORIGENS.txt`. Efeitos externos crescem gradualmente por raridade.

Unity 6000.6.4f1: build aprovado e inicialização de 8s sem exceções listadas.
Play Mode: **122 verificações aprovadas**, incluindo duas/três míticas, ganho,
brilho compartilhado, pausa, reroll, escolha, saída para menu e todas as 25 artes.
Logs: `Logs/CardAudio-v25-Play.log`, `Logs/WindowsDEV-Sparkle-v25-Build.log`
e `Logs/WindowsDEV-Sparkle-v25-Startup.log`.

Prévia com áudio: `Docs/ArtPreviews/CardAudio_v25/brilho-sem-musica-volume-controlado.mp4`.
Fotos atuais: `Docs/ArtPreviews/CardReview`. Detalhes: [AudioCartasBrilho_v25.md](AudioCartasBrilho_v25.md).
Empacotador ZIP/RAR: `Tools/PackageDev.ps1`. Compilações e arquivos compactados
ficam em Builds, ignorados pelo Git; o build de PR no CI gera seu próprio artifact.

Os backups completos locais ficam em `Backup/Cartas`, fora da pasta Unity.
O histórico já rastreado também está preservado nessa pasta no Git.

O workflow da raiz do repositório, `.github/workflows/windows-release.yml`, faz:

1. Validação rápida do projeto em pushes e pull requests para `main`.
2. Em push para `main` e PRs da própria origem, valida dados, cenas e scripts, compila Windows 64-bit com Mono
   usando a versão exata de `ProjectVersion.txt` e guarda a Library somente como cache.
3. Verifica o executável, a DLL do player e a pasta de dados, e cria um ZIP com a instalação completa.
4. Publica o ZIP em GitHub Releases e como artifact da execução. A Release só sai de
   draft após terminar o upload. Uma falha no build não publica uma versão incompleta.

O ZIP contém `CrocodiloInvaders.exe`, `UnityPlayer.dll`, `CrocodiloInvaders_Data` e
os demais arquivos do player. Extraia tudo na mesma pasta e execute o `.exe`.
O executável sozinho não é uma distribuição completa da Unity.

`workflow_dispatch` permite gerar um artifact manualmente. Apenas execuções da
`main` publicam Releases; execuções manuais de outras branches geram somente artifact.
O token de publicação é o `GITHUB_TOKEN` temporário da própria execução, sem PAT extra.
PRs de forks fazem apenas a validação rápida, sem acesso aos secrets. Builds de PRs
da própria origem geram artifacts, mas não publicam Releases.
As ações estão fixadas em commits verificáveis. O workflow usa o projeto
`Crocodilo Invaders/`, preservando a pasta `unity/` do port antigo.
A imagem Docker está fixada em `unityci/editor:ubuntu-6000.6.4f1-windows-mono-3`.
Ao atualizar a Unity, atualize também essa imagem e confira sua disponibilidade.

## Ativação da Unity no GitHub

Configure em **Settings → Secrets and variables → Actions** do repositório:

- `UNITY_LICENSE`: conteúdo da licença `.ulf` para Unity Personal.
- `UNITY_EMAIL`: e-mail da conta Unity associada à licença.
- `UNITY_PASSWORD`: senha dessa conta.

Para uma licença profissional, use `UNITY_SERIAL` no lugar de `UNITY_LICENSE`,
mantendo e-mail e senha. A [documentação de ativação do GameCI](https://game.ci/docs/github/activation/)
descreve esses métodos e a obtenção da licença pelo Hub. Os valores ficam nos secrets;
não devem estar no código, em commits ou em arquivos do projeto. O workflow verifica
se a configuração está presente e dá uma mensagem específica antes de baixar a Unity.

Essa configuração é necessária para o servidor compilar: a ativação existente no
computador local não ativa automaticamente o runner hospedado do GitHub.

## Build local

Use **Crocodilo → Build Windows** na Unity. A saída padrão é
`Builds/Windows/CrocodiloInvaders.exe`, ignorada pelo Git.
Também pode executar o Editor com `-batchmode -buildTarget Win64 -projectPath "..."`
e `-executeMethod CrocodiloWindowsBuild.Build`, adicionando `-customBuildPath "...exe"`
para escolher a saída. O método encerra com código diferente de zero se a validação
ou o build falhar; não gera uma Release a partir de um build malsucedido.

As cenas habilitadas são preservadas, começando por MainMenu e PlayerMovementTest.
A validação confere scripts ausentes nos prefabs e cenas, catálogo completo, chances
60/25/11/3,5/0,5%, ofertas com três tipos diferentes e os parâmetros do mixer.
O método de build é compartilhado entre execução local e GameCI.
