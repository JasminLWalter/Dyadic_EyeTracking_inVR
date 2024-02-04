using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;


public class Warnings : MonoBehaviour
{

    private InputBindings _inputBindings;
    private Vector3 lastHeadPosition;
    private Vector3 lastHeadRotation;
    private Vector3 currentHeadPosition;
    private Vector3 currentHeadRotation;
    [SerializeField] private XRBaseController _leftController;
    [SerializeField] private XRBaseController _rightController;
    

    // Start is called before the first frame update
    void Start()
    {
        _inputBindings = new InputBindings();
        _inputBindings.Player.Enable();

        lastHeadPosition = _inputBindings.Player.HeadPosition.ReadValue<Vector3>();
        lastHeadRotation = Camera.main.transform.rotation.eulerAngles;
        currentHeadPosition = lastHeadPosition;
        currentHeadRotation = lastHeadRotation;

        

        StartCoroutine(UpdateMovement());
    }

    // Update is called once per frame
    void Update()
    {
        currentHeadPosition = _inputBindings.Player.HeadPosition.ReadValue<Vector3>();
        currentHeadRotation = Camera.main.transform.rotation.eulerAngles;
        /**
        if (Math.Abs(currentHeadPosition.x - lastHeadPosition.x) > 0.05 
            || Math.Abs(currentHeadPosition.y - lastHeadPosition.y) > 0.05 
            || Math.Abs(currentHeadPosition.z - lastHeadPosition.z) > 0.05 )
        {
            Debug.LogWarning("Head position changed!");
            Debug.LogWarning("Position change of " + (currentHeadPosition - lastHeadPosition));
            _leftController.SendHapticImpulse(0.5f, 0.2f);
            _rightController.SendHapticImpulse(0.5f, 0.2f);
        }**/
        
        if (Math.Abs(currentHeadRotation.x - lastHeadRotation.x) > 30
            || Math.Abs(currentHeadRotation.y - lastHeadRotation.y) > 30
            || Math.Abs(currentHeadRotation.z - lastHeadRotation.z) > 30)
        {
            //Debug.LogWarning("Head rotation changed!");
            //Debug.LogWarning("Rotation change of " + (currentHeadRotation.eulerAngles - lastHeadRotation.eulerAngles));
            _leftController.SendHapticImpulse(0.5f, 0.2f);
            _rightController.SendHapticImpulse(0.5f, 0.2f);
        }
    }

    IEnumerator UpdateMovement()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(2);
            lastHeadPosition = currentHeadPosition;
            lastHeadRotation = currentHeadRotation;
            Debug.LogWarning("Updated movement");
            Debug.LogWarning("new last rotation: " + (lastHeadRotation.y));
        }
    }
}
