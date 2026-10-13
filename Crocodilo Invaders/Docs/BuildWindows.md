# Build Windows e publicação automática

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
