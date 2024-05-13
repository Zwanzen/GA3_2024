using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue")]
public class Dialogue : ScriptableObject
{
    public string NPCname;
    [TextArea(3, 10)]
    public string[] dialogueLines;
    public Dialogue[] choices;
    public string choiceText1;
    public string choiceText2;
    public string choiceText3;
    public string choiceText4;
}
