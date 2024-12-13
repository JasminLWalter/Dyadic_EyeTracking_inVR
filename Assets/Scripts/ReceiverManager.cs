using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;
using System;


public class ReceiverManager : MonoBehaviour
{

    private InputBindings _inputBindings;
    private Collider _lastHit;
    public Collider _lastHitController;
    private int _layerMask = 1 << 3;  // Only objects on Layer 3 should be considered

    public GameObject hmd;
    public Transform OriginTransform;

    //  for displaying the eye movement of the signaler
    // [SerializeField] private Transform combinedEyes;
    // [SerializeField] private Transform leftEye;
    // [SerializeField] private Transform rightEye;
    private Vector3 rayOrigin;
    private Vector3 rayDirection;

    public Vector3 eyePositionCombinedWorld;
    public Vector3 eyeDirectionCombinedWorld;
    public Quaternion eyeRotationCombinedWorld;
    public RaycastHit hitData;

    public Transform leftControllerTransform; // Reference to the VR controller's transform
    public Transform rightControllerTransform;
    public Transform preferredHandTransform;

    public GameManager gameManager;
    public MenuManager menuManager;
    public SignalerManager signalerManager;
    public ReceiverManager receiverManager;
    public bool boxSelected = false;
    public bool receiverReady = false;
    public bool didRunSecondPartReceiver = false;
    public List<TMP_Text> TextsPhase3Receiver;

    public GameObject avatar;
    public bool CountdownStarted = false;
    public GameObject invisibleObjectReceiver;

    private Vector3 offset = new Vector3(-57.7999992f,-0.810000002f,-0.419999987f);
    public int selectCounter = 0;
    private bool countdownRunning = false;
    public bool secondCheck = false;

    public LSLReceiverOutlets lSLReceiverOutlets;

    private Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = FindObjectOfType<Camera>();
        preferredHandTransform = rightControllerTransform;
        gameManager = FindObjectOfType<GameManager>();
        menuManager = FindObjectOfType<MenuManager>();
        signalerManager = FindObjectOfType<SignalerManager>();
        // lSLReceiverOutlets = FindObjectOfType<LSLReceiverOutlets>();

        _inputBindings = new InputBindings();
        _inputBindings.Player.Enable();

