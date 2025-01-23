using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;
using System;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;
using ViveSR.anipal.Eye;


public class SignalerManager : MonoBehaviour
{
   
    private InputBindings _inputBindings;
    private Collider _lastHit;
    private int _boxLayerMask;  // Only objects on the Box Layer should be hit by the raycast

    public GameObject hmd;

    public Vector3 eyePositionCombinedWorld;
    public Vector3 eyeDirectionCombinedWorld;
    public Quaternion eyeRotationCombinedWorld;

    public MenuManager menuManager;
    public GameManager gameManager;

    private EmbodimentManager embodimentManager;
    public GameObject simpleCrosshair;

    public List<TMP_Text> TextsPhase3;

    public bool signalerReady = false;  // What is this variable used for?

    public GameObject invisibleObject;
    public RaycastHit hitData;

    public int freezeCounter = 0;
    public LSLSignalerOutlets lSLSignalerOutlets;

    // Debug
    public GameObject focusDebugSphere;
    
   
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        menuManager = FindObjectOfType<MenuManager>();
        embodimentManager = FindObjectOfType<EmbodimentManager>();
        
        _inputBindings = new InputBindings();
        _inputBindings.Player.Enable();

        foreach (TMP_Text TextPhase3 in TextsPhase3)
        {
            TextPhase3.gameObject.SetActive(false);
        }

        _boxLayerMask = LayerMask.GetMask("Box");
    }

    // Update is called once per frame
    void Update()
    {
        // Start countdown // TODO: integrate countdown timer
        if (gameManager.frozen == false && freezeCounter > 2 && gameManager.countdownRunning == false)
        {
            //StartCoroutine(gameManager.CountdownTimer(gameManager.timerCountdownText));
        }

        // Get gaze data of the signaler
        SRanipal_Eye_v2.GetVerboseData(out VerboseData verboseData);
        eyePositionCombinedWorld = verboseData.combined.eye_data.gaze_origin_mm / 1000 + hmd.transform.position;
        Vector3 coordinateAdaptedGazeDirectionCombined = new Vector3(verboseData.combined.eye_data.gaze_direction_normalized.x * -1, verboseData.combined.eye_data.gaze_direction_normalized.y, verboseData.combined.eye_data.gaze_direction_normalized.z);
        eyeDirectionCombinedWorld = hmd.transform.rotation * coordinateAdaptedGazeDirectionCombined;
        invisibleObject.transform.position = eyePositionCombinedWorld + (eyeDirectionCombinedWorld * 5);

        // Create a ray from the eyes along the focus direction
        Ray ray;
        if (XRSettings.isDeviceActive)
        {
            ray = new Ray(eyePositionCombinedWorld, eyeDirectionCombinedWorld);
        }
        else  
        {
           Debug.LogError("No VR devices found in this frame. Using mouse position for signaler ray cast."); 
           Vector2 mouseScreenPosition = _inputBindings.Player.MousePosition.ReadValue<Vector2>();
           ray = Camera.main.ScreenPointToRay(mouseScreenPosition); 
        }

        // If the ray hits a box
        if (Physics.Raycast(ray, out hitData, Mathf.Infinity, _boxLayerMask)) // TODO: "&& gameManager.frozen" -> raycast only possible when it's the signaler's turn
        {
            // Let crosshair appear where the signaler is looking at 
            simpleCrosshair.SetActive(true);
            simpleCrosshair.transform.position = hitData.point;
            
            if (_lastHit == null)
            {
                _lastHit = hitData.collider;
                _lastHit.gameObject.SendMessage("StaredAt", SendMessageOptions.DontRequireReceiver);
            }
            else if (_lastHit != null && _lastHit != hitData.collider)
            {
                _lastHit.gameObject.SendMessage("NotLongerStaredAt", SendMessageOptions.DontRequireReceiver);
                _lastHit = hitData.collider;
                _lastHit.gameObject.SendMessage("StaredAt", SendMessageOptions.DontRequireReceiver);Vector3 screenPosition = Camera.main.WorldToScreenPoint(hitData.point);
            }
        }
        // If the former box is not stared at anymore and no new box is stared at
        else if (_lastHit != null)
        {
            _lastHit.gameObject.SendMessage("NotLongerStaredAt", SendMessageOptions.DontRequireReceiver);
            _lastHit = null;
            simpleCrosshair.SetActive(false);
        }


        if (gameManager.role == "signaler" && _inputBindings.Player.Freeze.triggered && gameManager.GetCurrentPhase() == 3)  // TODO: check if gameManager.role == "signaler" is necessary, since the script is disabled if the role is receiver anyways
        {
            if(freezeCounter < 1)
            {
                gameManager.PlayAudio();
            }
            freezeCounter += 1;

            Vector3 hitPoint = hitData.point;

            // Debug focus points 
            Debug.Log("Hit object: "+hitData.transform.name);
            focusDebugSphere.SetActive(true);
            focusDebugSphere.transform.position = hitPoint;

            // Create sample array
            float[] sample = new float[3];
            sample[0] = float.Parse(FormatFloat(hitPoint.x, 8)); // Convert string back to float
            sample[1] = float.Parse(FormatFloat(hitPoint.y, 8)); // Convert string back to float
            sample[2] = float.Parse(FormatFloat(hitPoint.z, 8)); // Convert string back to float

            // Push sample to LSL
            lSLSignalerOutlets.lslORaycastHitSignaler.push_sample(sample);
            int freezeCounterSignaler = freezeCounter;
            lSLSignalerOutlets.lslOFreezeCounterSignaler.push_sample(new int[] {freezeCounterSignaler});
            gameManager.firstFreeze = true; // Why is this necessary?


            if(signalerReady == false)
            {
                StartCoroutine(menuManager.ShowTexts(TextsPhase3, () => signalerReady = false));  // Once the coroutine is done, signalerReady will turn false
                signalerReady = true;
                string signalerReadyString = signalerReady.ToString();
                lSLSignalerOutlets.lslOSignalerReady.push_sample(new string[] {signalerReadyString});
            }

        }

    }

    public void Teleport(Vector3 location, GameObject avatar)
    {
        avatar.transform.position = location;
    }

    // // Prevent the eye gameobjects from moving according to the EyeTracking data.
    // public void Freeze()
    // {
    //     frozen = true;

    // }

    // // Make the eye gameobjects follow the participants' eye movements again.
    // public void Unfreeze()
    // {
    //     frozen = false;
    // }


    public static string FormatFloat(float value, int decimalPlaces)
    {
        return value.ToString($"F{decimalPlaces}");
    }



}



