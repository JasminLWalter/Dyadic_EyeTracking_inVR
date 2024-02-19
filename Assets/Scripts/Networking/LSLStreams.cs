using UnityEngine;
using LSL4Unity;

public class LSLStreams : MonoBehaviour
{
    public static LSLStreams Instance { get; private set; } // used to allow easy access of this script in other scripts
    private string participantUID; 
    private const double NominalRate = liblsl.IRREGULAR_RATE; // irregular sampling rate
    // variables to save date to LSL
    public liblsl.StreamInfo lslIValidationError; 
    public liblsl.StreamOutlet lslOValidationError; // saved in Validation.cs
    public liblsl.StreamInfo lslIEyeTrackingWorld;
    public liblsl.StreamOutlet lslOEyeTrackingWorld; // saved in ETRecorder.cs
    public liblsl.StreamInfo lslIEyeTrackingLocal;
    public liblsl.StreamOutlet lslOEyeTrackingLocal; // saved in ETRecorder.cs
    public liblsl.StreamInfo lslIHitObjectNames;
    public liblsl.StreamOutlet lslOHitObjectNames; // saved in ETRecorder.cs
    public liblsl.StreamInfo lslIHitObjectPositions;
    public liblsl.StreamOutlet lslOHitObjectPositions; // saved in ETRecorder.cs
    public liblsl.StreamInfo lslIHitPositionOnObjects;
    public liblsl.StreamOutlet lslOHitPositionOnObjects; // saved in ETRecorder.cs
    public liblsl.StreamInfo lslIAgentPosition;
    public liblsl.StreamOutlet lslOAgentPosition; // saved in NPCPatrol.cs && NPCPatrolAfterSpawn.cs
    public liblsl.StreamInfo lslIAgentRotation;
    public liblsl.StreamOutlet lslOAgentRotation; // saved in HeadRotationNPCs
    public liblsl.StreamInfo lslIStaticAgentPosition;
    public liblsl.StreamOutlet lslOStaticAgentPosition; // saved in StaticNPCSave.cs 
    public liblsl.StreamInfo lslIStaticAgentRotation;
    public liblsl.StreamOutlet lslOStaticAgentRotation; // saved in StaticNPCSave.cs 
    public liblsl.StreamInfo lslIPlayerPosition;
    public liblsl.StreamOutlet lslOPlayerPosition; // saved in PlayerPositionTracker.cs
    public liblsl.StreamInfo lslIHeadTracking;
    public liblsl.StreamOutlet lslOHeadTracking; // saved in ETRecorder.cs
    public liblsl.StreamInfo lslIButtonPresses;
    public liblsl.StreamOutlet lslOButtonPresses; // saved in MovementInput.cs
    public liblsl.StreamInfo lslIrot;
    public liblsl.StreamOutlet lslOrot; // saved in MovementInput.cs

