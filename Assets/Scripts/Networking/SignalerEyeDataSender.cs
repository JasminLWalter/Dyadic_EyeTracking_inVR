using UnityEngine;
using LSL;
using System.Collections.Generic;
using ViveSR.anipal.Eye;

public class SignalerEyeDataSender : MonoBehaviour
{
    private StreamOutlet outlet;
    private Dictionary<EyeShape_v2, float> eyeWeightings = new Dictionary<EyeShape_v2, float>();
    private EyeData_v2 eyeData = new EyeData_v2();

    private SignalerManager signalerManager;
    private ReceiverManager receiverManager;
    private GameManager gameManager;
    // private GameObject invisibleObject;
    public Transform headConstraint;

    private float leftBlink;
    
    private float rightBlink;
    private float leftWide;
    private float rightWide;
    private float leftSqueeze;
    private float rightSqueeze;
    private float eye_Left_Up;
    private float eye_Left_Down;
    private float eye_Left_Left;
    private float eye_Left_Right;
    private float eye_Right_Up;
    private float eye_Right_Down;
    private float eye_Right_Left;
    private float eye_Right_Right;

    private float leftBlinkFrozen;
    private float rightBlinkFrozen;
    private float leftWideFrozen;
    private float rightWideFrozen;
    private float leftSqueezeFrozen;
    private float rightSqueezeFrozen;
    private float eye_Left_UpFrozen;
    private float eye_Left_DownFrozen;
    private float eye_Left_LeftFrozen;
    private float eye_Left_RightFrozen;
    private float eye_Right_UpFrozen;
    private float eye_Right_DownFrozen;
    private float eye_Right_LeftFrozen;
    private float eye_Right_RightFrozen;

    private Vector3 combinedGazeDirectionFrozen = new Vector3(0f,0f,0f);
    private Vector3 combinedGazeDirection = new Vector3(0f,0f,0f);

    private Vector3 rightGazeDirection = new Vector3(0f,0f,0f);
    private Vector3 rightGazeDirectionFrozen = new Vector3(0f,0f,0f);

    public Vector3 invisibleObjectPosFrozen = new Vector3(0f,0f,0f);
    public Vector3 invisibleObjectPos = new Vector3(0f,0f,0f);
    public Vector4 headConstraintPos = new Vector3(0f,0f,0f);
    private Vector4 headConstraintFrozen = new Vector3(0f,0f,0f);

    private InputBindings _inputBindings;
    private StreamInlet inletFrozenReceiver;

    public LSLSignalerOutlets lSLSignalerOutlets;
   // private LSLReceiverOutlets lSLReceiverOutlets;
    void Start()
    {
        StreamInfo streamInfo = new StreamInfo("EyeTrackingSignaler", "Gaze", 27, 0, channel_format_t.cf_float32);
        outlet = new StreamOutlet(streamInfo);
        
        // StreamInfo[] frozenReceiverStreams = LSL.LSL.resolve_stream("name", "FrozenReceiver", 1, 0.0);
        // Debug.LogError("is Empty: " + frozenReceiverStreams.Length);
        //     if (frozenReceiverStreams.Length > 0)
        //     {
                
        //         inletFrozenReceiver = new StreamInlet(frozenReceiverStreams[0]);
        //     }
        //     else
        //     {
        //         Debug.LogError("No FrozenReceiver stream found.");
        //     }
        
        // lSLSignalerOutlets = FindObjectOfType<LSLSignalerOutlets>();
        // lSLReceiverOutlets = FindObjectOfType<LSLReceiverOutlets>();


        signalerManager = FindObjectOfType<SignalerManager>();
        receiverManager = FindObjectOfType<ReceiverManager>();
        gameManager = FindObjectOfType<GameManager>();

        _inputBindings = new InputBindings();
        _inputBindings.Player.Enable();

    }

