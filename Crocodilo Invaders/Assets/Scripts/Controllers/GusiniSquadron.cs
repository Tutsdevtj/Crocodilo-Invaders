using UnityEngine;

public sealed class GusiniSquadron : MonoBehaviour
{
    [SerializeField, Min(.1f)] private float speed = 7.2f;
    [SerializeField, Min(.1f)] private float lifetime = 5f;
    [SerializeField] private float despawnX = -12f;
    [SerializeField, Min(.1f)] private float spacing = 1f;
    [SerializeField] private GusiniSquadronGoose[] geese;
    [SerializeField] private Vector2[] slotOffsets;
    [SerializeField] private bool[] variantB;
    [SerializeField] private float[] flightStartTimes;
    [SerializeField] private float[] chorusDelays;
    private float age;

    private void Start()
    {
        for (int i = 0; i < geese.Length; i++)
        {
            geese[i].transform.localPosition = slotOffsets[i] * spacing;
            geese[i].Configure(variantB[i], flightStartTimes[i]);
            geese[i].Sing(chorusDelays[i]);
        }
    }

    private void Update()
    {
        transform.position += Vector3.left * (speed * Time.deltaTime);
        age += Time.deltaTime;
        // Inclui a cauda do V, não remove o grupo quando apenas o líder sai.
        if (age >= lifetime || transform.position.x + 2f * transform.lossyScale.x * spacing < despawnX)
            Destroy(gameObject);
    }
}
