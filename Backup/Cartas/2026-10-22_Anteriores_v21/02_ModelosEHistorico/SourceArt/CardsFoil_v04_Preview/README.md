# Prévia de foil e full art — 21/10/2026

Pedido do usuário: preservar a direção v03 aprovada, melhorar o encaixe dos
textos e mostrar acabamento progressivamente mais reflexivo e full art nas
raridades superiores. Esta é uma prévia: não modifica nenhum arquivo em Assets.

Progressão proposta, usando dano de ataque como comparação:

- Comum +5%: impressão fosca, arte enquadrada.
- Rara +10%: reflexo de foil somente dentro da janela de arte.
- Épica +15%: foil na imagem e nos componentes metálicos da borda.
- Lendária +20%: arte expandida sob um aro dourado estreito, superfície refletiva
  e realce em relevo do personagem; texto em placas translúcidas.
- Mítica +30%: full art até as bordas com aro mínimo carmesim, difração colorida,
  textura fina holográfica e brilho que preserva a expressão do Croc.

Reflexos variam com a direção da luz e o ponteiro. Raridades superiores recebem
textura adicional; não é apenas um halo externo. A região do rosto recebe menor
intensidade para manter forma, cor e expressão. Os textos são compostos depois
dos reflexos para não perder contraste. Títulos com tamanho e espaçamento
consistentes, acentos de fonte reais, separação entre atributo e percentual,
margem interna nas placas. Os percentuais não mudam.

`full_art_master.png` foi gerado com o atlas v03 como referência de identidade,
pixel art e equipamento. Sem texto, moldura ou brilho holográfico embutido: a
camada foil é dinâmica e separada. Exportações e prancha de comparação em
`Docs/ArtPreviews/CardsFoil_v04`. A prévia interativa da conversa é um fragmento
autossuficiente com imagens embutidas e shader WebGL.

Reexportar e conferir:

```powershell
node 'Crocodilo Invaders/Tools/Art/build_foil_preview_v04.cjs'
node 'Crocodilo Invaders/Tools/Art/verify_foil_preview_v04.cjs'
```

Verificação em Edge headless: shaders compilados, reflexão responde a duas
posições da luz, comum permanece fosca, zero erros JS e sem overflow em 736 e
360 px. Inspeção visual da prancha concluída. Isso não implementa o efeito em
Unity e não representa uma medição de desempenho do jogo.
