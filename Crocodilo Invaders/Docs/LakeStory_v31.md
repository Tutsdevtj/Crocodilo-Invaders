# Água parada — integração v31

A fase 2 usa a mesma caixa de diálogo, os controles e os sons de digitação do encontro Croc/Gusini. O elenco e as sequências do lago são próprios: o original do Gusini não foi alterado.

## Quatro cenas

1. Abertura antes da onda 1: Croc encontra a captação seca e a represa.
2. Encontro após a apresentação e a chegada física de Sapeneu: disputa pela água e apresentação do canal/berçário.
3. Metade da vida, uma vez: diálogo de fúria; Saperereca salta do HUD no marco `join` da fala “Daqui eu cuido”.
4. Vida zerada: Sapeneu fica exausto, o casal permanece no lago e Croc abre a passagem lateral. O marco `channel` remove a sucata em 1,7 s e inicia o escoamento. O capítulo só termina ao concluir/pular essa cena.

Enter, clique ou A avançam/revelam o texto; segurar Tab pula. Pausa congela o diálogo e a água normalmente. Durante a conversa, jogador e boss ficam protegidos, com o combate bloqueado; o cenário continua animado. Sair da cena cancela callbacks e libera as proteções pertencentes ao diálogo. Pular o final abre o canal antes da conclusão, sem repetir marcos da campanha. DEV não grava progresso de campanha.

## Arte e dados

- Quatro retratos do Sapeneu: orgulhoso, sério, furioso e vulnerável.
- Dois da Saperereca: firme e aliviada.
- Captação, canal e berçário surgem como cenário, sem hitboxes: início, ondas 4+ e ondas 7+, respectivamente.
- Fontes e prompts: SourceArt/LakeStory_v31. Recortes nativos: Assets/Resources/LakeStoryV31/art.asset.
- Elenco e 32 falas editáveis: Assets/Data/Dialogue/LakeStoryV31.
- LakeStoryTools.Install instala/reinstala nas cenas StageTwoLake e SapoDoPneuTest por APIs nativas, sem editar YAML.

O desfecho representa o escoamento e a proteção do berçário. Gestos opcionais do roteiro, como a mosca final pousando na cabeça do sapo, não receberam uma nova animação nesta versão.

## Validação

LakeStoryPlayValidation.RunAll verifica abertura e encontro/fúria/final; RunSkip cobre pulo antecipado e exatamente dois marcos de campanha em save isolado. Resultado: 31 verificações, 0 erros (6 abertura, 20 encontro/fúria/final, 5 skip/progresso). Evidências em Logs/LagoDoPneu/v31. Capturas reais em 1280×720, 1920×1080 e 800×600.

A suíte antiga DialoguePlayValidation avançou 163 verificações do Gusini, incluindo input, áudio, pausa, sequência final e conclusão única. Seu último passo ainda espera o antigo fallback de destino indisponível; agora BaseImprovisada existe, e esse passo terminou por timeout. O elenco e o prefab de diálogo originais permaneceram inalterados.

Integração v31 preparada para entrega na branch codex/sapo-do-pneu, com arte, implementação e validação em commits separados.

## Versão jogável

Build Windows-LakeStory-v31 concluída: 212383180 bytes. Inicialização por 8 s sem exceções detectadas. ZIP em Builds/Pacotes/CrocodiloInvaders-AguaParada-v31-TESTE-LOCAL-Windows.zip: 190 arquivos, 134361493 bytes, todas as entradas conferidas por SHA256. Hash do pacote: `d5c1a3c03d9300533c431ffb3c12a9832827ca1ae3e6203ae8967c49ee4dbc24`.

## Vídeo

[Prévia completa com áudio — 4min23s](ArtPreviews/LakeStory_v31/agua-parada-v31.mp4). Captura em Play Mode com corte entre abertura e teste do boss, jogador protegido e dano controlado nos trechos demonstrativos. Inclui todas as falas, entrada da Saperereca e abertura do canal; arquivo integralmente decodificado sem erros.
