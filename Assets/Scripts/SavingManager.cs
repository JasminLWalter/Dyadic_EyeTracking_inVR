using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;

public class SavingManager : MonoBehaviour
{
    private GameManager gameManager;
    private EyetrackingValidation eyetrackingValidation;
    private EmbodimentManager embodimentManager;
    private SignalerManager signalerManager;
    private ReceiverManager receiverManager;

    private LSLStreams lslStreams;
    public float sampleRate = 90.0f;

    private int phase;
    private Vector3 validationError;
    private int valCalCounter;
    private float embodimentTrainingStartedSignaler;
    private float embodimentTrainingEndSignaler;
    private float embodimentTrainingTimeSignaler;
    private float embodimentTrainingStartedReceiver;
    private float embodimentTrainingEndReceiver;
    private float embodimentTrainingTimeReceiver;
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
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        lslStreams = GameObject.Find("LSLStreams").GetComponent<LSLStreams>();
        eyetrackingValidation = GameObject.Find("EyetrackingValidation").GetComponent<EyetrackingValidation>();
        // embodimentManager = GameObject.Find("EmbodimentManager").GetComponent<EmbodimentManager>();
        // signalerManager = GameObject.Find("SignalerManager").GetComponent<SignalerManager>();
        // receiverManager = GameObject.Find("ReceiverManager").GetComponent<ReceiverManager>();

        // //hitData = Raycast.hitData;

        // StartCoroutine(SendData());
    }

    void Update()
    {
        phase = gameManager.phase;
        trialNumber = gameManager._currentRound;
        // trialFailedCount = gameManager.trialFailedCount;
        // validationError = eyetrackingValidation.GetValidationError();
        // valCalCounter = eyetrackingValidation.valCalCounter;
        // embodimentTrainingStarted = embodimentManager.embodimentTrainingStarted;
        // embodimentTrainingEnd = embodimentManager.embodimentTrainingEnd;
        // embodimentTrainingTime = embodimentTrainingEnd - embodimentTrainingStarted;

        float interval = 1.0f / sampleRate;
        
        lslStreams.lslOExperimentPhase.push_sample(new int [] { phase });
        lslStreams.lslOTrialNumber.push_sample(new int [] { trialNumber });
        // lslStreams.lslOFailedTrialCounter.push_sample(new int [] { trialFailedCount });

        
    }

    // private IEnumerator SendData()
    // {
    //     while (true)
    //     {
    //         phase = gameManager.phase;
    //         trialNumber = gameManager.trialNumber;
    //         // trialFailedCount = gameManager.trialFailedCount;
    //         // validationError = eyetrackingValidation.GetValidationError();
    //         // valCalCounter = eyetrackingValidation.valCalCounter;
    //         // embodimentTrainingStarted = embodimentManager.embodimentTrainingStarted;
    //         // embodimentTrainingEnd = embodimentManager.embodimentTrainingEnd;
    //         // embodimentTrainingTime = embodimentTrainingEnd - embodimentTrainingStarted;

    //         float interval = 1.0f / sampleRate;
            
    //         lslStreams.lslOExperimentPhase.push_sample(new int [] { phase });
    //         lslStreams.lslOTrialNumber.push_sample(new int [] { trialNumber });
    //         // lslStreams.lslOFailedTrialCounter.push_sample(new int [] { trialFailedCount });

    //         yield return new WaitForSeconds(1.0f / sampleRate);
    //     }
    // }
}

