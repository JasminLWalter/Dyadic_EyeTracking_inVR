using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;


public class ReceiverManager : MonoBehaviour
{

    private InputBindings _inputBindings;
    private Collider _lastHit;
    private int _layerMask = 1 << 3;  // Only objects on Layer 3 should be considered

    public GameObject hmd;
    public Transform OriginTransform;

    //  for displaying the eye movement of the signaler
    [SerializeField] private Transform combinedEyes;
    [SerializeField] private Transform leftEye;
    [SerializeField] private Transform rightEye;
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
    public bool phase3SecondPartCoroutineRunningReceiver = false;
    public bool didRunSecondPartReceiver = false;
    public List<TMP_Text> TextsPhase3Receiver;

    public GameObject avatar;

    public GameObject invisibleObjectReceiver;

    private Vector3 offset = new Vector3(-57.7999992f,-0.810000002f,-0.419999987f);
    public int selectCounter = 0;
    private bool countdownRunning = false;

    // Start is called before the first frame update
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
    }

    // Update is called once per frame
    void Update()
    {
        {
            StartCoroutine(gameManager.Countdown());
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
                _lastHit.gameObject.SendMessage("StaredAt");
            }
            else if (_lastHit != null && _lastHit != hitData.collider)
            {
                Debug.Log("Hit something new: " + hitData.collider.name);
                _lastHit.gameObject.SendMessage("NotLongerStaredAt");
                _lastHit = hitData.collider;
                _lastHit.gameObject.SendMessage("StaredAt");
            }
        }

        else
        {
            if (_lastHit != null)
            {
                _lastHit.gameObject.SendMessage("NotLongerStaredAt");
                _lastHit = null;
            }
        }


        Ray ray = new Ray(preferredHandTransform.position, preferredHandTransform.forward);
        RaycastHit hit;

        // Check if the ray hits any UI buttons
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, _layerMask))
        {
            if (_lastHit == null)
            {
                _lastHit = hit.collider;
                _lastHit.gameObject.SendMessage("StaredAtReceiver");
            }
            else if (_lastHit != null && _lastHit != hit.collider)
            {
                Debug.Log("Hit something new: " + hit.collider.name);
                _lastHit.gameObject.SendMessage("NotLongerStaredAt");
                _lastHit = hit.collider;
                _lastHit.gameObject.SendMessage("StaredAt");
            }
            else if (_inputBindings.Player.SelectBox.triggered && gameManager.GetCurrentPhase() == 3)
            {
               _lastHit.gameObject.SendMessage("Selected");
               boxSelected = true;
               selectCounter++;
               
                if(menuManager.didRunReceiver && !phase3SecondPartCoroutineRunningReceiver)
                {
                    StartCoroutine(menuManager.ShowTexts(TextsPhase3Receiver, () => phase3SecondPartCoroutineRunningReceiver = false));
                    phase3SecondPartCoroutineRunningReceiver = true;
                    
                    foreach (TMP_Text TextPhase3 in signalerManager.TextsPhase3)
                    {
                        TextPhase3.gameObject.SetActive(false);
                    }
                }/*
                if(gameManager.firstFreezeReceiver)
                {
                    Debug.LogError("Receiver freezes"); 
                    StartCoroutine(gameManager.CountdownTimer(gameManager.timerCountdownText));
                }*/
               
            }
        }
        else
        {
            if (_lastHit != null)
            {
                Debug.Log("Not longer stared at.");
                _lastHit.gameObject.SendMessage("NotLongerStaredAt");
                _lastHit = null;
            }
        }
    }
    public void Teleport(Vector3 location)
    {

        OriginTransform.transform.position = location;
    }

}
