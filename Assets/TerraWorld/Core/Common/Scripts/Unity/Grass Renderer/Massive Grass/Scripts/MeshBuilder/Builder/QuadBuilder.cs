using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Mewlist.MassiveGrass
{
    /// <summary>
    /// Quad のプリミティブを生成する
    /// </summary>
    public class QuadBuilder : IMeshBuilder
    {
        private Stack<MeshData> pool = new Stack<MeshData>();

        public async Task<MeshData> BuildMeshData(Terrain terrain, IReadOnlyCollection<Texture2D> alphaMaps, MassiveGrassProfile profile, Element[] elements)
        {
            var terrainData = terrain.terrainData;
            var w           = terrainData.alphamapWidth;
            var h           = terrainData.alphamapHeight;

            if (pool.Count <= 0)
            {
                pool.Push(new MeshData(elements.Length, 2));
            }
            var meshData = pool.Pop();
            var actualCount = 0;
            var alphas = new float[elements.Length];

            // TW Tweaks
            //var layers = new List<int>(profile.TerrainLayers.Length);
            //
            //foreach (var terrainLayer in profile.TerrainLayers)
            //{
            //    for (var i = 0; i < terrainData.terrainLayers.Length; i++)
            //    {
            //        if (terrainData.terrainLayers[i] == terrainLayer)
            //            layers.Add(i);
            //    }
            //}

            for (var i = 0; i < elements.Length; i++)
            {
                var element = elements[i];
                var alpha = 0f;
                bool excludedPixel = false;

                for (int k = 0; k < profile.exclusionOpacities.Length; k++)
                {
                    if (profile.exclusionOpacities[k] == 0) continue;
                    int pixelX = Mathf.Clamp((int)(element.normalizedPosition.x * w), 0, w - 1);
                    int pixelY = Mathf.Clamp((int)(element.normalizedPosition.y * h), 0, h - 1);
                    float[,,] alphamap = terrain.terrainData.GetAlphamaps(pixelX, pixelY, 1, 1);

                    if (alphamap[0, 0, k] > 1f - (profile.exclusionOpacities[k] / 100f))
                    {
                        excludedPixel = true;
                        break;
                    }
                }

                if (!excludedPixel)
                {
                    //Color pixel = profile.mask.GetPixelBilinear(
                    //element.normalizedPosition.x,
                    //element.normalizedPosition.y);
                    //
                    ////alpha = Mathf.Max(alpha, pixel.a);
                    //if (pixel.a > profile.AlphaMapThreshold)
                    //alpha = pixel.a;

                    int maskPosX = (int)(element.normalizedPosition.x * profile.maskDataFast.Length);
                    int maskPosY = (int)(element.normalizedPosition.y * profile.maskDataFast[0].row.Length);

                    // Neighbor cells to check for masking falloff
                    int neighbors = Mathf.CeilToInt(Mathf.InverseLerp(32f, 4096f, w) * profile.brushDamping) - Mathf.FloorToInt(w / 1000f);

                    if (profile.maskDataFast[maskPosX].row[maskPosY] > 0)
                        alpha = profile.maskDataFast[maskPosX].row[maskPosY];
                    else if (neighbors > 0)
                    {
                        float sum = 0;

                        for (int j = 1; j <= neighbors; j++)
                        {
                            int nextCol = maskPosX + j; if (nextCol > profile.maskDataFast.Length - 1) nextCol = profile.maskDataFast.Length - 1;
                            int prevCol = maskPosX - j; if (prevCol < 0) prevCol = 0;
                            int nextRow = maskPosY + j; if (nextRow > profile.maskDataFast[0].row.Length - 1) nextRow = profile.maskDataFast[0].row.Length - 1;
                            int prevRow = maskPosY - j; if (prevRow < 0) prevRow = 0;
                            float topLeft = profile.maskDataFast[prevCol].row[prevRow];
                            float top = profile.maskDataFast[prevCol].row[maskPosY];
                            float topRight = profile.maskDataFast[prevCol].row[nextRow];
                            float left = profile.maskDataFast[maskPosX].row[prevRow];
                            float right = profile.maskDataFast[maskPosX].row[nextRow];
                            float bottomLeft = profile.maskDataFast[nextCol].row[prevRow];
                            float bottom = profile.maskDataFast[nextCol].row[maskPosY];
                            float bottomRight = profile.maskDataFast[nextCol].row[nextRow];
                            sum += topLeft + top + topRight + left + right + bottomLeft + bottom + bottomRight * 1f;
                        }

                        if (sum >= 0)
                        {
                            //System.Random random = new System.Random(profile.Seed);
                            //alpha = (sum * 0.033f) + (float)random.NextDouble() * 0.66f;
                            alpha = sum / 8f;
                        }
                    }

                    //foreach (var layer in layers)
                    //{
                    //    var texIndex = layer / 4;
                    //
                    //    if (texIndex < alphaMaps.Count)
                    //    {
                    //        var pixel = alphaMaps.ElementAt(layer / 4).GetPixelBilinear(
                    //            element.normalizedPosition.x * (w - 1f) / w,
                    //            element.normalizedPosition.y * (h - 1f) / h);
                    //
                    //        switch (layer % 4)
                    //        {
                    //            case 0: alpha = Mathf.Max(alpha, pixel.r); break;
                    //            case 1: alpha = Mathf.Max(alpha, pixel.g); break;
                    //            case 2: alpha = Mathf.Max(alpha, pixel.b); break;
                    //            case 3: alpha = Mathf.Max(alpha, pixel.a); break;
                    //        }
                    //    }
                    //}
                }

                alphas[i] = alpha;
            }
            // TW Tweaks

            await Task.Run(() =>
            {
                System.Random random = new System.Random(profile.Seed);

                for (var i = 0; i < elements.Length; i++)
                {
                    var element = elements[i];

// TW Tweaks
                    if (element.position.y == -10000f && element.normal == Vector3.one) continue;

                    bool validHeight = profile.minAllowedHeight <= element.position.y &&
                        element.position.y <= profile.maxAllowedHeight;

                    bool validSlope = Vector3.Angle(element.normal, Vector3.up) >= profile.minAllowedAngle &&
                        Vector3.Angle(element.normal, Vector3.up) <= profile.maxAllowedAngle;

                    if (!validHeight) continue;
                    if (!validSlope) continue;
// TW Tweaks

                    var alpha = alphas[i];

// TW Tweaks
                    if (profile.DensityFactor > 0)
                    {
                        var rand = Mathf.Repeat(1f, ParkAndMiller.Get(i));

                        if (alpha > (profile.DensityFactor) * rand * rand)
                            AddQuad(meshData, profile, element, alpha, actualCount++);
                    }
                    else if (alpha > profile.DensityFactor)
                        AddQuad(meshData, profile, element, alpha, actualCount++);

                    //if (alpha >= profile.AlphaMapThreshold)
                    //{
                    //    AddQuad(meshData, profile, element, alpha, actualCount++);
                    //}
                    //else
                    //{
                    //    var rand = 1 - Mathf.Repeat(1f, ParkAndMiller.Get(i));
                    //    if (alpha > profile.DensityFactor * rand * rand)
                    //        AddQuad(meshData, profile, element, alpha, actualCount++);
                    //}
// TW Tweaks
                }

                meshData.SetActualCount(
                    4 * actualCount,
                    6 * actualCount);
            });

            return meshData;
        }

        public void BuildMesh(Mesh mesh, MeshData meshData)
        {
            //var vertices  = new Vector3[meshData.VertexCount];
            //var normals   = new Vector3[meshData.VertexCount];
            //var triangles = new int[meshData.IndexCount];
            //var colors    = new Color[meshData.VertexCount];
            //
            ////var uvs       = new Vector4[meshData.VertexCount];
            //var uvs = new Vector2[meshData.VertexCount];
            //
            //for (var c = 0; c < meshData.VertexCount; c++) vertices[c] = meshData.vertices[c];
            //for (var c = 0; c < meshData.VertexCount; c++) normals[c]  = meshData.normals[c];
            //for (var c = 0; c < meshData.IndexCount; c++) triangles[c] = meshData.triangles[c];
            //for (var c = 0; c < meshData.VertexCount; c++) colors[c]   = meshData.colors[c];
            //for (var c = 0; c < meshData.VertexCount; c++) uvs[c]      = meshData.uvs[c];
            //
            //mesh.Clear();
            //mesh.name = "MassiveGrass Quad Mesh";
            //
            //mesh.SetVertices( vertices, 0, meshData.VertexCount);
            //mesh.SetUVs(0, uvs, 0, meshData.VertexCount);
            //mesh.SetNormals(normals, 0, meshData.VertexCount);
            //mesh.SetTriangles(triangles, 0, meshData.IndexCount, 0);
            //mesh.SetColors(colors, 0, meshData.VertexCount);

// TW Tweaks
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals   = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Color> colors = new List<Color>();
            List<Vector4> uvs = new List<Vector4>();
            List<Vector4> uv4s = new List<Vector4>();

            for (var c = 0; c < meshData.VertexCount; c++) vertices.Add(meshData.vertices[c]);
            for (var c = 0; c < meshData.VertexCount; c++) normals.Add(meshData.normals[c]);
            for (var c = 0; c < meshData.IndexCount; c++) triangles.Add(meshData.triangles[c]);
            for (var c = 0; c < meshData.VertexCount; c++) colors.Add(meshData.colors[c]);
            for (var c = 0; c < meshData.VertexCount; c++) uvs.Add(meshData.uvs[c]);
            for (var c = 0; c < meshData.VertexCount; c++) uv4s.Add(meshData.uv4s[c]);

            mesh.Clear();
            mesh.name = "MassiveGrass Template Mesh";

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetUVs(3, uv4s);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.SetColors(colors);

            //Vector2[] uvs = new Vector2[meshData.vertices.Length];
            //for (int c = 0; c < meshData.VertexCount; c++) uvs[c] = meshData.uvs[c];
            //
            //mesh.Clear();
            //mesh.name = "MassiveGrass Template Mesh";
            //
            //mesh.vertices = meshData.vertices;
            //mesh.uv = uvs;
            //mesh.normals = meshData.normals;
            //mesh.triangles = meshData.triangles;
            //mesh.colors = meshData.colors;
// TW Tweaks

            pool.Push(meshData);
        }

        private void AddQuad(MeshData meshData, MassiveGrassProfile profile, Element element, float density, int index)
        {
            var vOrigin = index * 4;
            var iOrigin = index * 6;
            var rand    = ParkAndMiller.Get(element.index);
            var normalRot = Quaternion.LookRotation(element.normal);
            var slant     = Quaternion.AngleAxis(profile.Slant * 90f * (rand - 0.5f), Vector3.right);
            //var slantWeak = Quaternion.AngleAxis(profile.Slant * 45f * (rand - 0.5f), Vector3.right);
            var upRot     = Quaternion.AngleAxis(360f * rand, Vector3.up);
            var rot       = normalRot *
                                        Quaternion.AngleAxis(90f, Vector3.right) *
                                        upRot *
                                        slant;

            var scale = profile.Scale * (1 + 0.4f * (rand - 0.5f));
            var rightVec = rot * Vector3.right;
            var upVec = rot * Vector3.up;
            var p1 = scale.x * -rightVec * 0.5f + scale.y * upVec + Vector3.up * profile.GroundOffset;
            var p2 = scale.x *  rightVec * 0.5f + scale.y * upVec + Vector3.up * profile.GroundOffset;
            var p3 = scale.x *  rightVec * 0.5f + Vector3.up * profile.GroundOffset;
            var p4 = scale.x * -rightVec * 0.5f + Vector3.up * profile.GroundOffset;
            var normal = element.normal;
            var normalBottom = element.normal;

            switch(profile.NormalType)
            {
                case NormalType.KeepMesh:
                    break;
                case NormalType.Up:
                    normalBottom = normal = rot * Vector3.up;
                    break;
                case NormalType.Shading:
                    normal = rot * Vector3.up;
                    normalBottom = rot * Vector3.forward;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            meshData.vertices[vOrigin+0] = element.position + p1;
            meshData.vertices[vOrigin+1] = element.position + p2;
            meshData.vertices[vOrigin+2] = element.position + p3;
            meshData.vertices[vOrigin+3] = element.position + p4;
            meshData.normals[vOrigin+0] = normal;
            meshData.normals[vOrigin+1] = normal;
            meshData.normals[vOrigin+2] = normalBottom;
            meshData.normals[vOrigin+3] = normalBottom;

            var attr1 = new MassiveGrassProfile.VertAttribute(density, rand, element.position, p1);
            var attr2 = new MassiveGrassProfile.VertAttribute(density, rand, element.position, p2);
            var attr3 = new MassiveGrassProfile.VertAttribute(density, rand, element.position, p3);
            var attr4 = new MassiveGrassProfile.VertAttribute(density, rand, element.position, p4);

            {
                var vColorR = profile.GetCustomVertexData(VertexDataType.VertexColorR, attr1);
                var vColorG = profile.GetCustomVertexData(VertexDataType.VertexColorG, attr1);
                var vColorB = profile.GetCustomVertexData(VertexDataType.VertexColorB, attr1);
                var vColorA = profile.GetCustomVertexData(VertexDataType.VertexColorA, attr1);
                meshData.colors[vOrigin + 0] = new Color(vColorR, vColorG, vColorB, vColorA);
            }
            {
                var vColorR = profile.GetCustomVertexData(VertexDataType.VertexColorR, attr2);
                var vColorG = profile.GetCustomVertexData(VertexDataType.VertexColorG, attr2);
                var vColorB = profile.GetCustomVertexData(VertexDataType.VertexColorB, attr2);
                var vColorA = profile.GetCustomVertexData(VertexDataType.VertexColorA, attr2);
                meshData.colors[vOrigin+1] = new Color(vColorR, vColorG, vColorB, vColorA);
            }
            {
                var vColorR = profile.GetCustomVertexData(VertexDataType.VertexColorR, attr3);
                var vColorG = profile.GetCustomVertexData(VertexDataType.VertexColorG, attr3);
                var vColorB = profile.GetCustomVertexData(VertexDataType.VertexColorB, attr3);
                var vColorA = profile.GetCustomVertexData(VertexDataType.VertexColorA, attr3);
                meshData.colors[vOrigin+2] = new Color(vColorR, vColorG, vColorB, vColorA);
            }
            {
                var vColorR = profile.GetCustomVertexData(VertexDataType.VertexColorR, attr4);
                var vColorG = profile.GetCustomVertexData(VertexDataType.VertexColorG, attr4);
                var vColorB = profile.GetCustomVertexData(VertexDataType.VertexColorB, attr4);
                var vColorA = profile.GetCustomVertexData(VertexDataType.VertexColorA, attr4);
                meshData.colors[vOrigin+3] = new Color(vColorR, vColorG, vColorB, vColorA);
            }
            {
                var uv1Z    = profile.GetCustomVertexData(VertexDataType.UV1Z, attr1);
                var uv1W    = profile.GetCustomVertexData(VertexDataType.UV1W, attr1);
                meshData.uvs[vOrigin+0] = new Vector4(0f, 1f, uv1Z, uv1W);
            }
            {
                var uv1Z    = profile.GetCustomVertexData(VertexDataType.UV1Z, attr2);
                var uv1W    = profile.GetCustomVertexData(VertexDataType.UV1W, attr2);
                meshData.uvs[vOrigin+1] = new Vector4(1f, 1f, uv1Z, uv1W);
            }
            {
                var uv1Z    = profile.GetCustomVertexData(VertexDataType.UV1Z, attr3);
                var uv1W    = profile.GetCustomVertexData(VertexDataType.UV1W, attr3);
                meshData.uvs[vOrigin+2] = new Vector4(1f, 0f, uv1Z, uv1W);
            }
            {
                var uv1Z    = profile.GetCustomVertexData(VertexDataType.UV1Z, attr4);
                var uv1W    = profile.GetCustomVertexData(VertexDataType.UV1W, attr4);
                meshData.uvs[vOrigin+3] = new Vector4(0f, 0f, uv1Z, uv1W);
            }

            meshData.triangles[iOrigin+0] = vOrigin+0;
            meshData.triangles[iOrigin+1] = vOrigin+1;
            meshData.triangles[iOrigin+2] = vOrigin+2;
            meshData.triangles[iOrigin+3] = vOrigin+2;
            meshData.triangles[iOrigin+4] = vOrigin+3;
            meshData.triangles[iOrigin+5] = vOrigin+0;

            {
                var uv1Z = profile.GetCustomVertexData(VertexDataType.UV1Z, attr1);
                var uv1W = profile.GetCustomVertexData(VertexDataType.UV1W, attr1);
                meshData.uv4s[vOrigin + 0] = new Vector4(0f, 1f, uv1Z, uv1W);
            }
            {
                var uv1Z = profile.GetCustomVertexData(VertexDataType.UV1Z, attr2);
                var uv1W = profile.GetCustomVertexData(VertexDataType.UV1W, attr2);
                meshData.uv4s[vOrigin + 1] = new Vector4(1f, 1f, uv1Z, uv1W);
            }
            {
                var uv1Z = profile.GetCustomVertexData(VertexDataType.UV1Z, attr3);
                var uv1W = profile.GetCustomVertexData(VertexDataType.UV1W, attr3);
                meshData.uv4s[vOrigin + 2] = new Vector4(1f, 0f, uv1Z, uv1W);
            }
            {
                var uv1Z = profile.GetCustomVertexData(VertexDataType.UV1Z, attr4);
                var uv1W = profile.GetCustomVertexData(VertexDataType.UV1W, attr4);
                meshData.uv4s[vOrigin + 3] = new Vector4(0f, 0f, uv1Z, uv1W);
            }
        }
    }
}