using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    [Tooltip("Either signaller or receiver.")]
    public string role = null;

    [SerializeField] private Camera playerCamera;


    [SerializeField] private Transform eyes = null;

    private bool frozen = false;

    private Vector3 teleportPosition;

    // Start is called before the first frame update
    void Start()
    {
        playerCamera = GetComponent<Camera>();
        teleportPosition = new Vector3(10f, 10f, 10f);
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Make the eye gameobject rotate according to the eye movements of the participant
        Vector3 direction = Input.mousePosition - playerCamera.WorldToScreenPoint(transform.position);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Check if the right arrow key is pressed and initiate movement
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            transform.position = teleportPosition; // Set player position to the specified position
            Debug.Log("Right arrow key pressed.");
        }
    }

    public bool IsAtSpecificPosition()
    {
        return transform.position == new Vector3(10f, 10f, 10f);
    }

    // Teleports the player to another space
     public void Teleport(Vector3 location)
    {
        Transform currentLocation = GetComponent<Transform>();
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
