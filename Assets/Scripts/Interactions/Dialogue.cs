using FMODUnity;
using UnityEngine;


[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue")]
public class Dialogue : ScriptableObject
{
    public bool noInteraction = false;

    public string _name;
    public EventReference[] dialogueSounds;
    [TextArea(3, 10)]
    public string[] dialogueLines;
    public Dialogue[] choices;
    public string[] choiceText;

    public bool dontContinueChoices = false;
    public bool pushNextWaypoint = true;
    

}
