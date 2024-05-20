using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NPCInteraction : MonoBehaviour
{
    [Header("NPC & Ref")]
    [SerializeField] public PlayerController player;
    [SerializeField] public Dialogue dialogue;
    [SerializeField] private Transform lookAtPosition;

    [Space(20)]
    [Header("Interaction Controls")]
    public UnityEvent endInteractionEvent;
    [SerializeField] private KeycardType keycardType;

    private enum KeycardType
    {
        None,
        Red,
        Green,
        Blue
    }

    private GameObject dialogueUI;
    private GameObject dialogueChoicesUI;
    private GameObject[] choiceButtons;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI dialogueText;
    private TextMeshProUGUI continueText;
    private TextMeshProUGUI interactText;
    private KeycardController keycardController;

    private NPC_Controller npcController;

    public void Initialize(GameObject _dialogueUI, GameObject _dialogueChoicesUI, GameObject[] _choiceButtons,
        TextMeshProUGUI _nameText, TextMeshProUGUI _dialogueText, TextMeshProUGUI _continueText, TextMeshProUGUI _interactText,
        Animator _anim, KeycardController _keycardController)
    {
        dialogueUI = _dialogueUI;
        dialogueChoicesUI = _dialogueChoicesUI;
        choiceButtons = _choiceButtons;
        nameText = _nameText;
        dialogueText = _dialogueText;
        continueText = _continueText;
        interactText = _interactText;
        anim = _anim;
        keycardController = _keycardController;

    }

    public bool endedDialogue = false;
    public bool isInteracting = false;
    private bool canClick = false;

    //Private variables
    private const string interactTag = "NPC";
    private Queue<string> dialogueLines;
    [HideInInspector] public bool canInteract = false;
    private Animator anim;



    // Function that makes sure the object has the correct tag.
    private void SetTag()
    {
        transform.tag = interactTag;
    }

    private void Awake()
    {
        SetTag();
        player.InitializeNPC(this);
    }

    private void Start()
    {
        dialogueLines = new Queue<string>();
        if(lookAtPosition == null)
        {
            lookAtPosition = transform;
        }

        var npc = GetComponent<NPC_Controller>();
        if(npc != null)
        {
            npcController = npc;
        }
    }

    private void Update()
    {
        if (canInteract && Input.GetKeyDown(KeyCode.E))
        {
            StartDialogue();
            canInteract = false;
            interactText.gameObject.SetActive(false);
        }

        if (Input.GetMouseButtonDown(0) && canClick)
        {
            DisplayNextLine();
        }
    }

    // Check all requirements for interaction
    public bool CheckInteract()
    {

        if(endedDialogue)
        {
            return false;
        }

        if(keycardType == KeycardType.None)
        {
            return true;
        }

        if (keycardType == KeycardType.Red && keycardController.HasRedKeycard)
        {
            return true;
        }
        else if (keycardType == KeycardType.Green && keycardController.HasGreenKeycard)
        {
            return true;
        }
        else if (keycardType == KeycardType.Blue && keycardController.HasBlueKeycard)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    public void StartDialogue()
    {
        dialogueLines.Clear();
        anim.SetBool("Open", true);
        player.ToggleInteraction(true, lookAtPosition);
        canClick = true;
        dialogueUI.SetActive(true);
        dialogueChoicesUI.SetActive(false);
        continueText.text = "Click to continue...";
        nameText.text = dialogue._name;
        isInteracting = true;
        if(npcController != null)
        {
            npcController.IsInteracting();
        }


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
        StopAllCoroutines();
        StartCoroutine(TypeLine(line));

        if (dialogueLines.Count == 0 && dialogue.choices.Length == 0)
        {
            continueText.text = "End Conversation";
        }
    }

    IEnumerator TypeLine(string line)
    {
        dialogueText.text = "";
        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            // Make this a variable with a range probably
            yield return new WaitForSeconds(0.025f);
        }
    }

    private void EndDialogue()
    {
        isInteracting = false;
        dialogueUI.SetActive(false);
        dialogueChoicesUI.SetActive(false);
        player.ToggleInteraction(false, transform);
        anim.SetBool("Open", false);
        canClick = false;
        endedDialogue = true;

        if (npcController != null)
        {
            if (dialogue.pushNextDialogue)
            {
                npcController.StopInteracting();
                npcController.MoveToNextWaypoint();
            }
            npcController.StopInteracting();
        }
        endInteractionEvent.Invoke();
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
        StartDialogue(); 
    }
}
