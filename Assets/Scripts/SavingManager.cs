using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavingManager : MonoBehaviour
{
    private GameManager gameManager;
    private EyetrackingValidation eyetrackingValidation;
    private EmbodimentManager embodimentManager;
   // private EyeRaycast eyeRaycast;


    private int phase;
    private Vector3 validationError;
    private int valCalCounter;
    private float embodimentTrainingStarted;
    private float embodimentTrainingEnd;
    private float embodimentTrainingTime;
    private int trialNumber;
    private int trialFailedCount;
    private RaycastHit hitData;

    public double GetCurrentUnixTimeStamp()
    {
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        return (System.DateTime.UtcNow - epochStart).TotalSeconds;
    }
   /* private double GetCurrentTimestampInSeconds()
    {
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        return (System.DateTime.UtcNow - epochStart).TotalSeconds;
    } */


    // Start is called before the first frame update
    void Start()
    {
        phase = gameManager.phase;
        validationError = eyetrackingValidation.GetValidationError();
        valCalCounter = eyetrackingValidation.valCalCounter;
        embodimentTrainingStarted = embodimentManager.embodimentTrainingStarted;
        embodimentTrainingEnd = embodimentManager.embodimentTrainingEnd;
        embodimentTrainingTime = embodimentTrainingEnd - embodimentTrainingStarted;
        trialNumber = gameManager.trialNumber;
        //hitData = Raycast.hitData;
        trialFailedCount = gameManager.trialFailedCount;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
