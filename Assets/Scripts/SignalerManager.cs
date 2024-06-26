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
    [SerializeField] private Transform combinedEyes;
    [SerializeField] private Transform leftEye;
    [SerializeField] private Transform rightEye;
    private Vector3 rayOrigin;
    private Vector3 rayDirection;

    public MenuManager menuManager;
    public GameManager gameManager;

    public List<TMP_Text> TextsPhase3;

    public bool frozen = false;
    public bool phase3SecondPartCoroutineRunning = false;

    public GameObject invisibleObject;

    public GameObject avatar;
    private Vector3 offset = new Vector3(-57.7999992f,-0.810000002f,-0.419999987f);
   
    // Start is called before the first frame update
    void Start()
    {
         gameManager = FindObjectOfType<GameManager>();
         menuManager = FindObjectOfType<MenuManager>();

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

        //  gaze data of the signaler
        SRanipal_Eye_v2.GetVerboseData(out VerboseData verboseData);
        var eyePositionCombinedWorld = verboseData.combined.eye_data.gaze_origin_mm / 1000 + hmd.transform.position;
        Vector3 coordinateAdaptedGazeDirectionCombined = new Vector3(verboseData.combined.eye_data.gaze_direction_normalized.x * -1, verboseData.combined.eye_data.gaze_direction_normalized.y, verboseData.combined.eye_data.gaze_direction_normalized.z);

        var eyeDirectionCombinedWorld = hmd.transform.rotation * coordinateAdaptedGazeDirectionCombined;

        invisibleObject.transform.position = eyePositionCombinedWorld + (eyeDirectionCombinedWorld * 500);


        RaycastHit hitData;
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
        /*    else if (_inputBindings.UI.Select.triggered)
            {
                Debug.Log("Selected " + _lastHit);
                _lastHit.gameObject.SendMessage("Selected");
            } */
        }

        else
        {
            if (_lastHit != null)
            {
                Debug.Log("Not longer starred at.");
                _lastHit.gameObject.SendMessage("NotLongerStaredAt");
                _lastHit = null;
            }
        }

        if (_inputBindings.Player.Freeze.triggered && gameManager.GetCurrentPhase() == 3)
        {
            Freeze();
            Debug.LogError("Freeze Signaler Manager");
            if(phase3SecondPartCoroutineRunning == false)
            {
                StartCoroutine(menuManager.ShowTexts(TextsPhase3, () => phase3SecondPartCoroutineRunning = false));
                phase3SecondPartCoroutineRunning = true;
            }
        }

    }

    public void Teleport(Vector3 location)
    {
        avatar.transform.position = location;
        OriginTransform.transform.position = location; // + new Vector3(-0.4f, 5f, -0.7f);
        //currentLocation.position = location;


        Debug.LogError("teleport signaler");
        //Debug.LogError("avatar position before" + avatar.transform.position);
        //avatar.transform.position = location + offset;
        //Debug.LogError("avatar position after" + avatar.transform.position);


    }

    // Prevent the eye gameobjects from moving according to the EyeTracking data.
    public void Freeze()
    {
        frozen = true;

    }

    // Make the eye gameobjects follow the participants' eye movements again.
    public void Unfreeze()
    {
        frozen = false;
    }


}
