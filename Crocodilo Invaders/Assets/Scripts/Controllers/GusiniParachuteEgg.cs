using UnityEngine;

[RequireComponent(typeof(Animator))]
public sealed class GusiniParachuteEgg : MonoBehaviour
{
    [SerializeField, Min(0f)] private float initialFallSpeed = 3.4f;
    [SerializeField, Min(0f)] private float freeFallDuration = .1f;
    [SerializeField, Min(0f)] private float parachuteFallSpeed = .65f;
    [SerializeField, Min(.1f)] private float fuseDuration = .25f;
    [SerializeField, Min(0f)] private float yolkSpeed = 6f;
    [SerializeField, Min(.1f)] private float travelSpeed = 5.2f;
    [SerializeField, Min(1f)] private float lifetime = 10f;
    [SerializeField, Min(1)] private int damage = 1;
    [SerializeField, Min(.01f)] private float deployDuration = .21f;
    [SerializeField] private Projectile[] yolkPrefabs;
    [SerializeField] private Vector2[] yolkDirections;
    private Animator animator;
    private GusiniAudio audio;
    private bool crack1Played, crack2Played;
    private Transform threatRoot;
    private Collider2D owner;
    private float age;
    private float arrivedAt;
    private Vector2 destination;
    private bool deploying, opened, cracking, burst;
    public Vector2 Destination => destination;
    public bool HasArrived { get; private set; }
    public float FallSpeed { get; private set; }

    private void Awake() => animator = GetComponent<Animator>();
    public void SetAudio(GusiniAudio coordinator) => audio = coordinator;
    public void Begin(Transform root, Collider2D ownerCollider, Vector2 burstDestination)
    {
        threatRoot = root;
        owner = ownerCollider;
        destination = burstDestination;
    }

    private void Update()
    {
        if (threatRoot == null || !threatRoot.gameObject.activeInHierarchy) { Destroy(gameObject); return; }
        age += Time.deltaTime;
        if (!deploying && age >= freeFallDuration)
        {
            deploying = true;
            animator.SetTrigger("Deploy");
            if (audio != null) audio.Play(GusiniSound.Deploy, transform.position);
        }
        FallSpeed = !deploying ? initialFallSpeed : Mathf.Lerp(initialFallSpeed, parachuteFallSpeed,
            Mathf.Clamp01((age - freeFallDuration) / deployDuration));
        if (!burst && !opened) transform.position += Vector3.down * (FallSpeed * Time.deltaTime);
        if (opened && !HasArrived)
        {
            Vector2 next = Vector2.MoveTowards(transform.position, destination, travelSpeed * Time.deltaTime);
            transform.position = new Vector3(next.x, next.y, transform.position.z);
            if (Vector2.Distance(next, destination) < .001f)
            {
                HasArrived = true;
                arrivedAt = Time.time;
                FallSpeed = 0f;
            }
        }
        if (opened && HasArrived && !cracking && Time.time - arrivedAt >= fuseDuration)
        {
            cracking = true;
            animator.SetTrigger("Crack");
        }
        if (HasArrived || burst) FallSpeed = 0f;
        if (age > lifetime) Destroy(gameObject);
    }

    public void OnParachuteOpened()
    {
        if (opened || !deploying) return;
        opened = true;
        if (audio != null) audio.Play(GusiniSound.Flutter, transform.position, transform);
    }

    public void OnCrack1()
    {
        if (!cracking || crack1Played || audio == null) return;
        crack1Played = true;
        audio.StopOwnerLoop(transform);
        audio.Play(GusiniSound.Crack1, transform.position);
    }
    public void OnCrack2()
    {
        if (!cracking || crack2Played || audio == null) return;
        crack2Played = true;
        audio.Play(GusiniSound.Crack2, transform.position);
    }

    public void OnYolkRelease()
    {
        if (burst || !cracking || threatRoot == null || !threatRoot.gameObject.activeInHierarchy) return;
        burst = true;
        FallSpeed = 0f;
        for (int i = 0; i < yolkPrefabs.Length; i++)
        {
            Projectile yolk = Instantiate(yolkPrefabs[i], transform.position, Quaternion.identity, threatRoot);
            yolk.Launch(yolkDirections[i], owner, yolkSpeed, damage / yolk.BaseDamage);
        }
        if (audio != null)
        {
            audio.StopOwnerLoop(transform);
            audio.Play(GusiniSound.Burst, transform.position);
            audio.Play(GusiniSound.YolkWhoosh, transform.position);
        }
    }

    public void OnBurstFinished() => Destroy(gameObject);
    private void OnDisable() { if (audio != null) audio.StopOwnerLoop(transform); }
}
