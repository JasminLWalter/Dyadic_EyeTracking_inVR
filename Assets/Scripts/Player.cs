using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Tooltip("Either signaller or receiver.")]
    public string role = None;


    [SerializeField] private GameObject eyes = None

    private bool frozen = False



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
        frozen = True
    }

    // Make the eye gameobjects follow the participants' eye movements again.
    public void Unfreeze()
    {
        frozen = False
    }

    // Co-Routine!!
    // Make the eye gameobjects follow the participants' eye movements.
    private void MoveEyes()
    {
        if (frozen == False)
        {
            // 
        }
    }

}
