#if TERRAWORLD_PRO
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Xml;
using static TerraUnity.Edittime.MaskBlendOperator;

namespace TerraUnity.Edittime
{
#if UNITY_EDITOR
   public static class TModuleActions
    {
        public enum BlendingMode
        {
            OR,
            AND,
            NOT,
            SUB,
            XOR,
            Exaggerate
        }

        public enum ColoredMode
        {
            Black_White, Gray, Colored
        }
        public static float[,] HeightmapMaster(float[,] heightmap )
        {
            return heightmap;
        }

        public static float[,] HeightmapSource(TMap currentMap, float elevationExaggeration, bool highestResolution , int _resolution)
        {
            float[,] _heightmapData = currentMap.Heightmap.heightsData.Clone() as float[,];
            if (elevationExaggeration != 1)
                _heightmapData = THeightmapProcessors.ExaggerateHeightmap(_heightmapData, elevationExaggeration);

            if (!highestResolution)
            {
                _heightmapData = THeightmapProcessors.ResampleHeightmap(_heightmapData, THeightmapProcessors.ResampleMode.DOWN, _resolution + 1);

                int smoothIterration = (int)((_resolution + 1) * 1.0f / _heightmapData.GetLength(0));
                _heightmapData = THeightmapProcessors.SmoothHeightmap(_heightmapData, smoothIterration);
            }

            return _heightmapData;

        }

        public static float[,] SmoothProcess(float[,] heightmap,int _steps, float _blending, THeightmapProcessors.Neighbourhood _smoothMode )
        {
            float[,] _heightmapData = THeightmapProcessors.SmoothHeightmap(heightmap, _steps, _blending, _smoothMode);
            return _heightmapData;
        }

        public static float[,] HydraulicErosionMainProcess(float[,] heightmap, HydraulicErosionMethod hydraulicErosionMethod , int _iterations, float _rainAmount, float _sediment, float _evaporation, int _iterationsUltimate)
        {
            float[,] _heightmapData;
            switch (hydraulicErosionMethod)
            {
                case HydraulicErosionMethod.Normal:
                    _heightmapData = THeightmapProcessors.HydraulicErosion(heightmap, _iterations, _rainAmount, _sediment, _evaporation);
                    break;
                case HydraulicErosionMethod.Ultimate:
                    _heightmapData = THeightmapProcessors.HydraulicErosionUltimate(heightmap, _iterationsUltimate);
                    break;
                default:
                    _heightmapData = heightmap;
                    break;
            }
            return _heightmapData;
        }

        public static float[,] WaterErosionProcess(float[,] heightmap, int _iterations, float _shape, float _rivers, float _vertical, float _seaBedCarve)
        {
            float[,] _heightmapData = THeightmapProcessors.WaterErosion(heightmap, _iterations, _shape, _rivers, _vertical, _seaBedCarve);
            return _heightmapData;
        }

        public static float[,] ThermalErosionProcess(float[,] heightmap, int _iterations)
        {
            float[,] _heightmapData = THeightmapProcessors.ThermalErosion(heightmap, _iterations);
            return _heightmapData;
        }

        public static float[,] TerraceProcess(float[,] heightmap, ref float [] controlPoints, int _terraceCount, float _strength, float _terraceVariation)
        {
            float[,] _heightmapData = THeightmapProcessors.Terrace(heightmap, ref controlPoints, _terraceCount, _strength, _terraceVariation);
            return _heightmapData;
        }

        public static float[,] VoxelProcess(float[,] heightmap, int voxelSize)
        {
            float[,] _heightmapData = THeightmapProcessors.VoxelizeHeightmap(heightmap, voxelSize);
            return _heightmapData;
        }

        public static TMask Flowmap (float[,] heightmap, float _widthMultiplier, float _heightMultiplier, int _iterations , float minRange, float maxRange)
        {
            TMask _mask = THeightmapProcessors.CreateFlowMask(heightmap, _widthMultiplier, _heightMultiplier, _iterations);
            _mask = _mask.FilteredMask(minRange, maxRange);
            return _mask;
        }

        public static TMask Slopemask(TMap currentMap, float[,] heightmap, float MaxSlope, float MinSlope)
        {
            THeightmap theightmap = new THeightmap(heightmap);
            TMask _mask = THeightmapProcessors.GetSlopeMap(currentMap, theightmap, MaxSlope, MinSlope);
            return _mask;
        }

        public static void ApplyMask(List<TMask> masks, ref float[,] heightmap, float _depth, bool _flat )
        {
            TMask _mask = TMask.MergeMasks(masks);
            THeightmapProcessors.DeformByMask(ref heightmap, _mask, _depth, _flat, 0);
        }

        public static TDetailTexture SatelliteImage(TMap CurrentMap, int resolution)
        {
            Bitmap newImage = TImageProcessors.ResetResolution(CurrentMap.Image.Image, resolution);
            TDetailTexture _detailTexture = new TDetailTexture(newImage);
            _detailTexture.Tiling = new Vector2(CurrentMap._area.AreaSizeLat * 1000, CurrentMap._area.AreaSizeLon * 1000);
            return _detailTexture;
        }

        public static TDetailTexture ShadowRemover(Color _shadowColor, TDetailTexture detailTexture, int _blockSize)
        {
            detailTexture.DiffuseMap.Image = TImageProcessors.ShadowRemover(detailTexture.DiffuseMap.Image, _shadowColor, _blockSize);
            return detailTexture;
        }

        public static TDetailTexture Mask2DetailTexture
        (
            string terrainLayerPath, TMask mask, float minRange, float maxRange, 
            Vector2 tiling , Vector2 tilingOffset , Vector4 specular , float metallic,
            float smoothness , float normalScale , float opacity, string NodeName
        )
        {
            if (mask == null) mask = new TMask(32, 32);

            if (!string.IsNullOrEmpty(terrainLayerPath) && File.Exists(Path.GetFullPath(terrainLayerPath)))
            {
                TMask filteredMask = mask?.FilteredMask(minRange, maxRange);
                TDetailTexture _detailTexture = new TDetailTexture(terrainLayerPath, filteredMask);
                _detailTexture.Tiling = tiling;
                _detailTexture.TilingOffset = tilingOffset;
                _detailTexture.Specular = specular;
                _detailTexture.Metallic = metallic;
                _detailTexture.Smoothness = smoothness;
                _detailTexture.NormalScale = normalScale;
                _detailTexture.Opacity = opacity;
                return _detailTexture;
            }
            else
                throw new Exception("No Terrain Layers Selected For Node : " + NodeName + "\n\n Please Check The Node.");
        }