    void Update()
    {
        string frozenString = signalerManager.frozen.ToString();
        lSLSignalerOutlets.lslOFrozenGaze.push_sample(new string[] {frozenString} );

        // if(inletFrozenReceiver == null & gameManager.role == "signaler") {
            
        //     StreamInfo[] frozenReceiverStreams = LSL.LSL.resolve_stream("name", "FrozenReceiver", 1, 0.0);
        //     inletFrozenReceiver = new StreamInlet(frozenReceiverStreams[0]);
        //     Debug.LogError("ReceiverFrozen remotely: " + frozenReceiverStreams[0]);

        // }
        if (inletFrozenReceiver == null && gameManager.phase == 3)
        {

            // Resolve stream and check if there is a valid FrozenReceiver stream available
            StreamInfo[] frozenReceiverStreams = LSL.LSL.resolve_stream("name", "FrozenReceiver", 1, 2.0);
            
            if (frozenReceiverStreams.Length > 0)
            {
                // Create the inlet only if a valid stream is found
                inletFrozenReceiver = new StreamInlet(frozenReceiverStreams[0]);
                Debug.Log("ReceiverFrozen stream found and inlet created.");
            }
            else
            {
                // Handle the case where no stream is found (optional logging)
                Debug.LogError("No FrozenReceiver stream found.");
            }
        }

        if (inletFrozenReceiver != null)
        {
            // Pull sample from the frozen signaler inlet
            string[] sampleFrozenReceiver = new string[1];
            inletFrozenReceiver.pull_sample(sampleFrozenReceiver);
            Debug.Log($"Frozen Signaler Sample: {sampleFrozenReceiver[0]}");
            
            if(sampleFrozenReceiver[0] == "true"){
                signalerManager.frozen = true;
            }
            if(sampleFrozenReceiver[0] == "false"){
                signalerManager.frozen = false;
            }
        }
        
        if (SRanipal_Eye_v2.GetEyeWeightings(out eyeWeightings))
        {
            // invisibleObject = signalerManager.invisibleObject;
            
            // Vector3 leftGazeDirection, rightGazeDirection, combinedGazeDirection;
            Vector3 gazeOrigin;
           // new Vector3 leftGazeDirectionFrozen;
           // new Vector3 rightGazeDirectionFrozen, combinedGazeDirectionFrozen;



                if(signalerManager.frozen == false)
                {
                    SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out gazeOrigin, out rightGazeDirection);
                    SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out gazeOrigin, out combinedGazeDirection);
                    // Extract gaze direction
                    // SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out gazeOrigin, out leftGazeDirection);

                // Get blink weightings
                    leftBlink = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Blink) ? eyeWeightings[EyeShape_v2.Eye_Left_Blink] : 0.0f;
                    rightBlink = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Blink) ? eyeWeightings[EyeShape_v2.Eye_Right_Blink] : 0.0f;
                    leftWide = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Wide) ? eyeWeightings[EyeShape_v2.Eye_Left_Wide] : 0.0f;
                    rightWide = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Wide) ? eyeWeightings[EyeShape_v2.Eye_Right_Wide] : 0.0f;
                    leftSqueeze = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Squeeze) ? eyeWeightings[EyeShape_v2.Eye_Left_Squeeze] : 0.0f;
                    rightSqueeze = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Squeeze) ? eyeWeightings[EyeShape_v2.Eye_Right_Squeeze] : 0.0f;
                    eye_Left_Up = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Up) ? eyeWeightings[EyeShape_v2.Eye_Left_Up] : 0.0f;
                    eye_Left_Down = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Down) ? eyeWeightings[EyeShape_v2.Eye_Left_Down] : 0.0f;
                    eye_Left_Left =  eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Left) ? eyeWeightings[EyeShape_v2.Eye_Left_Left] : 0.0f;  
                    eye_Left_Right = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Right) ? eyeWeightings[EyeShape_v2.Eye_Left_Right] : 0.0f;
                    eye_Right_Up = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Up) ? eyeWeightings[EyeShape_v2.Eye_Right_Up] : 0.0f;
                    eye_Right_Down = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Down) ? eyeWeightings[EyeShape_v2.Eye_Right_Down] : 0.0f;   
                    eye_Right_Left = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Left) ? eyeWeightings[EyeShape_v2.Eye_Right_Left] : 0.0f;
                    eye_Right_Right = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Right) ? eyeWeightings[EyeShape_v2.Eye_Right_Right] : 0.0f;
                    
                    invisibleObjectPos.x = signalerManager.invisibleObject.transform.position.x;
                    invisibleObjectPos.y = signalerManager.invisibleObject.transform.position.y;
                    invisibleObjectPos.z = signalerManager.invisibleObject.transform.position.z;    

                    headConstraintPos.x = headConstraint.transform.rotation.x;
                    headConstraintPos.y = headConstraint.transform.rotation.y;
                    headConstraintPos.z = headConstraint.transform.rotation.z;
                    headConstraintPos.w = headConstraint.transform.rotation.w;
                    
                }
                else if(signalerManager.frozen)
                {
                    combinedGazeDirection.x = combinedGazeDirectionFrozen.x;
                    combinedGazeDirection.y = combinedGazeDirectionFrozen.y;
                    combinedGazeDirection.z = combinedGazeDirectionFrozen.z;
                    rightGazeDirection.x = rightGazeDirectionFrozen.x;
                    rightGazeDirection.y = rightGazeDirectionFrozen.y;
                    rightGazeDirection.z = rightGazeDirectionFrozen.z;


                    leftBlink = leftBlinkFrozen;
                    rightBlink = rightBlinkFrozen;
                    leftWide = leftWideFrozen;
                    rightWide = rightWideFrozen;
                    leftSqueeze = leftSqueezeFrozen;
                    rightSqueeze = rightSqueezeFrozen;
                    eye_Left_Up = eye_Left_UpFrozen;
                    eye_Left_Down = eye_Left_DownFrozen;
                    eye_Left_Left =  eye_Left_LeftFrozen;  
                    eye_Left_Right = eye_Left_RightFrozen;
                    eye_Right_Up = eye_Right_UpFrozen;
                    eye_Right_Down = eye_Right_DownFrozen;   
                    eye_Right_Left = eye_Right_LeftFrozen;
                    eye_Right_Right = eye_Right_RightFrozen;

                    invisibleObjectPos.x = invisibleObjectPosFrozen.x;
                    invisibleObjectPos.y = invisibleObjectPosFrozen.y;
                    invisibleObjectPos.z = invisibleObjectPosFrozen.z;

                    headConstraintPos.x = headConstraintFrozen.x;
                    headConstraintPos.y = headConstraintFrozen.y;
                    headConstraintPos.z = headConstraintFrozen.z;
                    headConstraintPos.w = headConstraintFrozen.w;
                }
                // Prepare LSL sample
                //Debug.Log("Invisible Object: " + signalerManager.invisibleObject.transform.position);

                float[] sample = new float[27];
                //float[] sample = new float[22];
                // sample[0] = leftGazeDirection.x;
                // sample[1] = leftGazeDirection.y;
                // sample[2] = leftGazeDirection.z;
                sample[0] = combinedGazeDirection.x;
                sample[1] = combinedGazeDirection.y;
                sample[2] = combinedGazeDirection.z;
                sample[3] = rightGazeDirection.x;
                sample[4] = rightGazeDirection.y;
                sample[5] = rightGazeDirection.z;
                sample[6] = leftBlink;
                sample[7] = rightBlink;
                sample[8] = leftWide;
                sample[9] = rightWide;
                sample[10] = leftSqueeze;
                sample[11] = rightSqueeze;
                
                sample[12] = eye_Left_Up;
                sample[13] = eye_Left_Down;
                sample[14] = eye_Left_Left;
                sample[15] = eye_Left_Right;   
                sample[16] = eye_Right_Up;
                sample[17] = eye_Right_Down;
                sample[18] = eye_Right_Left;
                sample[19] = eye_Right_Right;

                
                sample[20] = invisibleObjectPos.x;
                sample[21] = invisibleObjectPos.y;
                sample[22] = invisibleObjectPos.z;

                sample[23] = headConstraintPos.x;
                sample[24] = headConstraintPos.y;
                sample[25] = headConstraintPos.z;
                sample[26] = headConstraintPos.w;

                // Send sample via LSL
                outlet.push_sample(sample);

                if(_inputBindings.Player.Freeze.triggered && signalerManager.freezeCounter > 1)
                {
                    leftBlinkFrozen = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Blink) ? eyeWeightings[EyeShape_v2.Eye_Left_Blink] : 0.0f;
                    rightBlinkFrozen = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Blink) ? eyeWeightings[EyeShape_v2.Eye_Right_Blink] : 0.0f;
                    leftWideFrozen = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Wide) ? eyeWeightings[EyeShape_v2.Eye_Left_Wide] : 0.0f;
                    rightWideFrozen = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Wide) ? eyeWeightings[EyeShape_v2.Eye_Right_Wide] : 0.0f;
                    leftSqueezeFrozen = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Squeeze) ? eyeWeightings[EyeShape_v2.Eye_Left_Squeeze] : 0.0f;
                    rightSqueezeFrozen = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Squeeze) ? eyeWeightings[EyeShape_v2.Eye_Right_Squeeze] : 0.0f;
                    eye_Left_UpFrozen = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Up) ? eyeWeightings[EyeShape_v2.Eye_Left_Up] : 0.0f;
                    eye_Left_DownFrozen = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Down) ? eyeWeightings[EyeShape_v2.Eye_Left_Down] : 0.0f;
                    eye_Left_LeftFrozen =  eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Left) ? eyeWeightings[EyeShape_v2.Eye_Left_Left] : 0.0f;  
                    eye_Left_RightFrozen = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Right) ? eyeWeightings[EyeShape_v2.Eye_Left_Right] : 0.0f;
                    eye_Right_UpFrozen = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Up) ? eyeWeightings[EyeShape_v2.Eye_Right_Up] : 0.0f;
                    eye_Right_DownFrozen = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Down) ? eyeWeightings[EyeShape_v2.Eye_Right_Down] : 0.0f;   
                    eye_Right_LeftFrozen = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Left) ? eyeWeightings[EyeShape_v2.Eye_Right_Left] : 0.0f;
                    eye_Right_RightFrozen = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Right) ? eyeWeightings[EyeShape_v2.Eye_Right_Right] : 0.0f;

                    SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out gazeOrigin, out rightGazeDirection);
                    SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out gazeOrigin, out combinedGazeDirection);

                    combinedGazeDirectionFrozen.x = combinedGazeDirection.x;
                    combinedGazeDirectionFrozen.y = combinedGazeDirection.y;
                    combinedGazeDirectionFrozen.z = combinedGazeDirection.z;
                    
                    Debug.Log("Combined Gaze Direction: " + combinedGazeDirection.x + combinedGazeDirection.y + combinedGazeDirection.z);

                    Debug.Log("Combined Gaze Direction Frozen: " + combinedGazeDirectionFrozen.x + combinedGazeDirectionFrozen.y + combinedGazeDirectionFrozen.z);

                    rightGazeDirectionFrozen.x = rightGazeDirection.x;
                    rightGazeDirectionFrozen.y = rightGazeDirection.y;
                    rightGazeDirectionFrozen.z = rightGazeDirection.z;

                    // sample[20] = signalerManager.invisibleObject.transform.position.x;
                    // sample[21] = signalerManager.invisibleObject.transform.position.y;
                    // sample[22] = signalerManager.invisibleObject.transform.position.z;

                    invisibleObjectPosFrozen.x = signalerManager.invisibleObject.transform.position.x;
                    invisibleObjectPosFrozen.y = signalerManager.invisibleObject.transform.position.y;
                    invisibleObjectPosFrozen.z = signalerManager.invisibleObject.transform.position.z;

                    headConstraintFrozen.x = headConstraint.transform.rotation.x;
                    headConstraintFrozen.y = headConstraint.transform.rotation.y;
                    headConstraintFrozen.z = headConstraint.transform.rotation.z;
                    headConstraintFrozen.w = headConstraint.transform.rotation.w;

                    
                    signalerManager.frozen = true;

                    frozenString = signalerManager.frozen.ToString();
                    lSLSignalerOutlets.lslOFrozenGaze.push_sample(new string[] {frozenString} );
                    Debug.LogError("Frozen local :" + signalerManager.frozen);

                }
            

            
        }
    }
}
