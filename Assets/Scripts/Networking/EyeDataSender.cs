using UnityEngine;
using LSL;
using System.Collections.Generic;
using ViveSR.anipal.Eye;

public class EyeDataSender : MonoBehaviour
{
    private StreamOutlet outlet;
    private Dictionary<EyeShape_v2, float> eyeWeightings = new Dictionary<EyeShape_v2, float>();
    private EyeData_v2 eyeData = new EyeData_v2();

    private SignalerManager signalerManager;
    // private GameObject invisibleObject;
    public Transform headConstraint;

    void Start()
    {
        StreamInfo streamInfo = new StreamInfo("EyeTracking", "Gaze", 27, 0, channel_format_t.cf_float32, "eyeTracking12345");
        outlet = new StreamOutlet(streamInfo);
        signalerManager = FindObjectOfType<SignalerManager>();
    }

    void Update()
    {
        if (SRanipal_Eye_v2.GetEyeWeightings(out eyeWeightings))
        {
            // invisibleObject = signalerManager.invisibleObject;
            
            Vector3 leftGazeDirection, rightGazeDirection, combinedGazeDirection;
            Vector3 gazeOrigin;

            // Extract gaze direction
            // SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out gazeOrigin, out leftGazeDirection);
            SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out gazeOrigin, out rightGazeDirection);
            SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out gazeOrigin, out combinedGazeDirection);

            // Get blink weightings
            float leftBlink = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Blink) ? eyeWeightings[EyeShape_v2.Eye_Left_Blink] : 0.0f;
            float rightBlink = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Blink) ? eyeWeightings[EyeShape_v2.Eye_Right_Blink] : 0.0f;

            float leftWide = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Wide) ? eyeWeightings[EyeShape_v2.Eye_Left_Wide] : 0.0f;
            float rightWide = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Wide) ? eyeWeightings[EyeShape_v2.Eye_Right_Wide] : 0.0f;
            float leftSqueeze = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Squeeze) ? eyeWeightings[EyeShape_v2.Eye_Left_Squeeze] : 0.0f;
            float rightSqueeze = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Squeeze) ? eyeWeightings[EyeShape_v2.Eye_Right_Squeeze] : 0.0f;

            float eye_Left_Up = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Up) ? eyeWeightings[EyeShape_v2.Eye_Left_Up] : 0.0f;
            float eye_Left_Down = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Down) ? eyeWeightings[EyeShape_v2.Eye_Left_Down] : 0.0f;
            float eye_Left_Left =  eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Left) ? eyeWeightings[EyeShape_v2.Eye_Left_Left] : 0.0f;  
            float eye_Left_Right = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Right) ? eyeWeightings[EyeShape_v2.Eye_Left_Right] : 0.0f;

            float eye_Right_Up = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Up) ? eyeWeightings[EyeShape_v2.Eye_Right_Up] : 0.0f;
            float eye_Right_Down = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Down) ? eyeWeightings[EyeShape_v2.Eye_Right_Down] : 0.0f;   
            float eye_Right_Left = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Left) ? eyeWeightings[EyeShape_v2.Eye_Right_Left] : 0.0f;
            float eye_Right_Right = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Right) ? eyeWeightings[EyeShape_v2.Eye_Right_Right] : 0.0f;
            
            // Prepare LSL sample
            //Debug.Log("Invisible Object: " + signalerManager.invisibleObject.transform.position);
            Debug.Log("Head Constraint: " + headConstraint.position);

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

            
            sample[20] = signalerManager.invisibleObject.transform.position.x;
            sample[21] = signalerManager.invisibleObject.transform.position.y;
            sample[22] = signalerManager.invisibleObject.transform.position.z;

            sample[23] = headConstraint.transform.rotation.x;
            sample[24] = headConstraint.transform.rotation.y;
            sample[25] = headConstraint.transform.rotation.z;
            sample[26] = headConstraint.transform.rotation.w;

            Debug.Log("Sending Sample: " + string.Join(", ", sample));

            // Include additional eye weightings if necessary
           /* int index = 19;
            
            foreach (var weighting in eyeWeightings)
            {
                sample[index] = weighting.Value;
                index++;
                if (index >= 20) break; // Ensure we do not exceed the sample size
            }*/

            // Send sample via LSL
            outlet.push_sample(sample);
        }
    }
}
