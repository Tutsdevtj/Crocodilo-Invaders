using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

// Marcos narrativos permanentes (ex.: gusini_rescued, chapter1_completed) para o futuro HUB e missões.
// Mesmo padrão do ranking: JSON versionado, arquivo temporário e cópia de segurança.
public sealed class CampaignProgressStore
{
    [Serializable] private sealed class SaveData
    {
        public int version = 1;
        public List<string> flags = new List<string>();
    }

    private SaveData data;
    private bool validPrimary, readOnly;
    public string FilePath { get; }
    public string Error { get; private set; }
    public IReadOnlyList<string> Flags => data.flags.AsReadOnly();

    public CampaignProgressStore(string directory)
    {
        FilePath = Path.Combine(directory, "campaign.json");
        validPrimary = TryRead(FilePath, out data);
        if (!validPrimary && !TryRead(FilePath + ".bak", out data)) data = new SaveData();
    }

    public bool Has(string flag) => !string.IsNullOrEmpty(flag) && data.flags.Contains(flag);

    // Idempotente: registrar o mesmo marco de novo não duplica nem reescreve o arquivo.
    public bool Add(string flag)
    {
        if (string.IsNullOrWhiteSpace(flag) || Has(flag)) return false;
        data.flags.Add(flag);
        if (Save()) return true;
        // Falha de disco: não finge que gravou, permitindo nova tentativa.
        data.flags.Remove(flag);
        return false;
    }

    private bool TryRead(string path, out SaveData result)
    {
        result = null;
        if (!File.Exists(path)) return false;
        try
        {
            result = JsonUtility.FromJson<SaveData>(File.ReadAllText(path, Encoding.UTF8));
            if (result == null || result.flags == null || result.version != 1)
            {
                if (result != null && result.version > 1) readOnly = true;
                throw new InvalidDataException("Formato de progresso inválido.");
            }
            result.flags.RemoveAll(string.IsNullOrWhiteSpace);
            return true;
        }
        catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is ArgumentException)
        {
            Error = "Não foi possível ler o progresso. A cópia de segurança será usada quando disponível.";
            result = null;
            return false;
        }
    }

    private bool Save()
    {
        if (readOnly) { Error = "Este progresso foi criado por uma versão mais recente do jogo."; return false; }
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data, true));
            string temporary = FilePath + ".tmp";
            using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }
            if (File.Exists(FilePath)) File.Replace(temporary, FilePath, validPrimary ? FilePath + ".bak" : null);
            else { File.Move(temporary, FilePath); File.Copy(FilePath, FilePath + ".bak", true); }
            validPrimary = true;
            Error = null;
            return true;
        }
        catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
        {
            Error = "Não foi possível salvar o progresso da campanha.";
            Debug.LogWarning(Error + " " + error.Message);
            return false;
        }
    }
}

public static class CampaignProgress
{
    private static CampaignProgressStore store;
    public static CampaignProgressStore Store => store ?? (store = new CampaignProgressStore(Application.persistentDataPath));
    public static event Action<string> FlagRecorded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() { store = null; FlagRecorded = null; }

    // Testes usam uma pasta isolada para não marcar o progresso real do jogador.
    public static void UseDirectory(string directory) => store = new CampaignProgressStore(directory);
    public static bool Has(string flag) => Store.Has(flag);

    public static bool Record(string flag)
    {
        if (!Store.Add(flag)) return false;
        FlagRecorded?.Invoke(flag);
        return true;
    }
}
