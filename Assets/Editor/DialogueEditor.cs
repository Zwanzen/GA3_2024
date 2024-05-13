using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Dialogue))]
public class DialogueEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Dialogue dialogue = (Dialogue)target;
        dialogue.NPCname = EditorGUILayout.TextField("NPC Name", dialogue.NPCname);

        #region Dialogue Lines
        EditorGUILayout.PropertyField(serializedObject.FindProperty("dialogueLines"), true);
        #endregion

        #region Choices
        EditorGUILayout.HelpBox("After the text lines above are finished displaying, one of the choices below are continued. \n Max 4 choices.", MessageType.Info);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("choices"), true);
        EditorGUILayout.HelpBox("If no choices are selected, it will end the interaction.", MessageType.Info);


        dialogue.choiceText1 = EditorGUILayout.TextField("Choice1 Text", dialogue.choiceText1);
        EditorGUILayout.Space(5);
        dialogue.choiceText2 = EditorGUILayout.TextField("Choice2 Text", dialogue.choiceText2);
        EditorGUILayout.Space(5);
        dialogue.choiceText3 = EditorGUILayout.TextField("Choice3 Text", dialogue.choiceText3);
        EditorGUILayout.Space(5);
        dialogue.choiceText4 = EditorGUILayout.TextField("Choice4 Text", dialogue.choiceText4);
        #endregion

    }

}