    private void Awake()
    {
        // TODO -------------
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Validation Error
        // saved: 3 coordinates of the error
        participantUID = ExpManager.Instance.participantID.ToString();
        lslIValidationError = new liblsl.StreamInfo(
            "ValidationError",
            "Markers",
            3,
            NominalRate,
            liblsl.channel_format_t.cf_float32,
            participantUID);
        lslIValidationError.desc().append_child("ValX");
        lslIValidationError.desc().append_child("ValY");
        lslIValidationError.desc().append_child("ValZ");
        lslOValidationError = new liblsl.StreamOutlet(lslIValidationError);
        // World Coordinates
        // saved: Tobii timestamps (1); origin coordinates (3); direction coordinates (3), Left & right eye blinks (2), Check if ray is valid (1)
        lslIEyeTrackingWorld = new liblsl.StreamInfo(
            "EyeTrackingWorld",
            "Markers",
            10,
            NominalRate,
            liblsl.channel_format_t.cf_float32,
            participantUID);
        lslIEyeTrackingWorld.desc().append_child("ETWTime");
        lslIEyeTrackingWorld.desc().append_child("ETWoriginX");
        lslIEyeTrackingWorld.desc().append_child("ETWoriginY");
        lslIEyeTrackingWorld.desc().append_child("ETWoriginZ");
        lslIEyeTrackingWorld.desc().append_child("ETWdirectionX");
        lslIEyeTrackingWorld.desc().append_child("ETWdirectionY");
        lslIEyeTrackingWorld.desc().append_child("ETWdirectionZ");
        lslIEyeTrackingWorld.desc().append_child("leftBlink");
        lslIEyeTrackingWorld.desc().append_child("rightBlink");
        lslIEyeTrackingWorld.desc().append_child("valid");
        lslOEyeTrackingWorld = new liblsl.StreamOutlet(lslIEyeTrackingWorld);
        // Hit Objects 
        // saved: max 10 objects that the participant could potentially have looked up 
        lslIHitObjectNames = new liblsl.StreamInfo(
            "HitObjectNames",
            "Markers",
            30,
            NominalRate,
            liblsl.channel_format_t.cf_string,
            participantUID);
        lslIHitObjectNames.desc().append_child("HON");
        lslOHitObjectNames = new liblsl.StreamOutlet(lslIHitObjectNames);
        // Hit Object Coordinates (in World Coordinates)
        // saved: 3 coordinates for each object that was potentially looked up (obj1_x, obj1_y, obj1_z, obj2_x, ...)
        lslIHitObjectPositions = new liblsl.StreamInfo(
            "HitObjectPositions",
            "Markers",
            90,
            NominalRate,
            liblsl.channel_format_t.cf_float32,
            participantUID);
        lslIHitObjectPositions.desc().append_child("HOPX");
        lslIHitObjectPositions.desc().append_child("HOPY");
        lslIHitObjectPositions.desc().append_child("HOPZ");
        lslOHitObjectPositions = new liblsl.StreamOutlet(lslIHitObjectPositions);
        // Hit Positions on Objects (in World Coordinates)
        // saved: 3 coordinates on each object that was potentially looked up (obj1_x, obj1_y, obj1_z, obj2_x, ...)
        lslIHitPositionOnObjects = new liblsl.StreamInfo(
            "HitPositionOnObjects",
            "Markers",
            90,
            NominalRate,
            liblsl.channel_format_t.cf_float32,
            participantUID);
        lslIHitPositionOnObjects.desc().append_child("HPOOX");
        lslIHitPositionOnObjects.desc().append_child("HPOOY");
        lslIHitPositionOnObjects.desc().append_child("HPOOZ");
        lslOHitPositionOnObjects = new liblsl.StreamOutlet(lslIHitPositionOnObjects);
        // Local Coordinates
        // saved: origin coordinates (3); direction coordinates (3)
        lslIEyeTrackingLocal = new liblsl.StreamInfo(
            "EyeTrackingLocal",
            "Markers",
            6,
            NominalRate,
            liblsl.channel_format_t.cf_float32,
            participantUID);
        lslIEyeTrackingLocal.desc().append_child("ETLoriginX");
        lslIEyeTrackingLocal.desc().append_child("ETLoriginY");
        lslIEyeTrackingLocal.desc().append_child("ETLoriginZ");
        lslIEyeTrackingLocal.desc().append_child("ETLdirectionX");
        lslIEyeTrackingLocal.desc().append_child("ETLdirectionY");
        lslIEyeTrackingLocal.desc().append_child("ETLdirectionZ");
        lslOEyeTrackingLocal = new liblsl.StreamOutlet(lslIEyeTrackingLocal);
        // Agent Positions
        // saved: Agent ID (1); agent position (3)
        lslIAgentPosition = new liblsl.StreamInfo(
            "AgentPosition",
            "Markers",
            4,
            NominalRate,
            liblsl.channel_format_t.cf_float32,
            participantUID);
        lslIAgentPosition.desc().append_child("NPCid");
        lslIAgentPosition.desc().append_child("NPCposX");
        lslIAgentPosition.desc().append_child("NPCposY");
        lslIAgentPosition.desc().append_child("NPCposZ");
        lslOAgentPosition = new liblsl.StreamOutlet(lslIAgentPosition);
        // Agent Rotations
        // saved: Agent ID (1); agent rotation along y-axis (1)
        lslIAgentRotation = new liblsl.StreamInfo(
            "AgentRotation",
            "Markers",
            2,
            NominalRate,
            liblsl.channel_format_t.cf_float32,
            participantUID);
        lslIAgentRotation.desc().append_child("NPCid");
        lslIAgentRotation.desc().append_child("NPCrotY");
        lslOAgentRotation = new liblsl.StreamOutlet(lslIAgentRotation);
        // Static Agent Positions
        // saved at the beginning once: Agent ID (1); agent position (3)
        lslIStaticAgentPosition = new liblsl.StreamInfo(
            "StaticAgentPosition",
            "Markers",
            4,
            NominalRate,
            liblsl.channel_format_t.cf_float32,
            participantUID);
        lslIStaticAgentPosition.desc().append_child("SNPCid");
        lslIStaticAgentPosition.desc().append_child("SNPCposX");
        lslIStaticAgentPosition.desc().append_child("SNPCposY");
        lslIStaticAgentPosition.desc().append_child("SNPCposZ");
        lslOStaticAgentPosition = new liblsl.StreamOutlet(lslIStaticAgentPosition);
        // Static Agent Rotation
        // saved at the beginning once: Agent ID (1); agent rotation around y-axis (1)
        lslIStaticAgentRotation = new liblsl.StreamInfo(
            "StaticAgentRotation",
            "Markers",
            2,
            NominalRate,
            liblsl.channel_format_t.cf_float32,
            participantUID);
        lslIStaticAgentRotation.desc().append_child("SNPCid");
        lslIStaticAgentRotation.desc().append_child("SNPCrotY");
        lslOStaticAgentRotation = new liblsl.StreamOutlet(lslIStaticAgentRotation);
        // Player Positions
        // saved: Player position (3)
        lslIPlayerPosition = new liblsl.StreamInfo(
            "PlayerPosition",
            "Markers",
            3,
            NominalRate,
            liblsl.channel_format_t.cf_float32,
            participantUID);
        lslIPlayerPosition.desc().append_child("PPX");
        lslIPlayerPosition.desc().append_child("PPY");
        lslIPlayerPosition.desc().append_child("PPZ");
        lslOPlayerPosition = new liblsl.StreamOutlet(lslIPlayerPosition);
        // Head Tracking
        // saved: Head (camera) position (3); nose vector (3)
        lslIHeadTracking = new liblsl.StreamInfo(
            "HeadTracking",
            "Markers",
            6,
            NominalRate,
            liblsl.channel_format_t.cf_float32,
            participantUID);
        lslIHeadTracking.desc().append_child("HToriginX");
        lslIHeadTracking.desc().append_child("HToriginY");
        lslIHeadTracking.desc().append_child("HToriginZ");
        lslIHeadTracking.desc().append_child("HTdirectionX");
        lslIHeadTracking.desc().append_child("HTdirectionY");
        lslIHeadTracking.desc().append_child("HTdirectionZ");
        lslOHeadTracking = new liblsl.StreamOutlet(lslIHeadTracking);
        // Button Presses
        // saved: y-position of controller input and -2 if there is no input
        lslIButtonPresses = new liblsl.StreamInfo(
            "ButtonPresses",
            "Markers",
            1,
            NominalRate,
            liblsl.channel_format_t.cf_float32,
            participantUID);
        lslIButtonPresses.desc().append_child("buttonPressed");
        lslOButtonPresses = new liblsl.StreamOutlet(lslIButtonPresses);
        // Button Presses
        // saved: y-position of controller input and -2 if there is no input
        lslIrot = new liblsl.StreamInfo(
            "RotationStream",
            "Markers",
            1,
            NominalRate,
            liblsl.channel_format_t.cf_float32,
            participantUID);
        lslOrot = new liblsl.StreamOutlet(lslIrot);
    }
}
