# Criaturas apoiadas nas vitórias-régias v24

Sapinhos e besouros da travessia agora entram pela superfície da água sobre folhas pequenas, reutilizando os sprites rasos aprovados da v16. Os troncos presos aos sapinhos foram removidos. Mosquitos e libélulas continuam aéreos.

Cada criatura reserva uma posição de apoio; as folhas acompanham a corrente juntas na horizontal, com oscilação vertical e inclinação próprias. Folha e passageiro usam a mesma pose física planejada e interpolação, mantendo os pés alinhados durante chegada, espera e cuspida. O ponto de contato considera o pivô da arte.

O besouro prepara o ataque sobre a folha, salta em arco e volta ao apoio antes de expor a barriga. A aterrissagem produz um pequeno afundamento amortecido da folha. Ao morrer, a criatura libera a folha, que deriva e desaparece. Encerrar a horda ou perder a run limpa apoios temporários. As seis folhas decorativas existentes continuam separadas.

Mantidos os avisos pelas poses e cores dos personagens, sem círculos de mira. Não há novas colisões ou forças sobre a nave. Os dois antigos apoios grandes do boss não foram restaurados.

Validação nativa: `LakeJourneyLilyValidation.Run`, capturas e gravação em `Docs/ArtPreviews/LagoDoPneu_EnemyLilies_v24`; logs e restore em `Logs/LagoDoPneu/v24`. A câmera de captura fixa 16:9 para evitar mudança de proporção entre o cálculo físico e a renderização de teste.

Resultados: 17 verificações de apoio, movimento, pausa, salto, aterrissagem e limpeza, mais 75 verificações da travessia completa até o boss, todas sem erros. Checkout: 2607 GUIDs válidos. Os apoios usam somente folhas planas sem flor, para a altura da flor não influenciar o plano de contato dos pés.

Build: `Builds/Windows-LagoDoPneu-EnemyLilies-v24`. Pacote completo em `Builds/Pacotes/CrocodiloInvaders-LagoDoPneu-CriaturasNasFolhas-v24-TESTE-LOCAL-Windows.zip`.

Entrega local, sem commit ou push. Extrair o pacote inteiro, abrir `CrocodiloInvaders.exe` e escolher **FASE 2 · LAGO DO PNEU**. Setas movem, Espaço atira, C usa especial, Shift faz dash e Esc pausa.