        public static TDetailTexture Mask2DetailTexture(string terrainLayerPath, TMask mask)
        {
            if (mask == null) mask = new TMask(32, 32);
            TDetailTexture _detailTexture = new TDetailTexture(terrainLayerPath, mask);
            return _detailTexture;
        }

        public static TDetailTexture Mask2DetailTexture(string diffusemapPath, string normalmapPath, string maskmapPath, TMask mask, float minRange, float maxRange,
                                                Vector2 tiling, Vector2 tilingOffset, Vector4 specular, float metallic,
                                                float smoothness, float normalScale, float opacity, string NodeName)
        {
            TImage diffuse = new TImage();
            TImage normalmap = new TImage();
            TImage maskmap = new TImage();

            if (mask == null) mask = new TMask(32, 32);

            if (!string.IsNullOrEmpty(diffusemapPath))
            {
                using (Image source = Bitmap.FromFile(diffusemapPath))
                {
                    diffuse.Image = (Bitmap)source;
                //    diffuse.ObjectPath = diffusemapPath;
                }
            }
            else throw new Exception("No Diffusemap Selected For Node: " + NodeName + "\n\n Please Check The Node.");

            if (!string.IsNullOrEmpty(normalmapPath))
            {
                using (Image source = Bitmap.FromFile(normalmapPath))
                {
                    normalmap.Image = (Bitmap)source;
                //    normalmap.ObjectPath = normalmapPath;
                }
            }
            else
                normalmap = null;

            if (!string.IsNullOrEmpty(maskmapPath))
            {
                using (Image source = Bitmap.FromFile(maskmapPath))
                {
                    maskmap.Image = (Bitmap)source;
                //    maskmap.ObjectPath = maskmapPath;
                }
            }
            else
                maskmap = null;
            TMask filteredMask = mask?.FilteredMask(minRange, maxRange);
            TDetailTexture _detailTexture = new TDetailTexture(filteredMask, diffuse, normalmap, maskmap);
            _detailTexture.Tiling = tiling;
            _detailTexture.TilingOffset = tilingOffset;
            _detailTexture.Specular = specular;
            _detailTexture.Metallic = metallic;
            _detailTexture.Smoothness = smoothness;
            _detailTexture.NormalScale = normalScale;
            _detailTexture.Opacity = opacity;
            return _detailTexture;
        }

        public static TDetailTexture Mask2ColorMap(TMap CurrentMap, TImage DiffuseMap, TImage NormalMap, TImage MaskMap, List <TMask> masks,TMask mask, float minRange, float maxRange)
        {
            TMask imageMask = (masks.Count > 0) ? TMask.MergeMasks(masks) : new TMask(32, 32, true);
            TMask filteredMask = mask.FilteredMask(minRange, maxRange);
            filteredMask.AND(imageMask);
            TDetailTexture _detailTexture = new TDetailTexture(filteredMask, DiffuseMap, NormalMap, MaskMap);
            _detailTexture.Tiling = new Vector2(CurrentMap._area.AreaSizeLat * 1000, CurrentMap._area.AreaSizeLon * 1000);
            return _detailTexture;
        }

        public static TDetailTexture Mask2ColorMap(TMap CurrentMap, TImage DiffuseMap, TImage NormalMap, TImage MaskMap, List<TMask> masks, TMask mask, float minRange, float maxRange,
                                  int mostUsedColor)
        {

            TMask imageMask = (masks.Count > 0) ? TMask.MergeMasks(masks) : new TMask(32, 32, true);
            TMask filteredMask = mask.FilteredMask(minRange, maxRange);
            filteredMask.AND(imageMask);
            Bitmap quantizedImage = TImageProcessors.QuantizeImage(DiffuseMap.Image, mostUsedColor, filteredMask);
            TImage image = new TImage(quantizedImage);
            TDetailTexture _detailTexture = new TDetailTexture(filteredMask, image, NormalMap, MaskMap);
            _detailTexture.Tiling = new Vector2(CurrentMap._area.AreaSizeLat * 1000, CurrentMap._area.AreaSizeLon * 1000);
            return _detailTexture;

        }

        public static TImage QuantizeImage(TImage sourceImage, int mostUsedColor)
        {

            Bitmap quantizedImage = TImageProcessors.QuantizeImage2(sourceImage.Image, mostUsedColor);
            TImage image = new TImage(quantizedImage);
            return image;

        }

        public static TImage Mask2Image(TMask mask, Color color, ColoredMode coloredMode )
        {
            Bitmap bitmap = null;
            switch (coloredMode)
            {
                case ColoredMode.Black_White:
                        bitmap = mask.GetBWImage();
                    break;
                case ColoredMode.Gray:
                        bitmap = mask.GetGrayImage();
                    break;
                case ColoredMode.Colored:
                        bitmap = mask.GetColoredImage(color);
                    break;
                default:
                    break;
            }

            TImage image = new TImage(bitmap);
            return image;

        }

        public static TDetailTexture Mask2ColorMap(TMap CurrentMap, Color _colorMapColor, TMask mask, float minRange, float maxRange)
        {

            TMask filteredMask = mask.FilteredMask(minRange, maxRange);
            Bitmap coloredImage = filteredMask.GetColoredImage(_colorMapColor);
            TImage image = new TImage(coloredImage);
            TDetailTexture _detailTexture = new TDetailTexture(filteredMask, image, null, null);
            _detailTexture.Tiling = new Vector2(CurrentMap._area.AreaSizeLat * 1000, CurrentMap._area.AreaSizeLon * 1000);
            return _detailTexture;

        }

