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

    public Button StartRecording;
    public Button StopRecording;
    public Button ShowRecording;
    public Button Finish;

    public GameObject TV;
    public GameObject TV1;
    public GameObject TV2;
    public Material Invisible;
    public Material Screen_off;

    public TMP_Text RecordingText;

    public GameManager gameManager;

    public GameObject playerEyes;
    public GameObject recordedEyes;

    private bool FinishedRecording = false;
    private bool FinishedShow = false;
    private bool End = false;
    private bool startedFirstTime = false;
    private bool CoroutineRunning = false;

    private int Counter = 0;

    public float embodimentTrainingStarted;
    public float embodimentTrainingEnd;

    private Queue<Quaternion> storedRotations;

    public TMP_Text EmbodimentInstruction0;
    public List<TMP_Text> EmbodimentInstructions;
    
    private bool ShowedStartEM = false;
    private bool ShowedText3 = false;
    private bool ShowedText4 = false;
    private bool ShowedText6 = false;
    //private bool StopButtonClicked = false;
    private bool StartButtonClicked = false;

    //Test
    private InputBindings _inputBindings;



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
        EmbodimentInstruction0.gameObject.SetActive(false);

        StartRecording.onClick.AddListener(OnStartRecordingButtonClick);
        StopRecording.onClick.AddListener(OnStopRecordingButtonClick);
        ShowRecording.onClick.AddListener(OnShowRecordingButtonClick);
        Finish.onClick.AddListener(OnFinishButtonClick);

        storedRotations = new Queue<Quaternion>();


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

        if (startedFirstTime == false)
        {
            embodimentTrainingStarted = Time.time;
        }

        if (gameManager.GetCurrentPhase() == 1)
        {
            if (ShowedStartEM == false)   
            {
                
                StartCoroutine(ShowEmbodimentInstructions(EmbodimentInstructions, () => CoroutineRunning = false));
                
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

        if (Counter > 2 && !End)
        {
            Finish.gameObject.SetActive(true);
        }

        if (gameManager.GetCurrentPhase() != 1)
        {
            StartRecording.gameObject.SetActive(false);
            StopRecording.gameObject.SetActive(false);
            ShowRecording.gameObject.SetActive(false);
            Finish.gameObject.SetActive(false);
            RecordingText.gameObject.SetActive(false);
        }
    }






    public void OnStartRecordingButtonClick()
    {
        //InstructionText3.gameObject.SetActive(false);
        RecordingText.gameObject.SetActive(true);
        // start data collection
        // start task
        StartCoroutine(StoreRotations());
    }
    public void OnStopRecordingButtonClick()
    {
        RecordingText.gameObject.SetActive(false);
        FinishedRecording = true;
        FinishedShow = false;
        TV.gameObject.SetActive(true);
        ChangeColor(TV1, Screen_off);
        ChangeColor(TV2, Screen_off);
    }

    public void OnShowRecordingButtonClick()
    {
        //show data simulation...
        StartCoroutine(ApplyRotations());
        ChangeColor(TV1, Invisible);
        ChangeColor(TV2, Invisible);
    }

    public void OnFinishButtonClick()
    {
        End = true;
        StartRecording.gameObject.SetActive(false);
        StopRecording.gameObject.SetActive(false);
        Finish.gameObject.SetActive(false);
        gameManager.EnterNextPhase();
        embodimentTrainingEnd = Time.time;

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
        Counter += 1;
    }

    private void ChangeColor(GameObject obj, Material newMaterial)
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
                else
                {
                    Debug.LogError("Object does not have enough materials.");
                }
            }
            else
            {
                Debug.LogError("Object does not have a Renderer component.");
            }
        }


    IEnumerator ShowEmbodimentInstructions(List<TMP_Text> textComponents, Action coroutineFinishedCallback)
    {
                
        EmbodimentInstruction0.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        EmbodimentInstruction0.gameObject.SetActive(false);
        ShowedStartEM = true;
        yield return new WaitWhile(() => !ShowedStartEM);

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
            else if (_inputBindings.UI.Continue.triggered)
            {
                Debug.LogError("ShowEmbodimentInstructions something triggered");
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
                    coroutineFinishedCallback?.Invoke(); // Callback to signal coroutine completion
                }
            }

            yield return null;
            CoroutineRunning = true;
            
        }
    } 
    
}
