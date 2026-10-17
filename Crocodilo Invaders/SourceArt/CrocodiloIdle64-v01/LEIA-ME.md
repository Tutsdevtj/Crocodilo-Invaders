# Idle do crocodilo aprovado — v01

Fonte: última sprite 64×64 aprovada, correspondente ao anexo `codex-clipboard-32d7201b-8383-4838-990e-0384cece380c.png`. O portrait não foi utilizado para redesenhar esta animação.

O anexo 512×512 é uma ampliação exata de 8×. Os pixels RGB nativos têm SHA-256 `dd0ab646a46bf1e2314f0f51421b5104061378d1c36718b9e751c33446586f26`, coincidente com o guia. A comparação com o PNG transparente do projeto foi exata, incluindo os vazios internos da silhueta.

## Movimento e timing

| Frame | Fase | Alteração | Duração |
| --- | --- | --- | ---: |
| F0 | Repouso | Base original, intacta | 450 ms |
| F1 | Inspiração | Pequena elevação de ombros e ligação das mangas | 250 ms |
| F2 | Ápice | Cabeça e parte superior do peito sobem 1 px, com conexão à camisa | 450 ms |
| F3 | Expiração | Reutiliza F1; cabeça volta à posição neutra antes da acomodação dos ombros | 350 ms |

Loop: F0→F1→F2→F3→F0, 1.500 ms. Quatro posições na timeline e três desenhos distintos. A reutilização de F1 na saída evita alterações de textura sem função. Nenhum piscar, gesto, balanço de cauda ou caminhada foi acrescentado.

As partes foram copiadas por coordenadas inteiras no Aseprite. As máscaras incluem origem, destino e conexões. A ligação inferior da área móvel foi corrigida com cores existentes. Não houve regeneração de personagens independentes, alteração de escala ou recentralização.

## Preservação e validação

- F0: todos os pixels visíveis e vazios iguais à base transparente.
- Canvas: 64×64 em cada frame, alpha somente 0 ou 255.
- Paleta: as 23 cores autorizadas; nenhuma cor nova.
- Região protegida: todos os pixels das linhas y=34…63 idênticos em todos os frames, incluindo transparência. Botas, calça, cauda e partes baixas dos braços permanecem fixas.
- Cabeça: os pixels da máscara coincidem com F0, compensada a translação de −1 px em F2; nos outros frames, posição original.
- Sheet: 256×64, quatro células sem padding ou trim, correspondentes aos PNGs.
- GIF: 512×512, nearest-neighbor 8×, reprodução em loop com 450/250/450/350 ms. Pixels da prévia coincidem com os PNGs compostos sobre #E2E4E6.
- Arquivo `.aseprite`: quatro frames, tag `Idle`, durações preservadas e camada de referência F0 oculta. Frames de edição chapados independentes, acompanhados de máscaras e manifesto de alterações.

Os quatro frames foram inspecionados visualmente em tamanho nativo e ampliado; cabeça, ombros, pescoço, ligações das mangas e transições foram comparados. A preservação técnica não substitui a avaliação do ritmo e da naturalidade.

**Limitação da revisão:** a tentativa de reprodução no navegador falhou porque o navegador disponível não conseguiu alcançar a prévia local. A reprodução contínua, sua naturalidade e a revisão de dez ciclos permanecem pendentes. Não foi declarada fluidez garantida. O GIF e `preview.html` permitem essa revisão pelo usuário; a página oferece reprodução normal, lenta e avanço por frame.

Não foi feita integração nem substituição de assets no jogo.

## Arquivos

- `frames/idle_00.png` a `idle_03.png`: PNGs nativos transparentes.
- `crocodilo-idle-sheet-64.png`: sheet horizontal transparente.
- `crocodilo-idle-timing.json`: ordem, coordenadas, fases e durações.
- `crocodilo-idle-preview-x8.gif`: prévia animada com fundo claro.
- `crocodilo-idle-64.aseprite`: fonte editável.
- `preview.html`: visualizador local; abrir junto à pasta `frames`.
- `validation.json`: relatório técnico detalhado.
- `working/editable-mask.png`, `working/head-mask.png`: máscaras de edição e identidade da cabeça.
- `working/patches.json`: diferenças concretas de cada frame contra F0, com coordenadas e RGBA anterior/novo.
