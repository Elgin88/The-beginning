#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEditor;

namespace LandscapeBuilder
{
    [ExecuteInEditMode]
    public class LBVolumeHighlight : MonoBehaviour
    {
        #region Public Properties
        //public Color MeshColour { get { return gizmoColour; } set { gizmoColour = value; } }

        #endregion

        #region Private Variables
        private Vector3 landscapePos;
        private Vector3 currentPosition = Vector3.zero;
        private float currentYRotation = 0f;
        private Vector3 currentScale = Vector3.one;
        private Vector3 prevScale = Vector3.one;
        private Vector3 newScale = Vector3.one;
        private bool volumeRectSet = false;
        private Mesh volumePreviewMesh;
        private bool volumePreviewMeshSet = false;
        private bool clampPositions = false;
        private bool clampScales = false;
        private Vector3 minClampPosition = Vector3.zero;
        private Vector3 maxClampPosition = Vector3.one;
        private Vector3 minClampScale = Vector3.zero;
        private Vector3 maxClampScale = Vector3.one;

        // Draw the full mesh rather than the default wire mesh
        private bool isDrawFullMesh = false;
        private Color gizmoColour;
        #endregion

        #region Event Methods

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColour;
            if (volumePreviewMesh != null)
            {
                // Draw the preview mesh
                Quaternion previewRot = Quaternion.Euler(0f, currentYRotation, 0f);
                if (isDrawFullMesh) { Gizmos.DrawMesh(volumePreviewMesh, currentPosition, previewRot, currentScale); }
                else { Gizmos.DrawWireMesh(volumePreviewMesh, currentPosition, previewRot, currentScale); }
            }
            else
            {
                // If volume preview mesh is not defined, draw a cube instead
                Vector3 previewPos = currentPosition;
                previewPos.y += currentScale.y * 0.5f;
                Gizmos.DrawWireCube(previewPos, currentScale);
            }
        }

        private void OnScene(SceneView sceneView)
        {
            // Draw a position handle
            Vector3 handlePos = currentPosition;
            Quaternion handleRotation = Quaternion.Euler(0f, currentYRotation, 0f);

            // Let user change size of area in scene view using the Scaling tool
            if (Tools.current == Tool.Scale)
            {
                prevScale = currentScale;

                // Display the scale / resizing handles
                newScale = Handles.ScaleHandle(currentScale, handlePos, handleRotation, HandleUtility.GetHandleSize(handlePos));

                float deltaX = newScale.x - prevScale.x;
                float deltaY = newScale.y - prevScale.y;
                float deltaZ = newScale.z - prevScale.z;

                // Is the scale-all axes component of the handle being used?
                if (deltaX != 0f && deltaY != 0f && deltaZ != 0f)
                {
                    // Seem to be bug or at least change of behaviour in U2019.3+
                    // scaling all axes produces very large numbers.
                    #if UNITY_2019_3_OR_NEWER
                    newScale = prevScale;
                    #endif

                    //Debug.Log("[DEBUG] x: " + deltaX + "z: " + deltaZ / prevScale.z);

                    //deltaX /= prevScale.x;
                    //deltaY /= prevScale.y;
                    //deltaZ /= prevScale.z;

                    //newScale.x = prevScale.x + deltaX;
                    //newScale.y = prevScale.y + deltaY;
                    //newScale.z = prevScale.z + deltaZ;
                }

                // Limit scales if specified
                if (clampScales)
                {
                    currentScale.x = Mathf.Clamp(newScale.x, minClampScale.x, maxClampScale.x);
                    currentScale.y = Mathf.Clamp(newScale.y, minClampScale.y, maxClampScale.y);
                    currentScale.z = Mathf.Clamp(newScale.z, minClampScale.z, maxClampScale.z);
                }
                else
                {
                    currentScale = newScale;
                }
            }
            else if (Tools.current == Tool.Rotate)
            {
                // Display rotation handles
                handleRotation = Handles.RotationHandle(handleRotation, handlePos);
                currentYRotation = handleRotation.eulerAngles.y;
            }
            else
            {
                // Display position handles
                currentPosition = Handles.PositionHandle(handlePos, Quaternion.identity);
                // Limit positions if specified
                if (clampPositions)
                {
                    currentPosition.x = Mathf.Clamp(currentPosition.x, minClampPosition.x, maxClampPosition.x);
                    currentPosition.y = Mathf.Clamp(currentPosition.y, minClampPosition.y, maxClampPosition.y);
                    currentPosition.z = Mathf.Clamp(currentPosition.z, minClampPosition.z, maxClampPosition.z);
                }
            }
        }

