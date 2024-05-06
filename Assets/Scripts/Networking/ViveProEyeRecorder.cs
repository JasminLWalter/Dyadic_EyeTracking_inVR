/*
 * Stefan Balle
 * sballe@uni-osnabrueck.de
 * Year: 2020
 */



using System.Collections;
using System.Collections.Generic;
using ViveSR.anipal.Eye;
using UnityEngine;

using System;
using System.IO;
using System.Linq;
using UnityEditor;
using Valve.VR;
using Valve.VR.InteractionSystem;
using Hand = Valve.VR.InteractionSystem.Hand;

// latency checks
using LSL4Unity;
using UnityEngine.UI;


public class ViveProEyeRecorder : MonoBehaviour, IEyetrackingDevice
{
    
    // Do ray cast for left and right individually as well
    [Header ("Settings")]
    public bool rayCastLeftAndRightEye;
    public int numberOfRaycastHitsToSave; // if set to 0 or lower, save all 
    
    // SteamVR
    [Header ("Referenced SteamVR entities")]
    public Hand steamVrLeftHand;
    public Hand steamVrRightHand;
    
    // Body
    [Header("NavMeshAgent entity")] 
    public GameObject playerBody;
    
    // Debug 
    [Header ("Debug")]
    public LineRenderer debugLineRendererLeft; 
    public LineRenderer debugLineRendererRight;
    public LineRenderer debugLineRendererCombined;
    public bool activateDebugLineRenderers;
   
    
    // BodyTracker
    private int bodyTrackerIndex;
    private GameObject bodyTracker;
    
    // Keep track of last timestamp of data point - edit variable is unnecessary
    // private double lastDataPointTimeStamp; 
    
    // Keep track of current trial data
    private ExperimentTrialData currentTrialData;
    
    // Sampling Rate, default 90Hz
    private float samplingRate = 90.0f;
    
    // Sampling interval in seconds 
    private float samplingInterval;
    
    // Store recorded data in memory until saving to disk
    private List<ExperimentTrialData> trials; 
    
    // Is recording? 
    private bool isRecording;
    
    
    // Calibration 
    private int numberOfCalibrationAttempts;
    private bool calibrationIsRunning;
    private bool calibrated;
    
    
   
    // latency Checks
    private Color32 colorOne;
    private Color32 colorTwo;
    private float referenceTime;
    private bool isPlaying;
    private bool colorChanged;
    private Renderer renderer1;
    public Image img;
    private const int LslChannelCount = 2;
    private Guid uuidColor;
    private const double NominalRate = liblsl.IRREGULAR_RATE;
    private const liblsl.channel_format_t LslChannelFormat = liblsl.channel_format_t.cf_int32;
    private liblsl.StreamInfo lslStreamInfoColor;
    private liblsl.StreamOutlet lslOutletColor;
    
    
    
    
    

    // Start is called before the first frame update
    void Start()
    {
        
        // Init saved trials
        trials = new List<ExperimentTrialData>();
        
        // Find body tracker 
        try
        {
            VRDeviceManager.Instance.GetBodyTracker(out bodyTrackerIndex, out bodyTracker);
            Debug.Log("[EyeTrackingRecorder] Found VRDeviceManager.");
        }
        catch (Exception e)
        {
            Debug.Log("[EyeTrackingRecorder] Could not find VRDeviceManager.");
        }

        // Debug 
        UpdateDebugLineRendererVisibility();
        
        //latency checks
        colorOne = Color.black;
        colorTwo = Color.white;
        referenceTime = 0.5f;
        isPlaying = false;
        colorChanged = false;
            
        uuidColor = Guid.NewGuid();
        liblsl.StreamInfo streamInfoColor = new liblsl.StreamInfo(
            "Diode",
            "Markers",
            LslChannelCount,
            NominalRate,
            LslChannelFormat,
            uuidColor.ToString());

        lslOutletColor = new liblsl.StreamOutlet(streamInfoColor);
    }
    
    
    // Update is called once per frame
    void Update()
    {
        /*
        // Activate DebugLineRenderers
        if (Input.GetKeyDown(KeyCode.L))
        {
            ToggleDebugLineRenderers();
        }
        */
        
    }

    // Update display of Debug LineRenderers 
    private void UpdateDebugLineRendererVisibility()
    {
        // Activate 
        if (activateDebugLineRenderers)
        {
            // Show LineRenderers 
            debugLineRendererLeft.gameObject.SetActive(true);
            debugLineRendererLeft.startWidth = .002f;
            debugLineRendererLeft.endWidth = .002f;
            debugLineRendererRight.gameObject.SetActive(true);
            debugLineRendererRight.startWidth = .002f;
            debugLineRendererRight.endWidth = .002f;
            debugLineRendererCombined.gameObject.SetActive(true);
            debugLineRendererCombined.startWidth = .002f;
            debugLineRendererCombined.endWidth = .002f;
        }
        
        // Deactivate
        else
        {
            // Hide 
            debugLineRendererLeft.gameObject.SetActive(false); 
            debugLineRendererRight.gameObject.SetActive(false);
            debugLineRendererCombined.gameObject.SetActive(false);
        }
    }

