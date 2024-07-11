using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using System;

public class EmbodimentManager : MonoBehaviour
{
    public Transform leftControllerTransform; // Reference to the VR controller's transform
    public Transform rightControllerTransform;
    public Transform preferredHandTransform;
    public float raycastDistance = 100f; // Maximum distance of the raycast
    public LayerMask UI; // Layer mask for UI buttons

    

    private Queue<Quaternion> storedRotations;

    
    
    private InputBindings _inputBindings;

    public GameManager gameManager;
    public MenuManager menuManager;

    public GameObject playerEyes;
    public GameObject recordedEyes;



    //Signaler
    public float embodimentTrainingStartedSignaler;
    public float embodimentTrainingEndSignaler;

    private int Counter = 0;

    public Button StartRecording;
    public Button StopRecording;
    public Button ShowRecording;
    public Button Finish;

    public GameObject TV;
    public GameObject TV1;
    public GameObject TV2;
    public GameObject readySign;
    public Material Invisible;
    public Material Screen_off;

    public TMP_Text RecordingText;
    public List<TMP_Text> EmbodimentInstructions;
    
    private bool FinishedRecording = false;
    private bool FinishedShow = false;
    private bool End = false;
    private bool startedFirstTime = false;
    private bool CoroutineRunning = false;
    private bool startButtonClicked = false;
    private bool showStartEM = false;
    private bool didRun = false;
    public bool signalerReady = true;
    

    //Receiver
    public float embodimentTrainingStartedReceiver;
    public float embodimentTrainingEndReceiver;

    private int CounterReceiver = 0;

    public Button StartRecordingReceiver;
    public Button StopRecordingReceiver;
    public Button ShowRecordingReceiver;
    public Button FinishReceiver;

    public GameObject TVReceiver;
    public GameObject TV1Receiver;
    public GameObject TV2Receiver;
    public GameObject readySignReceiver;

    public TMP_Text RecordingTextReceiver;
    public List<TMP_Text> EmbodimentInstructionsReceiver;

    private bool FinishedRecordingReceiver = false;
    private bool FinishedShowReceiver = false;
    private bool EndReceiver = false;
    private bool startedFirstTimeReceiver = false;
    private bool startButtonClickedReceiver = false;
    private bool CoroutineRunningReceiver = false;
    private bool showStartEMReceiver = false;
    private bool didRunReceiver = false;
    public bool receiverReady = true;

    void Start()
    {
        
        preferredHandTransform = rightControllerTransform; //default is right for now
        StartRecording.gameObject.SetActive(false);
        StopRecording.gameObject.SetActive(false);
        ShowRecording.gameObject.SetActive(false);
        Finish.gameObject.SetActive(false);
        RecordingText.gameObject.SetActive(false);
        TV.gameObject.SetActive(false);
        
        foreach (TMP_Text EmbodimentInstruction in EmbodimentInstructions)
        {
            EmbodimentInstruction.gameObject.SetActive(false);
        }

        StartRecording.onClick.AddListener(OnStartRecordingButtonClick);
        StopRecording.onClick.AddListener(OnStopRecordingButtonClick);
        ShowRecording.onClick.AddListener(OnShowRecordingButtonClick);
        Finish.onClick.AddListener(OnFinishButtonClick);
        readySign.gameObject.SetActive(false);
        //Receiver
        RecordingTextReceiver.gameObject.SetActive(false);
        TVReceiver.gameObject.SetActive(false);
        
        foreach (TMP_Text EmbodimentInstructionReceiver in EmbodimentInstructionsReceiver)
        {
            EmbodimentInstructionReceiver.gameObject.SetActive(false);
        }

        StartRecordingReceiver.onClick.AddListener(OnStartRecordingButtonClick);
        StopRecordingReceiver.onClick.AddListener(OnStopRecordingButtonClick);
        ShowRecordingReceiver.onClick.AddListener(OnShowRecordingButtonClick);
        FinishReceiver.onClick.AddListener(OnFinishButtonClick);
        readySignReceiver.gameObject.SetActive(false);

        storedRotations = new Queue<Quaternion>();

        menuManager = FindObjectOfType<MenuManager>();

        _inputBindings = new InputBindings();
        _inputBindings.UI.Enable();

        
    }


