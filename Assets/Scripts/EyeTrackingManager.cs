using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using System.Timers;
using UnityEditor.Rendering;
using UnityEngine;
//using Valve.VR;
using ViveSR.anipal;
using ViveSR.anipal.Eye;
//using Valve.VR.InteractionSystem;
//using Hand = Valve.VR.InteractionSystem.Hand;

public class EyeTrackingManager : MonoBehaviour
{
    
    public static EyeTrackingManager Instance { get; private set; }
    //    private bool showGazeSphere = true;





    [SerializeField] private GameObject mainCamera;
 



    #region Recording Variables

    // Do ray cast for left and right individually as well
    [Header ("Settings")]
    public bool rayCastLeftAndRightEye;
    public int numberOfRaycastHitsToSave; // if set to 0 or lower, save all 
    
    // SteamVR

    
    // Body
    [Header("NavMeshAgent entity")] 
    // public GameObject playerBody;
    
    // Debug 
    [Header ("Debug")]
    // public LineRenderer debugLineRendererLeft; 
    // public LineRenderer debugLineRendererRight;
    // public LineRenderer debugLineRendererCombined;
    // public bool activateDebugLineRenderers;
   
    
    // BodyTracker
    // private int bodyTrackerIndex;
    // private GameObject bodyTracker;
    
    // Keep track of last timestamp of data point 
    private double lastDataPointTimeStamp; 
    
    // Keep track of current trial data
    // private ExperimentTrialData currentTrialData;
    
    // Sampling Rate, default 90Hz
    private float samplingRate = 90.0f;
    
    // Sampling interval in seconds 
    private float samplingInterval;
    
    // Store recorded data in memory until saving to disk
    // private List<ExperimentTrialData> trials; 
    
    // Is recording? 
    private bool isRecording;
    
   

    // gaze spheres
    public float localGazeSpherePosition;
    public GameObject localGazeSphere;

