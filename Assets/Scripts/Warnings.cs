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
        
        if (Math.Abs(currentHeadPosition.x - lastHeadPosition.x) > 0.05 
            || Math.Abs(currentHeadPosition.y - lastHeadPosition.y) > 0.05 
            || Math.Abs(currentHeadPosition.z - lastHeadPosition.z) > 0.05 )
        {
            _leftController.SendHapticImpulse(0.5f, 0.2f);
            _rightController.SendHapticImpulse(0.5f, 0.2f);
        }

        float value1 = Math.Abs(currentHeadRotation.x - lastHeadRotation.x);
        float value2 = 360 - value1;
        float angleX = Math.Min(value1, value2);

        value1 = Math.Abs(currentHeadRotation.y - lastHeadRotation.y);  
        value2 = 360 - value1;
        float angleY = Math.Min(value1, value2);

        value1 = Math.Abs(currentHeadRotation.z - lastHeadRotation.z);
        value2 = 360 - value1;
        float angleZ = Math.Min(value1, value2);

        if (angleX > 10
            || angleY > 10
            || angleZ > 10)
        {
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
        }
    }
}