    void Update()
    {
        // I think we are not using the internal ray for which the buttons are enabled to interact with
        // It is clear not even the first if condition is fulfilled, what shows us that something with the ray is off
        // Raycast from the controller position in the forward direction
        Ray ray = new Ray(preferredHandTransform.position, preferredHandTransform.forward);
        RaycastHit hit;

        // Check if the ray hits any UI buttons
        if (Physics.Raycast(ray, out hit, raycastDistance, UI))
        {
            Debug.Log("Button Pressed");
            // Check if the hit object has a Button component
            Button button = hit.collider.GetComponent<Button>();

            if (button != null)
            {


                // Check which button was pressed based on its tag
                if (hit.collider.tag == "StartRecording")
                {
                    Debug.Log("Start Button Pressed 1");
                    OnStartRecordingButtonClick();
                    Debug.Log("Start Button Pressed");
                }
                else if (hit.collider.CompareTag("StopRecording") && StopRecording.onClick != null)
                {
                    Debug.Log("Stop Button Pressed 1");
                    OnStopRecordingButtonClick();
                    Debug.Log("Stop Button Pressed");
                }
            }
        }
        if (gameManager.GetCurrentPhase() == 1)   
        {
            if (gameManager.role == "signaler")
            {

                if (!didRun && !CoroutineRunning)
                {  
                    StartCoroutine(ShowEmbodimentInstructions(EmbodimentInstructions, () => CoroutineRunning = false));
                    CoroutineRunning = true;
                    didRun = true;  
                }
                if (startButtonClicked  == true && startedFirstTime == false)
                {
                    embodimentTrainingStartedSignaler = Time.time;
                    startButtonClicked = false;
                    startedFirstTime = true;    
                }
                if (!FinishedRecording || FinishedShow)
                {
                    StartRecording.gameObject.SetActive(true);
                    StopRecording.gameObject.SetActive(true);
                    ShowRecording.gameObject.SetActive(false);
                    TV.gameObject.SetActive(false);
                }
                else
                {
                    StartRecording.gameObject.SetActive(false);
                    StopRecording.gameObject.SetActive(false);
                    ShowRecording.gameObject.SetActive(true);
                }


            }
            else if (gameManager.role == "receiver")
            {
                if (!didRunReceiver && !CoroutineRunningReceiver)
                {   
                    StartCoroutine(ShowEmbodimentInstructions(EmbodimentInstructionsReceiver, () => CoroutineRunningReceiver = false));
                    CoroutineRunningReceiver = true;
                    didRunReceiver = true;   
                }
                if (startedFirstTimeReceiver == false && startButtonClickedReceiver == true)
                {
                    embodimentTrainingStartedReceiver = Time.time;
                    Debug.Log("Started First Time"+ Time.time);
                    startButtonClickedReceiver = false;
                    startedFirstTimeReceiver = true;
                }
                if (!FinishedRecordingReceiver || FinishedShowReceiver)
                {
                    StartRecordingReceiver.gameObject.SetActive(true);
                    StopRecordingReceiver.gameObject.SetActive(true);
                    ShowRecordingReceiver.gameObject.SetActive(false);
                    TVReceiver.gameObject.SetActive(false);
                }

                else 
                {
                    StartRecordingReceiver.gameObject.SetActive(false);
                    StopRecordingReceiver.gameObject.SetActive(false);
                    ShowRecordingReceiver.gameObject.SetActive(true);
                } 
            }
            if (gameManager.role == "signaler")
            {
                if (Counter > 2 && !End)
                {
                    Finish.gameObject.SetActive(true);
                }
            }
            else if (gameManager.role == "receiver")
            {
                if (CounterReceiver > 2 && !EndReceiver)
                {
                    FinishReceiver.gameObject.SetActive(true);
                }
            }
        }    
        else if (gameManager.GetCurrentPhase() != 1)
        {
            StartRecording.gameObject.SetActive(false);
            StopRecording.gameObject.SetActive(false);
            ShowRecording.gameObject.SetActive(false);
            Finish.gameObject.SetActive(false);
            RecordingText.gameObject.SetActive(false);

            StartRecordingReceiver.gameObject.SetActive(false);
            StopRecordingReceiver.gameObject.SetActive(false);
            ShowRecordingReceiver.gameObject.SetActive(false);
            FinishReceiver.gameObject.SetActive(false);
            RecordingTextReceiver.gameObject.SetActive(false);
        }   
    }






    public void OnStartRecordingButtonClick()
    {

            //InstructionText3.gameObject.SetActive(false);
            RecordingText.gameObject.SetActive(true);
            RecordingTextReceiver.gameObject.SetActive(true);
            // start data collection
            // start task
            StartCoroutine(StoreRotations());
            
            if (gameManager.role == "signaler")
            {
                if(!startButtonClicked)
                {
                    startButtonClicked = true;
                    
                }
            }
            else if (gameManager.role == "receiver")
            {
                if(!startButtonClickedReceiver)
                {
                    startButtonClickedReceiver = true;
                
                }
            }
        


    }
    public void OnStopRecordingButtonClick()
    {
        if (gameManager.role == "signaler")
        {
            receiverReady = true; //to be changed  as soon as networking works
            signalerReady = true; //to be changed  as soon as networking works
            RecordingText.gameObject.SetActive(false);
            FinishedRecording = true;
            FinishedShow = false;
            TV.gameObject.SetActive(true);
            ChangeColor(TV1, Screen_off);
            ChangeColor(TV2, Screen_off);
        }
        if (gameManager.role == "receiver")
        {
            receiverReady = true; //to be changed  as soon as networking works
            signalerReady = true; //to be changed  as soon as networking works
            Debug.LogError("Receiver OnStopRecordingButtonClick");
            RecordingTextReceiver.gameObject.SetActive(false);
            FinishedRecordingReceiver = true;
            FinishedShowReceiver = false;
            TVReceiver.gameObject.SetActive(true);
            ChangeColor(TV1Receiver, Screen_off);
            ChangeColor(TV2Receiver, Screen_off);
        }
    }

