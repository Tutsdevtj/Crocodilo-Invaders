# Idle revisado — respiração distribuída, v02

Revisão dos pixels e frames existentes de `CrocodiloIdle64-v01`. A sprite não foi recriada. A versão anterior foi preservada em seus arquivos originais e aparece à esquerda do GIF de comparação; a revisão aparece à direita.

## Movimento

| Frame | Resposta | Duração |
| --- | --- | ---: |
| F0 | Repouso original, idêntico ao neutro aprovado | 450 ms |
| F1 | Peito expande discretamente e os ombros iniciam a inspiração | 250 ms |
| F2 | Cabeça e ombros conservam o ápice anterior; mangas, cotovelos e mãos acompanham com 1 px vertical. Quadril faz uma acomodação lateral de 1 px | 450 ms |
| F3 | Cabeça retorna; braços permanecem elevados por um instante, em atraso. Ponta da cauda sobe 1 px e depois volta no repouso | 350 ms |

Quatro desenhos distintos, sem aumentar a contagem. Loop de 1.500 ms. A expiração se diferencia da inspiração pelas respostas dos braços e da cauda. Não há translação única do personagem inteiro.

O teste de deslocamento do joelho abriu um espaço visual inadequado junto à cauda e foi retirado. Os joelhos mantêm seus contornos; a acomodação opcional da parte inferior fica no quadril, sem agachamento.

Mangas, braços, mãos e ponta da cauda usam os pixels existentes, copiados como seleções na grade. As ligações locais foram reparadas com cores da mesma paleta. Detalhes e manchas de textura acompanham suas partes, sem padrões novos.

## Preservação

- Canvas 64×64 e origem das células/cels (0,0), sem trim, reescala ou recentralização. As coordenadas e âncoras de apoio permanecem no mesmo sistema da versão anterior.
- F0 idêntico ao neutro aprovado.
- Paleta autorizada de 23 cores; nenhuma cor nova. Alpha dos PNGs somente 0 ou 255.
- Cabeça e focinho mantêm exatamente os pixels da máscara original, compensada a elevação de 1 px no ápice.
- Todas as botas e os espaços à sua volta, nas linhas y=55…63, coincidem com F0 em todos os frames. Contatos das solas preservados.
- A antiga proteção integral de y=34…63 foi substituída conforme o pedido: quadril, braços e ponta da cauda podem mudar localmente.
- Ligação da cauda ao quadril preservada; somente a região distal x=10…18 participa do movimento da cauda.

## Revisão e validação

Os frames foram comparados com suas fontes anteriores, F0 e os adjacentes. As ligações de ombros, mangas e cotovelos, mãos, cintura, cauda e botas foram inspecionadas em escala nativa e ampliada.

**Playback real:** a prévia foi reproduzida no navegador com os PNGs nativos, a comparação GIF entregue e versões ampliadas. Foram usados modo normal, modo lento de 25% e avanço por frame. O contador confirmou mais de dez ciclos na reprodução normal; foram feitas observações visuais das fases, incluindo F3→F0, em tamanho nativo e ampliado. O teste inicial de acesso local foi resolvido executando o servidor de revisão fora do isolamento, somente em loopback. Não foi feita publicação externa.

A avaliação foi feita por observações do playback e inspeção das poses; não constitui garantia universal de fluidez ou aprovação do usuário. Não houve integração ao jogo.

Validação automatizada em `validation.json`:

- Dimensões, paleta, alpha e máscaras autorizadas.
- Preservação de F0, cabeça, botas e ligação da cauda.
- Um único componente visível por frame, sem clusters destacados.
- Igualdade de todos os pixels da sheet com os PNGs.
- Igualdade dos pixels dos GIFs com os PNGs correspondentes sobre fundo #E2E4E6, nas ampliações inteiras de 8× e 6×.
- Ordem e durações dos GIFs: 450/250/450/350 ms.

## Arquivos

- `frames/idle_00.png` a `idle_03.png`: PNGs nativos transparentes.
- `crocodilo-idle-revisado-sheet.png`: sheet horizontal 256×64 sem padding.
- `timing.json`: ordem, ciclo, durações, origem e operações por fase.
- `crocodilo-idle-revisado-x8.gif`: prévia revisada em 512×512.
- `crocodilo-idle-comparacao.gif`: anterior à esquerda, revisado à direita, 816×384, escala inteira 6×.
- `crocodilo-idle-revisado-64.aseprite`: quatro frames editáveis, tag `Idle` e referência neutra oculta.
- `preview.html` e `before/`: comparação local com reprodução, velocidade e avanço por frame. A imagem GIF de comparação é independente dos controles dos PNGs.
- `validation.json`: relatório técnico e de revisão.
- `working/`: máscaras usadas e diferenças RGBA contra os frames anteriores.