    #endregion
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
       // localGazeSphere = GameObject.Find("local_gazeSphere");
        

    }

    // Update is called once per frame
    void Update()
    {


        
    }

    #region RecordingData


    public void StartRecording()
    {
        StartCoroutine(RecordData());
    }

    public void EndRecording()
    {
        StopCoroutine(RecordData());
    }

    // Get a timestamp 
    private double GetCurrentTimestampInSeconds()
    {
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        return (System.DateTime.UtcNow - epochStart).TotalSeconds;
    }
    
    
    // Holds data of one data point  

    // Record Data 
    private IEnumerator RecordData()  // orig: RecordControllerTriggerAndPositionData
    {
        Debug.Log("[EyeTrackingRecorder] Started Coroutine to Record Data.");
        
        // Measure until stopped
        while (true)
        {
            Debug.Log("Run RecordData");
            // Create new data point for current measurement data point 
            ExperimentDataPoint dataPoint = new ExperimentDataPoint(); // orig: FrameData(), custom class do not confuse with Unity class  
            
            
            //// ** 
            // Add supplementary info to data point before raycasting 

            // TimeStamp at start 
            double timeAtStart = GetCurrentTimestampInSeconds();
            dataPoint.timeStampDataPointStart = timeAtStart;
            
            // HMD and Hand Transforms
            Transform hmdTransform = GameObject.Find("Camera(eye)").transform; //Player.instance.hmdTransform;;



            // Set HMD Data  
            dataPoint.hmdPosition = hmdTransform.position; 
            dataPoint.hmdDirectionForward = hmdTransform.forward; 
            dataPoint.hmdDirectionUp = hmdTransform.up; 
            dataPoint.hmdDirectionRight = hmdTransform.right; 
            dataPoint.hmdRotation = hmdTransform.rotation.eulerAngles; 
            
            // Set Body data
            // dataPoint.playerBodyPosition = playerBody.transform.position;

            // EyeData 
            
            // Time stamp before obtaining verbose data 
            double timeBeforeGetVerboseData = GetCurrentTimestampInSeconds();
            dataPoint.timeStampGetVerboseData = timeBeforeGetVerboseData;
            
            // Obtain verbose data and later extract all relevant data from it 
            SRanipal_Eye_v2.GetVerboseData(out VerboseData verboseData); 
            
            // Extract gaze information for left, right and combined eye 
            //
            // verboseData's gaze_origin_mm has the same value as the ray's origin gotten through GetGazeRay, only multiplied by a factor of 1000 (millimeter vs meter) 
            // verboseData's gaze_direction_normalized has the same value as the ray's direction gotten through GetGazeRay, only the x axis needs to be inverted (according to SRanipal Docs: verboseData's gaze has right handed coordinate system) 
            // 
            Vector3 coordinateAdaptedGazeDirectionCombined = new Vector3(verboseData.combined.eye_data.gaze_direction_normalized.x * -1,  verboseData.combined.eye_data.gaze_direction_normalized.y, verboseData.combined.eye_data.gaze_direction_normalized.z);
            dataPoint.eyePositionCombinedWorld = verboseData.combined.eye_data.gaze_origin_mm / 1000 + hmdTransform.position;
            dataPoint.eyeDirectionCombinedWorld = hmdTransform.rotation * coordinateAdaptedGazeDirectionCombined;
            // dataPoint.eyeDirectionCombinedLocal = coordinateAdaptedGazeDirectionCombined;
            Vector3 coordinateAdaptedGazeDirectionLeft = new Vector3(verboseData.left.gaze_direction_normalized.x * -1,  verboseData.left.gaze_direction_normalized.y, verboseData.left.gaze_direction_normalized.z);
            //dataPoint.eyePositionLeftWorld = verboseData.left.gaze_origin_mm / 1000 + hmdTransform.position;
            //dataPoint.eyeDirectionLeftWorld = hmdTransform.rotation * coordinateAdaptedGazeDirectionLeft;
            //dataPoint.eyeDirectionLeftLocal = coordinateAdaptedGazeDirectionLeft;
            Vector3 coordinateAdaptedGazeDirectionRight = new Vector3(verboseData.right.gaze_direction_normalized.x * -1,  verboseData.right.gaze_direction_normalized.y, verboseData.right.gaze_direction_normalized.z);
            //dataPoint.eyePositionRightWorld = verboseData.right.gaze_origin_mm / 1000 + hmdTransform.position;
            //dataPoint.eyeDirectionRightWorld = hmdTransform.rotation * coordinateAdaptedGazeDirectionRight;
            //dataPoint.eyeDirectionRightLocal = coordinateAdaptedGazeDirectionRight;

            // RaycastHit firstHit;
            // if (Physics.Raycast(dataPoint.eyePositionCombinedWorld, dataPoint.eyeDirectionCombinedWorld, out firstHit, Mathf.Infinity))
            // {
            //     localGazeSphere.transform.position = firstHit.point;
            //
            // }




            // Raycast combined eyes 
            RaycastHit[] raycastHitsCombined;
            raycastHitsCombined = Physics.RaycastAll(dataPoint.eyePositionCombinedWorld, dataPoint.eyeDirectionCombinedWorld,Mathf.Infinity);
            
            // Make sure something was hit 
            if (raycastHitsCombined.Length > 0)
            {
                // Sort by distance
                raycastHitsCombined = raycastHitsCombined.OrderBy(x=>x.distance).ToArray();
                
                // Use only the specified number of hits 
                if (numberOfRaycastHitsToSave > 0)
                {
                    raycastHitsCombined = raycastHitsCombined.Take(Math.Min(numberOfRaycastHitsToSave,raycastHitsCombined.Length)).ToArray();
                }
                
                // Make data serializable and save 
                dataPoint.rayCastHitsCombinedEyes = makeRayCastListSerializable(raycastHitsCombined);
                
                // // Debug
                // if (activateDebugLineRenderers)
                // {
                //     Debug.Log("[EyeTrackingRecorder] Combined eyes first hit: " + raycastHitsCombined[0].collider.name);
                //     debugLineRendererCombined.SetPosition(0,dataPoint.eyePositionCombinedWorld);
                //     debugLineRendererCombined.SetPosition(1, raycastHitsCombined[0].point);
                // }
            }
            
            
            
            //RaycastHits
            
     
            
            // Eye Openness
            dataPoint.eyeOpennessLeft = verboseData.left.eye_openness;
            dataPoint.eyeOpennessRight = verboseData.right.eye_openness;
            
            // Pupil Diameter
            dataPoint.pupilDiameterMillimetersLeft = verboseData.left.pupil_diameter_mm;
            dataPoint.pupilDiameterMillimetersRight = verboseData.right.pupil_diameter_mm;

            // Gaze validity
            dataPoint.leftGazeValidityBitmask = verboseData.left.eye_data_validata_bit_mask;
            dataPoint.rightGazeValidityBitmask = verboseData.right.eye_data_validata_bit_mask;
            dataPoint.combinedGazeValidityBitmask = verboseData.combined.eye_data.eye_data_validata_bit_mask;

            
          
            // TimeStamp at end 
            double timeAtEnd = GetCurrentTimestampInSeconds();
            dataPoint.timeStampDataPointEnd = timeAtEnd;
            
            // End of EyeData 
            //// **
            
            
            
            
            // Add data point to current subject data 
            // currentTrialData.dataPoints.Add(dataPoint);
            
            
            
            //// **
            // Wait time to meet sampling rate  
            
            double timeBeforeWait = GetCurrentTimestampInSeconds();
            
           
            // 
            // Check how much time needs to be waited to meet sampling rate measuring next data point
            // (If lastDataPointTimeStamp is not yet set, i.e. 0, timeBeforeWait will be greater than samplingInterval so no waiting will occur)  

            // Computation was faster than sampling rate, i.e. wait to match sampling rate
            // Else: Computation was slower, i.e. continue directly with next data point 
            if ((timeBeforeWait - lastDataPointTimeStamp) < samplingInterval) 
            {
                // Debug.Log("waiting for " + (float)(samplingInterval - (timeBeforeWait - lastDataPointTimeStamp)));
                // Debug.Log(getCurrentTimestamp());

                // Wait for seconds that fill time to meet sampling interval 
                yield return new WaitForSeconds((float)(samplingInterval - (timeBeforeWait - lastDataPointTimeStamp)));
            }

            

            // Debug.Log("Real Framerate: " + 1 / (timeBeforeWait - lastDataPointTimeStamp));
            
            // Update last time stamp 
            lastDataPointTimeStamp = GetCurrentTimestampInSeconds();
            
            // Wait time End
            //// ** 
            
            

        }

        yield break;  // coroutine stops, when loop breaks 


    }


    
    private List<SerializableRayCastHit> makeRayCastListSerializable(RaycastHit[] rayCastHits)
    {
        List<SerializableRayCastHit> serilizableList = new List<SerializableRayCastHit>();

        // Keep track of the number of the hit 
        int ordinal = 1;
        
        // Go through each hit and add to list 
        foreach (RaycastHit hit in rayCastHits)
        {
            serilizableList.Add(new SerializableRayCastHit {
                hitPointOnObject = hit.point,
                hitObjectColliderName = hit.collider.name,
                hitObjectColliderBoundsCenter = hit.collider.bounds.center,
                ordinalOfHit = ordinal
            });

            ordinal += 1;
        }
        
        return serilizableList;

    }

    #endregion

    
    public double GetCurrentUnixTimeStamp()
    {
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        return (System.DateTime.UtcNow - epochStart).TotalSeconds;
    }
}

