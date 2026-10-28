# Integração da coleção v12 — 21/10/2026

Branch: `codex/cartas-expressoes-v12`. As 25 frentes estáticas da coleção v12 foram aplicadas em `Assets/Resources/Cards/`, nos mesmos caminhos usados pelo catálogo e pelo carregamento Resources. Inclui cinco tipos e cinco raridades com expressões progressivas.

`before/` preserva imagens anteriores à integração e metadados em arquivos `.meta.txt`. `manifest.json` registra hashes finais, percentuais e identidades originais dos sprites. Não sobrescrever esse backup.

Aplicador: `node Tools/Art/apply_card_collection_v12.cjs`. Verificação somente leitura: adicionar `--check`. Imagens 360×536, recorte completo, PPU200, Point, sem mipmaps/compressão, maxTextureSize1024. Identidades dos sprites e catálogo preservados.

Nenhum novo foil/holográfico dinâmico configurado, conforme pedido. A integração desses efeitos fica para depois. Código, cenas, prefabs, materiais e verso não foram alterados por esta aplicação.

Validação de arquivos passou. Play Mode não executado: projeto requer Unity6000.6.4f1, instalação local detectada6000.3.25f1. Não abrir o projeto na versão antiga para validar.
