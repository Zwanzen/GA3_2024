using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class NPCInteraction : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private GameObject dialogueChoicesUI;
    [SerializeField] private GameObject[] choiceButtons;
    [Space(5)]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI continueText;

    [Space(10)]
    [Header("Dialogue")]
    [SerializeField] private Dialogue dialogue;

    [Space(10)]
    public bool canClick = false;

    //Private variables
    private const string interactTag = "NPC";
    private Queue<string> dialogueLines;

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

    private void Start()
    {
        dialogueLines = new Queue<string>();
        StartDialogue(dialogue);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && canClick)
        {
            DisplayNextLine();
        }
    }


    public void StartDialogue(Dialogue dialogue)
    {
        canClick = true;
        dialogueLines.Clear();
        dialogueUI.SetActive(true);
        dialogueChoicesUI.SetActive(false);
        continueText.text = "Click to continue...";


        foreach (string line in dialogue.dialogueLines)
        {
            dialogueLines.Enqueue(line);
        }

        DisplayNextLine();
    }

    private void DisplayNextLine()
    {


        if(dialogueLines.Count == 0)
        {

            if (dialogue.choices.Length > 0)
            {
                StartChoices();
                return;
            }


            EndDialogue();

            return;
        }

        string line = dialogueLines.Dequeue();
        dialogueText.text = line;

        if(dialogueLines.Count == 0 && dialogue.choices.Length == 0)
        {
            continueText.text = "End Conversation";
        }
    }

    private void EndDialogue()
    {
        dialogueUI.SetActive(false);
        dialogueChoicesUI.SetActive(false);
    }

    private void StartChoices()
    {
        canClick = false;
        dialogueUI.SetActive(false);
        dialogueChoicesUI.SetActive(true);
        ButtonSetup();
    }

    private void ButtonSetup()
    {
        //Reset all buttons
        foreach (GameObject button in choiceButtons)
        {
            button.SetActive(false);
            button.GetComponent<Button>().onClick.RemoveAllListeners();
        }

        //Setup new buttons
        for (int i = 0; i < dialogue.choices.Length; i++)
        {
            choiceButtons[i].gameObject.SetActive(true);
            choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = dialogue.choiceText[i];

            // Add listener to the button
            int index = i;
            choiceButtons[i].GetComponent<Button>().onClick.AddListener(() => SelectedChoice(index));
        }
    }

    // The UI button has to ref this and send int depending on what choice button it is.
    public void SelectedChoice(int choice)
    {
        dialogue = dialogue.choices[choice];
        StartDialogue(dialogue); 
    }
}
