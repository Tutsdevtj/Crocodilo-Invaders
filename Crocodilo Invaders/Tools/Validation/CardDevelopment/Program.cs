using System.Reflection;

int checks = 0;
void Require(bool condition, string label)
{ checks++; if (!condition) throw new Exception(label); }
var catalog = new UpgradeCardCatalog();
var cards = new List<UpgradeCard>();
for (int kind = 0; kind < 5; kind++) for (int rank = 0; rank < 5; rank++)
    cards.Add(new UpgradeCard { kind = (UpgradeKind)kind, rarity = (UpgradeRarity)rank, percent = rank + 1,
        artworkPath = $"Cards/{(UpgradeKind)kind}/{(UpgradeRarity)rank}" });
typeof(UpgradeCardCatalog).GetField("cards", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(catalog, cards.ToArray());
Require(catalog.IsValid, "Valid catalog");
string Keys(UpgradeCard[] values) => string.Join(",", values.Select(c => $"{c.kind}:{c.rarity}"));

DevelopmentCardSession.End();
foreach (var mode in Enum.GetValues<DevelopmentCardSession.OfferMode>())
{
    DevelopmentCardSession.Mode = mode;
    Require(Keys(DevelopmentCardSession.RollOffers(catalog, new Random(112), .5f)) == Keys(catalog.RollOffers(new Random(112), .5f)),
        "Inactive development settings must never change normal offers");
}
DevelopmentCardSession.Begin();
DevelopmentCardSession.Mode = DevelopmentCardSession.OfferMode.Normal;
foreach (float progress in new[] { 0f, .5f, 1f })
    Require(Keys(DevelopmentCardSession.RollOffers(catalog, new Random(731), progress)) == Keys(catalog.RollOffers(new Random(731), progress)),
        "Normal test mode must use production randomness");

DevelopmentCardSession.Mode = DevelopmentCardSession.OfferMode.FixedRarity;
foreach (var rarity in Enum.GetValues<UpgradeRarity>())
{
    DevelopmentCardSession.Rarity = rarity;
    for (int i = 0; i < 50; i++)
    {
        var offers = DevelopmentCardSession.RollOffers(catalog, new Random(i), .5f);
        Require(offers.Length == 3 && offers.All(c => c.rarity == rarity), "Fixed rarity applies to all three");
        Require(offers.Select(c => c.kind).Distinct().Count() == 3, "Fixed mode preserves distinct kinds");
        Require(offers.All(c => ReferenceEquals(c, catalog.Get(c.kind, c.rarity))), "Uses real catalog entries");
    }
}
DevelopmentCardSession.Mode = DevelopmentCardSession.OfferMode.AllCards;
DevelopmentCardSession.RestartSequence();
var visited = new HashSet<string>();
for (int offer = 0; offer < 25; offer++)
{
    var generated = DevelopmentCardSession.RollOffers(catalog, new Random(offer), 0f);
    Require(generated.Length == 3 && generated.Select(c => c.kind).Distinct().Count() == 3, "Cycle keeps three distinct kinds");
    for (int i = 0; i < 3; i++)
    {
        int slot = (offer * 3 + i) % 25;
        Require(generated[i].kind == (UpgradeKind)(slot % 5) && generated[i].rarity == (UpgradeRarity)(slot / 5), "Ordered complete cycle");
        visited.Add($"{generated[i].kind}:{generated[i].rarity}");
    }
    if (offer == 8) Require(visited.Count == 25, "All 25 covered in nine offers");
}
Require(DevelopmentCardSession.SequenceCursor == 0, "Cycle wraps after 25 offers");
DevelopmentCardSession.End();
Require(!DevelopmentCardSession.IsActive && DevelopmentCardSession.SequenceCursor == 0, "Session ends cleanly");
var controller = new CardRewardController();
var pause = new PauseController();
void SetField(string name, object value) => typeof(CardRewardController).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(controller, value);
void SetProperty(string name, object value) => typeof(CardRewardController).GetProperty(name).SetValue(controller, value);
SetField("catalog", catalog); SetField("view", new CardRewardView()); SetField("pause", pause);
SetProperty("RerollsRemaining", 1); SetProperty("State", CardRewardController.RewardState.Ready);
Require(controller.TryReroll() && controller.RerollsRemaining == 0, "Normal game spends its one reroll");
SetProperty("State", CardRewardController.RewardState.Ready);
Require(!controller.TryReroll(), "Normal game rejects the second reroll");
DevelopmentCardSession.Begin();
DevelopmentCardSession.Mode = DevelopmentCardSession.OfferMode.AllCards;
for (int i = 0; i < 200; i++)
{
    SetProperty("State", CardRewardController.RewardState.Ready);
    Require(controller.TryReroll(), "DEV rerolls repeatedly even at zero remaining");
    Require(controller.RerollsRemaining == 0, "DEV counter never decreases or underflows");
    Require(!controller.TryReroll(), "Reroll animation blocks overlapping rerolls");
}
SetProperty("State", CardRewardController.RewardState.Ready);
pause.IsPaused = true;
Require(!controller.TryReroll(), "Pause still blocks DEV rerolls");
pause.IsPaused = false;
DevelopmentCardSession.End();
Require(!controller.HasUnlimitedRerolls && !controller.TryReroll(), "Unlimited rerolls do not leak into normal mode");
Console.WriteLine($"CARD DEVELOPMENT DATA VALIDATION PASSED: {checks} checks. Production catalog/weights/session code. UI and Play Mode require Unity.");
