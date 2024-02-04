using UnityEngine;
using System.Collections.Generic;
using TerraUnity.Utils;

namespace TerraUnity.Runtime
{
    public class DataToMatrix
    {
        public static List<Matrix4x4> GenerateMatrices
        (
            TScatterLayer.MaskDataFast[] maskData,
            Terrain terrain,
            TScatterLayer.PatchData patch,
            float averageDistance,
            float positionVariation,
            float density,
            bool bypassLake,
            bool underLake,
            int unityLayerMask,
            float minAllowedAngle,
            float maxAllowedAngle,
            float minAllowedHeight,
            float maxAllowedHeight,
            Vector3 positionOffset,
            bool getSurfaceAngle,
            bool lock90DegreeRotation,
            float minRotationRange,
            float maxRotationRange,
            Vector3 rotationOffset,
            bool lockYRotation,
            Vector3 scale,
            float minScale,
            float maxScale,
            bool checkBoundingBox,
            float biggestFace,
            float[] exclusionOpacities,
            int seedNo,
            bool placeSingleItem = false
        )
        {
            List<Matrix4x4> matrices = new List<Matrix4x4>();

            if (placeSingleItem)
                matrices.Add(Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one));
            else
            {
                System.Random randomSeed = new System.Random(seedNo);
                Physics.autoSimulation = false;
                Physics.Simulate(Time.fixedDeltaTime);
                int rows = (int)(patch.scale / (averageDistance));
                int cols = (int)(patch.scale / (averageDistance));
                float rowDistance = patch.scale / rows;
                float colDistance = patch.scale / cols;
                float displacement = Mathf.Clamp01(positionVariation / 100);
                rows = (int)(rows * density / 100f);
                cols = (int)(cols * density / 100f);
                int maskLength0 = 0;
                int maskLength1 = 0;

                if (maskData != null)
                {
                    maskLength0 = maskData.Length - 1;
                    maskLength1 = maskData[0].row.Length - 1;
                }

                float terrainWidth = terrain.terrainData.size.x;
                float terrainLength = terrain.terrainData.size.z;
                Vector3 origin;
                Quaternion rotation;

                int alphamapWidth = terrain.terrainData.alphamapWidth;
                int alphamapHeight = terrain.terrainData.alphamapHeight;

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        origin.y = 100000;
                        float offsetX = colDistance * (j + 0.5f) - (patch.scale / 2) + TUtils.RandomRangeSeed(randomSeed, -0.5f, 0.5f) * (displacement * colDistance);
                        float offsetZ = rowDistance * (i + 0.5f) - (patch.scale / 2) + TUtils.RandomRangeSeed(randomSeed, -0.5f, 0.5f) * (displacement * rowDistance);
                        origin.x = patch.positionX + offsetX;
                        origin.z = patch.positionZ + offsetZ;
                        float R1 = TUtils.RandomRangeSeed(randomSeed, minRotationRange, maxRotationRange);
                        float R2 = TUtils.RandomRangeSeed(randomSeed, 0f, 4f);
                        float R3 = TUtils.RandomRangeSeed(randomSeed, minRotationRange, maxRotationRange);
                        float R4 = TUtils.RandomRangeSeed(randomSeed, minRotationRange, maxRotationRange);
                        float R5 = TUtils.RandomRangeSeed(randomSeed, minRotationRange, maxRotationRange);
                        float R6 = TUtils.RandomRangeSeed(randomSeed, 0f, 4f);
                        float R7 = TUtils.RandomRangeSeed(randomSeed, 0f, 4f);
                        float R8 = TUtils.RandomRangeSeed(randomSeed, 0f, 4f);
                        float R9 = TUtils.RandomRangeSeed(randomSeed, minScale, maxScale);
                        float localOffsetNormalizedX = Mathf.Clamp01((patch.positionX + offsetX - terrain.transform.position.x) / terrainWidth);
                        float localOffsetNormalizedZ = Mathf.Clamp01((patch.positionZ + offsetZ - terrain.transform.position.z) / terrainLength);
                        bool excludedPixel = false;

                        for (int k = 0; k < exclusionOpacities.Length; k++)
                        {
                            if (exclusionOpacities[k] == 0) continue;
                            int pixelX = Mathf.Clamp((int)(localOffsetNormalizedX * alphamapWidth), 0, alphamapWidth - 1);
                            int pixelY = Mathf.Clamp((int)(localOffsetNormalizedZ * alphamapHeight), 0, alphamapHeight - 1);
                            float[,,] alphamap = terrain.terrainData.GetAlphamaps(pixelX, pixelY, 1, 1);

                            if (alphamap[0, 0, k] > 1f - (exclusionOpacities[k] / 100f))
                            {
                                excludedPixel = true;
                                break;
                            }
                        }

                        if (excludedPixel) continue;

                        if (maskData != null)
                        {
                            int pixelX = Mathf.RoundToInt(localOffsetNormalizedX * maskLength1);
                            int pixelY = Mathf.RoundToInt(localOffsetNormalizedZ * maskLength0);

                            //TODO: Later we can compare opacity like: if (mask[pixelX, pixelY] <= filterOpacity)
                            if (maskData[pixelX].row[pixelY] <= 0) continue;

                            // Check if the whole model bounding box is inside mask
                            bool allPixelsPassed = true;

                            if (checkBoundingBox)
                            {
                                float normalizedLength = Mathf.InverseLerp(0, terrainWidth, biggestFace);
                                int offsetPixels = (int)(normalizedLength * maskLength0);

                                if
                                (
                                    pixelX - offsetPixels < 0 ||
                                    pixelX + offsetPixels >= maskLength0 ||
                                    pixelY - offsetPixels < 0 ||
                                    pixelY + offsetPixels >= maskLength0
                                )
                                    allPixelsPassed = false;

                                if (!allPixelsPassed) continue;

                                for (int x = -offsetPixels; x <= offsetPixels; x++)
                                {
                                    for (int y = -offsetPixels; y <= offsetPixels; y++)
                                    {
                                        //TODO: Later we can compare opacity like: if (mask[pixelX, pixelY] <= filterOpacity)
                                        if (maskData[pixelX + x].row[pixelY + y] <= 0)
                                        {
                                            allPixelsPassed = false;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (checkBoundingBox && !allPixelsPassed) continue;
                        }

                        Ray ray = new Ray(origin, Vector3.down);
                        RaycastHit hit;

                        if (!Raycasts.RaycastNonAllocSorted(ray, bypassLake, underLake, out hit, unityLayerMask, Mathf.Infinity, QueryTriggerInteraction.Ignore))
                            continue;

                        Vector3 normal = hit.normal;
                        origin = hit.point;

                        if (Vector3.Angle(normal, Vector3.up) >= minAllowedAngle && Vector3.Angle(normal, Vector3.up) <= maxAllowedAngle)
                        {
                            if (origin.y >= minAllowedHeight && origin.y <= maxAllowedHeight)
                            {
                                // --- priority
                                //GameObject hitObject = hit.transform.gameObject;

                                //// Check if the whole model bounding box is inside defined slope range
                                //bool allSlopesPassed = true;
                                //
                                //if (checkBoundingBoxSlope)
                                //{
                                //    for (int x = -biggestFace; x <= biggestFace; x++)
                                //    {
                                //        for (int y = -biggestFace; y <= biggestFace; y++)
                                //        {
                                //            Vector3 offsetPosition = new Vector3(origin.x + x, 100000, origin.z + y);
                                //            ray = new Ray(offsetPosition, Vector3.down);
                                //
                                //            if (!Raycasts.RaycastNonAllocSorted(ray, bypassLake, underLake, unityLayerMask))
                                //            {
                                //                //continue;
                                //                allSlopesPassed = false;
                                //                Debug.Log("Skip Detected 1");
                                //                break;
                                //            }
                                //
                                //            hit = Raycasts.closestHit;
                                //            normal = hit.normal;
                                //
                                //            if (Vector3.Angle(normal, Vector3.up) < minAllowedAngle || Vector3.Angle(normal, Vector3.up) > maxAllowedAngle)
                                //            {
                                //                allSlopesPassed = false;
                                //                Debug.Log("Skip Detected 2");
                                //                break;
                                //            }
                                //        }
                                //    }
                                //}
                                //
                                //if (checkBoundingBoxSlope && !allSlopesPassed) continue;

                                // --- offset
                                origin += positionOffset;

                                if (getSurfaceAngle)
                                {
                                    Vector3 finalRotation = Quaternion.FromToRotation(Vector3.up, normal).eulerAngles;
                                    Quaternion surfaceRotation = Quaternion.Euler(finalRotation);

                                    if (!lock90DegreeRotation)
                                    {
                                        float rotationY = R1;
                                        surfaceRotation *= Quaternion.AngleAxis(rotationY, Vector3.up);
                                    }
                                    else
                                    {
                                        float rotationY = Mathf.Round(R2) * 90;
                                        surfaceRotation *= Quaternion.AngleAxis(rotationY, Vector3.up);
                                        surfaceRotation.eulerAngles = new Vector3(surfaceRotation.eulerAngles.x, rotationY, surfaceRotation.eulerAngles.z);
                                    }

                                    rotation = surfaceRotation * Quaternion.Euler(rotationOffset);
                                }
                                else
                                {
                                    float rotationX = rotationOffset.x;
                                    float rotationY = rotationOffset.y;
                                    float rotationZ = rotationOffset.z;

                                    if (!lock90DegreeRotation)
                                    {
                                        rotationX += R3;
                                        rotationY += R4;
                                        rotationZ += R5;
                                    }
                                    else
                                    {
                                        rotationX += Mathf.Round(R6) * 90;
                                        rotationY += Mathf.Round(R7) * 90;
                                        rotationZ += Mathf.Round(R8) * 90;
                                    }

                                    if (lockYRotation)
                                    {
                                        rotationX = rotationOffset.x;
                                        rotationZ = rotationOffset.z;
                                    }

                                    rotation = Quaternion.Euler(new Vector3(rotationX, rotationY, rotationZ));
                                }

                                // --- scaling
                                Vector3 localScale = scale;
                                float randomScale = R9;
                                localScale.x *= randomScale;
                                localScale.y *= randomScale;
                                localScale.z *= randomScale;

                                // This will never going to happen by default but we added it in case other calculations were wrong!
                                if (matrices.Count < 1023)
                                    matrices.Add(Matrix4x4.TRS(origin, rotation, localScale));
                                else
                                    break;
                            }
                        }
                    }
                }
            }

            Physics.autoSimulation = true;

            return matrices;
        }
    }
}

