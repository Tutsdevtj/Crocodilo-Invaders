using System;

[Serializable]
public sealed class EnemyWavePool
{
    public EnemyWave[] options;

    public bool Validate(out string error)
    {
        if (options == null || options.Length == 0)
        {
            error = "o pool precisa de pelo menos uma composição.";
            return false;
        }
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] != null && options[i].IsValid) continue;
            error = $"a opção {i + 1} é nula ou tem grupos, intervalo ou limite de vivos inválidos.";
            return false;
        }
        error = null;
        return true;
    }

    public EnemyWave Pick(Random random)
    {
        if (random == null) throw new ArgumentNullException(nameof(random));
        if (!Validate(out string error)) throw new InvalidOperationException(error);
        return options[random.Next(options.Length)];
    }
}
