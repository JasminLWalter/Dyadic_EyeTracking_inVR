using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using ViveSR.anipal.Eye;
using ViveSR.anipal.Lip;


public class ReceiverManager : MonoBehaviour
{

    [SerializeField] private Camera playerCamera;
    private InputBindings _inputBindings;
    private Ray _ray;
    public Collider _lastHit;
    private int _layerMask = 1 << 3;  // Only objects on Layer 3 should be considered
    [SerializeField] private bool inVR = true;

    public GameObject hmd;
    public Transform leftControllerTransform; // Reference to the VR controller's transform
    public Transform rightControllerTransform;
    private int raycastDistance = 100;
    Transform preferredHandTransform;

    // Start is called before the first frame update
    void Start()
    {
        preferredHandTransform = rightControllerTransform;
        _inputBindings = new InputBindings();
        _inputBindings.Player.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray(preferredHandTransform.position, preferredHandTransform.forward);
        RaycastHit hit;

        // Check if the ray hits any UI buttons
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, _layerMask))
        {
            if (_lastHit == null)
            {
                _lastHit = hit.collider;
                _lastHit.gameObject.SendMessage("StaredAt");
            }
            else if (_lastHit != null && _lastHit != hit.collider)
            {
                Debug.Log("Hit something new: " + hit.collider.name);
                _lastHit.gameObject.SendMessage("NotLongerStaredAt");
                _lastHit = hit.collider;
                _lastHit.gameObject.SendMessage("StaredAt");
            }
            else if (_inputBindings.Player.SelectBox.triggered)
            {
                Debug.LogError("Selected");
               _lastHit.gameObject.SendMessage("Selected");
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
}
