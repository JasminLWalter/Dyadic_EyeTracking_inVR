using System.Collections;
using System.Collections.Generic;
using ViveSR.anipal.Eye;
using UnityEngine;
using LSL;

public class LSLReceiverOutlets : MonoBehaviour
{
    private string participantUID; 
    private const double NominalRate = LSL.LSL.IRREGULAR_RATE; // irregular sampling rate
    private ReceiverManager receiverManager;
    private GameManager gameManager;
    
    // public StreamInfo lslIMetadata;
    // public StreamOutlet lslOMetadata; 
    public StreamInfo lslITimestamps;
    public StreamOutlet lslOTimestamps;
    // public StreamInfo lslIExperimentPhase;
    // public StreamOutlet lslOExperimentPhase;
    // public StreamInfo lslIValidationError; 
    // public StreamOutlet lslOValidationError; 
    // public StreamInfo lslIValidationErrorCounter;
    // public StreamOutlet lslOValidationErrorCounter; 
    // public StreamInfo lslICalibrationCounter;
    // public StreamOutlet lslOCalibrationCounter;
    // public StreamInfo lslIEmbodimentTrainingTime;
    // public StreamOutlet lslOEmbodimentTrainingTime;
    public StreamInfo lslIReceiverReady;
    public StreamOutlet lslOReceiverReady;
    // public StreamInfo lslIBoxSelectedBySignaler;
    // public StreamOutlet lslOBoxSelectedBySignaler;
    public StreamInfo lslIBoxSelectedByReceiver;
    public StreamOutlet lslOBoxSelectedByReceiver;
    public StreamInfo lslIRaycastHit; 
    public StreamOutlet lslORaycastHit;
    public StreamInfo lslIEyePosDirRot; 
    public StreamOutlet lslOEyePosDirRot;
    // public StreamInfo lslIRaycastHit; 
    // public StreamOutlet lslORaycastHit;
    public StreamInfo lslIEyeOpennessLR; 
    public StreamOutlet lslOEyeOpennessLR;
    public StreamInfo lslIPupilDiameterLR; 
    public StreamOutlet lslOPupilDiameterLR;
    public StreamInfo lslIhmdPosDirRot;
    public StreamOutlet lslOhmdPosDirRot;
    public StreamInfo lslIHandPosDirRot;
    public StreamOutlet lslOHandPosDirRot;
    public StreamInfo lslIPreferredHand;
    public StreamOutlet lslOPreferredHand;
    public StreamInfo lslIFrozenGaze;
    public StreamOutlet lslOFrozenGaze;
    // public StreamInfo lslITrialNumber;
    // public StreamOutlet lslOTrialNumber;
    // public StreamInfo lslIFailTrial;
    // public StreamOutlet lslOFailTrial;
    public StreamInfo lslIFailedTrialCounter;
    public StreamOutlet lslOFailedTrialCounter;
    public StreamInfo lslIReceiverFinished;
    public StreamOutlet lslOReceiverFinished;

    public StreamInfo lslIBreak;
    public StreamOutlet lslOBreak;
    public StreamInfo lslIEndTime;
    public StreamOutlet lslOEndTime;

    float[] sample;
    private int channelCount = 0;
    StreamInfo[] streamInfos;
    public StreamInlet streamInlet;

