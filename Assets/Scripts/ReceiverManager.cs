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

    public Transform leftControllerTransform; // Reference to the VR controller's transform
    public Transform rightControllerTransform;
    public Transform preferredHandTransform;

    public Transform OriginTransform;

    //  displaying the eye movement of the signaler
    [SerializeField] private Transform combinedEyes;
    [SerializeField] private Transform leftEye;
    [SerializeField] private Transform rightEye;
    private Vector3 rayOrigin;
    private Vector3 rayDirection;

    public GameManager gameManager;
    public MenuManager menuManager;
    public bool boxSelected = false;
    public bool phase3SecondPartCoroutineRunningReceiver = false;

    public List<TMP_Text> TextsPhase3Receiver;

    public GameObject avatar;

    private Vector3 offset = new Vector3(-57.7999992f,-0.810000002f,-0.419999987f);


    // Start is called before the first frame update
    void Start()
    {
        preferredHandTransform = rightControllerTransform;
        gameManager = FindObjectOfType<GameManager>();
        menuManager = FindObjectOfType<MenuManager>();
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
        
       
       /* // continuous eye movement for the receiver
        if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out rayOrigin, out rayDirection))
        {
            combinedEyes.Rotate(rayDirection.x, rayDirection.y, rayDirection.z, Space.Self);
            //Debug.LogError("Direction x:" + rayDirection.x + "Direction y:" + rayDirection.y + "Direction z:" + rayDirection.z);
        }

        if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out rayOrigin, out rayDirection))
        {
            leftEye.Rotate(rayDirection.x, rayDirection.y, rayDirection.z, Space.Self);
            //Debug.LogError("Direction x:" + rayDirection.x + "Direction y:" + rayDirection.y + "Direction z:" + rayDirection.z);
        }

        if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out rayOrigin, out rayDirection))
        {
            rightEye.Rotate(rayDirection.x, rayDirection.y, rayDirection.z, Space.Self);
            //Debug.LogError("Direction x:" + rayDirection.x + "Direction y:" + rayDirection.y + "Direction z:" + rayDirection.z);
        } */



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
            else if (_inputBindings.Player.SelectBox.triggered && gameManager.GetCurrentPhase() == 3)
            {
               _lastHit.gameObject.SendMessage("Selected");
               boxSelected = true;
               Debug.LogError("Receiver freezes"); 
                if(phase3SecondPartCoroutineRunningReceiver == false)
                {
                    StartCoroutine(menuManager.ShowTexts(TextsPhase3Receiver, () => phase3SecondPartCoroutineRunningReceiver = false));
                    phase3SecondPartCoroutineRunningReceiver = true;
                }
               
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

        avatar.transform.position = location;
        OriginTransform.transform.position = location + new Vector3(-0.4f, 6.8f, -0.7f);
        Debug.LogError("teleport receiver");
    }

}
