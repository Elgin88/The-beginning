﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Mewlist.MassiveGrass
{
    public class MassiveGrassGrid
    {
        public struct CellIndex : IEquatable<CellIndex>
        {
            public readonly int x;
            public readonly int y;
            public readonly int hash;

            public CellIndex(int x, int y)
            {
                this.x = x;
                this.y = y;
                this.hash = x + (y << 16);
            }

            public override string ToString() => "(" + x + "," + y + ")";
            public override int GetHashCode() => hash;
            public override bool Equals(object obj) => obj is CellIndex other && Equals(other);
            public bool Equals(CellIndex other) => hash == other.hash;
        }

        private int separation;     // 分割数
        private float cellSize;       // Cellの大きさ
        private float cellSizeHalf;   // Cellの大きさ
        private Rect rect;
        private Terrain terrain;
        private List<CellIndex> activeIndices = new List<CellIndex>();

        public IEnumerable<Rect> ActiveRects => activeIndices.Select(RectFromIndex);

        public MassiveGrassGrid(Terrain terrain, int separation)
        {
            var bounds = terrain.terrainData.bounds;
            rect = new Rect(
                bounds.min.x + terrain.transform.position.x,
                bounds.min.z + terrain.transform.position.z,
                bounds.size.x,
                bounds.size.z);

            this.terrain = terrain;
            this.separation = separation;
            this.cellSize = rect.width / separation;
            this.cellSizeHalf = cellSize / 2f;
        }

        public CellIndex IndexFromPosition(Vector2 position)
        {
            var local = new Vector2(position.x - rect.xMin, position.y - rect.yMin);

            return new CellIndex(
                Mathf.FloorToInt(local.x / cellSize),
                Mathf.FloorToInt(local.y / cellSize));
        }

        private Vector2 CenterPos(CellIndex index)
        {
            return new Vector2(
                       index.x * cellSize + cellSizeHalf,
                       index.y * cellSize + cellSizeHalf)
                   + rect.min;
        }

        private Vector3 CenterPos3D(CellIndex index)
        {
            var centerPos2D = CenterPos(index);
            var localPos = centerPos2D - new Vector2(terrain.transform.position.x, terrain.transform.position.z);
            localPos /= rect.size.x;
            // TW_Tweaks
            var height = terrain.terrainData.GetInterpolatedHeight(localPos.x, localPos.y) + terrain.transform.position.y;
            // TW_Tweaks
            return new Vector3(
                index.x * cellSize + cellSizeHalf,
                height,
                index.y * cellSize + cellSizeHalf) + terrain.transform.position;
        }

        private Vector2 MinimumPos(CellIndex index)
        {
            return new Vector2(
                       index.x * cellSize,
                       index.y * cellSize)
                   + rect.min;
        }

        private Rect RectFromIndex(CellIndex index)
        {
            return new Rect(MinimumPos(index), cellSize * Vector2.one);
        }

        List<CellIndex> list = new List<CellIndex>();
        private List<CellIndex> InnerSphereIndices(Vector3 position, float range)
        {
            list.Clear();
            var hPos = new Vector2(position.x, position.z);
            var rectMinIndex = IndexFromPosition(hPos - Vector2.one * range);
            var rectMaxIndex = IndexFromPosition(hPos + Vector2.one * range);

            for (var x = rectMinIndex.x; x < rectMaxIndex.x; x++)
            {
                if (x < 0 || separation <= x) continue;

                for (var y = rectMinIndex.y; y < rectMaxIndex.y; y++)
                {
                    if (y < 0 || separation <= y) continue;

                    var index = new CellIndex(x, y);
                    var cellPos = CenterPos3D(index);
                    var direction = cellPos - position;

                    if (direction.magnitude <= range)
                        list.Add(index);
                }
            }

            return list;
        }

        private bool activateRunning = false;

        // TW Tweaks
        public bool resetGrids = false;
        public List<CellIndex> paintedIndices;
        // TW Tweaks

        public async Task Activate(Vector3 position, float range, ICellOperationCallbacks cellOperationCallback)
        {
            if (activateRunning) return;
            activateRunning = true;

            // TW Tweaks
            if (resetGrids)
            {
                foreach (var paintedIndex in paintedIndices)
                {
                    cellOperationCallback.RemoveIndexOnly(paintedIndex);
                    cellOperationCallback.Create(paintedIndex, RectFromIndex(paintedIndex));
                }

                activateRunning = false;
                resetGrids = false;
            }
            // TW Tweaks
            else
            {
                var activatedIndices = InnerSphereIndices(position, range);
                var entered = new List<CellIndex>();
                var exited = new List<CellIndex>();

                await Task.Run(() =>
                {
                    foreach (var activatedIndex in activatedIndices)
                    {
                        //if (resetGrids)
                        //{
                        //    entered.Add(activatedIndex);
                        //    exited.Add(activatedIndex);
                        //    Debug.Log("Reseting!");
                        //}
                        //else
                        {
                            var found = false;

                            foreach (var activeIndex in activeIndices)
                            {
                                if (!activatedIndex.Equals(activeIndex)) continue;
                                found = true;
                                break;
                            }

                            if (!found)
                                entered.Add(activatedIndex);
                        }
                    }

                    foreach (var activeIndex in activeIndices)
                    {
                        //if (resetGrids)
                        //{
                        //    //exited.Add(activeIndex);
                        //}
                        //else
                        {
                            var found = false;

                            foreach (var activatedIndex in activatedIndices)
                            {
                                if (!activeIndex.Equals(activatedIndex)) continue;
                                found = true;
                                break;
                            }

                            if (!found)
                                exited.Add(activeIndex);
                        }
                    }
                });

                foreach (var cellIndex in exited)
                    cellOperationCallback.Remove(cellIndex);

                foreach (var cellIndex in entered)
                    cellOperationCallback.Create(cellIndex, RectFromIndex(cellIndex));

                activeIndices.Clear();
                activeIndices.AddRange(activatedIndices);
                activateRunning = false;
            }
        }
    }
}

