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

    public bool frozen = false;

    private InputBindings _inputBindings;

    // Start is called before the first frame update
    void Start()
    {
        playerCamera = GetComponent<Camera>();
        _inputBindings = new InputBindings();
        _inputBindings.Player.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Make the eye gameobject rotate according to the eye movements of the participant
        
        


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
