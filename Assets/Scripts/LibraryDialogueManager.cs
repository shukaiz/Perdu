using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LibraryDialogueManager : MonoBehaviour
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
    /* 0 */     "This is the library! Welcome. Here we have many books about many different things. Do you need any help?", 
    /* 1 */     "Okay. I'll be here if you need me. I'm not going anywhere. After all, this is my job",
    /* 2 */     "Ah yes, I think I know exactly which book you're talking about. Here it is!",
    /* 3 */     "The French President? Wet Shoes? What are you talking about?",
    /* 4 */     "Exactly, after reading, it will be as if you had been playing for years. But the effect will eventually wear off.",

    /* 5 */     "What about the book? Oh nevermind, I would love to hear a joke.", 
    /* 6 */     "Of course. We have books of all types, including pretty flowers. Here you are. Anything else?",
    /* 7 */     "I don't know. Maybe the people?",
    /* 8 */     "Ahahahaha that's a good one. Jokes aside, is there anything I can help you with?"
    };

    private List<List<string>> responses = new List<List<string>> {   // What you can say in response to NPC (Currently hardcoded to three responses no matter what)
    /* 0 */     new List<string>{"I think I'm fine, thank you. If I need anything, I'll be sure to ask", "I heard you had a special book on guitars. Do you know anything about that?", "Charles de Gaulle likes his shoes wet"},
    /* 1 */     new List<string>{"Sounds good. Talk to you later!", "N/A", "N/A"},
    /* 2 */     new List<string>{"And reading this book will teach me how to play guitar, right?", "Actually, can I get a book on flowers instead? I like looking at the pictures.", "Do you want to hear a joke?"},
    /* 3 */     new List<string>{"I must've misspoke. Can you remind me where we are again?", "Uh... (Run Away)", "N/A"},
    /* 4 */     new List<string>{"That's great. Now I can go make money at the cafe. This is my ticket out of here. Bye", "Do you want to hear a joke?", "N/A"},

    /* 5 */     new List<string>{"What's the best part about Switzerland?", "N/A", "N/A"},
    /* 6 */     new List<string>{"This is fantastic. That's all I need. Thank you.", "Oh wow.... look at all the colors.... Wait. I need a book on guitars. I heard you have a special guitar book.", "N/A"},
    /* 7 */     new List<string>{"I don't know either, but their flag is a big plus!", "N/A", "N/A"},
    /* 8 */     new List<string>{"I think I'm fine, thank you. If I need anything, I'll be sure to ask", "I heard you had a special book on guitars. Do you know anything about that?", "Charles de Gaulle likes his shoes wet"},
    };

    private List<List<int>> responseDestinations = new List<List<int>> {    // Where each response will take you (Ex. second response in the 4-index state will to you to 5)
    /* 0 */     new List<int>{1, 2, 3},
    /* 1 */     new List<int>{-1, -1, -1},
    /* 2 */     new List<int>{4, 6, 5},
    /* 3 */     new List<int>{0, -1, -1},
    /* 4 */     new List<int>{1, 5, -1},

    /* 5 */     new List<int>{7, -1, -1},
    /* 6 */     new List<int>{-1, 2, -1},
    /* 7 */     new List<int>{8, -1, -1},
    /* 8 */     new List<int>{1, 2, 3},
    };

    void Start() {
        NPCname.text = "Librarian";
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
            if(Input.GetKeyDown("m") || OVRInput.GetDown(OVRInput.Button.PrimaryTouchpad)) {
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
        // Camera.transform.position = new Vector3(NPC.transform.position.x, 2f, NPC.transform.position.z + 2f); 
        // Camera.transform.LookAt(new Vector3(NPC.transform.position.x, 2f, NPC.transform.position.z));
        
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
