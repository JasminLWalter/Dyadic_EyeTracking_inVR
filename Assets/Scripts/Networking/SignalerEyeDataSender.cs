using UnityEngine;
using LSL;
using System.Collections.Generic;
using ViveSR.anipal.Eye;
using TMPro;

public class SignalerEyeDataSender : MonoBehaviour
{
    private StreamOutlet outlet;
    private Dictionary<EyeShape_v2, float> eyeWeightings = new Dictionary<EyeShape_v2, float>();
    private EyeData_v2 eyeData = new EyeData_v2();

    public SignalerManager signalerManager;
    private ReceiverManager receiverManager;
    private GameManager gameManager;
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
    private string frozenString;

    public GameObject wait;
    public GameObject yourTurn;

    
    public TMP_Text TextFixedGaze;


    public LSLSignalerOutlets lSLSignalerOutlets;
    void Start()
    {
        StreamInfo streamInfo = new StreamInfo("EyeTrackingSignaler", "Gaze", 27, 0, channel_format_t.cf_float32);
        
        // description
        streamInfo.desc().append_child("combinedGazeDirection.x");
        streamInfo.desc().append_child("combinedGazeDirection.y");
        streamInfo.desc().append_child("combinedGazeDirection.z");
        streamInfo.desc().append_child("rightGazeDirection.x");
        streamInfo.desc().append_child("rightGazeDirection.y");
        streamInfo.desc().append_child("rightGazeDirection.z");
        streamInfo.desc().append_child("leftBlink");
        streamInfo.desc().append_child("rightBlink");
        streamInfo.desc().append_child("leftWide");
        streamInfo.desc().append_child("rightWide");
        streamInfo.desc().append_child("leftSqueeze");
        streamInfo.desc().append_child("rightSqueeze");

        streamInfo.desc().append_child("eye_Left_Up");
        streamInfo.desc().append_child("eye_Left_Down");
        streamInfo.desc().append_child("eye_Left_Left");
        streamInfo.desc().append_child("eye_Left_Right");
        streamInfo.desc().append_child("eye_Right_Up");
        streamInfo.desc().append_child("eye_Right_Down");
        streamInfo.desc().append_child("eye_Right_Left");
        streamInfo.desc().append_child("eye_Right_Right");

        streamInfo.desc().append_child("invisibleObjectPos.x");
        streamInfo.desc().append_child("invisibleObjectPos.y");
        streamInfo.desc().append_child("invisibleObjectPos.z");

        streamInfo.desc().append_child("headConstraintPos.x");
        streamInfo.desc().append_child("headConstraintPos.y");
        streamInfo.desc().append_child("headConstraintPos.z");
        streamInfo.desc().append_child("headConstraintPos.w");
        
        outlet = new StreamOutlet(streamInfo);
        

        receiverManager = FindObjectOfType<ReceiverManager>();
        gameManager = FindObjectOfType<GameManager>();

        _inputBindings = new InputBindings();
        _inputBindings.Player.Enable();

        wait.gameObject.SetActive(false);
        yourTurn.gameObject.SetActive(false);

        TextFixedGaze.gameObject.SetActive(false);
    }

    void Update()
    {
        frozenString = gameManager.frozen.ToString();  // will be sent via LSL
        
        if (SRanipal_Eye_v2.GetEyeWeightings(out eyeWeightings))
        {
            Vector3 gazeOrigin;
           

                if(gameManager.frozen == false)
                {
                    wait.gameObject.SetActive(false);
                    yourTurn.gameObject.SetActive(true);

                    SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out gazeOrigin, out rightGazeDirection);
                    SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out gazeOrigin, out combinedGazeDirection);

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
                    
                    invisibleObjectPos.x = signalerManager.invisibleObjectSignaler.transform.position.x;
                    invisibleObjectPos.y = signalerManager.invisibleObjectSignaler.transform.position.y;
                    invisibleObjectPos.z = signalerManager.invisibleObjectSignaler.transform.position.z;    
                    
                }
                else if(gameManager.frozen)
                {
                    if (signalerManager.freezeCounter > 1)
                    {
                        wait.gameObject.SetActive(true);
                        yourTurn.gameObject.SetActive(false);
                    }
                    
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

                }
                // Prepare LSL sample
                float[] sample = new float[27];
    
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

                // Send sample via LSL
                outlet.push_sample(sample);

                if(_inputBindings.Player.Freeze.triggered && signalerManager.freezeCounter > 1 && !gameManager.frozen)
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
                    
                   
                    rightGazeDirectionFrozen.x = rightGazeDirection.x;
                    rightGazeDirectionFrozen.y = rightGazeDirection.y;
                    rightGazeDirectionFrozen.z = rightGazeDirection.z;

                    invisibleObjectPosFrozen.x = signalerManager.invisibleObjectSignaler.transform.position.x;
                    invisibleObjectPosFrozen.y = signalerManager.invisibleObjectSignaler.transform.position.y;
                    invisibleObjectPosFrozen.z = signalerManager.invisibleObjectSignaler.transform.position.z;

                    
                    gameManager.FreezeSignaler();

                    frozenString = gameManager.frozen.ToString();
                    lSLSignalerOutlets.lslOFrozenGaze.push_sample(new string[] {frozenString} );
                }
            

            
        }
    }
}
