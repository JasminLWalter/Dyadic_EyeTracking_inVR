using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Tooltip("Either signaller or receiver.")]
    public string role = null;


    [SerializeField] private GameObject eyes = null;

    private bool frozen = false;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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

    // Co-Routine!!
    // Make the eye gameobjects follow the participants' eye movements.
    private void MoveEyes()
    {
        if (frozen == false)
        {
            // 
        }
    }

}
