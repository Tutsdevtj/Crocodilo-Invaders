using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BossHUD : MonoBehaviour
{
    [SerializeField] private WaveController waves;
    [SerializeField] private GameObject content;
    [SerializeField] private Image healthBar;
    [SerializeField] private Sprite[] healthSprites;
    [SerializeField] private TMP_Text nameText;
    private Boss boss;
    private Health health;

    private void OnEnable()
    {
        waves.BossStarted += Bind;
        waves.RunEnded += Hide;
        if (waves.ActiveBoss != null) Bind(waves.ActiveBoss);
        else content.SetActive(false);
    }

    private void OnDisable()
    {
        waves.BossStarted -= Bind;
        waves.RunEnded -= Hide;
        Unsubscribe();
    }

    private void Bind(Boss target)
    {
        Unsubscribe();
        boss = target;
        health = target.GetComponent<Health>();
        health.HealthChanged += Refresh;
        boss.Combat.EnrageStarted += ShowEnrage;
        content.SetActive(true);
        nameText.text = "EXECUTOR V-9";
        nameText.color = Color.white;
        Refresh(health.CurrentHealth, health.MaxHealth);
    }

    private void Refresh(int current, int maximum)
    {
        int stages = healthSprites.Length - 1;
        int index = Mathf.Clamp(stages - Mathf.CeilToInt(health.Fraction * stages), 0, stages);
        healthBar.sprite = healthSprites[index];
    }

    private void ShowEnrage() => nameText.color = new Color(1f, .5f, .35f);
    private void Hide(bool completed) => content.SetActive(false);

    private void Unsubscribe()
    {
        if (health != null) health.HealthChanged -= Refresh;
        if (boss != null) boss.Combat.EnrageStarted -= ShowEnrage;
    }
}
