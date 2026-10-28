# Banana disparada maior — v03

Pedido: aumentar a banana disparada. O projétil separado passou de **32×16 para 64×32 pixels**, dobro da largura e altura. A imagem foi revisada com image_gen integrado, mantendo fruta curva clara, sombra dourada e detalhe verde. Exportação em pixel art com paleta compartilhada e alpha binário.

Os 12 PNGs da arma são idênticos aos da v02, verificados por SHA-256; o ciclo de 1 s e o recoil de 8 pixels continuam. A prévia reposiciona o projétil maior com a extremidade traseira na boca da arma e centro vertical alinhado à saída. Evento de tiro no índice 3/320 ms, mesmo ponto nominal da boca (32,34).

Arquivos finais: `projectile/banana.png` (64×32), `frames/`, `weapon_idle.png`, `spritesheet_6x2.png`, `palette.json`, `animation.json` e `qa_report.json`. A nova fonte do projétil está em `sources/projectile.png`, prompt em `prompt_projectile.txt`; fontes da arma copiadas integralmente da v02. Os prompts anteriores do design da arma continuam no pacote v02.

Prévia ampliada 4× em `Docs/ArtPreviews/BananaAlienWeapon_v03_BigProjectile/banana_weapon_fire_x4.gif`. Ferramenta: `Tools/Art/package_banana_alien_weapon_v03.cjs`. V01 e v02 preservadas; sem integração em cenas, dano, colisões ou gameplay.
