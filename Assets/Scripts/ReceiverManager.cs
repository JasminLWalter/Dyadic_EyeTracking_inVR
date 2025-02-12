using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using System;


public class ReceiverManager : MonoBehaviour
{
    // Input bindings determine from what VR devices and which buttons the input values are retrieved 
    private InputBindings _inputBindings;

    // Eye tracking variables
    public GameObject hmd;
    public Vector3 eyePositionCombinedWorld;
    public Vector3 eyeDirectionCombinedWorld;
    public Quaternion eyeRotationCombinedWorld;
    public GameObject invisibleObjectReceiver;

    // Raycast variables
    private int _boxLayerMask;  // Only objects on the Box Layer should be hit by the raycast
    public RaycastHit hitData;
    public Collider _lastHit;

    // VR controller variables
    public Transform leftControllerTransform; // Reference to the VR controller's transform // TODO: why not used?
    public Transform rightControllerTransform;
    public Transform preferredHandTransform;

    // References to other managers
    public GameManager gameManager;
    public MenuManager menuManager;
    public SignalerManager signalerManager;

    // Game flow variables
    public bool boxSelected = false;  // Shows if a box has been selected in the current round
    public List<TMP_Text> TextsPhase3Receiver;  // A list of instructional texts for the training phase
    public bool receiverReady = false;  // TODO: what is it used for?
    public int selectCounter = 0;  // Stores the number of selections made

    // Countdown variables (not used at the moment)
    private bool countdownRunning = false;
    public bool CountdownStarted = false;


    public bool secondCheck = false;  // TODO: what is it used for?

    // Networking
    public LSLReceiverOutlets lSLReceiverOutlets;

    // Start is called before the first frame update
    // Used to assign the still missing attributes
    void Start()
    {
        preferredHandTransform = rightControllerTransform;
        gameManager = FindObjectOfType<GameManager>();
        menuManager = FindObjectOfType<MenuManager>();
        signalerManager = FindObjectOfType<SignalerManager>();

        _inputBindings = new InputBindings();
        _inputBindings.Player.Enable();

        foreach (TMP_Text TextPhase3Receiver in TextsPhase3Receiver)
        {
            TextPhase3Receiver.gameObject.SetActive(false);
        }

        _boxLayerMask = LayerMask.GetMask("Box");
    }

