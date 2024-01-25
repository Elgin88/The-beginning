using UnityEngine;
using TerraUnity.Runtime;

public class BendingImpulse : MonoBehaviour
{
    public BendGrassWhenEnabled BGWE;
    public float destrcutionTime = 3f;
    public float radius = 30f;
    public float dampingSpeed = 10f;
    public float returnSpeed = 0.5f;
    public float explostionLightIntensity;
    public Color explostionColor = new Color(1, 1, 1, 1);
    private float lifeTime = 0;
    private Light explosionLight;

    void Start()
    {
        InitExplosionLight();
        Destroy(this.gameObject, destrcutionTime);
    }

    private void InitExplosionLight()
    {
        explosionLight = gameObject.AddComponent<Light>();
        explosionLight.color = explostionColor;
        explosionLight.range = radius;
        explosionLight.intensity = explostionLightIntensity;
        explosionLight.shadows = LightShadows.Hard;
    }

    private void Update()
    {
        if (BGWE != null)
        {
            lifeTime += Time.deltaTime;

            if (lifeTime < destrcutionTime - 1)
                BGWE.BendRadius = Mathf.SmoothStep(0f, radius, Mathf.PingPong(lifeTime / dampingSpeed * radius, radius));
            else
                BGWE.BendRadius = Mathf.Lerp(BGWE.BendRadius, 0f, Time.deltaTime * returnSpeed);
        }

        explosionLight.intensity = Mathf.Lerp(explosionLight.intensity, 0f, Time.deltaTime * 3);
    }
}

