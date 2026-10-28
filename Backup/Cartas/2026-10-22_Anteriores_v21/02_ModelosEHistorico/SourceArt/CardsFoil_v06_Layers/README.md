# Molduras e acabamentos por camada — prévia v06

**Encaixe de textos v08 (atual):** após o usuário apontar desalinhamentos, a
tipografia passou para `card_typography_v08.cjs`. Tamanhos fixos por função em
todas as cartas: raridade/atributo 16 px, título 20 px e bônus 32 px na camada
360×536. Cada linha tem posição vertical fixa, centralização e caixa segura.
`+30%` é renderizado como uma única linha na mesma fonte/tamanho; não há mais
número grande com símbolos menores em posições separadas. O atributo desceu
para afastar-se do topo da placa; o bônus encolheu e subiu para liberar a margem
inferior. Acentos de raridade alinham pelo fim da linha, sem centrar separadamente
as alturas de cada palavra. Sombra discreta de 1px, sem invadir a borda.
As 25 combinações passaram pelas caixas seguras e pelos mesmos tamanhos/posições
por função. Bases e materiais continuam idênticos por hash ao modelo aprovado.
V07 permanece como histórico; a continuação editável usa V08.

**Modelo salvo e revisão de textos v07:** o modelo aprovado está arquivado em
`SourceArt/CardsModel_Approved_v06_20261021`, com HTML autossuficiente, camadas,
prancha e hashes. A prévia editável passa a usar `card_typography_v07.cjs`:
Press Start 2P em títulos e números, Pixelify Sans em raridade e atributo,
fontes já existentes em Assets. Raridade e atributo maiores/mais legíveis;
valor numérico dominante com +/% menores; contraste marfim, sombra discreta,
alinhamento e margens medidos. Acentos usam os glifos reais das fontes.
As 25 combinações de tipo/raridade cabem nas caixas seguras, incluindo
BLINDAGEM/PROPULSÃO e CADÊNCIA/VIDA MÁXIMA. Artes e materiais das cinco cartas
continuam idênticos por hash; o snapshot aprovado permanece intacto. A prévia
continua mostrando dano de ataque e ainda não está integrada ao runtime Unity.

**Acabamentos atuais (revisão seguinte):** épica tem apenas reflexo branco neutro
na ilustração e foil colorido na moldura. Lendária acrescenta reflexo branco neutro
à moldura, mantendo a arte e o holográfico de fundo anteriores; a arte foi
comparada pixel a pixel na mesma posição da luz e permanece idêntica. Mítica usa
dois materiais holográficos: padrão prismático curvo na arte e padrão escovado
diagonal na moldura, com direções/cores/ritmos distintos. O relevo e a intensidade
reduzida do efeito mantêm Croc destacado. A máscara G mítica vale 128 na arte/
canhões sobrepostos e 255 na moldura; o shader interpreta os dois acabamentos.
O recorte aprovado da sobreposição permanece igual, assim como todas as imagens
e textos. Comum e rara mantêm também suas máscaras e comportamento anteriores.

**Recorte mítico corrigido:** removidas as faixas horizontais de sobreposição
que cortavam canhões e copiavam blocos de céu sobre as laterais da moldura.
O compositor agora delimita os quatro grupos de canhões superiores por contorno,
seleciona metal/energia, recupera o contorno escuro e elimina partículas isoladas.
Sombra de contato de um pixel dá leitura de profundidade sobre a borda. A arte
original, os textos, materiais e outras quatro cartas permanecem iguais (bases,
materiais e textos das raridades 0–3 conferidos por hash). Apenas a composição
do vazamento da imagem mítica mudou; sem alterações em Assets.

**Correção posterior:** o usuário apontou a falta de moldura padrão na comum e
na mítica. Ambas agora usam molduras industriais completas da família v03,
incluindo cantos, placas de título/atributo e laterais. Comum mantém acabamento
fosco; mítica usa a carcaça carmesim com presas, holografia e sobreposição seletiva
dos canhões sobre os trilhos superiores/laterais. Rara, épica e lendária continuam
idênticas por hash nas imagens de base, materiais e textos.

Pedido do usuário: comum realmente simples; rara com reflexo uniforme apenas no
aro da imagem; épica com holografia ao redor da carta e foil brilhante na arte;
lendária com fundo holográfico e Croc sem holografia; mítica toda holográfica e
refletiva, preservando destaque no Croc. Só a mítica pode romper a moldura.

Esta prévia usa as artes alternativas aprovadas da v05, incluindo a mítica com
múltiplos canhões, boca aberta e olhar insano. Não gera novas artes nem altera
fontes anteriores, Assets, gameplay ou catálogo.

Implementação visual:

- Comum: moldura industrial padrão em oliva/aço, sem foil ou holografia.
- Rara: aro com máscara especular neutra, intensidade uniforme que muda com a
  direção da luz. Nenhum espectro colorido; a imagem permanece fosca.
- Épica: máscara de reflexo neutro/foil na imagem e máscara holográfica separada
  para os componentes externos da moldura. Placas de texto protegidas.
- Lendária: imagem contida dentro da moldura dourada, com máscara de recorte para
  rosto/capacete/armadura/canhão. Só o fundo da imagem recebe holografia; a moldura
  tem reflexo branco neutro. Não tem
  sobreposição do personagem na moldura, arte expandida ou saída da borda.
- Mítica: moldura industrial carmesim completa; recorte do arsenal é composto
  ACIMA de partes dos trilhos superiores/laterais e pode ultrapassá-los.
  Holografia prismática cobre a arte; holografia escovada cobre a moldura;
  intensidade menor no personagem e realce de contorno preservam expressão/volume.

As máscaras de materiais têm canais R=reflexo neutro, G=holografia,
B=personagem/primeiro plano e A=visibilidade. Os contornos são definidos por
polígonos no compositor da prévia e não constituem máscaras finais de produção.
Os textos usam camada separada após o brilho, preservando legibilidade.

Reexportar e validar:

```powershell
node 'Crocodilo Invaders/Tools/Art/build_foil_preview_v06.cjs'
node 'Crocodilo Invaders/Tools/Art/verify_foil_preview_v06.cjs'
```

Pranchas, PNGs de base/materiais/texto e prévia em
`Docs/ArtPreviews/CardsFoil_v06`. Validação: shaders, mudança com a luz,
comum fosca, arte rara inalterada em 18.848 pontos internos, personagem lendário
inalterado em 14.304 pontos internos, opção desligar foil e largura 736/360 px.
Inspeção visual concluída; segue sendo prévia, sem implementação em Unity.
