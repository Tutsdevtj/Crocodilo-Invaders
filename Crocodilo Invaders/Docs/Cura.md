# Caixa de vida

O port original criava a caixa ao chegar a 13 no contador de abates `powerCount`
(o especial podia acrescentar dois), independentemente da vida do player.
A caixa atravessava a tela e recuperava 6 HP, com animação `hp_up`
e som `powerUp_Sound`. Esse ganho e os recursos visuais/sonoros foram preservados.

## Nova condição

- Só nasce durante o combate de uma onda ou do boss, com player vivo.
- Abaixo ou igual a **65% de HP**, a necessidade deve durar **4 segundos**.
- Abaixo ou igual a **25%**, a espera cai para **1,5 segundo**.
- Precisa faltar pelo menos a quantidade que uma caixa cura, atualmente **6 HP**.
- Há **uma caixa ativa por vez**. Depois de coletar, há **20 segundos** de intervalo;
  se ela passar sem ser coletada, a nova tentativa espera **10 segundos**.
- O intervalo continua valendo no estado crítico. Não há uma corrente infinita
  de caixas ao permanecer com vida baixa.

Vida recuperada antes da espera cancela a necessidade pendente. A pausa congela
esses tempos e a passagem da caixa. Os valores são um primeiro balanceamento,
ajustáveis no Inspector do GameObject `HealthPickupSpawner`.

A caixa nasce além da borda direita da câmera e segue para a esquerda a
**2 unidades/segundo**. O Y é sorteado perto do player, com variação de até
**1,2 unidade**, limitado à área alcançável e acima da HUD/chão. Uma caixa já em
passagem pode ser coletada durante o intervalo entre ondas.

HP cheio não consome a caixa. A cura é limitada ao máximo da entidade, não revive
um player morto e só pode ser aplicada uma vez por pickup. Derrota ou fim do
boss remove as caixas restantes. Não há pontos por coletar vida.

## Responsabilidades e arquivos

- `HealthPickupSpawner.cs`: política de necessidade, intervalos, posição e limite ativo.
- `HealthPickup.cs`: contato físico e tentativa de cura pelo `Health.Heal()` existente.
- `HealthPickupPresentation.cs`: efeito de coleta. O som continua no `PlayerHealthAudio`,
  ouvindo `Health.Healed`, e a HUD continua ouvindo `Health.HealthChanged`.
- `ScrollingLayer.cs`: reutilizado para passagem. Quando existe Rigidbody2D, o movimento
  usa velocidade na física; os elementos visuais anteriores continuam com sua rolagem.
- `Assets/Prefabs/Pickups/HealthBox.prefab`: caixa animada, trigger, corpo cinemático e coleta.
- `Assets/Prefabs/Pickups/HealthRestoreEffect.prefab`: animação original subindo sobre o player.
- `Assets/Art/Pickups` e `Assets/Animations/Pickups`: PNGs originais copiados e animações Unity.

Os arquivos em Resources e o `CrocodiloTextureImporter` permanecem intactos;
somente as cópias necessárias são importadas como Sprites fora de Resources.
O `CrocodiloGame.cs` permanece intacto para a cena antiga.
