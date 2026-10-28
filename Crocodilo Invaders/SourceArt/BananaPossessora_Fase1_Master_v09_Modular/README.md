# Banana possessora — forma 1 modular v09

Base: personagem montado e aprovado na conversa de 19/10/2026. Essa aprovação sucede a v08. Casca amarela fechada, rosto malicioso e veios verdes; macaco totalmente preso, apenas membros e rabo visíveis. Goosini e B. Crocodilo do projeto foram fornecidos como referências de estilo.

Seis PNGs RGBA independentes em `parts/`, com raízes completas para sobreposição: corpo, dois braços, duas pernas e rabo. Os aros pertencem somente à casca. Centros dos encaixes transparentes permitem mostrar os membros por trás. Canvas nativo 256 × 272, exportação por vizinho mais próximo e alpha binário; fontes geradas preservadas em `atlas_fonte.png`. Não é apenas o personagem montado recortado: as peças têm raízes independentes.

- `master.png`: montagem transparente na resolução nativa.
- `master.ora`: seis camadas editáveis em Krita/GIMP.
- `master.svg`: seis objetos com PNGs incorporados; não é arte vetorial redesenhada.
- `layout.json`: posições inteiras, ordem de camadas e pivôs em pixels a partir do topo esquerdo.
- `qa_report.json`: limites, formato RGBA, pivôs e hashes dos seis módulos.

Exportação Unity em `Assets/Art/Boss/BananaPossessora/Fase1`: Sprite Single, Point, Full Rect, PPU 100, sem mipmaps/compressão e pivô na articulação. As configurações partem dos sprites nativos do Goosini. Para remontar: use a ordem crescente de `sorting_order` e posição local `(x + pivotX - 128, 258 - y - pivotY) / 100`. A raiz fica no ponto (128,258) do master. O pivô do corpo é seu centro; os demais são articulações.

Reconstrução: `Tools/Art/package_banana_v09.ps1`; prévia interativa: `Tools/Art/preview_banana_v09.ps1`. Prévias em `Docs/ArtPreviews/BananaPossessora_v09/`. Versões anteriores preservadas.

Arte estática modular preparada para importação. Sem prefab, rig de animação, Animator ou integração em cenas; importação ainda não validada dentro do Editor Unity. O slide de braços da prévia serve para inspecionar a sobreposição, não é uma animação de gameplay.
