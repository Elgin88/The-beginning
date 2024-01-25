using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Mewlist.MassiveGrass
{
    public class MassiveGrassRenderer : IDisposable, ICellOperationCallbacks
    {
        private int                 maxParallelJobCount;
        private IReadOnlyCollection<Texture2D> alphaMaps;

        private readonly Camera              camera;
        private readonly Terrain             terrain;
        private readonly MassiveGrassGrid    grid;
        private readonly MeshBuilder         meshBuilder;

        // TW Tweaks
        public MassiveGrassProfile profile;
        //private readonly MassiveGrassProfile profile;
        // TW Tweaks

        public int MaxParallelJobCount
        {
            get { return maxParallelJobCount; }
            set { maxParallelJobCount = Mathf.Max(1, value); }
        }
        public MassiveGrassGrid Grid => grid;

        private Dictionary<MassiveGrassGrid.CellIndex, Mesh>    meshes
            = new Dictionary<MassiveGrassGrid.CellIndex, Mesh>();
        private HashSet<MassiveGrassGrid.CellIndex>             activeIndices
            = new HashSet<MassiveGrassGrid.CellIndex>();
        private Dictionary<MassiveGrassGrid.CellIndex, Request> requestQueue
            = new Dictionary<MassiveGrassGrid.CellIndex, Request>();
        private Queue<(MassiveGrassGrid.CellIndex, MeshData)> composedData
            = new Queue<(MassiveGrassGrid.CellIndex, MeshData)>();

        public MassiveGrassRenderer(
            Camera camera,
            Terrain terrain,
            IReadOnlyCollection<Texture2D> alphaMaps,
            MassiveGrassProfile profile,
            int maxParallelJobCount)
        {
            this.camera    = camera;
            this.terrain   = terrain;
            this.profile   = profile;
            this.alphaMaps = alphaMaps;
            MaxParallelJobCount = maxParallelJobCount;

            var terrainSize = terrain.terrainData.bounds.size.x;
            grid        = new MassiveGrassGrid(terrain, Mathf.CeilToInt(terrainSize / profile.GridSize));
            meshBuilder = new MeshBuilder();
        }


        public void Render()
        {
            foreach (var keyValuePair in meshes)
            {
                var mesh = keyValuePair.Value;

                // TW Tweaks
                if (mesh.vertexCount > 0)
                {
                    //if (TerraUnity.Runtime.TTerraWorldManager.TerrainColors != null)
                    //{
                    //    //Debug.Log(mesh.colors[0]);
                    //    //Debug.Log(mesh.vertexCount);
                    //    //Debug.Log(meshes.Count);
                    //
                    //    MaterialPropertyBlock MPB = new MaterialPropertyBlock();
                    //    Color pointColor = mesh.colors[0];
                    //    //Color pointColor = mesh.colors[UnityEngine.Random.Range(0, mesh.colors.Length - 1)];
                    //    MPB.SetColor("_Color", pointColor);
                    //
                    //    Graphics.DrawMesh
                    //    (
                    //        mesh,
                    //        Vector3.zero,
                    //        Quaternion.identity,
                    //        profile.Material,
                    //        profile.Layer,
                    //        null,
                    //        0,
                    //        MPB,
                    //        profile.CastShadows
                    //    );
                    //}
                    //else
                    {
                        Graphics.DrawMesh
                            (
                            mesh,
                            Vector3.zero,
                            Quaternion.identity,
                            profile.Material,
                            profile.Layer,
                            null,
                            0,
                            null,
                            profile.CastShadows
                        );
                    }
                }
                // TW Tweaks
            }
        }

        private Stack<Mesh> pool = new Stack<Mesh>();

        private async Task Build(Request request)
        {
            var index = request.Index;
            var rect  = request.Rect;

            try
            {
                var meshData = await meshBuilder.BuildMeshData(terrain, alphaMaps, index, rect, profile);
                composedData.Enqueue((index, meshData));
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }
        }

        public async void OnBeginRender()
        {
            if (camera == null) return;
            await grid.Activate(camera.transform.position, profile.Radius, this);
        }

        private bool isProcessing = false;
        public async Task ProcessQueue()
        {
            if (isProcessing) return;
            isProcessing = true;

            while (requestQueue.Count > 0)
            {
                var processSize = Mathf.Min(MaxParallelJobCount, Mathf.CeilToInt(requestQueue.Count));
                var requests = requestQueue.Take(processSize).Select(x => x.Value)
                    .ToArray();

                var tasks = requests.Where(req =>
                    activeIndices.Contains(req.Index)).Select(req => Build(req))
                    .ToArray();

                foreach (var req in requests)
                    requestQueue.Remove(req.Index);

                await Task.WhenAll(tasks);
            }

            isProcessing = false;
        }

        public void InstantiateQueuedMesh()
        {
            while (composedData.Any())
            {
                var (index, meshData) = composedData.Dequeue();
                
                if (pool.Count == 0)
                {
                    Mesh newMesh = new Mesh();
// TW Tweaks
                    newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                    newMesh.MarkDynamic();
// TW Tweaks
                    pool.Push(newMesh);
                }

                var mesh = pool.Pop();
                meshBuilder.BuildMesh(mesh, meshData);

                if (!activeIndices.Contains(index))
                    SafeDestroy(mesh);
                else
                {
                    mesh.name     += " Active";
                    meshes[index] =  mesh;
                }
            }
        }

        private void SafeDestroy(Mesh mesh)
        {
            mesh.Clear();
            pool.Push(mesh);
        }

        
        #region IDisposable

        public void Dispose()
        {
            activeIndices.Clear();
            requestQueue.Clear();

            foreach (var v in meshes)
                SafeDestroy(v.Value);

            meshes.Clear();

            while (pool.Count > 0)
            {
                var mesh = pool.Pop();

                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(mesh);
                else
                    UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        #endregion

        
        #region ICellOperationCallbacks

        public void Create(MassiveGrassGrid.CellIndex index, Rect rect)
        {
            if (activeIndices.Contains(index)) return;

            activeIndices.Add(index);

            if (!requestQueue.ContainsKey(index))
                requestQueue[index] = (new Request(index, rect));
        }

        public void Remove(MassiveGrassGrid.CellIndex index)
        {
            if (!activeIndices.Contains(index)) return;

            if (meshes.ContainsKey(index))
            {
                meshes[index].name += " Removed";
                SafeDestroy(meshes[index]);
                meshes[index] = null;
                meshes.Remove(index);
            }

            activeIndices.Remove(index);
        }

        // TW Tweaks
        public void RemoveIndexOnly(MassiveGrassGrid.CellIndex index)
        {
            if (!activeIndices.Contains(index)) return;
            activeIndices.Remove(index);
        }
        // TW Tweaks

        #endregion

        private class Request
        {
            public MassiveGrassGrid.CellIndex Index;
            public Rect Rect;

            public Request(MassiveGrassGrid.CellIndex index, Rect rect)
            {
                Index = index;
                Rect = rect;
            }
        }
    }
}