        private void OnEnable()
        {
            // Add the OnScene method to the drawing of the scene event (delegate?)
            // so that handles will be drawn even when the object isn't selected
            #if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnScene;
            SceneView.duringSceneGui += OnScene;
            #else
            SceneView.onSceneGUIDelegate -= OnScene;
            SceneView.onSceneGUIDelegate += OnScene;
            #endif
            Tools.hidden = true;
        }

        /// <summary>
        /// Called automatically by Unity when the gameobject loses focus
        /// </summary>
        private void OnDisable()
        {
            // Turn on the default scene handles
            Tools.hidden = false;
            Tools.current = Tool.Move;
        }

        private void OnDestroy()
        {
            #if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnScene;
            #else
            SceneView.onSceneGUIDelegate -= OnScene;
            #endif
            DestroyImmediate(volumePreviewMesh);
        }

        #endregion

        #region Public Non-Static Methods

        public void SetVolume(Rect areaRect, float yOffset, float yScale, float yRotation, Vector3 landscapePosition)
        {
            landscapePos = landscapePosition;
            currentPosition.x = areaRect.x + landscapePos.x;
            // Y offset is position of the bottom of the volume
            currentPosition.y = yOffset + landscapePos.y;
            currentPosition.z = areaRect.y + landscapePos.z;
            currentScale.x = areaRect.width;
            currentScale.y = yScale;
            currentScale.z = areaRect.height;
            currentYRotation = yRotation;
            volumeRectSet = true;
        }

        public Rect GetAreaRect()
        {
            Rect areaRect = new Rect();
            areaRect.x = currentPosition.x - landscapePos.x;
            areaRect.y = currentPosition.z - landscapePos.z;
            areaRect.width = currentScale.x;
            areaRect.height = currentScale.z;
            return areaRect;
        }

        // Re-build the preview mesh based on a specified RAW file
        public void UpdatePreviewMesh(LBRaw rawFile, bool useBlending, float centreSize, float fillCorners)
        {
            if (rawFile != null)
            {
                volumePreviewMesh = rawFile.CreatePreviewMesh(new Vector3(-0.5f, 0f, -0.5f), 65, useBlending, centreSize, fillCorners);
                volumePreviewMeshSet = true;
            }
            else { volumePreviewMesh = null; volumePreviewMeshSet = false; }
        }

        public bool VolumePreviewMeshSet() { return volumePreviewMeshSet; }

        public float GetYOffset() { return currentPosition.y - landscapePos.y; }

        public float GetYScale() { return currentScale.y; }

        public float GetYRotation() { return currentYRotation; }

        public bool VolumeRectSet() { return volumeRectSet; }

        // Set min and max positions and scales
        public void SetLimits(bool limitPositions, Vector3 minPosition, Vector3 maxPosition, bool limitScales, Vector3 minScale, Vector3 maxScale)
        {
            clampPositions = limitPositions;
            minClampPosition = minPosition;
            maxClampPosition = maxPosition;
            clampScales = limitScales;
            minClampScale = minScale;
            maxClampScale = maxScale;
        }

        #endregion

        #region Public Static Methods
        /// <summary>
        /// Create a new volume highlighter and return a reference to the LBVolumeHighlight
        /// script attached to the gameobject
        /// </summary>
        /// <returns></returns>
        public static LBVolumeHighlight CreateVolumeHighLighter()
        {
            LBVolumeHighlight highLighter = null;

            GameObject th = new GameObject("Volume Highlight");
            highLighter = th.AddComponent<LBVolumeHighlight>();

            if (highLighter != null)
            {
                highLighter.gizmoColour = LBLandscape.GetDefaultMeshVolumeHighlighterColour();
                highLighter.isDrawFullMesh = false;
            }

            return highLighter;
        }

        /// <summary>
        /// Create a new volume highlighter and return a reference to the LBVolumeHighlight
        /// script attached to the gameobject.
        /// </summary>
        /// <param name="isDrawFullMesh"></param>
        /// <param name="meshColour"></param>
        /// <returns></returns>
        public static LBVolumeHighlight CreateVolumeHighLighter(bool isDrawFullMesh, Color meshColour)
        {
            LBVolumeHighlight highLighter = null;

            GameObject th = new GameObject("Volume Highlight");
            highLighter = th.AddComponent<LBVolumeHighlight>();

            if (highLighter != null)
            {
                highLighter.gizmoColour = meshColour;
                highLighter.isDrawFullMesh = isDrawFullMesh;
            }

            return highLighter;
        }

        #endregion
    }
}
#endif