// Script by Andrei Shulgach, 2017
// Feel free to use the code in your own project! If you do, shoot me an email witha link to your project or video. I'd love to check it out!

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class NPC : MonoBehaviour {

	bool inRange = false; // to be enabled when player is within range of NPC
	bool inChat = false; // to be enable and disabled when in/out of chat window
	bool inDialogue1 = true;
	bool inDialogueLeftSubTree = false;
	bool inDialogueUpSubTree = false;
	[Header("Objects")]
	public GameObject npcWindow;
	public Text chatText;
	public Text leftText;
	public Text upText;
	public Text rightText;
	[Header("All Possible Dialogue Options")]
	public string greeting;
	[Header("Dialogue 1")]
	public string left1;
	public string leftResponse1;
	public string up1;
	public string upResponse1;
	public string right1;
	public string rightResponse1;
	[Header("Dialogue 1 LEFT Sub Tree")]
	public string left2;
	public string leftResponse2;
	public string up2;
	public string upResponse2;
	public string right2;
	public string rightResponse2;
	[Header("Dialogue 1 UP Sub Tree")]
	public string left3;
	public string leftResponse3;
	public string up3;
	public string upResponse3;
	public string right3;
	public string rightResponse3;


	// Use this for initialization
	void Start () { 
		// by default for purpose of tutorial enable chat window
		inRange = true;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown("e")){
			// only load if in range and not already loaded
			if(inRange && !inChat){
				npcWindow.gameObject.SetActive(true);
				chatText.GetComponent<Text>().text = greeting;
				loadDialogue1();
			}
		}
        if(Input.GetKeyDown(KeyCode.UpArrow)){
			Up();
		}
        if(Input.GetKeyDown(KeyCode.LeftArrow)){
			Left();
		}
        if(Input.GetKeyDown(KeyCode.RightArrow)){
			Right();
		}
	}

	// first set of messages
	void loadDialogue1(){
		inChat = true;
		inDialogue1 = true;
		inDialogueLeftSubTree = false;
		inDialogueUpSubTree = false;
		leftText.GetComponent<Text>().text = left1;
		upText.GetComponent<Text>().text = up1;
		rightText.GetComponent<Text>().text = right1;
	}

	// first set, left button
	void loadDialogueLeftSubTree(){
		inDialogue1 = false;
		inDialogueLeftSubTree = true;
		inDialogueUpSubTree = false;
		leftText.GetComponent<Text>().text = left2;
		upText.GetComponent<Text>().text = up2;
		rightText.GetComponent<Text>().text = right2;
	}

	// second set, left button
	void loadDialogueLeftSubTree2(){
		inDialogue1 = false;
		inDialogueLeftSubTree = false;
		inDialogueUpSubTree = false;
		leftText.GetComponent<Text>().text = "";
		upText.GetComponent<Text>().text = "";
	}

	// first set, top button
	void loadDialogueUpSubTree(){
		inDialogue1 = false;
		inDialogueLeftSubTree = false;
		inDialogueUpSubTree = true;
		leftText.GetComponent<Text>().text = left3;
		upText.GetComponent<Text>().text = up3;
		rightText.GetComponent<Text>().text = right3;
	}

	// second set, top button
	void loadDialogueUpSubTree2(){
		inDialogue1 = false;
		inDialogueLeftSubTree = false;
		inDialogueUpSubTree = false;
		leftText.GetComponent<Text>().text = "";
		upText.GetComponent<Text>().text = "";
	}

	// if the player presses the left button at any point
	public void Left(){
		if(inDialogue1){
			chatText.GetComponent<Text>().text = leftResponse1;
			loadDialogueLeftSubTree();
		}else if(inDialogueLeftSubTree){
			chatText.GetComponent<Text>().text = leftResponse2;
			loadDialogueLeftSubTree2();
		}else if(inDialogueUpSubTree){
			chatText.GetComponent<Text>().text = leftResponse3;
			loadDialogueUpSubTree2();
		}
	}

	// if the player presses the up button at any point
	public void Up(){
		if(inDialogue1){
			chatText.GetComponent<Text>().text = upResponse1;
			loadDialogueUpSubTree();
		}else if(inDialogueLeftSubTree){
			chatText.GetComponent<Text>().text = upResponse2;
			loadDialogueLeftSubTree2();
		}else if(inDialogueUpSubTree){
			chatText.GetComponent<Text>().text = upResponse3;
			loadDialogueUpSubTree2();
		}
	}
		
	public void Right(){
		CloseDialogue();
	}

	void CloseDialogue(){
		npcWindow.gameObject.SetActive(false);
		inChat = false;
	}
}