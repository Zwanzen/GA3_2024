using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class DefaultNPCInteraction : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [SerializeField] private Dialogue dialogue;

    private const string interactTag = "NPC";

    // Function that makes sure the object has the correct tag.
    private void SetTag()
    {
        transform.tag = interactTag;
    }

    // Function that sets the tag in editor mode.
    // This is enough, but still have it in awake as well
    private void OnValidate()
    {
        SetTag();
    }

    private void Awake()
    {
        SetTag();
    }

    // The UI button has to ref this and send int depending on what choice button it is.
    public void SelectedChoice(int choice)
    {
        dialogue = dialogue.choices[choice];
        // StartDialogue(); when selecting a choice, it should loop again with the new dialogue
    }

}
