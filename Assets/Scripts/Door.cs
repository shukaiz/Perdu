using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    public GameObject personOfInterest;
    public GameObject interactText;
    public string door;
    public bool withinDistance;
    public float distance;

    // Start is called before the first frame update
    void Start()
    {
        withinDistance = false;
        interactText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(Vector3.Distance(transform.position, personOfInterest.transform.position));
        if(Vector3.Distance(transform.position, personOfInterest.transform.position) < distance) {
            interactText.SetActive(true);
            if(interactText.activeSelf && (Input.GetKey("t") || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))){
                if(door=="cafe"){
                    SceneManager.LoadScene("Scenes/CoffeeShop");
                }
                else if(door=="library"){
                    SceneManager.LoadScene("Scenes/Library");
                }
                else if(door=="train"){
                    SceneManager.LoadScene("Scenes/TrainStation");
                }
                else if(door=="environment"){
                    SceneManager.LoadScene("Scenes/Environment");
                }
            }
            withinDistance = true;
        }
        else{
            if(withinDistance){
                interactText.SetActive(false);
                withinDistance = false;
            }
        }
    }
}