#region Serializable

// Make RayCastHits Serializable
[Serializable]
public struct SerializableRayCastHit
{
    public Vector3 hitPointOnObject;
    public string hitObjectColliderName;
    public Vector3 hitObjectColliderBoundsCenter;
    public int ordinalOfHit; // starts at 1 
}


[Serializable]
public class ExperimentDataPoint
{
    // TimeStamps 
    public double timeStampDataPointStart;
    public double timeStampDataPointEnd;
    public double timeStampGetVerboseData;

    // EyeTracking 
    public float eyeOpennessLeft;
    public float eyeOpennessRight;
    public float pupilDiameterMillimetersLeft;
    public float pupilDiameterMillimetersRight;
    public Vector3 eyePositionCombinedWorld;
    public Vector3 eyeDirectionCombinedWorld;
    //public Vector3 eyeDirectionCombinedLocal;
    //public Vector3 eyePositionLeftWorld;
    //public Vector3 eyeDirectionLeftWorld;
    //public Vector3 eyeDirectionLeftLocal;
    //public Vector3 eyePositionRightWorld;
    //public Vector3 eyeDirectionRightWorld;
    //public Vector3 eyeDirectionRightLocal;
    public ulong leftGazeValidityBitmask;
    public ulong rightGazeValidityBitmask;
    public ulong combinedGazeValidityBitmask;
    
    // GazeRay hit object 
    public List<SerializableRayCastHit> rayCastHitsCombinedEyes;
    
    // HMD 
    public Vector3 hmdPosition;
    public Vector3 hmdDirectionForward;
    public Vector3 hmdDirectionRight;
    public Vector3 hmdRotation;
    public Vector3 hmdDirectionUp;


}

#endregion