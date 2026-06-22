using RPGame.Progression;
using UnityEditor;

namespace RPGame.Progression.Editor
{
    [CustomEditor(typeof(JobDefinition))]
    public sealed class JobDefinitionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (target is JobDefinition job && UnityEngine.GUILayout.Button("Open Perk Tree Editor"))
            {
                PerkTreeEditorWindow.Open(job);
            }
        }
    }
}
