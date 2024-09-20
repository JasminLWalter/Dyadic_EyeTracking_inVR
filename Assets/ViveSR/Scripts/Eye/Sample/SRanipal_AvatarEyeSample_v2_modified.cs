using UnityEngine;
using LSL;
using System.Collections.Generic;
using System.Collections;
using ViveSR.anipal.Eye;

namespace ViveSR.anipal.Eye
{
    public class SRanipal_AvatarEyeSample_v2_modified : MonoBehaviour
    {
        [SerializeField] private Transform[] EyesModels = new Transform[0];
        [SerializeField] private List<EyeShapeTable_v2> EyeShapeTables;
        [SerializeField] private AnimationCurve EyebrowAnimationCurveUpper;
        [SerializeField] private AnimationCurve EyebrowAnimationCurveLower;
        [SerializeField] private AnimationCurve EyebrowAnimationCurveHorizontal;

        private Dictionary<EyeShape_v2, float> EyeWeightings = new Dictionary<EyeShape_v2, float>();
        private AnimationCurve[] EyebrowAnimationCurves = new AnimationCurve[(int)EyeShape_v2.Max];
        private GameObject[] EyeAnchors;
        private const int NUM_OF_EYES = 2;

        // LSL variables
        private StreamInlet inlet;
        private float[] sample = new float[27];
        public GameObject invisibleObjectSecondary;
        public GameObject headConstraintSecondary;
        public GameManager gameManager;
        public float sampleRate = 1000f;
        public float sampleInterval;

        private void Start()
        {
            SetEyesModels(EyesModels[0], EyesModels[1]);
            SetEyeShapeTables(EyeShapeTables);

            AnimationCurve[] curves = new AnimationCurve[(int)EyeShape_v2.Max];
            for (int i = 0; i < EyebrowAnimationCurves.Length; ++i)
            {
                if (i == (int)EyeShape_v2.Eye_Left_Up || i == (int)EyeShape_v2.Eye_Right_Up) curves[i] = EyebrowAnimationCurveUpper;
                else if (i == (int)EyeShape_v2.Eye_Left_Down || i == (int)EyeShape_v2.Eye_Right_Down) curves[i] = EyebrowAnimationCurveLower;
                else curves[i] = EyebrowAnimationCurveHorizontal;
            }
            SetEyeShapeAnimationCurves(curves);

            // StartCoroutine(ProcessEyeTrackingData());

            // invisibleObjectSecondary = GameObject.Find("invisibleObjectSecondary").GetComponent<invisibleObjectSecondary>();
        }

        private void Update()
        {

            if(inlet == null){
            // Initialize LSL inlet
            if(gameManager.role == "signaler"){
                StreamInfo[] results = LSL.LSL.resolve_stream("name", "EyeTrackingReceiver",1,0.0);
                inlet = new StreamInlet(results[0]);
            }
            if(gameManager.role == "receiver"){
                StreamInfo[] results = LSL.LSL.resolve_stream("name", "EyeTrackingSignaler",1,0.0);
                inlet = new StreamInlet(results[0]);
            }
            
            }
            // Receive data from LSL
            inlet.pull_sample(sample, 1.0f);

            Vector3 combinedGazeDirection = new Vector3(sample[0], sample[1], sample[2]);
            Vector3 rightGazeDirection = new Vector3(sample[3], sample[4], sample[5]);
            bool leftBlink = sample[6] > 0.5f;
            bool rightBlink = sample[7] > 0.5f;

            invisibleObjectSecondary.transform.position = new Vector3(sample[20], sample[21], sample[22]);
            headConstraintSecondary.transform.rotation =  new Quaternion(sample[23], sample[24], sample[25],sample[26]);
            //Debug.Log("invisibleObjectSecondary Position: " + invisibleObjectSecondary.transform.position);
            
            // Debug.Log("Left Gaze Direction: " + leftGazeDirection);
            // Debug.Log("Right Gaze Direction: " + rightGazeDirection);
            // Update gaze and eye shapes based on received data
            UpdateGazeRay(combinedGazeDirection);
            UpdateEyeShapes(leftBlink, rightBlink, sample);
        }

        public void SetEyesModels(Transform leftEye, Transform rightEye)
        {
            if (leftEye != null && rightEye != null)
            {
                EyesModels = new Transform[NUM_OF_EYES] { leftEye, rightEye };
                DestroyEyeAnchors();
                CreateEyeAnchors();
            }
        }

        public void SetEyeShapeTables(List<EyeShapeTable_v2> eyeShapeTables)
        {
            bool valid = true;
            if (eyeShapeTables == null)
            {
                valid = false;
            }
            else
            {
                for (int table = 0; table < eyeShapeTables.Count; ++table)
                {
                    if (eyeShapeTables[table].skinnedMeshRenderer == null)
                    {
                        valid = false;
                        break;
                    }
                    for (int shape = 0; shape < eyeShapeTables[table].eyeShapes.Length; ++shape)
                    {
                        EyeShape_v2 eyeShape = eyeShapeTables[table].eyeShapes[shape];
                        if (eyeShape > EyeShape_v2.Max || eyeShape < 0)
                        {
                            valid = false;
                            break;
                        }
                    }
                }
            }
            if (valid)
                EyeShapeTables = eyeShapeTables;
        }