    // Switch visibility of Debug LineRenderers 
    public void ToggleDebugLineRenderers()
    {
        activateDebugLineRenderers = !activateDebugLineRenderers;
        UpdateDebugLineRendererVisibility();
    }
    
   
    
    // Launch eye tracker calibration
    public void CalibrateDevice()
    {
        Debug.Log("[EyeTrackingRecorder] Starting Eye Tracker Calibration.");
        
        // Increment number of calibration attempts 
        numberOfCalibrationAttempts += 1;
        
        // Keep track of calibration is running 
        calibrationIsRunning = true;
        
        // Launch 
        StartCoroutine("LaunchSRanipalCalibration");
    }
    
    
    // Coroutine of Launching Eye Calibration to prevent busy waiting
    private IEnumerator LaunchSRanipalCalibration()
    {
        
        // Calibration successful 
        if (SRanipal_Eye_v2.LaunchEyeCalibration())
        {
            Debug.Log("[EyeTrackingRecorder] Eye Tracker Calibration was successful.");
            
            // Update calibration status
            calibrated = true;
        }
        
        // Calibration did not succeed
        else
        {
            Debug.Log("[EyeTrackingRecorder] Eye Tracker Calibration did not succeed.");
            
            // Update calibration status
            calibrated = false;
        }
        
        // Keep track of calibration is running 
        calibrationIsRunning = false;

        yield break;
    }

    
  
    // Return whether eye tracker is calibrated
    public bool GetCalibrationStatus()
    {
        return calibrated;
    }

    
    // Indicate that the eye tracker needs to be calibrated again
    public void ResetEyeTrackingCalibration()
    {
        numberOfCalibrationAttempts = 0;
        calibrated = false;
    }
    

    // Set the eye tracking data sampling rate 
    public void SetSampleRateHertz(float rate)
    {
        // Update Sampling Rate 
        samplingRate = rate;
        Debug.Log("[EyeTrackingRecorder] Updated sampling rate to " + rate + "Hz.");
        samplingInterval = 1.0f / samplingRate;
    }


    // Return the current recording status  
    public bool GetRecordingStatus()
    {
        return isRecording;
    }

    // Write trials stored in memory to disk
    public void SaveRecordedData(string path)
    {
        // JSON serializer needs class to wrap list of trials 
        ExperimentSaveTrials saveTrials = new ExperimentSaveTrials();
        saveTrials.trials = trials;
        
        // Save 
        DataSavingManager.Instance.WriteSerializableDataToDisk(saveTrials,path,false);
        
        // Clear trial list after saving 
        trials = new List<ExperimentTrialData>();
    }
    


    // Start measurement
    public void StartRecording()
    {
        
        // TODO 
        int trialId = -42069;
        
        // Get Timestamp at start
        double timeStampAtStart = GetCurrentTimestampInSeconds();
        
        // Log 
        Debug.Log("[EyeTrackingRecorder] Starting measuring.");

        // Init new trial data 
        currentTrialData = new ExperimentTrialData();
        
        // Set data 
        currentTrialData.timeTrialMeasurementStarted = timeStampAtStart;
        currentTrialData.trialId = trialId;
        currentTrialData.someRandomInformation = "lel";
        
        
        // Reset gaze ray timestamp 
        // lastDataPointTimeStamp = 0;
        
        // Update recording status
        isRecording = true;
        
        // Start the actual measuring 
        StartCoroutine("RecordData");

    }
    
    
    // Stop measurement
    public void StopRecording()
    {
        // Get Timestamp at stop
        double timeStampAtStop = GetCurrentTimestampInSeconds();
        
        // Log
        Debug.Log("[EyeTrackingRecorder] Stopping Measuring.");
        
        // Stop the measurement coroutine
        StopCoroutine("RecordData");
        
        // Update recording status
        isRecording = false;
        
        // Set data 
        currentTrialData.timeTrialMeasurementStopped = timeStampAtStop;
        
        // Append trial data to list 
        trials.Add(currentTrialData);
    }
    
    
    // EEGStuff: Save RayCastHits
    void SaveRayCastHits(RaycastHit[] rayCastHits)
    {
        // lists to save information of hit objects
        List<string> hitObjectColliderNameL = new List<string>();
        List<float> hitPointOnObjectL = new List<float>();
        List<float> hitObjectColliderBoundsCenterL = new List<float>();
        // Go through each hit and add to list 
        foreach (RaycastHit hit in rayCastHits)
        {
            hitObjectColliderNameL.Add(hit.collider.name); 
            hitPointOnObjectL.Add(hit.point.x);
            hitPointOnObjectL.Add(hit.point.y);
            hitPointOnObjectL.Add(hit.point.z);
            hitObjectColliderBoundsCenterL.Add(hit.collider.bounds.center.x);
            hitObjectColliderBoundsCenterL.Add(hit.collider.bounds.center.y);
            hitObjectColliderBoundsCenterL.Add(hit.collider.bounds.center.z);
        }
        // fill up lists if I don't have the same number of hits
        for (int i = hitObjectColliderNameL.Count; i < 2; i++) hitObjectColliderNameL.Add("Empty");
        for (int i = hitPointOnObjectL.Count; i < 6; i++)
        {
            hitPointOnObjectL.Add(0);
            hitObjectColliderBoundsCenterL.Add(0);
        }

        string[] names = hitObjectColliderNameL.GetRange(0, 2).ToArray();
        float[] positionOnObject = hitPointOnObjectL.GetRange(0, 6).ToArray();
        float[] positions = hitObjectColliderBoundsCenterL.GetRange(0, 6).ToArray();
        
        LSLStreams.Instance.lslOHitObjectColliderNameL.push_sample(names);
        LSLStreams.Instance.lslOHitPointOnObject.push_sample(positionOnObject);
        LSLStreams.Instance.lslOhitObjectColliderBoundsCenter.push_sample(positions);
    }
    
