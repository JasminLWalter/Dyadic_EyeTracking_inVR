using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.InputSystem;
using Wave.Essence;

public class StopButton : MonoBehaviour
{
    private InputBindings _inputBindings;
    public EmbodimentManager embodimentManager;
    public Button StopRecording;

    void Awake()
    {

        _inputBindings = new InputBindings();
        _inputBindings.UI.Enable();
        StopRecording = GetComponent<Button>();
        embodimentManager = FindObjectOfType<EmbodimentManager>();
    }

    void Update()
    {
        if (_inputBindings != null && _inputBindings.UI.Pressed.triggered)
        {
            

            // Check if the StopRecording button is clicked
            if (StopRecording != null && StopRecording.onClick != null)
            {
                Debug.LogError("Stop Pressed");
                embodimentManager.OnStopRecordingButtonClick();
            }
        }


    }
}
