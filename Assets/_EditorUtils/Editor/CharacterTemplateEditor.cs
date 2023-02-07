using MetaVirus.Logic.Player;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace _EditorUtils.Editor
{
    [CustomEditor(typeof(CharacterTemplate))]
    public class CharacterTemplateEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var ct = serializedObject.targetObject.GetComponent<CharacterTemplate>();

            if (GUILayout.Button("Convert avatar setting to Avatar Long Data"))
            {
                ct.avatarLongData = ct.AvatarToLong();
            }

            if (GUILayout.Button("Parse avatar setting from Avatar Long Data"))
            {
                ct.ParseFromLongData(ct.avatarLongData);
            }
        }
    }
}