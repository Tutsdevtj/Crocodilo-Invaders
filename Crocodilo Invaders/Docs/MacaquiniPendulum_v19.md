# Macaquini — domínio dos cipós v19

A segunda forma abre seu repertório com um cipó preso acima da arena, que cruza toda a área jogável como um pêndulo. O Macaquini controla o balanço com um cipó de ligação e inclina o corpo durante a passagem. Mantém as artes, proporções, peito claro, olhos verdes e HUD existentes.

O arco dourado e sua seta avisam a direção por 1,15 s. A passagem dura 1,65 s, causa 2 de dano e atinge no máximo uma vez por passagem. Caminhar não basta para atravessar: use o dash contra o sentido do cipó. A colisão usa a proteção real do dash do jogador. Abaixo de 50% da segunda barra, há uma segunda passagem em sentido contrário, precedida por uma pausa de 1,4 s para recuperar a esquiva. As ameaças anteriores são removidas antes do balanço.

O ataque entra na rotação normal da segunda forma. A abertura ensina cada padrão; depois a ordem varia sem repetir o mesmo ataque consecutivamente. Pausa, morte e cancelamento encerram o movimento, a colisão e o áudio.

Na arena de testes: Shift dá dash; K esgota a primeira forma; F4 força o balanço na segunda; J remove 45 HP e permite experimentar o retorno; I alterna proteção; H cura; R reinicia.

Implementação em `MacaquiniPendulumVine.cs`, integrada a `MacaquiniCombat`. Physics2D legado com cápsulas amostradas ao longo do arco evita atravessamento entre passos de física. Reutiliza o sprite e o áudio dos cipós do projeto. O backup das fontes anteriores está em `SourceArt/MacaquiniPendulum_v19`.

Validação nativa em Play Mode: 79 verificações da luta sem erros e 93 da campanha sem erros. Inclui dano sem esquiva, travessia com o dash real, aviso sem dano, pausa, retorno com recarga disponível, limpeza das ameaças e preservação das proporções e da transformação. Verificação estrutural: 2406 GUIDs, cenas e referências aprovados por `Tools/validate_ci.py`.

Referências técnicas consultadas: skill `create-2d-physics/SKILL.md` e seu `references/legacy-2d-physics.md`; `unity-cli/SKILL.md`; [Physics2D.OverlapCapsule](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Physics2D.OverlapCapsule.html) e [Physics2D.SyncTransforms](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Physics2D.SyncTransforms.html).
