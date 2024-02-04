using UnityEngine;
using UnityEditor;

namespace TerraUnity.Runtime.UI
{
    [CustomEditor(typeof(ColormapBlendingManager))]
    public class ColormapBlendingManagerEditor : Editor
    {
        //private ColormapBlendingManager script { get => (ColormapBlendingManager)target; }

        public override void OnInspectorGUI()
        {
            if (ColormapBlendingManager.Colormap != null)
                DrawDefaultInspector();
            else
            {
                GUILayout.Space(10);
                THelpersUIRuntime.GUI_HelpBox("NO COLORMAP TEXTURES FOUND ON TERRAIN MATERIAL", MessageType.Warning);
                GUILayout.Space(20);
                EditorGUI.BeginDisabledGroup(true);
                DrawDefaultInspector();
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}

