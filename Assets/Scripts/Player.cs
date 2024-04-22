using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;
using ViveSR.anipal.Eye;
using static UnityEngine.GraphicsBuffer;

// For the avatar: https://developer.vive.com/resources/openxr/openxr-pcvr/tutorials/unity/integrate-facial-tracking-your-avatar/

public class Player : MonoBehaviour
{
    [Tooltip("Either signaller or receiver.")]
    public string role = null;

    [SerializeField] private Camera playerCamera;



    [SerializeField] private Transform eyes;

    public bool frozen = false;

    private InputBindings _inputBindings;

    private Vector3 rayOrigin;

    private Vector3 rayDirection;


    public Transform OriginTransform;

    private void Start()
    {
        //hmdTransform = GameObject.Find("NameOfYourHMDObject").transform;
        //hmdTransform = GetComponentInChildren<YourHMDComponent>().transform;

        playerCamera = GetComponent<Camera>();
        _inputBindings = new InputBindings();
        _inputBindings.Player.Enable();

        

    }

    // Update is called once per frame
    void Update()
    {
        if (!frozen)
        {
            if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out rayOrigin, out rayDirection))
            {
                eyes.Rotate(rayDirection.x, rayDirection.y, rayDirection.z, Space.Self);
                //Debug.LogError("Direction x:" + rayDirection.x + "Direction y:" + rayDirection.y + "Direction z:" + rayDirection.z);
            }
            //eyes.localRotation = _inputBindings.Player.EyeTracking.ReadValue<Quaternion>();
            /*
            if (!_inputBindings.Player.EyeGazeIsTracked.triggered) 
            {
                var mousePosition = _inputBindings.Player.MouseGaze.ReadValue<Vector2>();
                eyes.localRotation = Quaternion.Euler(mousePosition.y, mousePosition.x, 0);
            }
            else
            {
                eyes.localRotation = _inputBindings.Player.EyeTracking.ReadValue<Quaternion>();
            }*/
        }
        
    }
    
    // Teleports the player to another space
    public void Teleport(Vector3 location)
    {
        Transform currentLocation = OriginTransform;
        currentLocation.position = location;
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
