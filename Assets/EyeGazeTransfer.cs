using UnityEngine;
using ViveSR.anipal.Eye;

public class EyeGazeTransfer : MonoBehaviour
{
    public Transform[] EyesModels; // Assign the eye models of the target avatar
    //public SRanipal_AvatarEyeSample_v2 EyeSampleScript; // Reference to SRanipal_AvatarEyeSample_v2 script
        public GameObject hmd;
    public Transform OriginTransform;

    //  for displaying the eye movement of the signaler
    [SerializeField] private Transform combinedEyes;
    [SerializeField] private Transform leftEye;
    [SerializeField] private Transform rightEye;
    private Vector3 rayOrigin;
    private Vector3 rayDirection;
    public Vector3 eyePositionCombinedWorld;
    public Vector3 eyeDirectionCombinedWorld;
    public Quaternion eyeRotationCombinedWorld;
    void Update()
    {

            // Get gaze direction from SRanipal_AvatarEyeSample_v2
            
            SRanipal_Eye_v2.GetVerboseData(out VerboseData verboseData);
        eyePositionCombinedWorld = verboseData.combined.eye_data.gaze_origin_mm / 1000 + hmd.transform.position;
        Vector3 coordinateAdaptedGazeDirectionCombined = new Vector3(verboseData.combined.eye_data.gaze_direction_normalized.x * -1, verboseData.combined.eye_data.gaze_direction_normalized.y, verboseData.combined.eye_data.gaze_direction_normalized.z);


            // Apply gaze direction to target avatar's eye models
            for (int i = 0; i < EyesModels.Length; ++i)
            {
                Vector3 target = EyesModels[i].parent.TransformPoint(coordinateAdaptedGazeDirectionCombined);
                EyesModels[i].LookAt(target);
            }
        
    }
}
