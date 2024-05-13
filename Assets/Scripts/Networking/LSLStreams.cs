using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LSL;

public class LSLStreams : MonoBehaviour
{
    public static LSLStreams Instance { get; private set; } // used to allow easy access of this script in other scripts
    private string participantUID; 
    private const double NominalRate = LSL.LSL.IRREGULAR_RATE; // irregular sampling rate
    
    // variables to save through LSL
    public StreamInfo lslIMetadata;
    public StreamOutlet lslOMetadata; 
    public StreamInfo lslITimestamps;
    public StreamOutlet lslOTimestamps;
    public StreamInfo lslIExperimentPhase;
    public StreamOutlet lslOExperimentPhase;
    public StreamInfo lslIValidationError; 
    public StreamOutlet lslOValidationError; 
    public StreamInfo lslIValidationErrorCounter;
    public StreamOutlet lslOValidationErrorCounter; 
    public StreamInfo lslICalibrationCounter;
    public StreamOutlet lslOCalibrationCounter;
    public StreamInfo lslIEmbodimentTrainingTime;
    public StreamOutlet lslOEmbodimentTrainingTime;
    public StreamInfo lslIBoxSelectedBySignaler;
    public StreamOutlet lslOBoxSelectedBySignaler;
    public StreamInfo lslIBoxSelectedByReceiver;
    public StreamOutlet lslOBoxSelectedByReceiver;

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
    public StreamInfo lslITrialNumber;
    public StreamOutlet lslOTrialNumber;
    public StreamInfo lslIFailTrial;
    public StreamOutlet lslOFailTrial;
    public StreamInfo lslIFailedTrialCounter;
    public StreamOutlet lslOFailedTrialCounter;
    public StreamInfo lslIBreak;
    public StreamOutlet lslOBreak;
    public StreamInfo lslIEndTime;
    public StreamOutlet lslOEndTime;

