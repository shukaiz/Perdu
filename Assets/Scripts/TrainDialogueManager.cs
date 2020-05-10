using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainDialogueManager : MonoBehaviour
{
    public GameObject NPC;                  // The NPC object that's talking   
    public GameObject dialogueCanvas;       // The screen presented to the user when in dialogue
    public GameObject interactText;         // Text that appears when user is close enough to talk to NPC

    public GameObject Camera;               // Game camera

    public Text NPCname;                    // Which NPC is it
    public Text NPCdialogue;                // What the NPC is currently saying

    public GameObject topResponse;          // Top-most response (this is a panel in Unity)
    public GameObject midResponse;          // Middle response (this is a panel in Unity)
    public GameObject botResponse;          // Bot-most response (this is a panel in Unity)
    
    private bool inConversation;            // If we are currently engaged in a conversation with this NPC

    private int conversationIndex;          // What "state" of the conversation we're in. Used to index into dialogue, responses, and response destination lists
    private int selectionIndex;             // What response we're currently hovered over
    
    private Color selectedColor = new Color32(206, 234, 255, 255);  // Color of the box when the response is being "hovered" over 
    private Color normalColor = new Color32(255, 255, 255, 255);    // Color of the box when not being considered

    // THIS IS PURELY TO SHOW OFF HOW SPECIAL CHECKS WORK. THIS MEANS NOTHING IN THE CURRENT SCOPE OF THE PROJECT
    private bool legal = true;

    /*
    Consider using ReadOnlyCollections, probably make it faster
    */

    /*
    This is all just a state machine, with every index in each list corresponding to a node in a graph. DialogueOptions are what the NPC says, 
    responses are the users possible responses, and responseDestinations are where each response will take you in the state machine. 
    */

    private List<string> dialogueOptions = new List<string> {        // What the NPC says
    /* 0 */     "Hello, Welcome to the train station. How may I help you?", 
    /* 1 */     "One ticket costs 20 Euro. Could you please pay?",
    /* 2 */     "Well I can't give you a train ticket for free! What did you expect?",
    /* 3 */     "Would you like some suggestions on how to make money?",
    /* 4 */     "What situation would you like to hear about?",

    /* 5 */     "I'll be expecting you later. This is this the only way out of town.", 
    /* 6 */     "I might be wrong, but I heard people will pay for you to play guitar in the cafe.",
    /* 7 */     "I'm good friends with the librarian. I heard she needs help sorting books.",
    /* 8 */     "Thank you very much! You'll be able to board the next train. [Start End Game Sequence]",
    /* 9 */     "Excuse me? Please don't waste my time. I'll ask once again. How may I help you?"
    };

    private List<List<string>> responses = new List<List<string>> {   // What you can say in response to NPC (Currently hardcoded to three responses no matter what)
    /* 0 */     new List<string>{"I would like to buy a train ticket", "I would like to buy a hamburger", "My pet cat sings very pretty songs"},
    /* 1 */     new List<string>{"I have enough money to pay! Here you go.", "I don't have enough money to pay...", "*Fill in with non sense response*"},
    /* 2 */     new List<string>{"I don't know.... I'm sorry.", "I thought everything is free!", "*Fill in with non sense response*"},
    /* 3 */     new List<string>{"Yes please", "No, I'm fine. Thank you.", "*Fill in with non sense response*"},
    /* 4 */     new List<string>{"The Library", "The Cafe", "*Fill in with non sense response*"},

    /* 5 */     new List<string>{"Goodbye", "Thanks for the help. Bye", "*Fill in with non sense response*"},
    /* 6 */     new List<string>{"Ok, I'll go give it a try", "Sounds good. I should go before it's too late", "*Fill in with non sense response*"},
    /* 7 */     new List<string>{"Ok, I'll go give it a try", "Sounds good. I should go before it's too late", "*Fill in with non sense response*"},
    /* 8 */     new List<string>{"I can't wait to get out of here and finally go home...", "*Fill in with non sense response*", "*Fill in with non sense response*"},
    /* 9 */     new List<string>{"I would like to buy a train ticket", "I would like to buy a hamburger", "My pet cat sings very pretty songs"}
    };

    private List<List<int>> responseDestinations = new List<List<int>> {    // Where each response will take you (Ex. second response in the 4-index state will to you to 5)
    /* 0 */     new List<int>{1, 9, 9},
    /* 1 */     new List<int>{8, 2, 9},
    /* 2 */     new List<int>{3, 9, 9},
    /* 3 */     new List<int>{4, 5, 9},
    /* 4 */     new List<int>{7, 6, 9},

    /* 5 */     new List<int>{-1, -1, 9},
    /* 6 */     new List<int>{5, 5, 9},
    /* 7 */     new List<int>{5, 5, 9},
    /* 8 */     new List<int>{-1, 9, 9},
    /* 9 */     new List<int>{1, 9, 9}
    };

    void Start() {
        NPCname.text = "Train Conductor";
        selectionIndex = 0;
        conversationIndex = 0;
        UpdateResponseColors();
        UpdateConversation();
        dialogueCanvas.SetActive(false);
        inConversation = false;
    }

    void Update() {
        if(inConversation) {
            interactText.SetActive(false);

            // Exits the convo and resets it from the beginning
            if (Input.GetKey("q") || OVRInput.Get(OVRInput.Button.Back)) {
                ExitConversation();
                return;
            }

            // Cycles through the response options
            if(Input.GetKeyDown("m") || OVRInput.GetDown(OVRInput.Button.PrimaryTouchpad) ) {
                selectionIndex = (selectionIndex + 1) % 3;
            }

            // Selects a response
            if(Input.GetKeyDown("n") || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger)) {
                ProgressConversation(selectionIndex);
                selectionIndex = 0;
            }

            UpdateResponseColors();
        }

        else {
            // Find a better way to do this that doesn't if/else check every loop
            // Debug.Log(Vector3.Distance(transform.position, Camera.transform.position));
            if(Vector3.Distance(transform.position, Camera.transform.position) < 3.5f) {
                interactText.SetActive(true);
                if(Input.GetKeyDown("t") || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger)) {
                    EnterConversation();
                }
            }
            else{
                interactText.SetActive(false);
            }
        }
    }

    // There are hard coded values here. The better way to do is to get the forward vector of the NPC and have it set the camera a certain distance away
    void EnterConversation() {
        // Camera.transform.position = new Vector3(NPC.transform.position.x, 4f, NPC.transform.position.z + 1f); 
        // Camera.transform.LookAt(new Vector3(NPC.transform.position.x, 4f, NPC.transform.position.z));
        inConversation = true;
        dialogueCanvas.SetActive(true);
    }

    // Resets everything back to their starting conditions
    void ExitConversation() {
        Start();
    }

    void UpdateConversation() {
        NPCdialogue.text = dialogueOptions[conversationIndex];

        topResponse.GetComponentInChildren<Text>().text = responses[conversationIndex][0];
        midResponse.GetComponentInChildren<Text>().text = responses[conversationIndex][1];
        botResponse.GetComponentInChildren<Text>().text = responses[conversationIndex][2];
    }

    // Updates the conversation based on which button was pressed
    void ProgressConversation(int index) {
        switch (index) {
            case 0:
                conversationIndex = responseDestinations[conversationIndex][0];
                break;
            case 1:
                conversationIndex = responseDestinations[conversationIndex][1];
                break;
            case 2:
                conversationIndex = responseDestinations[conversationIndex][2];
                break;
            default:
                // This can't ever really happen
                return;
        }

        // Some dialogue options are only available with certain criteria
        CheckSpecialCases();

        // An index of -1 means to leave the conversation
        if(conversationIndex == -1) {
            ExitConversation();
        }

        UpdateConversation();
    }

    void CheckSpecialCases() {
        if(conversationIndex == 8) {
            if(legal == false){
                conversationIndex = 9;
            }
        }
    }

    void UpdateResponseColors() {
        switch(selectionIndex) {
            case 0:
                topResponse.GetComponent<Image>().color = selectedColor;
                midResponse.GetComponent<Image>().color = normalColor;
                botResponse.GetComponent<Image>().color = normalColor;
                break;
            case 1:
                topResponse.GetComponent<Image>().color = normalColor;
                midResponse.GetComponent<Image>().color = selectedColor;
                botResponse.GetComponent<Image>().color = normalColor;
                break;
            case 2:
                topResponse.GetComponent<Image>().color = normalColor;
                midResponse.GetComponent<Image>().color = normalColor;
                botResponse.GetComponent<Image>().color = selectedColor;
                break;
            default:
                break;
        }
    }

    // I added the camera object to make it cleaner, not sure how this fits in with that new design
    // void OnCollisionEnter(Collision collision)
    // {
    //     if(collision.gameObject.name=="OVRCameraRig")
    //     {
    //         interactText.SetActive(true);
    //     }
    // }

    // void OnCollisionExit(Collision collision)
    // {
    //     if(collision.gameObject.name=="OVRCameraRig")
    //     {
    //         interactText.SetActive(false);
    //     }
    // }

    // A small getter for the conversation
    public bool InConversation() {
        return inConversation;
    }
}
