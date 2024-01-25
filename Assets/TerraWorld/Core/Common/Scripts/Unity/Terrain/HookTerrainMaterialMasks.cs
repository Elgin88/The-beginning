using UnityEngine;

[ExecuteInEditMode]
public class HookTerrainMaterialMasks : MonoBehaviour
{
    private Material terrainMaterial;
    private TerrainLayer[] terrainLayers;

    private void OnEnable()
    {
        if (Approved()) UpdateShaders();
    }

    private void Start()
    {
        if (Approved()) UpdateShaders();
    }

    private void OnValidate()
    {
        if (Approved()) UpdateShaders();
    }

    void Update()
    {
        //if (!Application.isPlaying)
            //UpdateShaders();
    }

    private bool Approved ()
    {
        Terrain terrain = GetComponent<Terrain>();
        if (terrain == null) return false;
        terrainMaterial = terrain.materialTemplate;
        if (terrainMaterial == null) return false;
        if (terrainMaterial.shader != Shader.Find("TerraUnity/TerraFormer") && terrainMaterial.shader != Shader.Find("TerraUnity/TerraFormer Instanced")) return false;
        terrainLayers = terrain.terrainData.terrainLayers;
        return true;
    }

    private void UpdateShaders ()
    {
        for (int i = 0; i < terrainLayers.Length; i++)
            if (terrainLayers[i] != null && terrainLayers[i].maskMapTexture != null)
                terrainMaterial.SetTexture("_Maskmap" + i.ToString(), terrainLayers[i].maskMapTexture);
    }
}

