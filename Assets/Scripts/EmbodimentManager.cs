using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class EmbodimentManager : MonoBehaviour
{
    public Transform controllerTransform; // Reference to the VR controller's transform
    public float raycastDistance = 100f; // Maximum distance of the raycast
    public LayerMask buttonLayer; // Layer mask for UI buttons

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

    private int Counter = 0;

    private Queue<Quaternion> storedRotations;
    public UnityEventQueueSystem onPressed;
    
    //Test
    private InputBindings _inputBindings;

    void Start()
    {
        StartRecording.gameObject.SetActive(false);
        StopRecording.gameObject.SetActive(false);
        ShowRecording.gameObject.SetActive(false);
        Finish.gameObject.SetActive(false);
        RecordingText.gameObject.SetActive(false);
        TV.gameObject.SetActive(false);

        StartRecording.onClick.AddListener(OnStartRecordingButtonClick);
        StopRecording.onClick.AddListener(OnStopRecordingButtonClick);
        ShowRecording.onClick.AddListener(OnShowRecordingButtonClick);
        Finish.onClick.AddListener(OnFinishButtonClick);

        storedRotations = new Queue<Quaternion>();
        
         ////////Test
        _inputBindings = new InputBindings();
        _inputBindings.UI.Enable(); 
    }

    void Update()
    {
        triggerButton();

        /*
        Ray ray = new Ray(controllerTransform.position, controllerTransform.forward);
        Debug.DrawRay(controllerTransform.position, controllerTransform.forward * 100, Color.yellow);

        // Check for button hit
        RaycastHit hit;

        //if (Physics.Raycast(ray, out hit, raycastDistance, buttonLayer))

        if (Physics.Raycast(ray, out hit))
        {
            // Check if the hit object is a UI button
            if (hit.collider.CompareTag("Button"))
            {
                Debug.LogError("1");

                // Check if the hit object has a Button component
                Button button = hit.collider.GetComponent<Button>();
                if (button != null)
                {
                    Debug.LogError("2");

                    // Check for input to trigger the button click event
                    if (_inputBindings.UI.Pressed.triggered) // Adjust the input as needed
                    {
                        Debug.LogError("3");

                        // Trigger the button's click event programmatically
                        button.onClick.Invoke();
                    }
                }
            }
        }    */
        if (gameManager.GetCurrentPhase() == 1)
        {
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
        /* /Test/*
        if (_inputBindings.UI.startrecordingtest.triggered)
        {
            OnStartRecordingButtonClick();
            Debug.LogError("start");
        }
        
        if (_inputBindings.UI.stoprecordingtest.triggered )
        {
            OnStopRecordingButtonClick();
            Debug.LogError("stop");
        }

        if (_inputBindings.UI.showrecordingtest.triggered )
        {
            OnShowRecordingButtonClick();
            Debug.LogError("show");
        } */
    }

    private void triggerButton()
    {
        // Check if the XR controller's collider overlaps with any UI buttons
        Collider[] colliders = Physics.OverlapSphere(controllerTransform.position, 50, buttonLayer);
        foreach (Collider collider in colliders)
        
        {
            // Check if the collider belongs to a UI button
            Button button = collider.GetComponent<Button>();
            if (button != null)
            {
                Debug.LogError("2");
                // Check for input to trigger the button click event
                if (_inputBindings.UI.Pressed.triggered)
                {
                    // Trigger the button's click event programmatically
                    button.onClick.Invoke();
                    Debug.LogError("3");
                }
            }
        }
    }


    /*
    private void triggerButton(Collider other) 
    {

        if (other.tag == "Button")
        {
            Debug.LogError("1");
            // Check if the hit object is a UI button
            Button button = hit.collider.GetComponent<Button>();
            if (button != null)
            {
                Debug.LogError("2");
                // Check for input to trigger the button click event
                if (_inputBindings.UI.Pressed.triggered) // Adjust the input as needed  //Input.GetButtonDown("Fire1")
                {
                    Debug.LogError("3");
                    // Trigger the button's click event programmatically
                    button.onClick.Invoke();
                }
            }
        }
    }
    */
    public void OnStartRecordingButtonClick()
    {
        
        RecordingText.gameObject.SetActive(true);
       // start data collection
       // start task
        StartCoroutine(StoreRotations());
    }

    private IEnumerator StoreRotations()
    {   
        while (!FinishedRecording)
        {
            storedRotations.Enqueue(playerEyes.transform.rotation);
            yield return new WaitForSeconds(0.02f);
        }
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

    public void OnFinishButtonClick()
    {
        End = true;
        StartRecording.gameObject.SetActive(false);
        StopRecording.gameObject.SetActive(false);
        Finish.gameObject.SetActive(false);
        gameManager.EnterNextPhase();

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


}
