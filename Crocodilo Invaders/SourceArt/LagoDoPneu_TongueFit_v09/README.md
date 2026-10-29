# Língua inteira — correção visual local v09

**Não commitar nem publicar até o usuário dizer que está finalizado.**

`tongue-full-master.png` é a master RGBA gerada pela ferramenta integrada image_gen com a ponta e a boca da v08 como referências. `prompt.txt` contém o prompt completo. A imagem inteira foi preservada sem edição e copiada para `Assets/Resources/LakeTongueV09/tongue-full.png`.

O desenho reúne base, corpo e ponta pegajosa em uma peça contínua, com volume, contorno vinho e sombras e brilhos da mesma paleta. `LakeTongueSkinArtTools.Install` cria um recorte nativo baseado no alfa e registra o sprite em `skin.asset`, com material próprio e filtro Point, sem mipmaps ou compressão. O PNG não é recortado nem reexportado.

`LakeTongueSkin` deforma uma única malha texturizada: preserva as proporções da base e da ponta, esticando apenas o trecho central. A base se ancora no centro da língua enrolada da pose de boca aberta e se mistura ali por uma pequena transição de alfa. A conexão não usa LineRenderer nem uma ponta desenhada à parte.