    // Get a timestamp 
    private double GetCurrentTimestampInSeconds()
    {
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        return (System.DateTime.UtcNow - epochStart).TotalSeconds;
    }
    
    
    // Record Data 
    private IEnumerator RecordData()  // orig: RecordControllerTriggerAndPositionData
    {
        Debug.Log("[EyeTrackingRecorder] Started Coroutine to Record Data.");
        
        // Measure until stopped
        while (true)
        {
            // latency checks
            referenceTime -= Time.deltaTime;
            if (referenceTime < 0)
            {
                img.color = colorTwo;
                referenceTime = 0.5f;
            }
            else
            {
                img.color = colorOne;
            }
            colorChanged = img.color == colorTwo;
            var cFrame = Time.frameCount;
            var dataColor = new int[2];
            dataColor[0] = cFrame;
            dataColor[1] = colorChanged ? 1 : 0;
            lslOutletColor.push_sample(dataColor);
            //Debug.Log(referenceTime);
            
            
            
            
            
            // Create new data point for current measurement data point 
            ExperimentDataPoint dataPoint = new ExperimentDataPoint(); // orig: FrameData(), custom class do not confuse with Unity class  
            
            
            //// ** 
            // Add supplementary info to data point before raycasting 

            // TimeStamp at start 
            double timeAtStart = GetCurrentTimestampInSeconds();
            dataPoint.timeStampDataPointStart = timeAtStart;
            
            // HMD and Hand Transforms
            Transform hmdTransform = Player.instance.hmdTransform;;
            Transform leftHandTransform = steamVrLeftHand.transform; 
            Transform rightHandTransform = steamVrRightHand.transform; // right hand
        
            // Set Hand Data 
            dataPoint.handLeftPosition = leftHandTransform.position; 
            dataPoint.handLeftRotation = leftHandTransform.rotation.eulerAngles; 
            dataPoint.handLeftScale = leftHandTransform.lossyScale;
            dataPoint.handLeftDirectionForward = leftHandTransform.forward;
            dataPoint.handLeftDirectionRight = leftHandTransform.right;
            dataPoint.handLeftDirectionUp = leftHandTransform.up;
            dataPoint.handRightPosition = rightHandTransform.position; 
            dataPoint.handRightRotation = rightHandTransform.rotation.eulerAngles; 
            dataPoint.handRightScale = rightHandTransform.lossyScale;
            dataPoint.handRightDirectionForward = rightHandTransform.forward;
            dataPoint.handRightDirectionRight = rightHandTransform.right;
            dataPoint.handRightDirectionUp = rightHandTransform.up;
            
            // EEGStuff: HMD pos, dir, rot combined // EEGStuff: LSL
            float[] handPosDirRot =
            {
                dataPoint.handLeftPosition.x,
                dataPoint.handLeftPosition.y,
                dataPoint.handLeftPosition.z,
                dataPoint.handLeftDirectionForward.x,
                dataPoint.handLeftDirectionForward.y,
                dataPoint.handLeftDirectionForward.z,
                dataPoint.handLeftRotation.x,
                dataPoint.handLeftRotation.y,
                dataPoint.handLeftRotation.z,
                dataPoint.handRightPosition.x,
                dataPoint.handRightPosition.y,
                dataPoint.handRightPosition.z,
                dataPoint.handRightRotation.x,
                dataPoint.handRightRotation.y,
                dataPoint.handRightRotation.z,
                dataPoint.handRightDirectionForward.x,
                dataPoint.handRightDirectionForward.y,
                dataPoint.handRightDirectionForward.z
            };
            LSLStreams.Instance.lslOhandPosDirRot.push_sample(handPosDirRot);
            
            // Set HMD Data  
            dataPoint.hmdPosition = hmdTransform.position; 
            dataPoint.hmdDirectionForward = hmdTransform.forward; 
            dataPoint.hmdDirectionUp = hmdTransform.up; 
            dataPoint.hmdDirectionRight = hmdTransform.right; 
            dataPoint.hmdRotation = hmdTransform.rotation.eulerAngles; 
            
            // EEGStuff: HMD pos, dir, rot combined // EEGStuff: LSL
            float[] hmdPosDirRot =
            {
                dataPoint.hmdPosition.x,
                dataPoint.hmdPosition.y,
                dataPoint.hmdPosition.z,
                dataPoint.hmdDirectionForward.x,
                dataPoint.hmdDirectionForward.y,
                dataPoint.hmdDirectionForward.z,
                dataPoint.hmdDirectionUp.x,
                dataPoint.hmdDirectionUp.y,
                dataPoint.hmdDirectionUp.z,
                dataPoint.hmdDirectionRight.x,
                dataPoint.hmdDirectionRight.y,
                dataPoint.hmdDirectionRight.z,
                dataPoint.hmdRotation.x,
                dataPoint.hmdRotation.y,
                dataPoint.hmdRotation.z
            };
            LSLStreams.Instance.lslOhmdPosDirRot.push_sample(hmdPosDirRot);
            
            // Set Body data
            dataPoint.playerBodyPosition = playerBody.transform.position;

            // Set BodyTracker Data if available 
            if (bodyTracker != null)
            {
                dataPoint.bodyTrackerPosition = bodyTracker.transform.position;
                dataPoint.bodyTrackerRotation = bodyTracker.transform.rotation.eulerAngles;
                //Debug.Log(dataPoint.bodyTrackerPosition);
                //Debug.Log(dataPoint.bodyTrackerRotation);
            }
            // EEGStuff: LSL ; BodyTracker Pos, Dir combined
            float[] bodyTrackerPosRot =
            {
                dataPoint.bodyTrackerPosition.x,
                dataPoint.bodyTrackerPosition.y,
                dataPoint.bodyTrackerPosition.z,
                dataPoint.bodyTrackerRotation.x,
                dataPoint.bodyTrackerRotation.y,
                dataPoint.bodyTrackerRotation.z
            };
            LSLStreams.Instance.lslObodyTrackerPosRot.push_sample(bodyTrackerPosRot);
            
            // End of supplementary info
            //// ** 
            
            
            
            //// **
            // EyeData 
            
            // Time stamp before obtaining verbose data 
            double timeBeforeGetVerboseData = GetCurrentTimestampInSeconds();
            dataPoint.timeStampGetVerboseData = timeBeforeGetVerboseData;
            // EEGStuff: LSL
            double[] timeStampGetVerboseData =
            {
                dataPoint.timeStampGetVerboseData
            };
            LSLStreams.Instance.lslOTimeStampGetVerboseData.push_sample(timeStampGetVerboseData);
            
            
            // Obtain verbose data and later extract all relevant data from it 
            SRanipal_Eye_v2.GetVerboseData(out VerboseData verboseData); 
            
            """
            Extract gaze information for left, right and combined eye 
            
            verboseData's gaze_origin_mm has the same value as the ray's origin gotten through GetGazeRay, only multiplied by a factor of 1000 (millimeter vs meter) 
            verboseData's gaze_direction_normalized has the same value as the ray's direction gotten through GetGazeRay, only the x axis needs to be inverted (according to SRanipal Docs: verboseData's gaze has right handed coordinate system) 
            """;
            Vector3 coordinateAdaptedGazeDirectionCombined = new Vector3(verboseData.combined.eye_data.gaze_direction_normalized.x * -1,  verboseData.combined.eye_data.gaze_direction_normalized.y, verboseData.combined.eye_data.gaze_direction_normalized.z);
            dataPoint.eyePositionCombinedWorld = verboseData.combined.eye_data.gaze_origin_mm / 1000 + hmdTransform.position;
            dataPoint.eyeDirectionCombinedWorld = hmdTransform.rotation * coordinateAdaptedGazeDirectionCombined;
            dataPoint.eyeDirectionCombinedLocal = coordinateAdaptedGazeDirectionCombined;
            Vector3 coordinateAdaptedGazeDirectionLeft = new Vector3(verboseData.left.gaze_direction_normalized.x * -1,  verboseData.left.gaze_direction_normalized.y, verboseData.left.gaze_direction_normalized.z);
            dataPoint.eyePositionLeftWorld = verboseData.left.gaze_origin_mm / 1000 + hmdTransform.position;
            dataPoint.eyeDirectionLeftWorld = hmdTransform.rotation * coordinateAdaptedGazeDirectionLeft;
            dataPoint.eyeDirectionLeftLocal = coordinateAdaptedGazeDirectionLeft;
            Vector3 coordinateAdaptedGazeDirectionRight = new Vector3(verboseData.right.gaze_direction_normalized.x * -1,  verboseData.right.gaze_direction_normalized.y, verboseData.right.gaze_direction_normalized.z);
            dataPoint.eyePositionRightWorld = verboseData.right.gaze_origin_mm / 1000 + hmdTransform.position;
            dataPoint.eyeDirectionRightWorld = hmdTransform.rotation * coordinateAdaptedGazeDirectionRight;
            dataPoint.eyeDirectionRightLocal = coordinateAdaptedGazeDirectionRight;
            
            // EEGStuff: eyeTracking combined // EEGStuff: LSL
            float[] eyeTrackingPosDir =
            {
                dataPoint.eyePositionCombinedWorld.x,
                dataPoint.eyePositionCombinedWorld.y,
                dataPoint.eyePositionCombinedWorld.z,
                dataPoint.eyeDirectionCombinedWorld.x,
                dataPoint.eyeDirectionCombinedWorld.y,
                dataPoint.eyeDirectionCombinedWorld.z,
                dataPoint.eyeDirectionCombinedLocal.x,
                dataPoint.eyeDirectionCombinedLocal.y,
                dataPoint.eyeDirectionCombinedLocal.z,
                dataPoint.eyeDirectionLeftWorld.x,
                dataPoint.eyeDirectionLeftWorld.y,
                dataPoint.eyeDirectionLeftWorld.z,
                dataPoint.eyeDirectionRightWorld.x,
                dataPoint.eyeDirectionRightWorld.y,
                dataPoint.eyeDirectionRightWorld.z
            };
            LSLStreams.Instance.lslOEyeTrackingPosDir.push_sample(eyeTrackingPosDir);
            

            
            
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
                
                // EEGStuff: Save data for LSL
                SaveRayCastHits(raycastHitsCombined); //--> gives an error?
                
                // Make data serializable and save 
                dataPoint.rayCastHitsCombinedEyes = makeRayCastListSerializable(raycastHitsCombined);

                
                
                // Debug
                if (activateDebugLineRenderers)
                {
                    Debug.Log("[EyeTrackingRecorder] Combined eyes first hit: " + raycastHitsCombined[0].collider.name);
                    debugLineRendererCombined.SetPosition(0,dataPoint.eyePositionCombinedWorld);
                    debugLineRendererCombined.SetPosition(1, raycastHitsCombined[0].point);
                }
            }
            
            
            
            
            // ** If intended, ray cast for left and right eye individually as well 
            if (rayCastLeftAndRightEye)
            {
                
                // Raycast left eye, calculate all hits 
                RaycastHit[] raycastHitsLeft;
                raycastHitsLeft = Physics.RaycastAll(dataPoint.eyePositionLeftWorld, dataPoint.eyeDirectionLeftWorld,
                    Mathf.Infinity);

                // Make sure something was hit 
                if (raycastHitsLeft.Length > 0)
                {
                    // Sort by distance
                    raycastHitsLeft = raycastHitsLeft.OrderBy(x => x.distance).ToArray();
                    
                    // Use only the specified number of hits 
                    if (numberOfRaycastHitsToSave > 0)
                    {
                        raycastHitsLeft = raycastHitsLeft.Take(Math.Min(numberOfRaycastHitsToSave,raycastHitsLeft.Length)).ToArray();
                    }
                    
                    // Make data serializable and save 
                    dataPoint.rayCastHitsLeftEye = makeRayCastListSerializable(raycastHitsLeft);

                    // Debug
                    if (activateDebugLineRenderers)
                    {
                        Debug.Log("[EyeTrackingRecorder] Left eye first hit: " + raycastHitsLeft[0].collider.name);
                        debugLineRendererLeft.SetPosition(0,dataPoint.eyePositionLeftWorld);
                        debugLineRendererLeft.SetPosition(1, raycastHitsLeft[0].point);
                    }
                }
                
                // Raycast right eye, calculate all hits 
                RaycastHit[] raycastHitsRight;
                raycastHitsRight = Physics.RaycastAll(dataPoint.eyePositionRightWorld, dataPoint.eyeDirectionRightWorld,
                    Mathf.Infinity);

                // Make sure something was hit 
                if (raycastHitsRight.Length > 0)
                {
                    // Sort by distance
                    raycastHitsRight = raycastHitsRight.OrderBy(x => x.distance).ToArray();

                    // Use only the specified number of hits 
                    if (numberOfRaycastHitsToSave > 0)
                    {
                        raycastHitsRight = raycastHitsRight.Take(Math.Min(numberOfRaycastHitsToSave,raycastHitsRight.Length)).ToArray();
                    }
                    
                    // Make data serializable and save 
                    dataPoint.rayCastHitsRightEye = makeRayCastListSerializable(raycastHitsRight);

                    // Debug
                    if (activateDebugLineRenderers)
                    {
                       Debug.Log("[EyeTrackingRecorder] Right eye first hit: " + raycastHitsRight[0].collider.name);
                       debugLineRendererRight.SetPosition(0,dataPoint.eyePositionRightWorld);
                       debugLineRendererRight.SetPosition(1, raycastHitsRight[0].point);
                    }

                }
                
            }
            
            
            
            
            // Eye Openness
            dataPoint.eyeOpennessLeft = verboseData.left.eye_openness;
            dataPoint.eyeOpennessRight = verboseData.right.eye_openness;

            // EEGStuff: LSL
            float[] eyeOpennessLR =
            {
                verboseData.left.eye_openness,
                verboseData.right.eye_openness
            };
            LSLStreams.Instance.lslOEyeOpennessLR.push_sample(eyeOpennessLR);
            
            
            // Pupil Diameter
            dataPoint.pupilDiameterMillimetersLeft = verboseData.left.pupil_diameter_mm;
            dataPoint.pupilDiameterMillimetersRight = verboseData.right.pupil_diameter_mm;

            // EEGStuff: LSL
            float[] pupilDiameterLR =
            {
                verboseData.left.pupil_diameter_mm,
                verboseData.right.pupil_diameter_mm
            };
            LSLStreams.Instance.lslOPupilDiameterLR.push_sample(pupilDiameterLR);

            // Gaze validity
            dataPoint.leftGazeValidityBitmask = verboseData.left.eye_data_validata_bit_mask;
            dataPoint.rightGazeValidityBitmask = verboseData.right.eye_data_validata_bit_mask;
            dataPoint.combinedGazeValidityBitmask = verboseData.combined.eye_data.eye_data_validata_bit_mask;

            // EEGStuff: LSL
            float[] gazeValidityCLR =
            {
                verboseData.combined.eye_data.eye_data_validata_bit_mask,
                verboseData.left.eye_data_validata_bit_mask,
                verboseData.right.eye_data_validata_bit_mask
            };
            LSLStreams.Instance.lslOGazeValidityCLR.push_sample(gazeValidityCLR);
            
          
            // TimeStamp at end 
            double timeAtEnd = GetCurrentTimestampInSeconds();
            dataPoint.timeStampDataPointEnd = timeAtEnd;
            
            // End of EyeData 
            //// **
            
            
            
            
            // Add data point to current subject data 
            currentTrialData.dataPoints.Add(dataPoint);
            
            
            
            //// **
            // Wait time to meet sampling rate  
            
            double timeBeforeWait = GetCurrentTimestampInSeconds();
            
           
            // 
            // Check how much time needs to be waited to meet sampling rate measuring next data point
            // (If lastDataPointTimeStamp is not yet set, i.e. 0, timeBeforeWait will be greater than samplingInterval so no waiting will occur)  

            // Computation was faster than sampling rate, i.e. wait to match sampling rate
            // Else: Computation was slower, i.e. continue directly with next data point 
            // if ((timeBeforeWait - lastDataPointTimeStamp) < samplingInterval) 
            if ((timeBeforeWait - timeAtStart) < samplingInterval) 
            {
                // Debug.Log("waiting for " + (float)(samplingInterval - (timeBeforeWait - lastDataPointTimeStamp)));
                // Debug.Log(getCurrentTimestamp());

                // Wait for seconds that fill time to meet sampling interval 
                yield return new WaitForSeconds((float)(samplingInterval - (timeBeforeWait - timeAtStart)));
            }

            

            // Debug.Log("Real Framerate: " + 1 / (timeBeforeWait - lastDataPointTimeStamp));
            
            // Update last time stamp 
           //  lastDataPointTimeStamp = timeBeforeWait;
            
            // Wait time End
            //// ** 
            
            

        }

