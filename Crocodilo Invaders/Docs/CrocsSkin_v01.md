# Croc — skin Crocs v01

Estado atual (05/11/2026): skin guardada e desativada por pedido do usuário; nave original restaurada. Nenhuma cena de build ou conteúdo de Resources a utiliza. O instalador não equipa mais automaticamente.

Histórico v01: skin anteriormente equipada no prefab compartilhado `Assets/Prefabs/Player.prefab` do projeto Unity ativo. O avião original continua disponível e intacto. Nenhuma mudança de gameplay.

Direção: mesma pixel art lateral, Croc com capacete oliva/fone/olho âmbar, mesmo tamanho e registro. A nave vira um Crocs verde com furinhos, sola clara, alça e pequenas saídas de arma/propulsão. Piloto e expressões reutilizam pixels dos 26 quadros originais; o casco vem de edição referenciada pelo `image_gen` integrado e acabamento na grade nativa.

- Sprites: `Assets/Art/Player/Crocs/{Idle,Fly,Shoot,Special,Hurt}`.
- Animações: `Assets/Animations/Player/Crocs` — cinco clips e um AnimatorOverrideController sobre o controller original.
- Fonte, prompt e hashes: `SourceArt/CrocsSkin_v01/README.md` e `manifest.json`.
- Exportador reproduzível: `Tools/Art/prepare_crocs_skin_v01.cjs`.
- Menus Unity: `Crocodilo > Skins > Install and equip Crocs` / `Equip original airplane`.
- Canvas 160×120, 100 PPU, pivot (0.45, 1/3), Point, Full Rect, sem compressão/mipmaps.

Validação nativa de dados passou: 26 sprites únicos, cinco clips, tempos e Animation Events de tiro/especial idênticos, pivôs e prefab equipado. Play Mode passou **17 verificações / zero erros**, incluindo tiros realmente liberados pelos eventos, especial, dash nativo, pausa e todos os estados com a skin.

Capturas reais do Play Mode e relatório: `Docs/ArtPreviews/CrocsSkin_v01`. A primeira preparação do teste encontrou os controles bloqueados pela abertura da fase; o teste foi corrigido para liberar gameplay antes de disparar e a execução seguinte passou. Não foi necessário mudar código de gameplay.

Build Windows separado: `Builds/Windows-Crocs-Skin-v01/CrocodiloInvaders.exe` (verificar o log `Logs/CrocsSkin-Build.log` para o resultado). Preservar a pasta inteira ao distribuir. Sem commit/push nesta tarefa.

Resultado final: build Windows concluído com sucesso (212.484.832 bytes reportados), inicialização headless de oito segundos sem exceções no log; checkout com 2.687 GUIDs únicos e metadados presentes. Logs: CrocsSkin-Install.log, CrocsSkin-Play.log, CrocsSkin-Build.log e CrocsSkin-Startup.log.
