using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Mewlist.MassiveGrass
{
#if UNITY_EDITOR
    public partial class MassiveGrass
    {
        //private bool baking        = false;
        //private bool reserveBaking = false;

        public void Refresh(MassiveGrassProfile filter = null, Vector2 mousePos = new Vector2())
        {
            // TW Tweaks
            if (filter != null)
            {
                foreach (var cameraCollection in rendererCollections.Values)
                    foreach (RendererCollection massiveGrassRenderer in cameraCollection.Values)
                        foreach (MassiveGrassRenderer renderer in massiveGrassRenderer.renderers.Values)
                        {
                            MassiveGrassProfile profile = renderer.profile;

                            if (profile.Equals(filter))
                            {
                                float gridSizeHalf = profile.GridSize / 2f;
                                MassiveGrassGrid MGG = renderer.Grid;
                                Vector2 topOffset = mousePos; topOffset.y += gridSizeHalf;
                                Vector2 leftOffset = mousePos; leftOffset.x -= gridSizeHalf;
                                Vector2 bottomOffset = mousePos; bottomOffset.y -= gridSizeHalf;
                                Vector2 rightOffset = mousePos; rightOffset.x += gridSizeHalf;
                                MassiveGrassGrid.CellIndex paintedIndex = new MassiveGrassGrid.CellIndex(MGG.IndexFromPosition(mousePos).x, MGG.IndexFromPosition(mousePos).y);
                                MassiveGrassGrid.CellIndex paintedIndexTop = new MassiveGrassGrid.CellIndex(MGG.IndexFromPosition(mousePos).x, MGG.IndexFromPosition(topOffset).y);
                                MassiveGrassGrid.CellIndex paintedIndexLeft = new MassiveGrassGrid.CellIndex(MGG.IndexFromPosition(leftOffset).x, MGG.IndexFromPosition(mousePos).y);
                                MassiveGrassGrid.CellIndex paintedIndexBottom = new MassiveGrassGrid.CellIndex(MGG.IndexFromPosition(mousePos).x, MGG.IndexFromPosition(bottomOffset).y);
                                MassiveGrassGrid.CellIndex paintedIndexRight = new MassiveGrassGrid.CellIndex(MGG.IndexFromPosition(rightOffset).x, MGG.IndexFromPosition(mousePos).y);

                                MGG.paintedIndices = new List<MassiveGrassGrid.CellIndex>()
                                {
                                    paintedIndex,
                                    paintedIndexTop,
                                    paintedIndexLeft,
                                    paintedIndexBottom,
                                    paintedIndexRight
                                };

                                MGG.resetGrids = true;
                                renderer.OnBeginRender();
                                break;
                            }
                        }
            }
            // TW Tweaks
            else
            {
                foreach (var cameraCollection in rendererCollections.Values)
                    foreach (var massiveGrassRenderer in cameraCollection.Values)
                        massiveGrassRenderer.Dispose();  
                
                rendererCollections.Clear();
              
                SetupBounds();
              
                //TODO: Check to see if the following line is needed!
                Render();
            }
        }

// TW Tweaks
        //private void OnDrawGizmosSelected()
        //{
        //    foreach (var terrain in terrains)
        //    {
        //        if (terrain != null)
        //        {
        //            Gizmos.color = Color.cyan;
        //            MassiveGrassGizmo.DrawBounds(terrain.transform.position, terrain.terrainData.bounds);
        //        }
        //    }
        //        
        //    if (!isActiveAndEnabled) return;
        //
        //    foreach (var keyValuePair in rendererCollections)
        //    {
        //        var targetTerrain = keyValuePair.Key;
        //        var cameraCollection = keyValuePair.Value;
        //        if (targetTerrain == null) continue;
        //
        //        // bounds
        //        Gizmos.color = Color.black;
        //        //MassiveGrassGizmo.DrawBounds(targetTerrain.transform.position, boundsMesh.bounds);
        //        cameraCollection.TryGetValue(Camera.current, out var grassRenderer);
        //        var count = cameraCollection[Camera.current].renderers.Count;
        //
        //        var colors = Enumerable
        //            .Range(0, count)
        //            .Select(v => new Color((float) v / count, (1f + Mathf.Sin((float)v / count)) / 2f, 1 - (float) v / count))
        //            .ToList();
        //        
        //        if (grassRenderer != null)
        //        {
        //            int i = 0;
        //            foreach (var v in cameraCollection[Camera.current].renderers.Values)
        //            {
        //                Gizmos.color = colors[i++];
        //
        //                // grid
        //                foreach (var gridActiveRect in v.Grid.ActiveRects)
        //                {
        //                    var localPos = gridActiveRect.center - new Vector2(targetTerrain.transform.position.x,
        //                                       targetTerrain.transform.position.z);
        //                    localPos /= targetTerrain.terrainData.bounds.size.x;
        //                    var height = targetTerrain.terrainData.GetInterpolatedHeight(localPos.x, localPos.y);
        //                    MassiveGrassGizmo.DrawRect(gridActiveRect, height);
        //                }
        //            }
        //        }
        //    }
        //}
// TW Tweaks
    }
#endif
}

