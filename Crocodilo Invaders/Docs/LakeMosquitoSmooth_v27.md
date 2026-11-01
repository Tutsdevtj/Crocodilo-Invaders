# Mosquitos — suavidade v27

Retomados os desenhos originais aprovados para os dois sentidos. O retorno usa espelhamento dos mesmos quadros, preservando tamanho, alinhamento e identidade visual.

Removido o achatamento de 65% durante a virada. A orientação usa velocidade horizontal amortecida, com limiar para evitar trocas rápidas perto do ponto de retorno. Inclinação leve, limitada a 3 graus. Mosquitos pequenos mantêm 14 quadros/s; os maiores passam a 12 quadros/s, sem mudar a frequência das asas durante o aviso. As fases das asas variam por inseto.

Mantidos voo durante a fila, avisos curtos e investidas. Demais correções v26 continuam ativas. Nenhuma nova imagem gerada; a arte de voo para a direita da v26 fica preservada como fonte, fora da animação ativa.

Validação nativa LakeMosquitoSmoothValidation: acompanha as duas espécies em voo e retorno, registra os quatro quadros originais de cada uma, confere escala constante, limite de inclinação, orientação dos dois lados, investida e pausa. Vídeo real em Docs/ArtPreviews/LakeMosquitoSmooth_v27/mosquitos-suaves-v27.mp4.

Teste: menu FASE 2 · LAGO DO PNEU, a partir da primeira horda. Sem novos commits/push ou atualização da PR.


Resultado: sete verificações nativas passaram, zero erros. Checkout com 2627 GUIDs válidos. Vídeo decodificado sem erros. A mudança é visual; os novos movimentos da fila e ataques da v26 foram preservados.

Build Windows-LagoDoPneu-MosquitoSmooth-v27 concluída com sucesso. O auxiliar temporário de captura e conferência foi arquivado em Logs/LagoDoPneu/v27/CaptureHelper; não acrescenta uma suíte permanente para este ajuste visual.
