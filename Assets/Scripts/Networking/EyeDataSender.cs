using UnityEngine;
using LSL;
using System.Collections.Generic;
using ViveSR.anipal.Eye;

public class EyeDataSender : MonoBehaviour
{
    private StreamOutlet outlet;
    private Dictionary<EyeShape_v2, float> eyeWeightings = new Dictionary<EyeShape_v2, float>();
    private EyeData_v2 eyeData = new EyeData_v2();

    void Start()
    {
        StreamInfo streamInfo = new StreamInfo("EyeTracking", "Gaze", 16, 0, channel_format_t.cf_float32, "eyeTracking12345");
        outlet = new StreamOutlet(streamInfo);
    }

    void Update()
    {
        if (SRanipal_Eye_v2.GetEyeWeightings(out eyeWeightings))
        {
            Vector3 leftGazeDirection, rightGazeDirection;
            Vector3 gazeOrigin;

            // Extract gaze direction
            SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out gazeOrigin, out leftGazeDirection);
            SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out gazeOrigin, out rightGazeDirection);

            // Get blink weightings
            float leftBlink = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Blink) ? eyeWeightings[EyeShape_v2.Eye_Left_Blink] : 0.0f;
            float rightBlink = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Blink) ? eyeWeightings[EyeShape_v2.Eye_Right_Blink] : 0.0f;

            float leftWide = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Wide) ? eyeWeightings[EyeShape_v2.Eye_Left_Wide] : 0.0f;
            float rightWide = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Wide) ? eyeWeightings[EyeShape_v2.Eye_Right_Wide] : 0.0f;
            float leftSqueeze = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Left_Squeeze) ? eyeWeightings[EyeShape_v2.Eye_Left_Squeeze] : 0.0f;
            float rightSqueeze = eyeWeightings.ContainsKey(EyeShape_v2.Eye_Right_Squeeze) ? eyeWeightings[EyeShape_v2.Eye_Right_Squeeze] : 0.0f;

            // Prepare LSL sample
            float[] sample = new float[16];
            sample[0] = leftGazeDirection.x;
            sample[1] = leftGazeDirection.y;
            sample[2] = leftGazeDirection.z;
            sample[3] = rightGazeDirection.x;
            sample[4] = rightGazeDirection.y;
            sample[5] = rightGazeDirection.z;
            sample[6] = leftBlink;
            sample[7] = rightBlink;
            sample[8] = leftWide;
            sample[9] = rightWide;
            sample[10] = leftSqueeze;
            sample[11] = rightSqueeze;

            Debug.Log("Sending Sample: " + string.Join(", ", sample));

            // Include additional eye weightings if necessary
            int index = 12;
            foreach (var weighting in eyeWeightings)
            {
                sample[index] = weighting.Value;
                index++;
                if (index >= 16) break; // Ensure we do not exceed the sample size
            }

            // Send sample via LSL
            outlet.push_sample(sample);
        }
    }
}
