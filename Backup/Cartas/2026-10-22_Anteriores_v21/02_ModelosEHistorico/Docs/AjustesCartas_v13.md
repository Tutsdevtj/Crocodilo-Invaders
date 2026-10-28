# Ajustes finos das cartas — v13

## Textos

As 25 frentes foram recompostas a partir das mesmas bases v12. O render anterior
via Pango/Sharp cortava pixels na esquerda de alguns glifos. A nova camada usa
Pillow/FreeType numa tela completa com margem, sem recortar o texto antes da
composição. As fontes do jogo e os tamanhos por função permanecem uniformes.
Atributo e bônus sobem quatro pixels; as áreas seguras são verificadas por linha.
O gerador comprova que nenhum pixel fora das placas de texto foi alterado.

## Som de revelação

Quatro composições originais substituem os stingers anteriores; a rara passa a
ter som próprio. O som genérico continua apenas na comum.

| Raridade | Composição | Duração |
| --- | --- | --- |
| Rara | Dois sinais brilhantes e clique mecânico | 0,42 s |
| Épica | Subida de quatro notas e impacto mais encorpado | 0,74 s |
| Lendária | Fanfarra de seis notas, grave e acorde | 1,04 s |
| Mítica | Impacto grave, subida de sete notas, energia e acorde com brilho | 1,42 s |

O RMS efetivo aumenta por raridade. A soma simulada de três míticas espaçadas
por 200 ms atinge pico 0,763, com folga abaixo de 1. Importação PCM, 44,1 kHz,
mono de origem, sem normalização por conversão para mono. Os sons reutilizam o
AudioSource e mixer existentes e param ao ocultar/rerolar as cartas.

## Verso

Novo desenho simétrico de metal militar, padrão de escamas e selo do Croc, sem
retrato, atributos ou placas de título. Asset final em `Assets/Art/Cards/Back.png`,
180×268. Fonte gerada via ferramenta nativa image_gen, preservada em
`SourceArt/CardCollection_v13_FineTuning/Back-source.png`; prompt integral em
`SourceArt/CardCollection_v13_FineTuning/Back-prompt.txt`. Verso anterior e sons
anteriores preservados na mesma pasta. GUID e identidade do sprite mantidos.

## Reprodução e validação

`Tools/Art/build_card_fine_tuning_v13.py` gera as camadas, PNGs e WAVs e grava o
manifesto. `--check` apenas confere os arquivos ativos. Esse verificador substitui
o aplicador v12 para a coleção atual; executar o aplicador antigo restaura os
textos antigos. Os históricos aprovados v08–v12 permanecem intactos.

`CardFineTuningTools.Install` configura os importadores e liga os sons aos três
slots via PrefabUtility/SerializedObject. A reordenação do YAML pelo Editor muda
o diff textual, mas os IDs e propriedades foram comparados: as únicas alterações
lógicas do prefab são as três referências ao novo som raro.

`CardFineTuningTools.Validate` verifica 25 sprites completos, três versos, quatro
stingers, RMS crescente e referências. `CardEffectsPlayValidation.Run` passou
57 checks com zero erros do jogo, incluindo seleção dos clips nos três slots,
versos, pausa, reroll, escolha e menu. Capturas do jogo conferidas em 1200×720,
800×600 e 1920×1080. A validação técnica de áudio não substitui a avaliação
auditiva do jogador no volume e equipamento de uso.

Pacote: `Builds/Pacotes/CrocodiloInvaders-DEV-AjustesCartas-v13-Windows.zip`.