    void Start()
    {
        // Metadata
        lslIMetadata = new StreamInfo(
            "Metadata",
            "Markers",
            4,
            NominalRate,
            LSL.channel_format_t.cf_string,
            participantUID);
        lslIMetadata.desc().append_child("Participant ID");
        lslIMetadata.desc().append_child("Session ID");
        lslIMetadata.desc().append_child("Experiment Start Time");
        lslIMetadata.desc().append_child("Experiment End Time");
        lslOMetadata = new StreamOutlet(lslIMetadata);
        
        // Timestamps
        lslITimestamps = new StreamInfo(
            "Timestamps",
            "Markers",
            2,
            NominalRate,
            LSL.channel_format_t.cf_double64,
            participantUID);
        lslITimestamps.desc().append_child("Timestamp Signaler");
        lslITimestamps.desc().append_child("Timestamp Receiver");
        lslOTimestamps = new StreamOutlet(lslITimestamps);

        // Experiment Phase
        lslIExperimentPhase = new StreamInfo(
            "ExperimentPhase",
            "Markers",
            1,
            NominalRate,
            LSL.channel_format_t.cf_int32,
            participantUID);
        lslIExperimentPhase.desc().append_child("Phase");
        lslOExperimentPhase = new StreamOutlet(lslIExperimentPhase);
        
        // Validation Error 
        lslIValidationError = new StreamInfo(
            "ValidationError",
            "Markers",
            3,
            NominalRate,
            LSL.channel_format_t.cf_float32,
            participantUID);
        lslIValidationError.desc().append_child("ValX");
        lslIValidationError.desc().append_child("ValY");
        lslIValidationError.desc().append_child("ValZ");
        lslOValidationError = new StreamOutlet(lslIValidationError);

        // Validation Error Counter
        lslIValidationErrorCounter = new StreamInfo(
            "ValidationCounter",
            "Markers",
            1,
            NominalRate,
            LSL.channel_format_t.cf_int32,
            participantUID);
        lslIValidationErrorCounter.desc().append_child("Count");
        lslOValidationErrorCounter = new StreamOutlet(lslIValidationErrorCounter);

        // Calibration Counter
        lslICalibrationCounter = new StreamInfo(
            "CalibrationCounter",
            "Markers",
            1,
            NominalRate,
            LSL.channel_format_t.cf_int32,
            participantUID);
        lslICalibrationCounter.desc().append_child("Count");
        lslOCalibrationCounter = new StreamOutlet(lslICalibrationCounter);

        // Embodiment Training Time
        lslIEmbodimentTrainingTime = new StreamInfo(
            "EmbodimentTrainingTime",
            "Markers",
            1,
            NominalRate,
            LSL.channel_format_t.cf_string,
            participantUID);
        lslOEmbodimentTrainingTime = new StreamOutlet(lslIEmbodimentTrainingTime);

        // Box Selected by Signaler
        lslIBoxSelectedBySignaler = new StreamInfo(
            "BoxSelectedBySignaler",
            "Markers",
            4,
            NominalRate,
            LSL.channel_format_t.cf_float32,
            participantUID);
        lslIBoxSelectedBySignaler.desc().append_child("BoxPosX");
        lslIBoxSelectedBySignaler.desc().append_child("BoxPosY");
        lslIBoxSelectedBySignaler.desc().append_child("BoxPosZ");
        lslIBoxSelectedBySignaler.desc().append_child("Associated Reward");
        lslOBoxSelectedBySignaler = new StreamOutlet(lslIBoxSelectedBySignaler);

        // Box Selected by Receiver
        lslIBoxSelectedByReceiver = new StreamInfo(
            "BoxSelectedByReceiver",
            "Markers",
            4,
            NominalRate,
            LSL.channel_format_t.cf_float32,
            participantUID);
        lslIBoxSelectedByReceiver.desc().append_child("BoxPosX");
        lslIBoxSelectedByReceiver.desc().append_child("BoxPosY");
        lslIBoxSelectedByReceiver.desc().append_child("BoxPosZ");
        lslIBoxSelectedByReceiver.desc().append_child("Reward Received");
        lslOBoxSelectedByReceiver = new StreamOutlet(lslIBoxSelectedByReceiver);

        // Eye Position, Direction, Rotation
        lslIEyePosDirRot = new StreamInfo(
            "EyePosDirRot",
            "Markers",
            9,
            NominalRate,
            LSL.channel_format_t.cf_float32,
            participantUID);
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
            "EyeOpennessLR",
            "Markers",
            2,
            NominalRate,
            LSL.channel_format_t.cf_float32,
            participantUID);
        lslIEyeOpennessLR.desc().append_child("OpennessL");
        lslIEyeOpennessLR.desc().append_child("OpennessR");
        lslOEyeOpennessLR = new StreamOutlet(lslIEyeOpennessLR);

        // Pupil Diameter
        lslIPupilDiameterLR = new StreamInfo(
            "PupilDiameterLR",
            "Markers",
            2,
            NominalRate,
            LSL.channel_format_t.cf_float32,
            participantUID);
        lslIPupilDiameterLR.desc().append_child("DiameterL");
        lslIPupilDiameterLR.desc().append_child("DiameterR");
        lslOPupilDiameterLR = new StreamOutlet(lslIPupilDiameterLR);

