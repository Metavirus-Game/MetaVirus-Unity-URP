using MetaVirus.Logic.Player;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace _EditorUtils.Editor
{
    [CustomEditor(typeof(NavMeshAgentTest))]
    public class NavMeshAgentTestEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (Application.isPlaying)
            {
                var test = serializedObject.targetObject.GetComponent<NavMeshAgentTest>();
                if (GUILayout.Button("Navigate to"))
                {
                    test.NavigateTo();
                }
            }
        }
    }
}