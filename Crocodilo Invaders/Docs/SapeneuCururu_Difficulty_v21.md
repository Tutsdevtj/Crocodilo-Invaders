# Sapeneu Cururu — dificuldade v21

Protótipo local, sem commit, push ou atualização da PR. Implementa o conjunto de ataques aprovado em 29/10/2026.

- Salto: trava a posição horizontal do jogador antes do aviso de 0,9 s; altura depende da posição avisada. O destino não segue a nave depois do aviso.
- Impacto: primeira onda rápida e baixa (3 gotas; 4 na fúria). Após 0,32 s, segunda onda mais alta e com deslocamento horizontal lento (2 gotas; 3 na fúria). Mantém os totais anteriores de 5/7 gotas.
- Moscas: continuam pequenas, em reserva de três, surgindo ao longo da luta. Após permanecer em uma região de 0,85 unidade por 1,5 s, algumas novas moscas surgem perto desse corredor, inclusive no alto. Só a primeira mosca é colocada obrigatoriamente atrás da nave. Uma única mosca agitada anuncia cada linguada; a mira trava antes do golpe.
- Saperereca: mergulho de 0,42 s, deslocamento submerso de 0,72 s, bolhas no destino, aviso de mira de 0,65 s, emergência de 0,25 s. Rajada de três tiros separados por 0,11 s; trajetória central calculada para alcançar a posição travada, sem o antigo limite vertical. Usa as poses existentes e o recorte da superfície da água.
- Fúria: durante a sequência de língua, a Saperereca pode mergulhar e preparar a próxima rajada. Só dispara depois que a língua recolhe. O Sapeneu espera a rajada e suas gotas terminarem antes de outro ataque.
- Arroto: janela de 1,4 s, multiplicador de dano recebido x2. A Saperereca aguarda terminar sua sequência para liberar essa abertura.

HP 200 preservado. As seis folhas pequenas continuam à deriva; os dois apoios grandes permanecem removidos. Fundo, pássaros, peixes, piranhas, HUD de água e abertura de onda 10 continuam integrados.

## Implementação e validação

Física 2D legada (`Rigidbody2D`, contato varrido via `LakeHit`), sem alterar a física da nave. `LakeDiveBubbles` desenha sinais com o material de água existente, sem criar materiais por quadro. Pausa usa tempo escalado. Encerrar combate remove avisos, bolhas, língua e gotas, incluindo a segunda onda pendente.

`LakeDifficultyPlayValidation.RunFromMenu` entra pelo menu DEV na fase real e registra o combate. Evidências em `Logs/LagoDoPneu/v21`; imagens e vídeo em `Docs/ArtPreviews/SapeneuCururu_Difficulty_v21`. Restore dos três scripts anteriores em `Logs/LagoDoPneu/v21/restore`. A validação automatizada confere comportamento; o ajuste final de dificuldade depende do teste jogando.

Resultado nativo: **32 verificações, zero erros**. Inclui entrada real, 200 HP, mira do salto travada, ambas as ondas (5 gotas normais/7 na fúria), pressão contra camping alto, mergulho submerso e bolhas, balística até o corredor alto, pausa, sequência coordenada, contato real da gota consumido durante dash sem dano, contato sem proteção causando exatamente 1 HP e encerramento sem ameaças restantes. Checkout: 2586 GUIDs válidos.

Build local: `Builds/Windows-SapeneuCururu-Difficulty-v21/CrocodiloInvaders.exe`. Teste pelo menu DEV → bosses → Sapeneu Cururu. Controles: setas para mover, Espaço para tiro e Shift para dash. Distribuir a pasta inteira ou o ZIP; o EXE sozinho não contém as dependências.