        // HMD Position, Direction, Rotation
        lslIhmdPosDirRot = new StreamInfo(
            "HMDPosDirRot",
            "Markers",
            15,
            NominalRate,
            LSL.channel_format_t.cf_float32,
            participantUID);
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
        lslIHandPosDirRot = new StreamInfo(
            "HandPosDirRot",
            "Markers",
            30,
            NominalRate,
            LSL.channel_format_t.cf_float32,
            participantUID);
        lslIHandPosDirRot.desc().append_child("LeftPosX");
        lslIHandPosDirRot.desc().append_child("LeftPosY");
        lslIHandPosDirRot.desc().append_child("LeftPosZ");
        lslIHandPosDirRot.desc().append_child("LeftDirForwardX");
        lslIHandPosDirRot.desc().append_child("LeftDirForwardY");
        lslIHandPosDirRot.desc().append_child("LeftDirForwardZ");
        lslIHandPosDirRot.desc().append_child("LeftDirVerticalX");
        lslIHandPosDirRot.desc().append_child("LeftDirVerticalY");
        lslIHandPosDirRot.desc().append_child("LeftDirVerticalZ");
        lslIHandPosDirRot.desc().append_child("LeftDirHorizontalX");
        lslIHandPosDirRot.desc().append_child("LeftDirHorizontalY");
        lslIHandPosDirRot.desc().append_child("LeftDirHorizontalZ");
        lslIHandPosDirRot.desc().append_child("LeftRotX");
        lslIHandPosDirRot.desc().append_child("LeftRotY");
        lslIHandPosDirRot.desc().append_child("LeftRotZ");
        lslIHandPosDirRot.desc().append_child("RightPosX");
        lslIHandPosDirRot.desc().append_child("RightPosY");
        lslIHandPosDirRot.desc().append_child("RightPosZ");
        lslIHandPosDirRot.desc().append_child("RightDirForwardX");
        lslIHandPosDirRot.desc().append_child("RightDirForwardY");
        lslIHandPosDirRot.desc().append_child("RightDirForwardZ");
        lslIHandPosDirRot.desc().append_child("RightDirVerticalX");
        lslIHandPosDirRot.desc().append_child("RightDirVerticalY");
        lslIHandPosDirRot.desc().append_child("RightDirVerticalZ");
        lslIHandPosDirRot.desc().append_child("RightDirHorizontalX");
        lslIHandPosDirRot.desc().append_child("RightDirHorizontalY");
        lslIHandPosDirRot.desc().append_child("RightDirHorizontalZ");
        lslIHandPosDirRot.desc().append_child("RightRotX");
        lslIHandPosDirRot.desc().append_child("RightRotY");
        lslIHandPosDirRot.desc().append_child("RightRotZ");
        lslOHandPosDirRot = new StreamOutlet(lslIHandPosDirRot);

        // Preferred Hand
        lslIPreferredHand = new StreamInfo(
            "PreferredHand",
            "Markers",
            1,
            NominalRate,
            LSL.channel_format_t.cf_string,
            participantUID);
        lslOPreferredHand = new StreamOutlet(lslIPreferredHand);

        // Frozen Gaze
        lslIFrozenGaze = new StreamInfo(
            "FrozenGaze",
            "Markers",
            1,
            NominalRate,
            LSL.channel_format_t.cf_string,
            participantUID);
        lslOFrozenGaze = new StreamOutlet(lslIFrozenGaze);

        // Trial Number
        lslITrialNumber = new StreamInfo(
            "TrialNumber",
            "Markers",
            1,
            NominalRate,
            LSL.channel_format_t.cf_int32,
            participantUID);
        lslOTrialNumber = new StreamOutlet(lslITrialNumber);

        // Fail Trial
        lslIFailTrial = new StreamInfo(
            "FailTrial",
            "Markers",
            1,
            NominalRate,
            LSL.channel_format_t.cf_string,
            participantUID);
        lslOFailTrial = new StreamOutlet(lslIFailTrial);

        // Failed Trial Counter
        lslIFailedTrialCounter = new StreamInfo(
            "FailedTrialCounter",
            "Markers",
            1,
            NominalRate,
            LSL.channel_format_t.cf_int32,
            participantUID);
        lslIFailedTrialCounter.desc().append_child("Count");
        lslOFailedTrialCounter = new StreamOutlet(lslIFailedTrialCounter);

        // Break
        lslIBreak = new StreamInfo(
                "Break",
                "Markers",
                2,
                NominalRate,
                LSL.channel_format_t.cf_string,
                participantUID);
        lslIBreak.desc().append_child("Break start time");
        lslIBreak.desc().append_child("Break end time");
        lslOBreak = new StreamOutlet(lslIBreak);

    }

}