        public static TMask Image2Mask(Bitmap bitmap, List<TMask> masks,Color color, int tolerance)
        {
            TMask mask = (masks != null && masks.Count > 0) ? TMask.MergeMasks(masks) : null;
            TMask result = TImageProcessors.GetMaskMap(bitmap, color, tolerance, mask);
            return result;
        }

        public static TMask ColorFilter(Bitmap bitmap, Color color, int tolerance)
        {
            TMask result = TImageProcessors.GetMaskMap(bitmap, color, tolerance);
            return result;
        }

        public static TMask Blender (BlendingMode blendingMode , List<TMask> masks1 , List<TMask> masks2)
        {
            TMask resultMask = null;
            switch (blendingMode)
            {
                case BlendingMode.OR:
                    {
                        List<TMask> inputMasks = new List<TMask>();
                        if (masks1 != null)
                            for (int i = 0; i < masks1.Count; i++)
                                inputMasks.Add(masks1[i]);
                        if (masks2 != null)
                            for (int i = 0; i < masks2.Count; i++)
                                inputMasks.Add(masks2[i]);
                        resultMask = TMask.MergeMasks(inputMasks);
                    }
                    break;
                case BlendingMode.AND:
                    {
                        List<TMask> inputMasks = new List<TMask>();
                        if (masks1 != null)
                            for (int i = 0; i < masks1.Count; i++)
                                inputMasks.Add(masks1[i]);
                        if (masks2 != null)
                            for (int i = 0; i < masks2.Count; i++)
                                inputMasks.Add(masks2[i]);
                        resultMask = TMask.AND(inputMasks);
                    }
                    break;
                case BlendingMode.NOT:
                    {
                        List<TMask> inputMasks = new List<TMask>();
                        if (masks1 != null)
                            for (int i = 0; i < masks1.Count; i++)
                                inputMasks.Add(masks1[i]);
                        if (masks2 != null)
                            for (int i = 0; i < masks2.Count; i++)
                                inputMasks.Add(masks2[i]);
                        resultMask = TMask.Inverse(inputMasks);
                    }
                    break;
                case BlendingMode.XOR:
                    {
                        List<TMask> inputMasks = new List<TMask>();
                        if (masks1 != null)
                            for (int i = 0; i < masks1.Count; i++)
                                inputMasks.Add(masks1[i]);
                        if (masks2 != null)
                            for (int i = 0; i < masks2.Count; i++)
                                inputMasks.Add(masks2[i]);
                        resultMask = TMask.XOR(inputMasks);
                    }
                    break;
                case BlendingMode.SUB:
                    {
                        resultMask = TMask.Subtract(masks1, masks2);
                    }
                    break;
                case BlendingMode.Exaggerate:
                    {
                        if (masks1 != null && masks1.Count > 0)
                            resultMask = TMask.Exaggerate(masks1[0], 1); ;
                    }
                    break;
                default:
                    break;
            }

            if (resultMask == null) resultMask = new TMask(8, 8);
            return resultMask;
        }

