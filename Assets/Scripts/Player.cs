using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public GameObject centerEye;
    public GameObject cameraRig;
    public float speed;
    public static bool hasGuitar;
    public static int money; 
    public static string currentScene;
    public LibraryDialogueManager library_dm;
    public TrainDialogueManager train_dm;
    public CafeDialogueManager cafe_dm;

    void Start()
	{
        // Users arrives in environment
        if(SceneManager.GetActiveScene().name == "Environment"){
            // Startup
            if(currentScene == null){
                currentScene = "Environment";
                transform.position = new Vector3(333f, 1.5f, 455f);
                transform.Rotate(0f, 135f, 0f, Space.World);
            }
            // From library
            else if(currentScene == "Library"){
                currentScene = "Environment";
                transform.position = new Vector3(359f, 1.5f, 458.5f);
                transform.Rotate(0f, 0f, 0f, Space.World);
            }
            // From cafe
            else if(currentScene == "Cafe"){
                currentScene = "Environment";
                transform.position = new Vector3(331f, 1.5f, 472f);
                transform.Rotate(0f, -90f, 0f, Space.World);
            }
            // From train
            else if(currentScene == "Train"){
                currentScene = "Environment";
                transform.position = new Vector3(353f, 1.5f, 440f);
                transform.Rotate(0f, 45f, 0f, Space.World);
            }
        }
        else if(SceneManager.GetActiveScene().name == "Library"){
            currentScene = "Library";
        }
        else if(SceneManager.GetActiveScene().name == "CoffeeShop"){
            currentScene = "Cafe";
        }
        else if(SceneManager.GetActiveScene().name == "TrainStation"){
            currentScene = "Train";
        }
	}

    void Update()
    {
        // TODO: Stop movement when NPC game dialogue is active
        // #if UNITY_EDITOR
        //     if(!library_dm.InConversation() && !train_dm.InConversation() && !cafe_dm.InConversation()){
        //         if (Input.GetKey("w")) {
        //             transform.Translate(Vector3.forward * Time.deltaTime * 5);
        //         }
        //         else if (Input.GetKey("s")) {
        //             transform.Translate(-5 * Vector3.forward * Time.deltaTime);
        //         }

        //         if (Input.GetKey("a")) {
        //             Quaternion pitch = Quaternion.Euler(new Vector3(0, -1.5f * Time.deltaTime * 60, 0));
        //             transform.rotation *= pitch; 
        //         }
        //         else if (Input.GetKey("d")) {
        //             Quaternion pitch = Quaternion.Euler(new Vector3(0, 1.5f * Time.deltaTime * 60, 0));
        //             transform.rotation *= pitch; 
        //         }
        //     }
        // #else
            // if(!library_dm.InConversation() && !train_dm.InConversation() && !cafe_dm.InConversation()){
                // Debug.Log("Library: " + library_dm.InConversation());
                // Debug.Log("Train: " + train_dm.InConversation());
                // Debug.Log("Cafe: " + cafe_dm.InConversation());
                
                Vector2 joystick = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad);

                Debug.Log("X: " + joystick.x + "\tY:" + joystick.y);
                transform.eulerAngles = new Vector3(0, centerEye.transform.localEulerAngles.y, 0);
                transform.Translate(Vector3.forward * speed * joystick.y * Time.deltaTime);
                transform.Translate(Vector3.right * speed * joystick.x * Time.deltaTime);

                cameraRig.transform.position = Vector3.Lerp(cameraRig.transform.position, transform.position, 10f * Time.deltaTime);
            // }
        // #endif
    }
}