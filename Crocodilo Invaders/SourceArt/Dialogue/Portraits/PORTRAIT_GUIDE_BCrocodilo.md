# B. Crocodilo: portraits de diálogo

O ponto de partida é o portrait novo aprovado. Ele foi reconstruído como pixel art nativa: a imagem não foi só reduzida.

1. **Base:** a referência foi reamostrada para 160 px.
2. **Paleta:** cada região de material (capacete, pele, mandíbula, uniforme, camiseta, metal) foi repintada com uma rampa controlada de 4 ou 5 cores, mantendo a luz da referência.
3. **Limpeza:** os clusters foram simplificados (mediana do nível de tom) e o contorno externo foi refeito com 1 px.
4. **Redesenho:** olhos, sobrancelhas, dentes, linha da boca e mandíbula foram redesenhados como camada paramétrica.

## Arquivos
| Arquivo | Uso |
|---|---|
| `portraits/crocodilo_master_neutral.png` | **Master**: neutro, confiante e marrento. Referência única do conjunto |
| `portraits/crocodilo_01_surpreso_indignado.png` | CROC_01: choque com indignação. Olhos abertos, sobrancelha tensa, mandíbula levemente aberta |
| `portraits/crocodilo_02_serio_autoritario.png` | CROC_02: olhar firme, pálpebras baixas em V, boca fechada e reta |
| `portraits/crocodilo_03_confuso_preocupado.png` | CROC_03: sobrancelha interna erguida, ruga entre os olhos, boca entreaberta, canto da boca para baixo |
| `portraits/crocodilo_04_sincero_magoado.png` | CROC_04: pálpebras caídas para fora, olhar baixo, brilho úmido sutil no olho, canto da boca para baixo |
| `portraits/crocodilo_05_determinado.png` | CROC_05: olhar estreito, sobrancelha firme, dentes cerrados (músculo da bochecha marcado) |
| `dialogue_portraits.json` | Mapa fala → expressão (14 falas) |
| `previews/` | Folha de contato x3, nativo 1x, 2x, rostos x5, referência vs master |
| `tools/` | Geradores (Python), para ajustes futuros. Não importar no Unity |

## Especificações técnicas
- **Canvas:** **160×160**, igual em todas as imagens. Fundo transparente, alpha só 0 ou 255. Sem moldura, sem fundo e sem texto.
- **Por que 160:** a 128 o olho ficaria com cerca de 8 px e as pálpebras não teriam espaço para atuar. A 160 o olho próximo tem cerca de 14×11 px.
- **Enquadramento:** o busto toca a borda inferior (y = 159) e o topo do capacete fica em y ≈ 20.
- **Pivô sugerido:** **(80, 0)**, centro da base = Unity **(0.5, 0)**. Assim, todas as expressões trocam sem pular.
- **Import:** Filter Mode **Point**, Compression **None**, sem mipmaps.
  - PPU livre: escale por inteiro (2× ou 3×) para manter os pixels quadrados.
- **Direção:** virado para a direita. Se precisar dele do lado direito da tela, use flipX. O desenho não tem texto nem emblema assimétrico, então o flip funciona.

## Paleta
| Material | Cores (escuro → claro) |
|---|---|
| Contorno | `#0D0D15` |
| Pele | `#1A2A1E` `#284226` `#3C602E` `#5A8038` `#86A248` |
| Capacete | `#24261A` `#404226` `#606232` `#868A40` `#BEBA58` |
| Uniforme | `#22221A` `#3A3826` `#585232` `#7A7240` `#A49A52` |
| Mandíbula / garganta | `#744E2E` `#A67840` `#D0A058` `#EEC87A` `#FAE4A8` |
| Camiseta | `#766850` `#AA9A74` `#D6CAA4` `#ECE4C4` |
| Metal | `#2C2C32` `#4A4A50` `#707072` `#A2A09C` `#D4D2CA` |
| Olho | `#966014` `#E4A81E` `#FCD648` `#FFF4AA` |
| Dentes | `#C2B28C` `#F6EED2` |
| Boca / língua | `#341016` `#963C40` `#C46060` |

## Regras de consistência (para novas expressões)
- **Nunca mexer:** silhueta, capacete, fone, focinho, uniforme, paleta, luz (de cima/esquerda) e canvas.
- **Pode variar:**
  - pálpebra superior e inferior (altura e inclinação);
  - ângulo da sobrancelha;
  - posição e tamanho da pupila;
  - canto da boca;
  - abertura da mandíbula: no máximo cerca de 5°, girando na dobradiça (76,84). O focinho nunca muda de tamanho.
- **Novas expressões:** adicione uma linha em `EXPR` no `tools/croc_master.py` e rode o script. Todas as expressões saem do mesmo master.
