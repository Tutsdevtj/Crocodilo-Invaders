# Arma mais banana, menos tecnológica — v04

Pedido: “faz a arma um pouco menos tech e mais banana.” A casca passa a dominar a silhueta, com curvatura natural, sulcos longitudinais e talo marrom formando a pegada. Foram retirados colar metálico roxo, câmaras de energia, placas de reforço, aleta e luzes eletrônicas. A origem alienígena permanece nos veios verdes irregulares da casca e no interior esverdeado da abertura.

Preservados o projétil grande de64×32, a abertura da casca em três abas, o disparo para a esquerda, recoil8px e retorno fechando o ciclo de1s. Arma separada do personagem, canvas128×96, paleta máxima20cores (18 usadas com projétil), alpha binário e pixels uniformes.

`frames/` contém12 posições: seis desenhos de abertura/impulso e retorno em ordem inversa, com deslocamentos de recoil; último frame idêntico ao idle. `weapon_idle.png`, `spritesheet_6x2.png` (768×192), `projectile/banana.png`, `animation.json`, `palette.json` e `qa_report.json` são os exports finais. Ponto de pegada nominal(86,60); saída nominal(32,39); evento de tiro índice3/320ms. A prévia reposiciona verticalmente a banana para essa saída.

Fonte redesenhada com image_gen integrado em `sources/weapon_01.png`, prompt em `prompt_weapon.txt`; fonte do projétil preservada da v03. GIF ampliado4× e imagens de revisão em `Docs/ArtPreviews/BananaAlienWeapon_v04_Organic/`. Ferramenta de reconstrução: `Tools/Art/package_banana_alien_weapon_v04.cjs`. Conferidos idle, pose de disparo, cores, alpha, ciclo e dimensões. Sem prefab/Animator/gameplay ou encaixe no macaco. Versões tecnológicas anteriores preservadas.
