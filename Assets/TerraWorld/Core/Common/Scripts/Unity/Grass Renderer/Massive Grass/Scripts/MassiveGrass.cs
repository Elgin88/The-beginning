using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// TW Tweaks
#if UNITY_2019_1_OR_NEWER
using UnityEngine.Rendering;
#else
using UnityEngine.Experimental.Rendering;
#endif
using TerraUnity.Runtime;
// TW Tweaks

namespace Mewlist.MassiveGrass
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    [ExecuteInEditMode]
    public partial class MassiveGrass : MonoBehaviour
    {
        [SerializeField, Range(1, 200)] public int maxParallelJobCount = 4;
        
        [Tooltip("Select whether grass layers' mask data should be embedded in build data or as external files on device while building to target platform." +
            "\nHaving them as external files will decrease build content size especially suited for mobile platforms but it is not recommended unless developer knows how to share external content with users!.")]
        [SerializeField] public bool externalMaskDataFiles = false;

// TW Tweaks
        [SerializeField] public List<MassiveGrassProfile> profiles = default(List<MassiveGrassProfile>);
        //private Texture2D[] masks;
// TW Tweaks

        private readonly Dictionary<Terrain, Dictionary<Camera, RendererCollection>>
            rendererCollections = new Dictionary<Terrain, Dictionary<Camera, RendererCollection>>();

        private MeshFilter meshFilter;
        private Mesh       boundsMesh;


        private MeshFilter MeshFilter => meshFilter ? meshFilter : (meshFilter = GetComponent<MeshFilter>());

// TW Tweaks
        public void RegenerateLayer ()
        {
            Clear();
            SetupBounds();
            Render();
        }
// TW Tweaks

        // Terrain と同じ大きさの Bounds をセットして
        // Terrain が描画されるときに強制的に描画処理を走らせるようにする
        private void SetupBounds()
        {
            if (boundsMesh == null)
            {
                boundsMesh = new Mesh();
                boundsMesh.name = "Massive Grass Terrain Bounds";
            }

            // TODO: correct bounds
            boundsMesh.bounds = new Bounds(Vector3.zero, 50000f * Vector3.one);
            MeshFilter.sharedMesh = boundsMesh;
        }

        private void DestroyBounds()
        {
            if (boundsMesh != null)
            {
                if (Application.isPlaying) Destroy(boundsMesh);
                else                       DestroyImmediate(boundsMesh);
                boundsMesh = null;
            }

            MeshFilter.sharedMesh = null;
        }

        private void OnEnable()
        {
            HaltonSequence.Warmup();
            ParkAndMiller.Warmup();
            SetupBounds();

// TW Tweaks
#if TW_HDRP || TW_URP
            RenderPipelineManager.beginCameraRendering += OnBeginRender; // for SRP
#endif
// TW Tweaks
        }

        private void OnDisable()
        {
// TW Tweaks
#if TW_HDRP || TW_URP
            RenderPipelineManager.beginCameraRendering -= OnBeginRender; // for SRP
#endif
// TW Tweaks

            Clear();
        }

        private void Clear()
        {
            foreach (var cameraCollection in rendererCollections.Values)
                foreach (var massiveGrassRenderer in cameraCollection.Values)
                    massiveGrassRenderer.Dispose();

            rendererCollections.Clear();
            DestroyBounds();
        }

        private void Update()
        {
            Render();
        }

        private void Render()
        {
            foreach (var cameraCollection in rendererCollections.Values)
                foreach (var massiveGrassRenderer in cameraCollection.Values)
                    massiveGrassRenderer.Render();
        }

        private void OnValidate()
        {
            foreach (var cameraCollection in rendererCollections.Values)
                foreach (var massiveGrassRenderer in cameraCollection.Values)
                    foreach (var renderer in massiveGrassRenderer.renderers.Values)
                        renderer.MaxParallelJobCount = maxParallelJobCount;
        }

        private void OnWillRenderObject()
        {
            OnBeginRender(default, Camera.current);
        }

        private void OnBeginRender(ScriptableRenderContext context, Camera camera)
        {
            if (camera == null || profiles == null) return;
            if (!profiles.Any()) return;

            DetectTerrains();

            // カメラ毎に Renderer を作る
            foreach (var keyValuePair in rendererCollections)
            {
                var terrain = keyValuePair.Key;
                if (terrain != null)
                {
                    var cameraCollection = keyValuePair.Value;

                    if (!cameraCollection.ContainsKey(camera))
                        cameraCollection[camera] = new RendererCollection();

                    foreach (var profile in profiles)
                    {
                        // TW Tweaks
                        if (profile == null) continue;
                        if (!profile.isActive) continue;
                        // TW Tweaks

                        cameraCollection[camera]
                            // TW Tweaks
                            //.OnBeginRender(camera, profile, terrain, terrain.terrainData.alphamapTextures, maxParallelJobCount);
                            .OnBeginRender(camera, profile, terrain, null, maxParallelJobCount);
                            // TW Tweaks
                    }
                }
            }

            foreach (var cameraCollection in rendererCollections.Values)
                foreach (var massiveGrassRenderer in cameraCollection.Values)
                    massiveGrassRenderer.Update();
        }

        private int waitCounter = 10;
        private HashSet<Terrain> terrains = new HashSet<Terrain>();

        private void DetectTerrains()
        {
            if (--waitCounter > 0) return;
            var found = FindObjectsOfType<Terrain>();

            foreach (var terrain in found)
            {
// TW Tweaks
                if (terrain.name.Equals("Background Terrain")) continue;
// TW Tweaks

                if (!terrains.Contains(terrain))
                    terrains.Add(terrain);

                if (!rendererCollections.ContainsKey(terrain))
                    rendererCollections[terrain] = new Dictionary<Camera, RendererCollection>();

                var toRemove = new List<Terrain>();

                foreach (var t in terrains)
                {
                    if (t == null) toRemove.Add(t);
                }

                foreach (var t in toRemove)
                {
                    if (rendererCollections.ContainsKey(t))
                    {
                        foreach (var rendererCollection in rendererCollections[t].Values)
                            rendererCollection.Dispose();

                        rendererCollections.Remove(t);
                    }

                    terrains.Remove(t);
                }
            }

            waitCounter = 10;
        }

        private class RendererCollection : IDisposable
        {
            public Dictionary<MassiveGrassProfile, MassiveGrassRenderer> renderers =
                new Dictionary<MassiveGrassProfile, MassiveGrassRenderer>();

            public void OnBeginRender(
                Camera              camera,
                MassiveGrassProfile profile,
                Terrain             terrain,
                Texture2D[]         alphaMaps,
                int                 maxParallelJobCount)
            {
                if (profile == null) return;

                // TW Tweaks
                if (!profile.isActive) return;
                // TW Tweaks

                if (!renderers.ContainsKey(profile))
                {
                    renderers[profile] = new MassiveGrassRenderer(
                        camera,
                        terrain,
                        alphaMaps,
                        profile,
                        maxParallelJobCount);
                }

                renderers[profile].OnBeginRender();
            }

            public void Render()
            {
                foreach (var v in renderers.Values)
                    v.Render();
            }

            public void Dispose()
            {
                foreach (var v in renderers.Values)
                    v.Dispose();
                renderers.Clear();
            }

            public void Update()
            {
                foreach (var v in renderers.Values)
                {
                    v.InstantiateQueuedMesh();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    v.ProcessQueue();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
        }
    }
}

