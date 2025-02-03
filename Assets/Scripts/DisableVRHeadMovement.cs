using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class DisableVRHeadMovement : MonoBehaviour
{
    // This class is used for making the Eyetracking Validation two-dimensional by temporarily disabling head movement 

    private bool disableHeadMovement = false;

    // Update is called once per frame
    void Update()
    {
        if (disableHeadMovement)
        {
            Debug.Log("Disabling head movement");
            // Negate the head tracking by setting the camera's position and rotation
            transform.localPosition = -InputTracking.GetLocalPosition(XRNode.CenterEye);
            transform.localRotation = Quaternion.Inverse(InputTracking.GetLocalRotation(XRNode.CenterEye));
        }
    }

    public void DisableHeadMovement()
    {
        disableHeadMovement = true;
    }

    public void EnableHeadMovement()
    {
        disableHeadMovement = false;
        // Reset the position and rotation to default
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}
