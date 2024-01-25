using UnityEngine;
using UnityEditor;
using TerraUnity.Runtime.UI;

namespace TerraUnity.Runtime
{
    [CustomEditor(typeof(RuntimeSpawnerGO))]
    public class RuntimeSpawnerGOEditor : Editor
    {
        private RuntimeSpawnerGO script;

        public override void OnInspectorGUI()
        {
            if (script == null) script = (RuntimeSpawnerGO)target;

            if (script.bakeCombinedMeshes && script.isDirty)
            {
                GUIStyle style = new GUIStyle(EditorStyles.toolbarButton);
                style.fixedWidth = 128;
                style.fixedHeight = 32;
                THelpersUIRuntime.GUI_Button(new GUIContent("UPDATE LAYER", "Update layer and bake mesh data for further viewing distance using inserted billboard prefab"), style, UpdateLayer, 10, 0, new Color(0.9f, 0.375f, 0f, 1f));
                GUILayout.Space(20);
            }

            DrawDefaultInspector();
        }

        private void UpdateLayer ()
        {
            script.isChanged = true;
            script.isDirty = false;
        }
    }
}

