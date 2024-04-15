using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ViveSR.anipal.Eye;

public class Raycast : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    private InputBindings _inputBindings;
    private Ray _ray;
    private Collider _lastHit;
    private int _layerMask = 1<<3;  // Only objects on Layer 3 should be considered
    [SerializeField] private bool inVR = true;


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


        if (inVR)
        {
            Transform hmdTransform = playerCamera.transform;//Player.instance.hmdTransform;
            Debug.LogError("hmdTransform " + hmdTransform.position);
            SRanipal_Eye_v2.GetVerboseData(out VerboseData verboseData);
            var eyePositionCombinedWorld = verboseData.combined.eye_data.gaze_origin_mm / 1000 + hmdTransform.position;
            Vector3 coordinateAdaptedGazeDirectionCombined = new Vector3(verboseData.combined.eye_data.gaze_direction_normalized.x * -1, verboseData.combined.eye_data.gaze_direction_normalized.y, verboseData.combined.eye_data.gaze_direction_normalized.z);
            var eyeDirectionCombinedWorld = hmdTransform.rotation * coordinateAdaptedGazeDirectionCombined;

            RaycastHit firstHit;
            if (Physics.Raycast(eyePositionCombinedWorld, eyeDirectionCombinedWorld, out firstHit))
            {
                transform.position = firstHit.point;

            }

            Debug.DrawRay(eyePositionCombinedWorld, eyeDirectionCombinedWorld * 100, Color.magenta);
            /*

            rayOrigin = playerCamera.transform.position;
            Quaternion eyeRotation = _inputBindings.Player.EyeTracking.ReadValue<Quaternion>();
            Debug.LogError("rayOrigin" + rayOrigin);
            // rayDirection = eyeRotation * rayOrigin * (-1);
            rayDirection = Vector3.Scale(eyeRotation.eulerAngles / 180, playerCamera.transform.forward);
              */
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
        if (Physics.Raycast(new Ray(rayOrigin, rayDirection), out hitData, Mathf.Infinity, _layerMask))
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