    void Start()
    {
        receiverManager = GameObject.Find("Complete XR Origin Set Up").GetComponent<ReceiverManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // // Metadata
        // lslIMetadata = new StreamInfo(
        //     "Metadata",
        //     "Markers",
        //     4,
        //     NominalRate,
        //     LSL.channel_format_t.cf_string);
        // lslIMetadata.desc().append_child("Participant ID");
        // lslIMetadata.desc().append_child("Session ID");
        // lslIMetadata.desc().append_child("Experiment Start Time");
        // lslIMetadata.desc().append_child("Experiment End Time");
        // lslOMetadata = new StreamOutlet(lslIMetadata);
        
        // Timestamps
        lslITimestamps = new StreamInfo(
            "TimestampsReceiver",
            "Markers",
            1,
            NominalRate,
            LSL.channel_format_t.cf_double64);
        lslOTimestamps = new StreamOutlet(lslITimestamps);

        // Receiver Ready
        lslIReceiverReady = new StreamInfo(
            "ReceiverReady",
            "Markers",
            1,
            NominalRate,
            LSL.channel_format_t.cf_int32);

        // // Experiment Phase
        // lslIExperimentPhase = new StreamInfo(
        //     "ExperimentPhase",
        //     "Markers",
        //     1,
        //     NominalRate,
        //     LSL.channel_format_t.cf_int32);
        // lslIExperimentPhase.desc().append_child("Phase");
        // lslOExperimentPhase = new StreamOutlet(lslIExperimentPhase);
        
        // // Validation Error 
        // lslIValidationError = new StreamInfo(
        //     "ValidationError",
        //     "Markers",
        //     3,
        //     NominalRate,
        //     LSL.channel_format_t.cf_float32);
        // lslIValidationError.desc().append_child("ValX");
        // lslIValidationError.desc().append_child("ValY");
        // lslIValidationError.desc().append_child("ValZ");
        // lslOValidationError = new StreamOutlet(lslIValidationError);

        // // Validation Error Counter
        // lslIValidationErrorCounter = new StreamInfo(
        //     "ValidationCounter",
        //     "Markers",
        //     1,
        //     NominalRate,
        //     LSL.channel_format_t.cf_int32);
        // lslIValidationErrorCounter.desc().append_child("Count");
        // lslOValidationErrorCounter = new StreamOutlet(lslIValidationErrorCounter);

        // // Calibration Counter
        // lslICalibrationCounter = new StreamInfo(
        //     "CalibrationCounter",
        //     "Markers",
        //     1,
        //     NominalRate,
        //     LSL.channel_format_t.cf_int32);
        // lslICalibrationCounter.desc().append_child("Count");
        // lslOCalibrationCounter = new StreamOutlet(lslICalibrationCounter);

        // // Embodiment Training Time
        // lslIEmbodimentTrainingTime = new StreamInfo(
        //     "EmbodimentTrainingTime",
        //     "Markers",
        //     1,
        //     NominalRate,
        //     LSL.channel_format_t.cf_string);
        // lslOEmbodimentTrainingTime = new StreamOutlet(lslIEmbodimentTrainingTime);

        // Box Selected by Signaler
        // lslIBoxSelectedBySignaler = new StreamInfo(
        //     "BoxSelectedBySignaler",
        //     "Markers",
        //     4,
        //     NominalRate,
        //     LSL.channel_format_t.cf_float32);
        // lslIBoxSelectedBySignaler.desc().append_child("BoxPosX");
        // lslIBoxSelectedBySignaler.desc().append_child("BoxPosY");
        // lslIBoxSelectedBySignaler.desc().append_child("BoxPosZ");
        // lslIBoxSelectedBySignaler.desc().append_child("Associated Reward");
        // lslOBoxSelectedBySignaler = new StreamOutlet(lslIBoxSelectedBySignaler);

        // Box Selected by Receiver
        lslIBoxSelectedByReceiver = new StreamInfo(
            "BoxSelectedByReceiver",
            "Markers",
            4,
            NominalRate,
            LSL.channel_format_t.cf_float32);
        lslIBoxSelectedByReceiver.desc().append_child("BoxPosX");
        lslIBoxSelectedByReceiver.desc().append_child("BoxPosY");
        lslIBoxSelectedByReceiver.desc().append_child("BoxPosZ");
        lslIBoxSelectedByReceiver.desc().append_child("Reward Received");
        lslOBoxSelectedByReceiver = new StreamOutlet(lslIBoxSelectedByReceiver);

        // Raycast hit
        lslIRaycastHit = new StreamInfo(
            "RaycastHitReceiver",
            "Markers",
            3,
            NominalRate,
            LSL.channel_format_t.cf_int32);
        lslIRaycastHit.desc().append_child("Hit.x");
        lslIRaycastHit.desc().append_child("Hit.y");
        lslIRaycastHit.desc().append_child("Hit.z");

        // Eye Position, Direction, Rotation
        lslIEyePosDirRot = new StreamInfo(
            "EyePosDirRotReceiver",
            "Markers",
            9,
            NominalRate,
            LSL.channel_format_t.cf_float32);
        lslIEyePosDirRot.desc().append_child("PosX");
        lslIEyePosDirRot.desc().append_child("PosY");
        lslIEyePosDirRot.desc().append_child("PosZ");
        lslIEyePosDirRot.desc().append_child("DirX");
        lslIEyePosDirRot.desc().append_child("DirY");
        lslIEyePosDirRot.desc().append_child("DirZ");
        lslIEyePosDirRot.desc().append_child("RotX");
        lslIEyePosDirRot.desc().append_child("RotY");
        lslIEyePosDirRot.desc().append_child("RotZ");
        lslOEyePosDirRot = new StreamOutlet(lslIEyePosDirRot);

        // Eye Openness
        lslIEyeOpennessLR = new StreamInfo(
            "EyeOpennessLRReceiver",
            "Markers",
            2,
            NominalRate,
            LSL.channel_format_t.cf_float32);
        lslIEyeOpennessLR.desc().append_child("OpennessL");
        lslIEyeOpennessLR.desc().append_child("OpennessR");
        lslOEyeOpennessLR = new StreamOutlet(lslIEyeOpennessLR);

        // Pupil Diameter
        lslIPupilDiameterLR = new StreamInfo(
            "PupilDiameterLRReceiver",
            "Markers",
            2,
            NominalRate,
            LSL.channel_format_t.cf_float32);
        lslIPupilDiameterLR.desc().append_child("DiameterL");
        lslIPupilDiameterLR.desc().append_child("DiameterR");
        lslOPupilDiameterLR = new StreamOutlet(lslIPupilDiameterLR);

        // HMD Position, Direction, Rotation
        lslIhmdPosDirRot = new StreamInfo(
            "HMDPosDirRotReceiver",
            "Markers",
            15,
            NominalRate,
            LSL.channel_format_t.cf_float32);
        lslIhmdPosDirRot.desc().append_child("PosX");
        lslIhmdPosDirRot.desc().append_child("PosY");
        lslIhmdPosDirRot.desc().append_child("PosZ");
        lslIhmdPosDirRot.desc().append_child("DirForwardX");
        lslIhmdPosDirRot.desc().append_child("DirForwardY");
        lslIhmdPosDirRot.desc().append_child("DirForwardZ");
        lslIhmdPosDirRot.desc().append_child("DirVerticalX");
        lslIhmdPosDirRot.desc().append_child("DirVerticalY");
        lslIhmdPosDirRot.desc().append_child("DirVerticalZ");
        lslIhmdPosDirRot.desc().append_child("DirHorizontalX");
        lslIhmdPosDirRot.desc().append_child("DirHorizontalY");
        lslIhmdPosDirRot.desc().append_child("DirHorizontalZ");
        lslIhmdPosDirRot.desc().append_child("RotX");
        lslIhmdPosDirRot.desc().append_child("RotY");
        lslIhmdPosDirRot.desc().append_child("RotZ");
        lslOhmdPosDirRot = new StreamOutlet(lslIhmdPosDirRot);

        // Hand Position, Direction, Rotation
        // lslIHandPosDirRot = new StreamInfo(
        //     "HandPosDirRotReceiver",
        //     "Markers",
        //     30,
        //     NominalRate,
        //     LSL.channel_format_t.cf_float32);
        // lslIHandPosDirRot.desc().append_child("LeftPosX");
        // lslIHandPosDirRot.desc().append_child("LeftPosY");
        // lslIHandPosDirRot.desc().append_child("LeftPosZ");
        // lslIHandPosDirRot.desc().append_child("LeftDirForwardX");
        // lslIHandPosDirRot.desc().append_child("LeftDirForwardY");
        // lslIHandPosDirRot.desc().append_child("LeftDirForwardZ");
        // lslIHandPosDirRot.desc().append_child("LeftDirVerticalX");
        // lslIHandPosDirRot.desc().append_child("LeftDirVerticalY");
        // lslIHandPosDirRot.desc().append_child("LeftDirVerticalZ");
        // lslIHandPosDirRot.desc().append_child("LeftDirHorizontalX");
        // lslIHandPosDirRot.desc().append_child("LeftDirHorizontalY");
        // lslIHandPosDirRot.desc().append_child("LeftDirHorizontalZ");
        // lslIHandPosDirRot.desc().append_child("LeftRotX");
        // lslIHandPosDirRot.desc().append_child("LeftRotY");
        // lslIHandPosDirRot.desc().append_child("LeftRotZ");
        // lslIHandPosDirRot.desc().append_child("RightPosX");
        // lslIHandPosDirRot.desc().append_child("RightPosY");
        // lslIHandPosDirRot.desc().append_child("RightPosZ");
        // lslIHandPosDirRot.desc().append_child("RightDirForwardX");
        // lslIHandPosDirRot.desc().append_child("RightDirForwardY");
        // lslIHandPosDirRot.desc().append_child("RightDirForwardZ");
        // lslIHandPosDirRot.desc().append_child("RightDirVerticalX");
        // lslIHandPosDirRot.desc().append_child("RightDirVerticalY");
        // lslIHandPosDirRot.desc().append_child("RightDirVerticalZ");
        // lslIHandPosDirRot.desc().append_child("RightDirHorizontalX");
        // lslIHandPosDirRot.desc().append_child("RightDirHorizontalY");
        // lslIHandPosDirRot.desc().append_child("RightDirHorizontalZ");
        // lslIHandPosDirRot.desc().append_child("RightRotX");
        // lslIHandPosDirRot.desc().append_child("RightRotY");
        // lslIHandPosDirRot.desc().append_child("RightRotZ");
        // lslOHandPosDirRot = new StreamOutlet(lslIHandPosDirRot);

        // Preferred Hand
        lslIPreferredHand = new StreamInfo(
            "PreferredHandReceiver",
            "Markers",
            1,
            NominalRate,
            LSL.channel_format_t.cf_string);
        lslOPreferredHand = new StreamOutlet(lslIPreferredHand);

        // // Trial Number
        // lslITrialNumber = new StreamInfo(
        //     "TrialNumber",
        //     "Markers",
        //     1,
        //     NominalRate,
        //     LSL.channel_format_t.cf_int32);
        // lslOTrialNumber = new StreamOutlet(lslITrialNumber);

        // // Fail Trial
        // lslIFailTrial = new StreamInfo(
        //     "FailTrial",
        //     "Markers",
        //     1,
        //     NominalRate,
        //     LSL.channel_format_t.cf_string);
        // lslOFailTrial = new StreamOutlet(lslIFailTrial);

        // // Failed Trial Counter
        lslIFailedTrialCounter = new StreamInfo(
            "FailedTrialCounter",
            "Markers",
            1,
            NominalRate,
            LSL.channel_format_t.cf_int32);
        lslIFailedTrialCounter.desc().append_child("Count");
        lslOFailedTrialCounter = new StreamOutlet(lslIFailedTrialCounter);

        // // Frozen Gaze
        lslIFrozenGaze = new StreamInfo(
            "FrozenReceiver",
            "Markers",
            1,
            NominalRate,
            LSL.channel_format_t.cf_string);
        lslOFrozenGaze = new StreamOutlet(lslIFrozenGaze);

        // Receiver Finished
        lslIReceiverFinished = new StreamInfo(
                "ReceiverFinished",
                "Markers",
                1,
                NominalRate,
                LSL.channel_format_t.cf_string);
        lslOReceiverFinished = new StreamOutlet(lslIReceiverFinished);

        // Break
        lslIBreak = new StreamInfo(
                "BreakReceiver",
                "Markers",
                2,
                NominalRate,
                LSL.channel_format_t.cf_string);
        lslIBreak.desc().append_child("Break start time");
        lslIBreak.desc().append_child("Break end time");
        lslOBreak = new StreamOutlet(lslIBreak);

    }
    void Update()
    {
        


        //  Raycast
        var raycast = new float[3];
        raycast[0] = receiverManager.hitData.transform.position.x;
        raycast[1] = receiverManager.hitData.transform.position.x;
        raycast[2] = receiverManager.hitData.transform.position.x;

        lslORaycastHit.push_sample(raycast);

        if (SRanipal_Eye_v2.GetVerboseData(out VerboseData eyeData))
        {
            // Eye openness
            var opennessData = new float[2];
            opennessData[0] = eyeData.left.eye_openness;
            opennessData[1] = eyeData.right.eye_openness;
            lslOEyeOpennessLR.push_sample(opennessData);

            // Pupil diameter
            var pupilDiameterData = new float[2];
            pupilDiameterData[0] = eyeData.left.pupil_diameter_mm;
            pupilDiameterData[1] = eyeData.right.pupil_diameter_mm;
            lslOPupilDiameterLR.push_sample(pupilDiameterData);
        }
    }    

}