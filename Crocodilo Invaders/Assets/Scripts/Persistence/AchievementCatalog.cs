using System;
using System.Collections.Generic;

public sealed class AchievementDefinition
{
    public readonly string Id, Name, Description;
    public readonly int Goal;
    public readonly bool Hidden;
    public AchievementDefinition(string id, string name, string description, int goal = 1, bool hidden = false)
    { Id = id; Name = name; Description = description; Goal = goal; Hidden = hidden; }
}

public static class AchievementCatalog
{
    public const string FirstWave = "first_wave", Dodge = "sport_mode", Special = "boss_special", CleanWave = "clean_wave",
        Gusini = "gusini_defeated", Macaquini = "macaquini_defeated", Sapeneu = "sapeneu_defeated",
        Cards = "all_card_kinds", Mythic = "first_mythic", LastTooth = "last_tooth";
    static readonly AchievementDefinition[] definitions = {
        new AchievementDefinition(FirstWave, "Primeiro Expediente", "Conclua a primeira onda."),
        new AchievementDefinition(Dodge, "Modo Esportivo", "Desvie de 10 ataques usando o dash.", 10),
        new AchievementDefinition(Special, "Bafo de Respeito", "Acerte um especial em um boss."),
        new AchievementDefinition(CleanWave, "Casco Intacto", "Conclua uma onda sem sofrer dano."),
        new AchievementDefinition(Gusini, "Ganso Depenado", "Derrote o Gusini nas duas formas."),
        new AchievementDefinition(Macaquini, "Descascou o Problema", "Derrote o Macaquini."),
        new AchievementDefinition(Sapeneu, "Borracha Furada", "Derrote o Sapeneu Cururu."),
        new AchievementDefinition(Cards, "Carteira Assinada", "Escolha uma carta de cada um dos cinco tipos.", 5),
        new AchievementDefinition(Mythic, "Hoje Eu Tô Mítico", "Escolha sua primeira carta mítica."),
        new AchievementDefinition(LastTooth, "Por Um Dente", "Derrote um boss com até 10% da vida restante.", 1, true)
    };
    public static IReadOnlyList<AchievementDefinition> All => Array.AsReadOnly(definitions);
    public static AchievementDefinition Get(string id)
    { foreach (var item in definitions) if (item.Id == id) return item; return null; }
}
