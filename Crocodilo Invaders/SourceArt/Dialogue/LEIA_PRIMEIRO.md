# Crocodilo Invaders — Sistema de diálogo completo

| Pasta | Conteúdo |
|---|---|
| `01_Dialogue_UI/` | Interface "Rádio de Piloto": painel 9-slice, placa de nome, rádio, setinha de continuar e realce. Inclui a fonte Pixelify Sans (OFL) e o **`DIALOGUE_UI_GUIDE.md`**, que traz escala, 9-slice, TMP, timings e hierarquia no Canvas |
| `02_Portraits_BCrocodilo/` | B. Crocodilo: master e 5 expressões (160×160). Guia em `PORTRAIT_GUIDE.md` |
| `03_Portraits_Gusini_F1/` | Gusini Fase 1: 5 expressões (smug, resentful, power, vulnerable, hostile), 160×160 |
| `04_Portrait_Gusini_F2/` | Gusini Fase 2 corrompido: retrato único (furious), 160×160, troca direta pelo da Fase 1 |
| `dialogue_scene_pre_boss_gusini.json` | As 14 falas da cena com falante, nome exibido, expressão, caminho do portrait e cor da tarja da placa |

## Regras comuns
- **Portraits:**
  - todos têm **160×160**, fundo transparente e pivô no **centro da base**, Unity (0.5, 0);
  - o Croc olha para a **direita** e o Gusini para a **esquerda**; use flipX se precisar inverter;
  - import com Filter Point, Compression None, sem mipmaps;
  - exibir em escala inteira (2× na referência 1200×720).
- **Interface:** arte nativa exibida em 2×. Os detalhes estão no guia da UI.
- **Posições:** a posição dos portraits na tela é decisão do Codex. Nenhum asset depende do lado.
- **Cores das tarjas:**
  - Croc: `#96BE5A`
  - Gusini: `#E2463C`
  - Gusini Corrompido: `#7CEC48`