        public static List<TMask> BiomeExtractor(TMap CurrentMap , BiomeTypes biomeType, float MinSize, bool bordersOnly , int edgeSize, float scaleFactor , float riverWidth)
        {
            List<TMask> OutMasks = new List<TMask>();

            //switch (biomeType)
            //{
            //    case BiomeTypes.Waters:
            //        {
            //            List<T2DObject> lakes = new List<T2DObject>();
            //            OSMParser.ExtractLakes(CurrentMap.LandcoverXML, ref lakes, areaBounds, CurrentMap._area);
            //            TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref lakes, MinSize);
            //            List<TMask> lakesMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, lakes, bordersOnly, edgeSize, scaleFactor, true);
            //            if (lakesMasks?.Count > 0)  OutMasks.Add(lakesMasks[0]);
            //            List<T2DObject> Oceans = new List<T2DObject>();
            //            OSMParser.ExtractOceans(CurrentMap.LandcoverXML, ref Oceans, areaBounds, CurrentMap._area);
            //            TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref Oceans, MinSize);
            //            List<TMask> OceanMasks =TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, Oceans, bordersOnly, edgeSize, scaleFactor, true);
            //            if (OceanMasks?.Count > 0) OutMasks.Add(OceanMasks[0]);
            //            List<TLinearObject> rivers = new List<TLinearObject>();
            //            OSMParser.ExtractRivers(CurrentMap.LandcoverXML, ref rivers, areaBounds, CurrentMap._area);
            //            List<TMask> RiverMasks = TLandcoverProccessor.GetBiomesMasksLinear(_currentMap._refTerrain, rivers, bordersOnly, edgeSize, scaleFactor, riverWidth / 2f, true);
            //            if (RiverMasks?.Count > 0) OutMasks.Add(RiverMasks[0]);
            //            TMask allmasks = TMask.MergeMasks(OutMasks);
            //            OutMasks.Clear();
            //            OutMasks.Add(allmasks);
            //        }
            //        break;
            //
            //
            //    case BiomeTypes.Lakes:
            //        {
            //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
            //            OSMParser.ExtractLakes(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
            //            TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
            //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
            //        }
            //        break;
            //
            //    case BiomeTypes.Sea:
            //        {
            //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
            //            OSMParser.ExtractOceans(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
            //            TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
            //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
            //        }
            //        break;
            //
            //    case BiomeTypes.Trees:
            //        {
            //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
            //            OSMParser.ExtractForest(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
            //            TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
            //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
            //        }
            //        break;
            //
            //    case BiomeTypes.Wood:
            //        {
            //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
            //            OSMParser.ExtractWood(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
            //            TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
            //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
            //        }
            //        break;
            //
            //    case BiomeTypes.Meadow:
            //        {
            //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
            //            OSMParser.ExtractMeadow(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
            //            TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
            //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
            //        }
            //        break;
            //
            //    case BiomeTypes.Orchard:
            //        {
            //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
            //            OSMParser.ExtractOrchard(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
            //            TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
            //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
            //        }
            //        break;
            //
            //    case BiomeTypes.Grass:
            //        {
            //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
            //            OSMParser.ExtractGrass(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
            //            TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
            //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
            //        }
            //        break;
            //
            //    case BiomeTypes.Greenfield:
            //        {
            //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
            //            OSMParser.ExtractGreenField(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
            //            //TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
            //            TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
            //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
            //        }
            //        break;
            //
            //    case BiomeTypes.River:
            //        {
            //            TLinearMeshLayer _Biomeslayer = new TLinearMeshLayer();
            //            List<TLinearObject> rivers = _Biomeslayer.Lines;
            //            OSMParser.ExtractRivers(CurrentMap.LandcoverXML, ref rivers, areaBounds, CurrentMap._area);
            //            //TLandcoverProccessor.FilterRiverBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer);
            //            TLandcoverProccessor.FilterRiverBordersByBoundArea(CurrentMap, ref _Biomeslayer);
            //            OutMasks = TLandcoverProccessor.GetBiomesMasksLinear(_currentMap._refTerrain, rivers, bordersOnly, edgeSize, scaleFactor, riverWidth / 2f, true);
            //        }
            //        break;
            //
            //    case BiomeTypes.Wetland:
            //        {
            //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
            //            OSMParser.ExtractWetland(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
            //            //TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer, MinSize);
            //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
            //        }
            //        break;
            //
            //    case BiomeTypes.Beach:
            //        {
            //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
            //            OSMParser.ExtractBeach(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
            //            //TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer, MinSize);
            //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
            //        }
            //        break;
            //
            //    case BiomeTypes.Bay:
            //        {
            //            TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
            //            OSMParser.ExtractBay(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, areaBounds, CurrentMap._area);
            //            //TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer, MinSize);
            //            OutMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
            //        }
            //        break;
            //}

            switch (biomeType)
            {
                case BiomeTypes.Waters:
                    {
                        List<T2DObject> lakes = new List<T2DObject>();
                        OSMParser.ExtractLakes(CurrentMap.LandcoverXML, ref lakes, CurrentMap._area);
                        TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref lakes, MinSize);
                        List<TMask> lakesMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, lakes, bordersOnly, edgeSize, scaleFactor, true);
                        if (lakesMasks?.Count > 0) OutMasks.Add(lakesMasks[0]);
                        List<T2DObject> Oceans = new List<T2DObject>();
                        OSMParser.ExtractOceans(CurrentMap.LandcoverXML, ref Oceans, CurrentMap._area);
                        TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref Oceans, MinSize);
                        List<TMask> OceanMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, Oceans, bordersOnly, edgeSize, scaleFactor, true);
                        if (OceanMasks?.Count > 0) OutMasks.Add(OceanMasks[0]);
                        List<TLinearObject> rivers = new List<TLinearObject>();
                        OSMParser.ExtractRivers(CurrentMap.LandcoverXML, ref rivers, CurrentMap._area);
                        List<TMask> RiverMasks = TLandcoverProccessor.GetBiomesMasksLinear(CurrentMap._refTerrain, rivers, bordersOnly, edgeSize, scaleFactor, riverWidth / 2f, true);
                        if (RiverMasks?.Count > 0) OutMasks.Add(RiverMasks[0]);
                        TMask allmasks = TMask.MergeMasks(OutMasks);
                        OutMasks.Clear();
                        OutMasks.Add(allmasks);
                    }
                    break;

                case BiomeTypes.Lakes:
                    {
                        TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                        OSMParser.ExtractLakes(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                        TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                        OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                    }
                    break;

                case BiomeTypes.Sea:
                    {
                        TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                        OSMParser.ExtractOceans(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                        TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                        OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                    }
                    break;

                case BiomeTypes.Trees:
                    {
                        TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                        OSMParser.ExtractForest(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                        TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                        OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                    }
                    break;

                case BiomeTypes.Wood:
                    {
                        TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                        OSMParser.ExtractWood(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                        TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                        OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                    }
                    break;

                case BiomeTypes.Meadow:
                    {
                        TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                        OSMParser.ExtractMeadow(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                        TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                        OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                    }
                    break;

                case BiomeTypes.Orchard:
                    {
                        TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                        OSMParser.ExtractOrchard(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                        TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                        OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                    }
                    break;

                case BiomeTypes.Grass:
                    {
                        TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                        OSMParser.ExtractGrass(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                        TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                        OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                    }
                    break;

                case BiomeTypes.Greenfield:
                    {
                        TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                        OSMParser.ExtractGreenField(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                        //TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                        TLandcoverProccessor.FilterBiomesBordersByBoundArea(CurrentMap, ref _Biomeslayer.MeshArea, MinSize);
                        OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                    }
                    break;

                case BiomeTypes.River:
                    {
                        TLinearMeshLayer _Biomeslayer = new TLinearMeshLayer();
                        List<TLinearObject> rivers = _Biomeslayer.Lines;
                        OSMParser.ExtractRivers(CurrentMap.LandcoverXML, ref rivers, CurrentMap._area);
                        //TLandcoverProccessor.FilterRiverBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer);
                        OutMasks = TLandcoverProccessor.GetBiomesMasksLinear(CurrentMap._refTerrain, rivers, bordersOnly, edgeSize, scaleFactor, riverWidth / 2f, true);
                    }
                    break;

                case BiomeTypes.Wetland:
                    {
                        TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                        OSMParser.ExtractWetland(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                        //TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer, MinSize);
                        OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                    }
                    break;

                case BiomeTypes.Beach:
                    {
                        TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                        OSMParser.ExtractBeach(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                        //TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer, MinSize);
                        OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                    }
                    break;

                case BiomeTypes.Bay:
                    {
                        TPolygonMeshLayer _Biomeslayer = new TPolygonMeshLayer();
                        OSMParser.ExtractBay(CurrentMap.LandcoverXML, ref _Biomeslayer.MeshArea, CurrentMap._area);
                        //TLandcoverProccessor.FilterBiomesBordersByBoundArea(areaBounds, CurrentMap, ref _Biomeslayer, MinSize);
                        OutMasks = TLandcoverProccessor.GetBiomesMasks(CurrentMap._refTerrain, _Biomeslayer.MeshArea, bordersOnly, edgeSize, scaleFactor, true);
                    }
                    break;
            }
            return OutMasks;

        }

        public static TObjectScatterLayer TreeScatter
        (
            string prefabName,
            int seedNo,
            bool bypassLakes ,
            bool underLakes ,
            bool underLakesMask ,
            bool onLakes ,
            float minRotationRange ,
            float maxRotationRange ,
            float positionVariation ,
            Vector3 scaleMultiplier ,
            float minScale ,
            float maxScale ,
            float minSlope ,
            float maxSlope ,
            int priority ,
            float minRange ,
            float maxRange ,
            int maskLayer ,
            float averageDistance ,
            bool checkBoundingBox ,
            float maxElevation ,
            float minElevation ,
            List<TMask> masks ,
            string layerName

        )
        {
            TMask _mask = null;

            if (masks != null)
            {
                _mask = TMask.MergeMasks(masks);
                _mask = _mask?.FilteredMask(minRange, maxRange);
            }

            TObjectScatterLayer _objectScatterLayer = new TObjectScatterLayer(true);
            _objectScatterLayer.SeedNo = seedNo;
            _objectScatterLayer.bypassLake = bypassLakes;
            _objectScatterLayer.underLake = underLakes;
            _objectScatterLayer.underLakeMask = underLakesMask;
            _objectScatterLayer.onLake = onLakes;
            _objectScatterLayer.MaxRotationRange = (int)maxRotationRange;
            _objectScatterLayer.MinRotationRange = (int)minRotationRange;
            _objectScatterLayer.PositionVariation = positionVariation;
            _objectScatterLayer.ScaleMultiplier = scaleMultiplier;
            _objectScatterLayer.MaxScale = maxScale;
            _objectScatterLayer.MinScale = minScale;
            _objectScatterLayer.LayerName = layerName;
            _objectScatterLayer.MaxSlope = maxSlope;
            _objectScatterLayer.MinSlope = minSlope;
            _objectScatterLayer.Priority = priority;
            _objectScatterLayer.UnityLayerMask = maskLayer;
            _objectScatterLayer.useLayer = true;
            _objectScatterLayer.prefabNames.Add(prefabName);
            _objectScatterLayer.averageDistance = averageDistance;
            _objectScatterLayer.checkBoundingBox = checkBoundingBox;
            _objectScatterLayer.MaxElevation = maxElevation;
            _objectScatterLayer.MinElevation = minElevation;

            if (_mask != null) 
                _objectScatterLayer.maskData = _mask.MaskData;

            return _objectScatterLayer;
        }

        public static TObjectScatterLayer ObjectScatter
        (  
            List<string> prefabNames,
            int seedNo,
            bool rotation90Degrees,
            bool bypassLakes,
            bool underLakes,
            bool underLakesMask,
            bool onLakes,
            bool lockYRotation,
            bool getSurfaceAngle,
            float minRotationRange,
            float maxRotationRange,
            float positionVariation,
            Vector3 scaleMultiplier,
            float minScale,
            float maxScale,
            bool hasCollider,
            bool hasPhysics,
            string unityLayerName,
            int maskLayer,
            string NodeName,
            float minSlope,
            float maxSlope,
            Vector3 positionOffset,
            Vector3 rotationOffset,
            int priority,
            List<ObjectBounds> bounds,
            List<Vector3> objectScales,
            float minRange,
            float maxRange,
            float averageDistance,
            bool checkBoundingBox,
            float maxElevation,
            float minElevation,
            bool placeSingleItem,
            List<TMask> masks
        )
        {
            TMask _mask = null;

            if (masks != null)
            {
                _mask = TMask.MergeMasks(masks);
                _mask = _mask?.FilteredMask(minRange, maxRange);
            }

            TObjectScatterLayer _objectScatterLayer = new TObjectScatterLayer(true);
            _objectScatterLayer = new TObjectScatterLayer(false);
            _objectScatterLayer.SeedNo = seedNo;
            _objectScatterLayer.rotation90Degrees = rotation90Degrees;
            _objectScatterLayer.bypassLake = bypassLakes;
            _objectScatterLayer.underLake = underLakes;
            _objectScatterLayer.underLakeMask = underLakesMask;
            _objectScatterLayer.onLake = onLakes;
            _objectScatterLayer.lockYRotation = lockYRotation;
            _objectScatterLayer.getSurfaceAngle = getSurfaceAngle;
            _objectScatterLayer.MaxRotationRange = (int)maxRotationRange;
            _objectScatterLayer.MinRotationRange = (int)minRotationRange;
            _objectScatterLayer.PositionVariation = positionVariation;
            _objectScatterLayer.ScaleMultiplier = scaleMultiplier;
            _objectScatterLayer.MaxScale = maxScale;
            _objectScatterLayer.MinScale = minScale;
            _objectScatterLayer.LayerName = NodeName;
            _objectScatterLayer.HasCollider = hasCollider;
            _objectScatterLayer.HasPhysics = hasPhysics;
            _objectScatterLayer.MaxSlope = maxSlope;
            _objectScatterLayer.MinSlope = minSlope;
            _objectScatterLayer.Priority = priority;
            _objectScatterLayer.UnityLayerName = unityLayerName;
            _objectScatterLayer.UnityLayerMask = maskLayer;
            _objectScatterLayer.useLayer = true;
            _objectScatterLayer.Offset = positionOffset;
            _objectScatterLayer.RotationOffset = rotationOffset;
            _objectScatterLayer.prefabNames = prefabNames;
            _objectScatterLayer.averageDistance = averageDistance;
            _objectScatterLayer.checkBoundingBox = checkBoundingBox;
            _objectScatterLayer.MaxElevation = maxElevation;
            _objectScatterLayer.MinElevation = minElevation;
            _objectScatterLayer.placeSingleItem = placeSingleItem;

            if (_mask != null)
                _objectScatterLayer.maskData = _mask.MaskData;

            return _objectScatterLayer;
        }

        public static TInstanceScatterLayer InstanceScatter
        (
            string prefabName,
            int seedNo,
            float averageDistance,
            //int gridResolution,
            bool rotation90Degrees,
            bool lockYRotation,
            bool getSurfaceAngle,
            float minRotationRange,
            float maxRotationRange,
            float positionVariation,
            Vector3 scaleMultiplier,
            float minScale,
            float maxScale,
            string unityLayerName,
            int maskLayer,
            string layerName,
            float minSlope,
            float maxSlope,
            Vector3 positionOffset,
            Vector3 rotationOffset,
            int priority,
            List<ObjectBounds> bounds,
            float minRange,
            float maxRange,
            bool receiveShadows,
            bool bypassLakes,
            bool underLakes,
            bool underLakesMask,
            bool onLakes,
            TShadowCastingMode shadowCastingMode,
            float LODMultiplier,
            List<TMask> _masks,
            bool isWorldOffset,
            bool prefabHasCollider,
            float maxDistance,
            float frustumMultiplier,
            bool checkBoundingBox,
            float maxElevation,
            float minElevation,
            bool occlusionCulling,
            List<TMask> masks,
            string NodeName
        )
        {
            TMask _mask = null;

            if (masks != null)
            {
                _mask = TMask.MergeMasks(masks);
                _mask = _mask?.FilteredMask(minRange, maxRange);
            }

            TInstanceScatterLayer _instanceScatterLayer = new TInstanceScatterLayer();
            _instanceScatterLayer.SeedNo = seedNo;
            _instanceScatterLayer.averageDistance = averageDistance;
            _instanceScatterLayer.rotation90Degrees = rotation90Degrees;
            _instanceScatterLayer.lockYRotation = lockYRotation;
            _instanceScatterLayer.getSurfaceAngle = getSurfaceAngle;
            _instanceScatterLayer.MaxRotationRange = (int)maxRotationRange;
            _instanceScatterLayer.MinRotationRange = (int)minRotationRange;
            _instanceScatterLayer.PositionVariation = positionVariation;
            _instanceScatterLayer.ScaleMultiplier = scaleMultiplier;
            _instanceScatterLayer.MinScale = minScale;
            _instanceScatterLayer.MaxScale = maxScale;
            _instanceScatterLayer.LayerName = NodeName;
            _instanceScatterLayer.MaxElevation = maxElevation;
            _instanceScatterLayer.MinElevation = minElevation;
            _instanceScatterLayer.MaxSlope = maxSlope;
            _instanceScatterLayer.MinSlope = minSlope;
            _instanceScatterLayer.Priority = priority;
            _instanceScatterLayer.UnityLayerName = unityLayerName;
            _instanceScatterLayer.UnityLayerMask = maskLayer;
            _instanceScatterLayer.useLayer = true;
            _instanceScatterLayer.Offset = positionOffset;
            _instanceScatterLayer.RotationOffset = rotationOffset;
            _instanceScatterLayer.shadowCastingMode = shadowCastingMode;
            _instanceScatterLayer.receiveShadows = receiveShadows;
            _instanceScatterLayer.bypassLake = bypassLakes;
            _instanceScatterLayer.underLake = underLakes;
            _instanceScatterLayer.underLakeMask = underLakesMask;
            _instanceScatterLayer.onLake = onLakes;
            _instanceScatterLayer.prefabName = prefabName;
            _instanceScatterLayer.HasCollider = prefabHasCollider;
            _instanceScatterLayer.maxDistance = maxDistance;
            _instanceScatterLayer.LODMultiplier = LODMultiplier;
            //_instanceScatterLayer.gridResolution = gridResolution;
            _instanceScatterLayer.frustumMultiplier = frustumMultiplier;
            _instanceScatterLayer.checkBoundingBox = checkBoundingBox;
            _instanceScatterLayer.occlusionCulling = occlusionCulling;

            if (_mask != null)
                _instanceScatterLayer.maskData = _mask.MaskData;

            return _instanceScatterLayer;
        }

        public static TGrassScatterLayer GrassScatter
        (
            TMaterial material,
            int maxParallelJobCount,
            Vector2 scale,
            float radius,
            float gridSize,
            float slant,
            float groundOffset,
            int amountPerBlock,
            string unityLayerName,
            string layerName,
            float alphaMapThreshold,
            float densityFactor,
            BuilderType builderType,
            NormalType normalType,
            TShadowCastingMode shadowCastingMode,
            int seedNo,
            float minSlope,
            float maxSlope,
            float minElevation,
            float maxElevation,
            float minRange,
            float maxRange,
            string Modelpath,
            string MeshName,
            bool layerBasedPlacement,
            bool bypassWater,
            bool underWater,
            bool onWater,
            int maskLayer,
            List<TMask> masks,
            string NodeName,
            float maskDamping
        )
        {
            TMask _mask = null;

            if (masks != null)
            {
                _mask = TMask.MergeMasks(masks);
                _mask = _mask?.FilteredMask(minRange, maxRange);
            }

            TGrassScatterLayer _grassScatterLayer = new TGrassScatterLayer();
            _grassScatterLayer.maxParallelJobCount = maxParallelJobCount;
            _grassScatterLayer.material = material;
            _grassScatterLayer.meshName = MeshName;
            _grassScatterLayer.modelPath = Modelpath;
            _grassScatterLayer.scale = scale;
            _grassScatterLayer.radius = radius;
            _grassScatterLayer.gridSize = gridSize;
            _grassScatterLayer.slant = slant;
            _grassScatterLayer.amountPerBlock = amountPerBlock;
            _grassScatterLayer.alphaMapThreshold = alphaMapThreshold;
            _grassScatterLayer.densityFactor = densityFactor;
            _grassScatterLayer.builderType = builderType;
            _grassScatterLayer.normalType = normalType;
            _grassScatterLayer.groundOffset = groundOffset;
            _grassScatterLayer.shadowCastingMode = shadowCastingMode;
            _grassScatterLayer.seedNo = seedNo;
            _grassScatterLayer.MinSlope = minSlope;
            _grassScatterLayer.MaxSlope = maxSlope;
            _grassScatterLayer.MinElevation = minElevation;
            _grassScatterLayer.MaxElevation = maxElevation;
            _grassScatterLayer.unityLayerName = unityLayerName;
            _grassScatterLayer.LayerName = NodeName;
            _grassScatterLayer.layerBasedPlacement = layerBasedPlacement;
            _grassScatterLayer.UnityLayerMask = maskLayer;
            _grassScatterLayer.bypassWater = bypassWater;
            _grassScatterLayer.underWater = underWater;
            _grassScatterLayer.onWater = onWater;
            _grassScatterLayer.maskDamping = maskDamping;

            if (_mask != null)
                _grassScatterLayer.maskData = _mask.MaskData;

            return _grassScatterLayer;
        }

        public static TLakeLayer  GetLakes
        (
            XmlDocument LandcoverXML,
            TMaterial material,
            float lodCulling,
            int AroundPointsDensity,
            float AroundVariation,
            string unityLayerName,
            Vector3 positionOffset,
            int priority,
            float Depth,
            float LakeMinSizeInM2,
            float deformAngle,
            TMap _currentMap,
            string NodeName
        )
        {
            float deformAngleRadian = deformAngle * (float)Math.PI / 180f;

            TLakeLayer _lakeLayer = new TLakeLayer();
            _lakeLayer.AroundPointsDensity = AroundPointsDensity;
            _lakeLayer.AroundVariation = AroundVariation;
            _lakeLayer.LayerName = NodeName;
            _lakeLayer.material = material;
            _lakeLayer.Priority = priority;
            _lakeLayer.UnityLayerName = unityLayerName;
            _lakeLayer.useLayer = true;
            _lakeLayer.Offset = positionOffset;
            _lakeLayer.depth = Depth;
            _lakeLayer.LODCulling = lodCulling;
            _lakeLayer._minSizeInM2 = LakeMinSizeInM2;
            List<T2DObject> lakes = _lakeLayer.LakesList;

            //OSMParser.ExtractLakes(CurrentMap.LandcoverXML, ref lakes, areaBounds, CurrentMap._area);
            //TLandcoverProccessor.FilterLakesBordersByBoundArea(areaBounds, CurrentMap, ref _lakeLayer, LakeMinSizeInM2);
            //_lakeLayer.WaterMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, lakes, false, 1, 1f, false);

            OSMParser.ExtractLakes(LandcoverXML, ref lakes, _currentMap._area);
            TLandcoverProccessor.FilterLakesBordersByBoundArea(_currentMap, ref _lakeLayer, LakeMinSizeInM2);
            _lakeLayer.WaterMasks = TLandcoverProccessor.GetBiomesMasks(_currentMap._refTerrain, lakes, false, 1, 1f, false);

            if (_lakeLayer?.WaterMasks?.Count > 0)
            {
                for (int i = 0; i < _lakeLayer.WaterMasks.Count; i++)
                {
                    TMask mask = _lakeLayer.WaterMasks[i];
                    THeightmapProcessors.DeformWaterByMask(ref _currentMap._refTerrain.Heightmap.heightsData, mask, false, null, 2,
                                                           TTerraWorld.WorldArea.AreaSizeLat,
                                                           deformAngleRadian, out lakes[i].minHeight, out lakes[i].maxHeight, out lakes[i].avgHeight);

                    //TDebug.LogInfoToUnityUI("THeightmapProcessors.GetminValue");
                    //THeightmapProcessors.GetminValue(ref _currentMap._refTerrain.Heightmap.heightsData, mask, out lakes[i].minHeight, out lakes[i].maxHeight, out lakes[i].avgHeight);
                    //mask.GetBWImage().Save("../Masks/" + lakes[i].name + ".jpg");
                }
            }

            //if (smoothOperation)
            //{
            //    List<TMask> masks = TLandcoverProccessor.GetBiomesMasks(_currentMap._refTerrain, lakes, false, 1, 1f, true);
            //    if (masks.Count > 0)
            //        THeightmapProcessors.SmoothWaterByMask(ref _currentMap._refTerrain.Heightmap.heightsData, masks[0],
            //                                            TTerraWorld.WorldGraph.areaGraph.WorldArea.WorldSizeKMLat, 20);
            //}

            return _lakeLayer;
        }

        public static TOceanLayer GetOceans
        (
            XmlDocument LandcoverXML,
            TMaterial material,
            float lodCulling,
            string unityLayerName,
            Vector3 positionOffset,
            int priority,
            float Depth,
            float deformAngle,
            TMap _currentMap,
            string NodeName
        )
        {
            float deformAngleRadian = deformAngle * (float)Math.PI / 180f;

            TOceanLayer _oceanLayer = new TOceanLayer();
            _oceanLayer.LayerName = NodeName;
            _oceanLayer.material = material;
            _oceanLayer.Priority = priority;
            _oceanLayer.UnityLayerName = unityLayerName;
            _oceanLayer.useLayer = true;
            _oceanLayer.Offset = positionOffset;
            _oceanLayer.depth = Depth;
            _oceanLayer.LODCulling = lodCulling;
            List<T2DObject> Oceans = _oceanLayer.Coastlines;

            //OSMParser.ExtractOceans(CurrentMap.LandcoverXML, ref Oceans, areaBounds, CurrentMap._area);
            //TLandcoverProccessor.FilterOceansBordersByBoundArea(areaBounds, CurrentMap, ref _oceanLayer);
            //_oceanLayer.WaterMasks = TLandcoverProccessor.GetBiomesMasks(areaBounds, _currentMap._refTerrain, Oceans, false, 0, 1f, true);

            OSMParser.ExtractOceans(LandcoverXML, ref Oceans, _currentMap._area);
            _oceanLayer.WaterMasks = TLandcoverProccessor.GetBiomesMasks(_currentMap._refTerrain, Oceans, false, 0, 1f, true);

            if (_oceanLayer.WaterMasks.Count > 0)
            {
                TMask mask = _oceanLayer.WaterMasks[0];
                T2DObject ocean = _oceanLayer.Coastlines[0];
                THeightmapProcessors.DeformWaterByMask(ref _currentMap._refTerrain.Heightmap.heightsData, mask, true, null, 2,
                                                       TTerraWorld.WorldArea.AreaSizeLat,
                                                       deformAngleRadian, out ocean.minHeight, out ocean.maxHeight, out ocean.avgHeight);
                //if (smoothOperation)
                //{
                //    THeightmapProcessors.SmoothWaterByMask(ref _currentMap._refTerrain.Heightmap.heightsData, mask,
                //                                                      TTerraWorld.WorldGraph.areaGraph.WorldArea.WorldSizeKMLat);
                //}
                //
                //THeightmapProcessors.GetminValue(ref _currentMap._refTerrain.Heightmap.heightsData, mask, out ocean.minHeight, out ocean.maxHeight, out ocean.avgHeight);
                //mask.GetBWImage().Save("../Masks/" + ocean.name + ".jpg");
            }

            return _oceanLayer;
        }

        public static TRiverLayer GetRivers
        (
            XmlDocument LandcoverXML,
            TMaterial material,
            float lodCulling,
            string unityLayerName,
            Vector3 positionOffset,
            int priority,
                float RiverWidthInMeter,
            float Depth,
            bool smoothOperation,
            TMap _currentMap,
            string NodeName
        )
        {
            TRiverLayer _riverLayer = new TRiverLayer();
            _riverLayer.LayerName = NodeName;
            _riverLayer.material = material;
            //_riverLayer.MaxElevation = areaBounds.maxElevation;
            //_riverLayer.MinElevation = areaBounds.minElevation;
            _riverLayer.Priority = priority;
            _riverLayer.UnityLayerName = unityLayerName;
            _riverLayer.useLayer = true;
            _riverLayer.Offset = positionOffset;
            _riverLayer.depth = Depth;
            _riverLayer.LODCulling = lodCulling;
            _riverLayer._width = RiverWidthInMeter;

            List<TLinearObject> rivers = _riverLayer.RiversList;

            //OSMParser.ExtractRivers(CurrentMap.LandcoverXML, ref rivers, areaBounds, CurrentMap._area);
            //TLinearMeshLayer river = (TLinearMeshLayer)_riverLayer;
            //TLandcoverProccessor.FilterRiverBordersByBoundArea(areaBounds, CurrentMap, ref river);

            OSMParser.ExtractRivers(LandcoverXML, ref rivers, _currentMap._area);
            //TLinearMeshLayer river = (TLinearMeshLayer)_riverLayer;
            //TLandcoverProccessor.FilterRiverBordersByBoundArea(CurrentMap, ref river);

            _riverLayer.WaterMasks = TLandcoverProccessor.GetBiomesMasksLinear(_currentMap._refTerrain, rivers, false, 0, 1f, (RiverWidthInMeter) / 2f, true);

            if (_riverLayer?.WaterMasks?.Count > 0)
            {
                for (int i = 0; i < _riverLayer.WaterMasks.Count; i++)
                {
                    TMask mask = _riverLayer.WaterMasks[i];
                    THeightmapProcessors.DeformByMask(ref _currentMap._refTerrain.Heightmap.heightsData, mask, _riverLayer.depth + 4, false, 1);
                    //mask.GetBWImage().Save("../Masks/" + river[i].name + ".jpg");
                }
            }

            if (smoothOperation)
            {
                //List<TMask> masks = TLandcoverProccessor.GetBiomesMasksLinear(_currentMap._refTerrain, rivers, false, 0, 1f, (RiverWidthInMeter) / 2f, true);
                //
                //if (masks.Count > 0)
                _currentMap._refTerrain.Heightmap.heightsData = THeightmapProcessors.SmoothHeightmap(
                                        _currentMap._refTerrain.Heightmap.heightsData, 1, 0, THeightmapProcessors.Neighbourhood.Moore);
            }

            return _riverLayer;
        }

        public static TGridLayer GetMeshLayer
        (
            int densityResolutionPerKilometer,
            int density,
            float edgeCurve,
            int gridCount,
            float lodCulling,
            Vector3 scale,
            string unityLayerName,
            bool hasShadowCasting,
            bool hasCollider,
            bool hasPhysics,
            bool SeperatedObject,
            string layerName,
            Vector3 positionOffset,
            int priority,
            TMaterial material,
            List<TMask> _masks,
            TMap _currentMap,
            string NodeName
        )
        {
            TGridLayer _gridLayer = new TGridLayer();
            _gridLayer.HasShadowCasting = hasShadowCasting;
            _gridLayer.HasCollider = hasCollider;
            _gridLayer.HasPhysics = hasPhysics;
            _gridLayer.KM2Resulotion = densityResolutionPerKilometer;
            _gridLayer.LayerName = NodeName;
            _gridLayer.material = material;
            _gridLayer.MinElevation = -100000;
            _gridLayer.MaxElevation = 100000;
            _gridLayer.MinSlope = 0;
            _gridLayer.MaxSlope = 90;
            _gridLayer.Priority = priority;
            _gridLayer.UnityLayerName = unityLayerName;
            _gridLayer.Offset = positionOffset;
            _gridLayer.Scale = scale;
            _gridLayer.EdgeCurve = edgeCurve;
            _gridLayer.density = density;
            _gridLayer.LODCulling = lodCulling;

            //if (_masks == null)
            //    TLandcoverProccessor.GenerateGrid(currentMap._refTerrain, null, ref _gridLayer, areaBounds, gridCount, SeperatedObject);
            //else if (_masks.Count > 0)
            //    for (int i = 0; i < _masks.Count; i++)
            //        TLandcoverProccessor.GenerateGrid(currentMap._refTerrain, _masks[i], ref _gridLayer, areaBounds, gridCount, SeperatedObject);

            if (_masks == null)
                TLandcoverProccessor.GenerateGrid(_currentMap._refTerrain, null, ref _gridLayer, gridCount, SeperatedObject);
            else if (_masks.Count > 0)
                for (int i = 0; i < _masks.Count; i++)
                    TLandcoverProccessor.GenerateGrid(_currentMap._refTerrain, _masks[i], ref _gridLayer, gridCount, SeperatedObject);

            return _gridLayer;
        }
    }
#endif
}
#endif
#endif

