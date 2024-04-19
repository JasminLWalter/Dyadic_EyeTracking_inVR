using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using ViveSR.anipal.Eye;

public class Raycast : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    private InputBindings _inputBindings;
    private Ray _ray;
    private Collider _lastHit;
    private int _layerMask = 1<<3;  // Only objects on Layer 3 should be considered
    [SerializeField] private bool inVR = true;
    public GameObject hmd;


    // Start is called before the first frame update
    void Start()
    {
        _inputBindings = new InputBindings();
        _inputBindings.Player.Enable();


    }

    // Update is called once per frame
    void Update()
    {
        Vector3 rayOrigin = new Vector3();
        Vector3 rayDirection = new Vector3();

        SRanipal_Eye_v2.GetVerboseData(out VerboseData verboseData);
        var eyePositionCombinedWorld = verboseData.combined.eye_data.gaze_origin_mm / 1000 + hmd.transform.position;
        Vector3 coordinateAdaptedGazeDirectionCombined = new Vector3(verboseData.combined.eye_data.gaze_direction_normalized.x * -1, verboseData.combined.eye_data.gaze_direction_normalized.y, verboseData.combined.eye_data.gaze_direction_normalized.z);

        var eyeDirectionCombinedWorld = hmd.transform.rotation * coordinateAdaptedGazeDirectionCombined;



        if (inVR)
        {

            
            /*
            RaycastHit firstHit;
            if (Physics.Raycast(eyePositionCombinedWorld, eyeDirectionCombinedWorld, out firstHit, Mathf.Infinity))
            {
                transform.position = firstHit.point;

            } */
            


            Debug.DrawRay(eyePositionCombinedWorld, eyeDirectionCombinedWorld * 100, Color.magenta);

            
        }
    else
        {
            var mousePosition = _inputBindings.Player.MouseGaze.ReadValue<Vector2>();
            // Creates a Ray from the mouse position
            _ray = playerCamera.ScreenPointToRay(mousePosition);
            rayOrigin = _ray.origin;
            rayDirection = _ray.direction;
        }
        

        RaycastHit hitData;
        if (Physics.Raycast(new Ray(eyePositionCombinedWorld, eyeDirectionCombinedWorld), out hitData, Mathf.Infinity, _layerMask))
        {   
            if (_lastHit == null)
            {
                _lastHit = hitData.collider;
                _lastHit.gameObject.SendMessage("StarredAt");
            }
            else if (_lastHit != null && _lastHit != hitData.collider)
            {
                Debug.Log("Hit something new: " + hitData.collider.name);
                _lastHit.gameObject.SendMessage("NotLongerStarredAt");
                _lastHit = hitData.collider;
                _lastHit.gameObject.SendMessage("StarredAt");
            }
            else if (_inputBindings.UI.Select.triggered)
            {
                Debug.Log("Selected " + _lastHit);
                _lastHit.gameObject.SendMessage("Selected");
            }
        }
        else 
        {
            if (_lastHit != null)
            {
                Debug.Log("Not longer starred at.");
                _lastHit.gameObject.SendMessage("NotLongerStarredAt");
                _lastHit = null;
            }
        }

    }  
}