        yield break;  // coroutine stops, when loop breaks 


    }


    
    // Transforms a list of raycast hits into a serializable list 
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
            
            //Debug.Log(hit.collider.name);
            ordinal += 1;
        }

        return serilizableList;

    }
    
    
    
}


// Make RayCastHits Serializable
[Serializable]
public struct SerializableRayCastHit
{
    public Vector3 hitPointOnObject;
    public string hitObjectColliderName;
    public Vector3 hitObjectColliderBoundsCenter;
    public int ordinalOfHit; // starts at 1 
}



// Struct is OBSOLETE! 
// Refer here for naming and meaning of values though 
// Bitmask has order as values are listed below: Top value is right and low value is left in bitmask
// Only bits 1 and 2 cannot be discerned clearly, their order in the bitmask might be swapped, all others are verified 
// Validity of gaze 
// All valid in base 10: Left eye is 31, right eye is 31, combined eye is 3 
[Serializable]
public struct GazeValidity
{
    public bool SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY; /** The validity of the origin of gaze of the eye data */ // Bit 1 or 2 in mask
    public bool SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY; /** The validity of the direction of gaze of the eye data */ // Bit 2 or 1 in mask
    public bool SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY; /** The validity of the diameter of gaze of the eye data */ // Bit 3 in mask
    public bool SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY;  /** The validity of normalized position of pupil */ // Bit 4 in mask
    public bool SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY; /** The validity of the openness of the eye data */ // Bit 5 in mask 
    public ulong bitmask; // should be order as above  
    public bool allValid; 
    public bool originAndDirectionValid; // combined gaze seems to only have origin and direction set to valid 
}