        public void SetEyeShapeAnimationCurves(AnimationCurve[] eyebrowAnimationCurves)
        {
            if (eyebrowAnimationCurves.Length == (int)EyeShape_v2.Max)
                EyebrowAnimationCurves = eyebrowAnimationCurves;
        }

        // public void UpdateGazeRay(Vector3 leftGazeDirection, Vector3 rightGazeDirection)
        public void UpdateGazeRay(Vector3 combinedGazeDirection)

        {
            for (int i = 0; i < EyesModels.Length; ++i)
            {
                // Vector3 target = EyeAnchors[i].transform.TransformPoint(i == 0 ? leftGazeDirection : rightGazeDirection);
                Vector3 target = EyeAnchors[i].transform.TransformPoint(combinedGazeDirection);
                EyesModels[i].LookAt(target);
            }
        }

        public void UpdateEyeShapes(bool leftBlink, bool rightBlink, float[] sample)
        {
            foreach (var table in EyeShapeTables)
            {
                for (int i = 0; i < table.eyeShapes.Length; ++i)
                {
                    EyeShape_v2 eyeShape = table.eyeShapes[i];
                    if (eyeShape == EyeShape_v2.Eye_Left_Blink)
                    {
                        table.skinnedMeshRenderer.SetBlendShapeWeight(i, sample[6]);
                    }
                    else if (eyeShape == EyeShape_v2.Eye_Right_Blink)
                    {
                        table.skinnedMeshRenderer.SetBlendShapeWeight(i, sample[7]);
                    }
                    else if(eyeShape == EyeShape_v2.Eye_Left_Wide)
                    {
                        table.skinnedMeshRenderer.SetBlendShapeWeight(i, sample[8]);
                    }
                    else if(eyeShape == EyeShape_v2.Eye_Right_Wide)
                    {
                        table.skinnedMeshRenderer.SetBlendShapeWeight(i, sample[9]);
                    }
                    else if(eyeShape == EyeShape_v2.Eye_Left_Squeeze)
                    {
                        table.skinnedMeshRenderer.SetBlendShapeWeight(i, sample[10]);
                    }
                    else if(eyeShape == EyeShape_v2.Eye_Right_Squeeze)
                    {
                        table.skinnedMeshRenderer.SetBlendShapeWeight(i, sample[11]);
                    }
                    else if(eyeShape == EyeShape_v2.Eye_Left_Up){
                        table.skinnedMeshRenderer.SetBlendShapeWeight(i, sample[12]);
                    } 
                    else if(eyeShape == EyeShape_v2.Eye_Left_Down){
                        table.skinnedMeshRenderer.SetBlendShapeWeight(i, sample[13]);
                    }
                    else if(eyeShape == EyeShape_v2.Eye_Left_Left){
                        table.skinnedMeshRenderer.SetBlendShapeWeight(i, sample[14]);
                    }
                    else if(eyeShape == EyeShape_v2.Eye_Left_Right){
                        table.skinnedMeshRenderer.SetBlendShapeWeight(i, sample[15]);
                    }
                    else if(eyeShape == EyeShape_v2.Eye_Right_Up){
                        table.skinnedMeshRenderer.SetBlendShapeWeight(i, sample[16]);
                    }
                    else if(eyeShape == EyeShape_v2.Eye_Right_Down){
                        table.skinnedMeshRenderer.SetBlendShapeWeight(i, sample[17]);
                    }
                    else if(eyeShape == EyeShape_v2.Eye_Right_Left){
                        table.skinnedMeshRenderer.SetBlendShapeWeight(i, sample[18]);
                    }
                    else if(eyeShape == EyeShape_v2.Eye_Right_Right){
                        table.skinnedMeshRenderer.SetBlendShapeWeight(i, sample[19]);
                    }
                }

            }
        }

        private void DestroyEyeAnchors()
        {
            if (EyeAnchors != null)
            {
                foreach (var anchor in EyeAnchors)
                {
                    if (anchor != null) Destroy(anchor);
                }
            }
        }

        private void CreateEyeAnchors()
        {
            EyeAnchors = new GameObject[NUM_OF_EYES];
            for (int i = 0; i < EyesModels.Length; ++i)
            {
                EyeAnchors[i] = new GameObject("EyeAnchor_" + i);
                EyeAnchors[i].transform.SetParent(EyesModels[i], false);
                EyeAnchors[i].transform.localPosition = Vector3.zero;
                EyeAnchors[i].transform.localRotation = Quaternion.identity;
            }
        }
    }
}
