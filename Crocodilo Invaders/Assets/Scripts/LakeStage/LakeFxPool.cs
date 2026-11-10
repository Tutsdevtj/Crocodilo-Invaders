using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// Lives in the scene, so everything it holds goes away with the stage.
public sealed class LakeFxPool : MonoBehaviour
{
    private const int PrewarmDrops = 32, PrewarmSplashes = 16, PrewarmRipples = 16, MaxPooled = 256;
    private static LakeFxPool instance;
    public static LakeFxPool Instance
    {
        get
        {
            if (instance == null)
                instance = new GameObject("LakeFxPool").AddComponent<LakeFxPool>();
            return instance;
        }
    }
    private readonly List<LakeDroplet> liveDrops = new List<LakeDroplet>(64);
    private ObjectPool<LakeDroplet> drops;
    private ObjectPool<LakeWaterFX> splashes;
    private ObjectPool<LakeSurfaceRipple> ripples;
    private ParticleSystem splashPixels;
    private ParticleSystem.EmitParams pixel;
    public Material WaterMaterial { get; private set; }
    public int LiveDropCount => liveDrops.Count;
    public int SpareDropCount => drops.CountInactive;

    private void Awake()
    {
        WaterMaterial = LakeBackdrop.Current != null ? LakeBackdrop.Current.WaterFXMaterial : null;
        drops = NewPool(CreateDrop, PrewarmDrops);
        splashes = NewPool(CreateSplash, PrewarmSplashes);
        ripples = NewPool(CreateRipple, PrewarmRipples);
        BuildSplashPixels();
    }
    private ObjectPool<T> NewPool<T>(System.Func<T> create, int prewarm) where T : Component
    {
        var pool = new ObjectPool<T>(create, item => item.gameObject.SetActive(true), item => item.gameObject.SetActive(false),
            item => Destroy(item.gameObject), false, prewarm, MaxPooled);
        for (int i = 0; i < prewarm; i++)
            pool.Release(create());
        return pool;
    }
    private T Child<T>(string name) where T : Component
    {
        var item = new GameObject(name).AddComponent<T>();
        item.transform.SetParent(transform, false);
        return item;
    }
    private LakeDroplet CreateDrop() => Child<LakeDroplet>("PooledLakeDrop");
    private LakeWaterFX CreateSplash()
    {
        var splash = Child<LakeWaterFX>("PooledLakeSplash");
        splash.Build(WaterMaterial);
        return splash;
    }
    private LakeSurfaceRipple CreateRipple()
    {
        var ripple = Child<LakeSurfaceRipple>("PooledLakeRipple");
        ripple.Build(WaterMaterial);
        return ripple;
    }
    public LakeDroplet TakeDrop()
    {
        var drop = drops.Get();
        liveDrops.Add(drop);
        return drop;
    }
    public LakeWaterFX TakeSplash() => splashes.Get();
    public LakeSurfaceRipple TakeRipple() => ripples.Get();
    public static void Return(LakeDroplet drop)
    {
        if (instance == null)
            return;
        instance.liveDrops.Remove(drop);
        instance.drops.Release(drop);
    }
    public static void Return(LakeWaterFX splash)
    {
        if (instance != null)
            instance.splashes.Release(splash);
    }
    public static void Return(LakeSurfaceRipple ripple)
    {
        if (instance != null)
            instance.ripples.Release(ripple);
    }
    public static void ReleaseDrops(ILakeDropOwner owner)
    {
        if (instance == null)
            return;
        for (int i = instance.liveDrops.Count - 1; i >= 0; i--)
            if (instance.liveDrops[i].Owner == owner)
                instance.liveDrops[i].Release();
    }
    private void BuildSplashPixels()
    {
        if (WaterMaterial == null)
            return;
        splashPixels = Child<ParticleSystem>("LakeSplashPixels");
        var main = splashPixels.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 1024;
        main.startSize3D = true;
        main.gravityModifier = LakeWaterFX.PixelGravity / Mathf.Abs(Physics.gravity.y);
        var emission = splashPixels.emission;
        emission.enabled = false;
        var shape = splashPixels.shape;
        shape.enabled = false;
        var fade = splashPixels.colorOverLifetime;
        fade.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
            new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) });
        fade.color = gradient;
        var view = splashPixels.GetComponent<ParticleSystemRenderer>();
        view.sharedMaterial = WaterMaterial;
        view.sortingOrder = 24;
        splashPixels.Play();
    }
    public void EmitPixel(Vector2 position, Vector2 velocity, Vector2 size, float life, Color color)
    {
        if (splashPixels == null)
            return;
        pixel.position = position;
        pixel.velocity = velocity;
        pixel.startSize3D = new Vector3(size.x, size.y, 1);
        pixel.startLifetime = life;
        pixel.startColor = color;
        splashPixels.Emit(pixel, 1);
    }
}