// Store list of trials in class for serialization to work 
[Serializable]
public class ExperimentSaveTrials
{
    // Data points per current trial
    public List<ExperimentTrialData> trials;
    
    // Constructor
    public ExperimentSaveTrials()
    {
        // Automatically create list of experiment data points 
        trials = new List<ExperimentTrialData>();
    }
}



// Holds all data per measured trial 
[Serializable]
public class ExperimentTrialData 
{
    
    public int trialId;
    public string someRandomInformation;

    public double timeTrialMeasurementStarted;
    public double timeTrialMeasurementStopped; 
    
    
    // Data points per current trial
    public List<ExperimentDataPoint> dataPoints;
    
    // Constructor
    public ExperimentTrialData()
    {
        // Automatically create list of experiment data points 
        dataPoints = new List<ExperimentDataPoint>();
    }
}


// Holds data of one data point  
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
    public Vector3 eyeDirectionCombinedLocal;
    public Vector3 eyePositionLeftWorld;
    public Vector3 eyeDirectionLeftWorld;
    public Vector3 eyeDirectionLeftLocal;
    public Vector3 eyePositionRightWorld;
    public Vector3 eyeDirectionRightWorld;
    public Vector3 eyeDirectionRightLocal;
    public ulong leftGazeValidityBitmask;
    public ulong rightGazeValidityBitmask;
    public ulong combinedGazeValidityBitmask;
    
    // GazeRay hit object 
    public List<SerializableRayCastHit> rayCastHitsCombinedEyes;
    public List<SerializableRayCastHit> rayCastHitsLeftEye;
    public List<SerializableRayCastHit> rayCastHitsRightEye;
    
    // HMD 
    public Vector3 hmdPosition;
    public Vector3 hmdDirectionForward;
    public Vector3 hmdDirectionRight;
    public Vector3 hmdRotation;
    public Vector3 hmdDirectionUp;
    
    // Hands
    public Vector3 handLeftPosition;
    public Vector3 handLeftRotation;
    public Vector3 handLeftScale;
    public Vector3 handLeftDirectionForward;
    public Vector3 handLeftDirectionRight;
    public Vector3 handLeftDirectionUp;
    public Vector3 handRightPosition;
    public Vector3 handRightRotation;
    public Vector3 handRightScale;
    public Vector3 handRightDirectionForward;
    public Vector3 handRightDirectionRight;
    public Vector3 handRightDirectionUp;
    
    // Body
    public Vector3 playerBodyPosition; 
    
    // Body Tracker 
    public Vector3 bodyTrackerPosition;
    public Vector3 bodyTrackerRotation;


}


