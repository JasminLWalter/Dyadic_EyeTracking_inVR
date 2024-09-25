using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;
using ViveSR.anipal.Eye;


public class SignalerManager : MonoBehaviour
{
   
    private InputBindings _inputBindings;
    private Collider _lastHit;
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

    public MenuManager menuManager;
    public GameManager gameManager;

    public ReceiverManager receiverManager;

    private EmbodimentManager embodimentManager;
    private SimpleCrosshair simpleCrosshair;

    public List<TMP_Text> TextsPhase3;

    public bool signalerReady = false;

    public GameObject invisibleObject;
    public RaycastHit hitData;

    public GameObject avatar;
    private Vector3 offset = new Vector3(-57.7999992f,-0.810000002f,-0.419999987f);

    public int freezeCounter = 0;
    public  Vector3 frozenPosition;
    public LSLSignalerOutlets lSLSignalerOutlets;
    public GameObject Box1Left;
    
   
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Box1"+ Box1Left.transform.position);

        gameManager = FindObjectOfType<GameManager>();
        menuManager = FindObjectOfType<MenuManager>();
        embodimentManager = FindObjectOfType<EmbodimentManager>();
        simpleCrosshair = FindObjectOfType<SimpleCrosshair>();
        
        _inputBindings = new InputBindings();
        _inputBindings.Player.Enable();

        foreach (TMP_Text TextPhase3 in TextsPhase3)
        {
            TextPhase3.gameObject.SetActive(false);
        }

    }

    // Update is called once per frame
    void Update()
    {

       /* if (!frozen)
        {
            if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out rayOrigin, out rayDirection))
            {
                combinedEyes.Rotate(rayDirection.x, rayDirection.y, rayDirection.z, Space.Self);
                //Debug.LogError("Direction x:" + rayDirection.x + "Direction y:" + rayDirection.y + "Direction z:" + rayDirection.z);
            }

            if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out rayOrigin, out rayDirection))
            {
                leftEye.Rotate(rayDirection.x, rayDirection.y, rayDirection.z, Space.Self);
                //Debug.LogError("Direction x:" + rayDirection.x + "Direction y:" + rayDirection.y + "Direction z:" + rayDirection.z);

                //eyes.localRotation = _inputBindings.Player.EyeTracking.ReadValue<Quaternion>();
                
                if (!_inputBindings.Player.EyeGazeIsTracked.triggered) 
                {
                    var mousePosition = _inputBindings.Player.MouseGaze.ReadValue<Vector2>();
                    leftEye.localRotation = Quaternion.Euler(mousePosition.y, mousePosition.x, 0);
                }
                else
                {
                    eyes.localRotation = _inputBindings.Player.EyeTracking.ReadValue<Quaternion>();
                }
            }

            if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out rayOrigin, out rayDirection))
            {
                rightEye.Rotate(rayDirection.x, rayDirection.y, rayDirection.z, Space.Self);
                //Debug.LogError("Direction x:" + rayDirection.x + "Direction y:" + rayDirection.y + "Direction z:" + rayDirection.z);
            }

        }*/
        if (gameManager.frozen == false && freezeCounter > 2 && gameManager.countdownRunning == false)
        {
            //StartCoroutine(gameManager.CountdownTimer(gameManager.timerCountdownText));
        }

        //  gaze data of the signaler
        SRanipal_Eye_v2.GetVerboseData(out VerboseData verboseData);
        eyePositionCombinedWorld = verboseData.combined.eye_data.gaze_origin_mm / 1000 + hmd.transform.position;
        Vector3 coordinateAdaptedGazeDirectionCombined = new Vector3(verboseData.combined.eye_data.gaze_direction_normalized.x * -1, verboseData.combined.eye_data.gaze_direction_normalized.y, verboseData.combined.eye_data.gaze_direction_normalized.z);

        eyeDirectionCombinedWorld = hmd.transform.rotation * coordinateAdaptedGazeDirectionCombined;
        // eyeRotationCombinedWorld = hmd.transform.rotation;
    
        invisibleObject.transform.position = eyePositionCombinedWorld + (eyeDirectionCombinedWorld * 5);
    
        if (Physics.Raycast(new Ray(eyePositionCombinedWorld, eyeDirectionCombinedWorld), out hitData, Mathf.Infinity))
        {
            
            
            if (_lastHit == null)
            {
                _lastHit = hitData.collider;
                _lastHit.gameObject.SendMessage("StaredAt");
            }
            else if (_lastHit != null && _lastHit != hitData.collider)
            {
                // Debug.Log("Hit something new: " + hitData.collider.name);
                _lastHit.gameObject.SendMessage("NotLongerStaredAt");
                _lastHit = hitData.collider;
                _lastHit.gameObject.SendMessage("StaredAt");
                // Debug.Log("Hit data collider:" + hitData.transform.position);
            }
        /*    else if (_inputBindings.UI.Select.triggered)
            {
                Debug.Log("Selected " + _lastHit);
                _lastHit.gameObject.SendMessage("Selected");
            } */
        }

        else if (_lastHit != null)
        {
            _lastHit.gameObject.SendMessage("NotLongerStaredAt");
            _lastHit = null;
        }


        if (gameManager.role == "signaler" && _inputBindings.Player.Freeze.triggered && gameManager.GetCurrentPhase() == 3)
        {
            // frozenPosition = signalerOutletScript.invisibleObject.transform.position;
            if(freezeCounter < 1)
            {
                gameManager.PlayAudio();
            }
            freezeCounter += 1;

            Vector3 hitPoint = hitData.point;
            
            // Create sample array
            float[] sample = new float[3];
            sample[0] = float.Parse(FormatFloat(hitPoint.x, 8)); // Convert string back to float
            sample[1] = float.Parse(FormatFloat(hitPoint.y, 8)); // Convert string back to float
            sample[2] = float.Parse(FormatFloat(hitPoint.z, 8)); // Convert string back to float

            // sample[0] = hitPoint.x;
            // sample[1] = hitPoint.y;
            // sample[2] = hitPoint.z;
            Debug.Log("hitpoint" + sample[0]);
            // Push sample to LSL
            lSLSignalerOutlets.lslORaycastHitSignaler.push_sample(sample);

            
            Debug.LogError("Sent hit point to LSL: " + sample[0]+sample[1]);
            Debug.LogError("Hitpoint signaler gaze: "+hitData.point);
            
            int freezeCounterSignaler = freezeCounter;
            lSLSignalerOutlets.lslOFreezeCounterSignaler.push_sample(new int[] {freezeCounterSignaler});
            gameManager.firstFreeze = true;
            if(signalerReady == false)
            {
                StartCoroutine(menuManager.ShowTexts(TextsPhase3, () => signalerReady = false));
                signalerReady = true;
                string signalerReadyString = signalerReady.ToString();
                lSLSignalerOutlets.lslOSignalerReady.push_sample(new string[] {signalerReadyString});
            }

        }

    }

    public void Teleport(Vector3 location, GameObject avatar)
    {
        avatar.transform.position = location;// + new Vector3(-0.4f, 5f, -0.7f);
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