    public void OnShowRecordingButtonClick()
    {
        //show data simulation...
        StartCoroutine(ApplyRotations());
        if (gameManager.role == "signaler")
        {        
            ChangeColor(TV1, Invisible);
            ChangeColor(TV2, Invisible);
            //ShowRecording.gameObject.SetActive(false);

            }
        if (gameManager.role == "receiver")
        {
            ChangeColor(TV1Receiver, Invisible);
            ChangeColor(TV2Receiver, Invisible);
            //ShowRecordingReceiver.gameObject.SetActive(false);
        }

    }

    public void OnFinishButtonClick()
    {
        if (gameManager.role == "signaler")
        {
            End = true;
            StartRecording.gameObject.SetActive(false);
            StopRecording.gameObject.SetActive(false);
            Finish.gameObject.SetActive(false);
            embodimentTrainingEndSignaler = Time.time;
            signalerReady = true;
            Debug.LogError(""+ receiverReady);
            Debug.LogError(""+ signalerReady);
            if(receiverReady)
            {
                gameManager.EnterNextPhase();
            }
        }
        if (gameManager.role == "receiver")
        {
            EndReceiver = true;
            StartRecordingReceiver.gameObject.SetActive(false);
            StopRecordingReceiver.gameObject.SetActive(false);
            FinishReceiver.gameObject.SetActive(false);
            embodimentTrainingEndReceiver = Time.time;
            receiverReady = true;
            Debug.LogError(""+ signalerReady);
            if(signalerReady)
            {
                gameManager.EnterNextPhase();
            }
        }

    }
    private IEnumerator StoreRotations()
    {
        while (!FinishedRecording)
        {
            storedRotations.Enqueue(playerEyes.transform.rotation);
            yield return new WaitForSeconds(0.02f);
        }
    }
    private IEnumerator ApplyRotations()
    {
        while (storedRotations.Count != 0)
        {
            recordedEyes.transform.localRotation = storedRotations.Dequeue();
            yield return new WaitForSeconds(0.02f);
        }
        //when recording finished:
        FinishedShow = true;
        FinishedShowReceiver = true;
        Counter += 1;
        CounterReceiver += 1;
    }

    public void ChangeColor(GameObject obj, Material newMaterial)
    {
        Renderer objectRenderer = obj.GetComponent<Renderer>();

        if (objectRenderer != null)
        {
            Material[] materials = objectRenderer.materials;

            if (materials.Length > 1)
            {
                 materials[1] = newMaterial; // Assuming you want to change the second material (index 1)
                objectRenderer.materials = materials; // Assign the modified materials array back to the Renderer
            }
        }
        Debug.LogError("color changed");
    }

    private IEnumerator ShowEmbodimentInstructions(List<TMP_Text> textComponents, Action coroutineFinishedCallback)
    {
       
        StartRecording.interactable = false;
        StopRecording.interactable = false;
        StartRecordingReceiver.interactable = false;
        StopRecordingReceiver.interactable = false;

        int currentTextIndex = 0;
        while (currentTextIndex < textComponents.Count)
        {
            textComponents[currentTextIndex].gameObject.SetActive(true);

            if (_inputBindings.UI.Return.triggered && currentTextIndex > 0)
            {
                textComponents[currentTextIndex].gameObject.SetActive(false);
                currentTextIndex -= 1;
                textComponents[currentTextIndex].gameObject.SetActive(true);
            }
            if (_inputBindings.UI.Continue.triggered)
            {
                textComponents[currentTextIndex].gameObject.SetActive(false);
                currentTextIndex += 1;
                if (currentTextIndex < textComponents.Count)
                {
                    textComponents[currentTextIndex].gameObject.SetActive(true);
                }
                else
                {
                    // When the last text component is reached
                    textComponents[textComponents.Count - 1].gameObject.SetActive(false);
                    ActivateButtons();
                    coroutineFinishedCallback?.Invoke(); // Invoke the callback if not null
                    yield break; // Exit the coroutine
                    
                }

                
            }
            yield return null;
        }
    }

    private void OtherPlayerReady()
    {
        if (signalerReady)
        {
            readySign.gameObject.SetActive(true);
        }
        else if (receiverReady)
        {
            readySignReceiver.gameObject.SetActive(true);
        }
    }

    void  ActivateButtons()
    {
        
        if (gameManager.role == "signaler")
        {
            //yield return new WaitWhile(() => CoroutineRunning = false);
            StartRecording.interactable = true;
            StopRecording.interactable = true;
        }
        if (gameManager.role == "receiver")
        {
            //yield return new WaitWhile(() => didRunReceiver = false);
            StartRecordingReceiver.interactable = true;
            StopRecordingReceiver.interactable = true;
        }
    }
}




