# Fauna do Lago do Pneu v11 — teste local

Sprites gerados com image_gen para este projeto, com os prompts preservados em `prompt-fish.txt` e `prompt-piranha.txt`.

- `fish-swim-master.png`: quatro quadros de nado, grade 2 × 2.
- `piranha-master.png`: seis quadros, grade 3 × 2; linha superior nadando, inferior mordendo.

As cópias em `Assets/Resources/LakeWildlifeV11` preservam os PNGs originais. `LakeWildlifeTools` cria os recortes e registra os pivôs pelos olhos via Unity, sem redesenhar os arquivos. Pixel filtering Point, sem compressão e sem mipmaps.

Oito peixes decorativos nadam em sentidos alternados sob os reflexos. Ao tocar a linha da água, cinco piranhas se agrupam, avisam por 0,65 s e saltam com intervalo de 0,12 s. A permanência na água repete os grupos. Cada mordida tira 1 HP; intervalo compartilhado de 0,65 s evita dano simultâneo do cardume. Subir 0,55 unidade acima da superfície com a parte inferior da nave encerra o perigo e recolhe as piranhas.

O mato e suas duas repetições usam WorldFront/100. A HUD continua em Canvas Overlay. Não commitar, fazer push ou atualizar a PR até o usuário finalizar este protótipo.