/*
 
 //
 // Previous implementation
 // 
 
 
 // Get the rays
            SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out Ray rayCombineEye);
            SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out Ray rayLeftEye);
            SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out Ray rayRightEye);
            

            dataPoint.eyePositionCombinedWorld = hmdTransform.position + rayCombineEye.origin; // ray origin is at transform of hmd + offset 
            dataPoint.eyeDirectionCombinedWorld = hmdTransform.rotation * rayCombineEye.direction; // ray direction is local, so multiply with hmd transform to get world direction 
            dataPoint.eyeDirectionCombinedLocal = rayCombineEye.direction;
            
            
//Debug.Log("before: "  + rayCombineEye.origin.ToString("F4") + " " + rayCombineEye.direction.ToString("F4") + " " + dataPoint.eyePositionCombinedWorld.ToString("F4") + " " + dataPoint.eyeDirectionCombinedWorld.ToString("F4"));
//Debug.Log("after: " + verboseData.combined.eye_data.gaze_origin_mm.ToString("F4") + " " + verboseData.combined.eye_data.gaze_direction_normalized.ToString("F4") + " " + 
//         (verboseData.combined.eye_data.gaze_origin_mm / 1000 + hmdTransform.position).ToString("F4") + " " + (hmdTransform.rotation * verboseData.combined.eye_data.gaze_direction_normalized).ToString("F4") );


 // Raycast Left Eye
                // Get Eye Position and Gaze Direction 
               
                
                dataPoint.eyePositionLeftWorld = hmdTransform.position + rayLeftEye.origin; // ray origin is at transform of hmd + offset 
                dataPoint.eyeDirectionLeftWorld = hmdTransform.rotation * rayLeftEye.direction; // ray direction is local, so multiply with hmd transform to get world direction 
                dataPoint.eyeDirectionRightLocal = rayLeftEye.direction;
                
                
                
//Debug.Log("before: "  + rayLeftEye.origin.ToString("F4") + " " + rayLeftEye.direction.ToString("F4") + " " + dataPoint.eyePositionLeftWorld.ToString("F4") + " " + dataPoint.eyeDirectionLeftWorld.ToString("F4"));
//Debug.Log("after: " + verboseData.left.gaze_origin_mm.ToString("F4") + " " + verboseData.left.gaze_direction_normalized.ToString("F4") + " " + 
//          (verboseData.left.gaze_origin_mm / 1000 + hmdTransform.position).ToString("F4") + " " + (hmdTransform.rotation * verboseData.left.gaze_direction_normalized).ToString("F4") );



// Raycast Right Eye
                // Get Eye Position and Gaze Direction 
                
                
                dataPoint.eyePositionRightWorld =
                    hmdTransform.position + rayRightEye.origin; // ray origin is at transform of hmd + offset 
                dataPoint.eyeDirectionRightWorld =
                    hmdTransform.rotation *
                    rayRightEye
                        .direction; // ray direction is local, so multiply with hmd transform to get world direction 
                dataPoint.eyeDirectionRightLocal = rayRightEye.direction;
                
                
                
                
// Debug.Log("before: "  + rayRightEye.origin.ToString("F4") + " " + rayRightEye.direction.ToString("F4") + " " + dataPoint.eyePositionRightWorld.ToString("F4") + " " + dataPoint.eyeDirectionRightWorld.ToString("F4"));
//  Debug.Log("after: " + verboseData.right.gaze_origin_mm.ToString("F4") + " " + verboseData.right.gaze_direction_normalized.ToString("F4") + " " + 
//         (verboseData.right.gaze_origin_mm / 1000 + hmdTransform.position).ToString("F4") + " " + (hmdTransform.rotation * verboseData.right.gaze_direction_normalized).ToString("F4") );



// Gaze validitiy


            
            dataPoint.leftGazeValidity = new GazeValidity
            {
               SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY = verboseData.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY), 
               SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY = verboseData.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY),
               SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY = verboseData.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY),
               SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY = verboseData.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY),
               SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY = verboseData.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY),
               allValid = verboseData.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY) & verboseData.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY) & verboseData.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY) & verboseData.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY) & verboseData.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY),
               originAndDirectionValid = verboseData.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY) & verboseData.left.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY)
               //bitmask = verboseData.left.eye_data_validata_bit_mask
            };
            dataPoint.rightGazeValidity = new GazeValidity
            {
                SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY = verboseData.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY), 
                SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY = verboseData.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY),
                SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY = verboseData.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY),
                SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY = verboseData.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY),
                SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY = verboseData.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY),
                allValid = verboseData.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY) & verboseData.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY) & verboseData.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY) & verboseData.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY) & verboseData.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY),
                originAndDirectionValid = verboseData.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY) & verboseData.right.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY)
                //bitmask = verboseData.right.eye_data_validata_bit_mask
            };
            dataPoint.combinedGazeValidity= new GazeValidity
            {
                SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY = verboseData.combined.eye_data.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY), 
                SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY = verboseData.combined.eye_data.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY),
                SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY = verboseData.combined.eye_data.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY),
                SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY = verboseData.combined.eye_data.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY),
                SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY = verboseData.combined.eye_data.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY),
                allValid = verboseData.combined.eye_data.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY) & verboseData.combined.eye_data.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY) & verboseData.combined.eye_data.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY) & verboseData.combined.eye_data.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY) & verboseData.combined.eye_data.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY),
                originAndDirectionValid = verboseData.combined.eye_data.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY) & verboseData.combined.eye_data.GetValidity(SingleEyeDataValidity.SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY)
                //bitmask = verboseData.combined.eye_data.eye_data_validata_bit_mask
            };
            


            // Gaze validity logging and mask comparison 
            
            Debug.Log(verboseData.combined.eye_data.GetValidity(SingleEyeDataValidity
                .SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY) + " SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY combined"); // POSITION MIGHT BE SWITCHED WITH SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY
            Debug.Log(verboseData.combined.eye_data.GetValidity(SingleEyeDataValidity
                          .SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY) + " SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY combined"); // POSITION MIGHT BE SWITCHED WITH SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY
            Debug.Log(verboseData.combined.eye_data.GetValidity(SingleEyeDataValidity
                          .SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY) + " SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY combined"); // POSITION BY MEANS OF EXCLUSION OK! 
            Debug.Log(verboseData.combined.eye_data.GetValidity(SingleEyeDataValidity
                          .SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY) + " SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY combined"); // POSITION OK! 
            Debug.Log(verboseData.combined.eye_data.GetValidity(SingleEyeDataValidity
                .SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY) + " SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY combined"); // POSITION OK!
            Debug.Log(Convert.ToString((long)verboseData.combined.eye_data.eye_data_validata_bit_mask, 2) + " bitmask combined");
            
            
            
            // Positions in bitmask from right (top) to left (bottom) 
            Debug.Log(verboseData.right.GetValidity(SingleEyeDataValidity
                          .SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY) + " SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY right"); // POSITION MIGHT BE SWITCHED WITH SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY
            Debug.Log(verboseData.right.GetValidity(SingleEyeDataValidity
                          .SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY) + " SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY right"); // POSITION MIGHT BE SWITCHED WITH SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY
            Debug.Log(verboseData.right.GetValidity(SingleEyeDataValidity
                          .SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY) + " SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY right"); // POSITION BY MEANS OF EXCLUSION OK! 
            Debug.Log(verboseData.right.GetValidity(SingleEyeDataValidity
                          .SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY) + " SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY right"); // POSITION OK! 
            Debug.Log(verboseData.right.GetValidity(SingleEyeDataValidity
                          .SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY) + " SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY right");  // POSITION OK!
            Debug.Log(Convert.ToString((long)verboseData.right.eye_data_validata_bit_mask, 2) + " bitmask right");
            
         
            
            Debug.Log(verboseData.left.GetValidity(SingleEyeDataValidity
                          .SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY) + " SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY left"); // POSITION MIGHT BE SWITCHED WITH SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY
            Debug.Log(verboseData.left.GetValidity(SingleEyeDataValidity
                          .SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY) + " SINGLE_EYE_DATA_GAZE_DIRECTION_VALIDITY left"); // POSITION MIGHT BE SWITCHED WITH SINGLE_EYE_DATA_GAZE_ORIGIN_VALIDITY
            Debug.Log(verboseData.left.GetValidity(SingleEyeDataValidity
                          .SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY) + " SINGLE_EYE_DATA_PUPIL_DIAMETER_VALIDITY left"); // POSITION BY MEANS OF EXCLUSION OK! 
            Debug.Log(verboseData.left.GetValidity(SingleEyeDataValidity
                          .SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY) + " SINGLE_EYE_DATA_PUPIL_POSITION_IN_SENSOR_AREA_VALIDITY left"); // POSITION OK! 
            Debug.Log(verboseData.left.GetValidity(SingleEyeDataValidity
                          .SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY) + " SINGLE_EYE_DATA_EYE_OPENNESS_VALIDITY right");  // POSITION OK!
            Debug.Log(Convert.ToString((long)verboseData.left.eye_data_validata_bit_mask, 2) + " bitmask left");

    



 */

