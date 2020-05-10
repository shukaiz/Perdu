using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CafeDialogueManager : MonoBehaviour
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
    /* 0 */     "Welcome to the cafe. May I take your order?", 
    /* 1 */     "Of course! That will be 4,30 Franc. Would you like some Nutella as well?",
    /* 2 */     "Thank you very much. Is there anything else?",
    /* 3 */     "That's a great alternative. A common staple here in France. I'll have that right up",
    /* 4 */     "....? Oh wait! Yes! We need someone to play guitar. Our last guitar player left two days ago. We will pay you 15 francs",

    /* 5 */     "Spoken like a true Italian, only a fool would believe such a thing!", 
    /* 6 */     "Uh..... okay. Is there anything else you want to say?",
    /* 7 */     "I've never tried myself, but I've been told there's a book in the library that will teach you how to play",
    /* 8 */     "When you have your guitar, just start playing here in the cafe and we'll pay you. Anyways, if you ordered anything, it's probably ready by now.",
    /* 9 */     "If you're not here to order, I'm going to ask you to leave!",

    /* 10 */    "Like I said, I just overheard it. I don't know myself. If you ordered anything, it's probably ready by now"
    };

    private List<List<string>> responses = new List<List<string>> {   // What you can say in response to NPC (Currently hardcoded to three responses no matter what)
    /* 0 */     new List<string>{"Hi! May I have a baguette and a cafe creme?", "Italy is actually better at soccer than France...", "I heard that there are job opportunities at the cafe?"},
    /* 1 */     new List<string>{"That sounds delicious. Here is 5 francs. You can keep the change", "Could I have some Jam instead? Regardless, here is your payment", "Oh 4,30 Franc? I did't know it would cost so much. I'm afraid I don't have enough money. Do you know anyway else I could pay you?"},
    /* 2 */     new List<string>{"No, that's all for me today. Thank you kindly!", "Actually, I heard there I can make money at the cafe. Do you know anything about this?", "The octopus is capable of flying south every winter!"},
    /* 3 */     new List<string>{"Thank you. Have a nice day", "Actually, I heard there's a way I can make money at the cafe. Do you know anything about this?", "*Fill in with non sense response*"},
    /* 4 */     new List<string>{"Guitar? I don't know how to play guitar.", "I'm a pretty decent guitar player. When can I start?", "The ice cream will be arriving during the movie"},

    /* 5 */     new List<string>{"You're just mad because it's true", "Tooth paste tastes very good!", "I'm sorry, someone told me it would be a funny joke to make."},
    /* 6 */     new List<string>{"Hi! May I have a baguette and a cafe creme?", "Italy is actually better at soccer than France...", "I heard that there are job opportunities at the cafe?"},
    /* 7 */     new List<string>{"There is no way you can learn an instrument just by reading a book", "Huh, that sounds like a good place to start", "Your whale drinks water like a car"},
    /* 8 */     new List<string>{"Will do. I'll see you later!", "*Fill in with non sense response*", "*Fill in with non sense response*"},
    /* 9 */     new List<string>{"..... (leave the cafe)", "*Fill in with non sense response*", "*Fill in with non sense response*"},

    /* 10 */    new List<string>{"Thank you. Have a nice day!", "*Fill in with non sense response*", "*Fill in with non sense response*"}

    };

    private List<List<int>> responseDestinations = new List<List<int>> {    // Where each response will take you (Ex. second response in the 4-index state will to you to 5)
    /* 0 */     new List<int>{1, 5, 4},
    /* 1 */     new List<int>{2, 3, 4},
    /* 2 */     new List<int>{-1, 4, 6},
    /* 3 */     new List<int>{-1, 4, -1},
    /* 4 */     new List<int>{7, 8, 6},

    /* 5 */     new List<int>{9, 6, 6},
    /* 6 */     new List<int>{1, 5, 4},
    /* 7 */     new List<int>{10, 10, 6},
    /* 8 */     new List<int>{-1, -1, -1},
    /* 9 */     new List<int>{-1, -1, -1},

    /* 10 */    new List<int>{-1, -1, -1}
    };

    void Start() {
        NPCname.text = "Cashier";
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
            Debug.Log(Vector3.Distance(transform.position, Camera.transform.position));
            if(Vector3.Distance(transform.position, Camera.transform.position) < 2f) {
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
        // Camera.transform.position = new Vector3(NPC.transform.position.x, 1f, NPC.transform.position.z - 1f); 
        // Camera.transform.LookAt(new Vector3(NPC.transform.position.x, 1f, NPC.transform.position.z));
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