    // Update is called once per frame
    // Meaning the following code runs again and again throughout the game
    void Update()
    {
        // Create a ray from the controller along the pointing direction
        Ray ray;
        if (XRSettings.isDeviceActive)
        {
            ray = new Ray(preferredHandTransform.position, preferredHandTransform.forward);
        }
        else
        {
            Debug.LogError("No VR devices found in this frame. Using mouse position for receiver ray cast."); 
            Vector2 mouseScreenPosition = _inputBindings.Player.MousePosition.ReadValue<Vector2>();
            ray = Camera.main.ScreenPointToRay(mouseScreenPosition);
        }

        // If the ray hits a box 
        if (Physics.Raycast(ray, out hitData, Mathf.Infinity, _boxLayerMask)) // TODO: should be only possible when the signaler is frozen?
        {
            // Let the box be highlighted while the receiver is pointing at it
            if (_lastHit == null) // if it is the first box that is pointed at
            {
                _lastHit = hitData.collider;
                _lastHit.gameObject.SendMessage("PointedAt"); // TODO: change the name of the functtion
            }
            else if (_lastHit != null && _lastHit != hitData.collider) // if it is the second (or more) box that is pointed at
            {
                // Make the old box not be highlighted anymore
                _lastHit.gameObject.SendMessage("NotLongerPointedAt");
                _lastHit = hitData.collider;
                // Let the new box be highlighted
                _lastHit.gameObject.SendMessage("PointedAt");
            }
            
            // If the receiver is selecting the current box and the current phase is the testing phase (isn't the second condition redundant?)
            if (_inputBindings.Player.SelectBox.triggered && gameManager.GetCurrentPhase() == 3)   // change to GetCurrentPhase() == 2
            {   
                // If still in training phase
                if (selectCounter < 1)
                {
                    if(menuManager.didRunReceiver && !receiverReady)
                    {
                        gameManager.PlayAudio();
                        selectCounter++;
                        secondCheck = true;
                        StartCoroutine(menuManager.ShowTexts(TextsPhase3Receiver, () => receiverReady = false));
                        receiverReady = true;
                        string receiverReadyString = receiverReady.ToString();
                        lSLReceiverOutlets.lslOReceiverReady.push_sample(new string[] {receiverReadyString} );
                        
                        
                        foreach (TMP_Text TextPhase3 in signalerManager.TextsPhase3Signaler)
                        {
                            TextPhase3.gameObject.SetActive(false);
                        }
                    }
                }
                // If not in training phase anymore
                else if (selectCounter >= 1 && gameManager.frozen)  // TODO: put gameManager.frozen to the Raycast-if
                {
                    _lastHit.gameObject.SendMessage("Selected");
                    selectCounter++;
                    boxSelected = true;

                    // Store the coordinates of where the receiver pointed at when selecting the box // TODO: why are coordinates necessary?
                    Vector3 hitPoint = hitData.point;

                    // Create sample array
                    float[] sample = new float[3];
                    sample[0] = hitPoint.x;
                    sample[1] = hitPoint.y;
                    sample[2] = hitPoint.z;

                    // Push sample to LSL
                    lSLReceiverOutlets.lslOBoxSelectedByReceiver.push_sample(sample);

                    StartCoroutine(gameManager.Condition1());  // TODO: let this not be called from this script
                    //StartCoroutine(CountdownTimer(timerCountdownText));
                    gameManager.trialNumber++;
                    lSLReceiverOutlets.lslOSelectCounter.push_sample(new int[] {selectCounter} );
                    
                    if (gameManager.role == "receiver") // TODO: if-clause not necessary, since the script is disabled if role is signaler anyways
                    {
                        gameManager._startedRound = false;
                    }
                }
            }

        }
        // If nothing is pointed at anymore
        else if (_lastHit != null) 
        {
            _lastHit.gameObject.SendMessage("NotLongerPointedAt");
            _lastHit = null;
        }

        else if (gameManager.frozen && gameManager.countdownRunning == false)
        {
            gameManager.StartCoroutine(gameManager.CountdownTimer(gameManager.timerCountdownTextReceiver));
        }

        
        // Get eye data of receiver 
        // Only necessary for free moving condition! which is not implemented yet
        SRanipal_Eye_v2.GetVerboseData(out VerboseData verboseData);
        eyePositionCombinedWorld = verboseData.combined.eye_data.gaze_origin_mm / 1000 + hmd.transform.position;
        Vector3 coordinateAdaptedGazeDirectionCombined = new Vector3(verboseData.combined.eye_data.gaze_direction_normalized.x * -1, verboseData.combined.eye_data.gaze_direction_normalized.y, verboseData.combined.eye_data.gaze_direction_normalized.z);

        eyeDirectionCombinedWorld = hmd.transform.rotation * coordinateAdaptedGazeDirectionCombined;
        eyeRotationCombinedWorld = hmd.transform.rotation;

        invisibleObjectReceiver.transform.position = eyePositionCombinedWorld + (eyeDirectionCombinedWorld * 5);

        // The following could be used to get continuous focus points on the boxes of the receiver
        /*
        if (Physics.Raycast(new Ray(eyePositionCombinedWorld, eyeDirectionCombinedWorld), out hitData, Mathf.Infinity, _boxLayerMask))
        {
            Vector3 hitPoint = hitData.point;

            // Create sample array for LSL (see signaler manager)
            
        }
        */
    }
        
    public void Teleport(Vector3 location, GameObject avatar)
    {
        avatar.transform.position = location;
    }
    
}