        foreach (TMP_Text TextPhase3Receiver in TextsPhase3Receiver)
        {
            TextPhase3Receiver.gameObject.SetActive(false);
        }

    }

    // Update is called once per frame
    void Update()
    {
        Ray ray;
        if (gameManager.playedInVR)
        {
            ray = new Ray(preferredHandTransform.position, preferredHandTransform.forward);
        }
        else
        {
            Vector2 mouseScreenPosition = _inputBindings.Player.MousePosition.ReadValue<Vector2>();
            ray = Camera.main.ScreenPointToRay(mouseScreenPosition);
        }
        RaycastHit hit;

        // Check if the ray hits any UI buttons
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, _layerMask))
        {
            if (_lastHitController == null)
            {
                _lastHitController = hit.collider;
                _lastHitController.gameObject.SendMessage("StaredAtReceiver");
            }
            else if (_lastHitController != null && _lastHitController != hit.collider)
            {
                // Debug.Log("Hit something new: " + hit.collider.name);
                _lastHitController.gameObject.SendMessage("NotLongerStaredAt");
                _lastHitController = hit.collider;
                _lastHitController.gameObject.SendMessage("StaredAtReceiver");
            }
            else if (_inputBindings.Player.SelectBox.triggered && gameManager.GetCurrentPhase() == 3 && _lastHitController.gameObject.layer == LayerMask.NameToLayer("Box"))
            {
                if ( selectCounter < 1)
                {
                    if(menuManager.didRunReceiver && !receiverReady)
                    {
                        gameManager.PlayAudio();
                        // Debug.LogError("Ready Receiver");
                        selectCounter++;
                        secondCheck = true;
                        StartCoroutine(menuManager.ShowTexts(TextsPhase3Receiver, () => receiverReady = false));
                        receiverReady = true;
                        string receiverReadyString = receiverReady.ToString();
                        lSLReceiverOutlets.lslOReceiverReady.push_sample(new string[] {receiverReadyString} );
                        
                        
                        foreach (TMP_Text TextPhase3 in signalerManager.TextsPhase3)
                        {
                            TextPhase3.gameObject.SetActive(false);
                        }
                    }
                }
                // Debug.Log("selectCounter"+selectCounter);
                if (selectCounter >= 1 && gameManager.frozen) 
                {
                    _lastHitController.gameObject.SendMessage("Selected");
                    selectCounter++;
                    receiverManager.boxSelected = true;
                     // Access hit point
                    Vector3 hitPoint = hit.point;

                    // Create sample array
                    float[] sample = new float[3];
                    sample[0] = hitPoint.x;
                    sample[1] = hitPoint.y;
                    sample[2] = hitPoint.z;

                    // Push sample to LSL
                    lSLReceiverOutlets.lslOBoxSelectedByReceiver.push_sample(sample);

                    StartCoroutine(gameManager.Condition1());
                    //StartCoroutine(CountdownTimer(timerCountdownText));
                    gameManager.trialNumber++;
                    lSLReceiverOutlets.lslOSelectCounter.push_sample(new int[] {selectCounter} );
                    // Debug.Log("selectCounter" + selectCounter);
                    if (gameManager.role == "receiver")
                    {
                            gameManager._startedRound = false;
                    }
                }
            }

        }
        else if (_lastHitController != null)
        {
            // Debug.Log("Not longer stared at.");
            _lastHitController.gameObject.SendMessage("NotLongerStaredAt");
            _lastHitController = null;
        }

        else if (gameManager.frozen && gameManager.countdownRunning == false)
        {
            gameManager.StartCoroutine(gameManager.CountdownTimer(gameManager.timerCountdownTextReceiver));
        }

        
        SRanipal_Eye_v2.GetVerboseData(out VerboseData verboseData);
        eyePositionCombinedWorld = verboseData.combined.eye_data.gaze_origin_mm / 1000 + hmd.transform.position;
        Vector3 coordinateAdaptedGazeDirectionCombined = new Vector3(verboseData.combined.eye_data.gaze_direction_normalized.x * -1, verboseData.combined.eye_data.gaze_direction_normalized.y, verboseData.combined.eye_data.gaze_direction_normalized.z);

        eyeDirectionCombinedWorld = hmd.transform.rotation * coordinateAdaptedGazeDirectionCombined;
        eyeRotationCombinedWorld = hmd.transform.rotation;

        invisibleObjectReceiver.transform.position = eyePositionCombinedWorld + (eyeDirectionCombinedWorld * 5);


        if (Physics.Raycast(new Ray(eyePositionCombinedWorld, eyeDirectionCombinedWorld), out hitData, Mathf.Infinity, _layerMask))
        {
            
            
            if (_lastHit == null)
            {
                _lastHit = hitData.collider;
              //  _lastHit.gameObject.SendMessage("StaredAt");
            }
            else if (_lastHit != null && _lastHit != hitData.collider)
            {
                // Debug.Log("Hit something new: " + hitData.collider.name);
              //  _lastHit.gameObject.SendMessage("NotLongerStaredAt");
                _lastHit = hitData.collider;
                //_lastHit.gameObject.SendMessage("StaredAt");
            }
        }

        else
        {
            if (_lastHit != null)
            {
              //  _lastHit.gameObject.SendMessage("NotLongerStaredAt");
                _lastHit = null;
            }
        }


    }
        
    public void Teleport(Vector3 location, GameObject avatar)
    {
        avatar.transform.position = location;// + new Vector3(-0.4f, 5f, -0.7f);
    }
    
}
