# Macaquini lutador v22 — segunda forma jogável

A segunda forma agora usa combinações de golpes físicos, com aproximação rápida, avisos de postura, investida e recuperação. A primeira forma de jetpack, a transformação, o chute com volta pelo fundo e o HUD existente são preservados.

## Leitura da luta

- **Punhos dourados e guarda de boxe:** soco direto ou cruzado. Trajetória travada antes da investida; desviar na vertical.
- **Punho levantado e arco azul:** gancho ascendente. O macaco se posiciona por baixo e sobe; sair para o lado.
- **Duas mãos acima da cabeça e círculo vermelho:** martelo. O macaco sobe e mergulha na área marcada; sair do círculo.
- **Anel verde e recuo:** recuperação e abertura para contra-atacar.

Quatro combinações de 2–3 golpes passam por uma sacola embaralhada, sem repetir a última combinação na troca de sacola. Algumas terminam com o chute já aprovado; o pêndulo aparece ocasionalmente. Abaixo de 45% de vida, os avisos iniciais caem de 0,68 para 0,52 s e o recuo fica mais curto. A direção não passa a perseguir a nave.

O coordenador reserva pelo menos 1,14 s entre o fim de uma janela de dano e a próxima ameaça, ou a recarga real do dash +0,04 s se ela for maior. Socos causam 1, gancho e martelo 2; um acerto por investida. A proteção real do dash é respeitada. Esses valores são a base jogável para avaliação humana, sem afirmar equilíbrio definitivo.

## Reforços

A cada algumas rodadas entram 2 ajudantes, ou 3 abaixo da metade da vida: Batedor cobre, Acrobata castanho e Rolador cinza-azulado. Cada um tem sua preparação e uma investida própria. Apenas um ataca por vez e o boss aguarda durante a chamada. Ajudantes têm 2/2/3 HP, causam 1 de dano por passagem e concedem 200 pontos e um abate ao morrer. Saem depois da passagem; transformação, morte, cancelamento, chute e pêndulo não deixam ajudantes ativos sobrepostos.

## Arte

Sprites completos em `Assets/Art/JungleStage/Macaquini_v22`. Fontes, restauração e registro em `SourceArt/MacaquiniFighter_v22`. Exportador: `Tools/Art/prepare_macaquini_fighter_v22.py`.

Transparência binária, RGB zerado fora da silhueta, componentes isolados removidos, mãos/pés/rabos com margem segura, registro comum na pelve, paleta compartilhada de 96 cores no boss e 64 nos ajudantes, tons verdes reservados para os olhos. Importação em 64 PPU, Point, sem mipmaps nem compressão. A primeira forma e as artes anteriores permanecem intactas. Conferência em fundo quadriculado: `Docs/ArtPreviews/MacaquiniFighter_v22/pixel-art-clean-cutouts.png`.

## Testar

`MacaquiniFighterTest` abre direto na transformação para a segunda forma. O treino tem 24 HP e proteção desligada. Espaço atira, Shift dá dash, H cura, I alterna proteção, R reinicia, Esc pausa. F1 força soco, F2 gancho, F3 martelo, F4 pêndulo, F5 chute, F6 combinação, F7 reforços. A campanha continua com o fluxo normal das duas formas.

Implementação: `MacaquiniFighter.cs`, `MacaquiniFighterMinion.cs`, integração ao `MacaquiniCombat` e à apresentação de corpo completo. Instalação/build via `MacaquiniFighterV22Tools`. Teste nativo: `MacaquiniFighterPlayValidation.Run`.

Validação final: 84 verificações da luta nova, 90 de regressão da primeira forma/transformação/chute e 93 da campanha, todas sem erros. Checkout aprovado com 2434 GUIDs. Build Windows validado por 12 s de inicialização e pacote ZIP com 219 arquivos conferidos por hash. SHA256 do pacote: `294876C82D95274078B5520E1CDA58111ED1A85558DCFF5ABBF79C75E998220B`. A prévia dos ajudantes também foi atualizada com os seis PNGs de produção e verificada no navegador, sem erros ou overflow.

Referências consultadas: skills `create-2d-physics/SKILL.md`, `references/legacy-2d-physics.md`, `unity-cli/SKILL.md`, `ui/SKILL.md`, `ui-ugui/SKILL.md`; [Physics2D.OverlapCapsule](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Physics2D.OverlapCapsule.html) e [Physics2D.SyncTransforms](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Physics2D.SyncTransforms.html).
