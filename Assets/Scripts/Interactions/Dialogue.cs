using UnityEngine;


[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue")]
public class Dialogue : ScriptableObject
{
    public string _name;
    // FMOD SOUND REFERENCE HERE
    [TextArea(3, 10)]
    public string[] dialogueLines;
    public Dialogue[] choices;
    public string[] choiceText;

    public bool pushNextDialogue = true;

}
