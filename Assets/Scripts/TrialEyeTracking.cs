using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialEyeTracking : MonoBehaviour
{
    private InputBindings _inputBindings;
    private Ray _ray;
    // Start is called before the first frame update
    void Start()
    {
        _inputBindings = new InputBindings();
        _inputBindings.Player.Enable();
        
    }

    private void Update()
    {
        /**
        var rayOrigin = gameObject.transform.position;   //_inputBindings.Player.CenterEye.ReadValue<Vector3>();
        Quaternion eyeRotation = _inputBindings.Player.EyeTracking.ReadValue<Quaternion>();
        Vector3 rayDirection = eyeRotation * rayOrigin;

        Debug.Log("rayOrigin: " + rayOrigin);
        Debug.Log("eyeRotation: " + eyeRotation);
        Debug.Log("rayDirection: " + rayDirection);

        Debug.DrawRay(rayOrigin, -rayDirection, Color.green);
        **/
    }

    IEnumerator StoreValues()
    {
        float countDown = 60f;
        for (int i = 0; i < 60000; i++)
        {
            while (countDown >= 0)
            {
                Debug.Log(i++);
                countDown -= Time.smoothDeltaTime;
                yield return null;
            }
        }
    }

    /**
    private IEnumerator StartCounter()
    {
        countDown = 10f;
        for (int i = 0; i < 60000; i++)
        {
            while (countDown >= 0)
            {
                Debug.Log(i++);
                countDown -= Time.smoothDeltaTime;
                yield return null;
            }
        }
    }
    **/
}
