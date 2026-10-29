# Linguada — fontes locais em teste

**Não commitar, fazer push ou atualizar a PR com este trabalho até o usuário dizer que está finalizado.**

Geradas pela ferramenta integrada image_gen, usando a master aprovada do sapo e a captura do lago como referências:

- `sapo-mouth-master.png`: pose completa de boca aberta, mesma resolução, registro, corpo e pneu da master. Prompt em `prompt-mouth.txt`.
- `fly-flight-master.png`: seis quadros de voo em grade 3 × 2, com corpo, olhos e asas desenhados. Prompt em `prompt-fly.txt`.
- `tongue-tip-master.png`: ponta carnuda e pegajosa da língua, voltada à esquerda. Prompt em `prompt-tip.txt`.

As imagens geradas foram preservadas sem edição. Cópias idênticas estão em `Assets/Resources/LakeTongueV08`. O importador `LakeTongueArtTools.Install` cria seis sprites registrados pelo centro dos olhos vermelhos da mosca e um recorte nativo da ponta da língua. Esses recortes são metadados de sprites do Unity, sem reexportar ou modificar os PNGs. O corpo elástico da língua é desenhado por três ribbons no runtime, com contorno vinho, miolo coral e brilho rosa.

O asset nativo `art.asset` reúne pose, ponta, seis quadros e material. Os PNGs usam Point, sem mipmaps ou compressão. A pose mantém 300 PPU e pivô (0,5; 0,38); as moscas usam 500 PPU com registro por quadro para evitar tremedeira durante a batida de asas.
