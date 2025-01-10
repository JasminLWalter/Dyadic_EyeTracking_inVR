 using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using ViveSR.anipal.Eye;

public class EyetrackingValidation : MonoBehaviour
{
    public static EyetrackingValidation Instance { get; private set; }

    #region Fields
    public bool startCal = false;  //  For initializing calibration from editor
    public bool startVal = false;  // For initializing validation from editor
    public bool validationFinished = false;
    

    [Space] [Header("Eye-tracker validation field")]
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject fixationPoint;
    [SerializeField] private List<Vector3> keyPositions;
    public GameManager gameManager;
    [SerializeField] private DisableVRHeadMovement disableHeadMovement;
    public LSLReceiverOutlets lslReceiverOutlets;
    public LSLSignalerOutlets lslSignalerOutlets;

    private bool _isValidationRunning;
    private bool _isErrorCheckRunning;
   // private bool _isExperiment;
    
    private int _validationId;
    private int _calibrationFreq;
    public int valCalCounter = 0;

    private string _participantId;
    private string _sessionId;

  //  private Instructions _instructions;
    private Coroutine _runValidationCo;
    private Coroutine _runErrorCheckCo;
    private Transform _hmdTransform;
    private List<EyeValidationData> _eyeValidationDataFrames;
    private EyeValidationData _eyeValidationData;
    private const float ErrorThreshold = 1.0f;

    private Vector3 rayOrigin = new Vector3();
    private Vector3 rayDirection = new Vector3();

    private InputBindings _inputBindings;

    #endregion

    #region Private methods

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        _inputBindings = new InputBindings();
        _inputBindings.UI.Enable();

        fixationPoint.SetActive(false);
        _eyeValidationDataFrames = new List<EyeValidationData>();

        disableHeadMovement = FindObjectOfType<DisableVRHeadMovement>();
    }
    /*private void SaveValidationFile()
    {
        var fileName = _participantId + "_EyeValidation_" + Instance.GetCurrentUnixTimeStamp();

        DataSavingManager.Instance.SaveList(_eyeValidationDataFrames, fileName);
        
    } */

    private void Update()
    {

        
        if (startCal || _inputBindings.UI.Calibration.triggered)
        {
            SRanipal_Eye_v2.LaunchEyeCalibration();
            startCal = false;
        }

        if (startVal || _inputBindings.UI.Validation.triggered) { 
            ValidateEyeTracking();
            
            startVal = false;
        }

        if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out rayOrigin, out rayDirection))
        {
            Debug.DrawRay(Camera.main.transform.position, rayDirection * 100, Color.blue);
        }

        

    }

    public Vector3 GetValidationError()
    {
        return _eyeValidationData.EyeValidationError;
    }

    private IEnumerator ValidateEyeTracker(float delay=2)
    {
        if (_isValidationRunning) yield break;

        _isValidationRunning = true;

        // Disable head movement 
        disableHeadMovement.DisableHeadMovement();

        _validationId++;

        valCalCounter++;

        fixationPoint.transform.SetParent(null, true);

        fixationPoint.transform.parent = mainCamera.gameObject.transform;

        

        _hmdTransform = Camera.main.transform;
        Debug.Log("Initial camera position: " + _hmdTransform.position);
        fixationPoint.transform.position = _hmdTransform.position + _hmdTransform.rotation * new Vector3(0,0,30);

        fixationPoint.transform.LookAt(_hmdTransform);
        
        Debug.Log("Initial fixation point position: " + fixationPoint.transform.position);

        yield return new WaitForSeconds(.15f);
        
        fixationPoint.SetActive(true);

        yield return new WaitForSeconds(delay);
        
        var anglesX = new List<float>();
        var anglesY = new List<float>();
        var anglesZ = new List<float>();
        
        for (var i = 1; i < keyPositions.Count; i++)
        {
            var startTime = Time.time;
            float timeDiff = 0;

            while (timeDiff < 1f)
            {
                fixationPoint.transform.position = _hmdTransform.position + _hmdTransform.rotation * Vector3.Lerp(keyPositions[i-1], keyPositions[i], timeDiff / 1f);   
                fixationPoint.transform.LookAt(_hmdTransform);
                Debug.Log("Fixation point position during movement: " + fixationPoint.transform.position);
                yield return new WaitForEndOfFrame();
                timeDiff = Time.time - startTime;
            }
            
            // _validationPointIdx = i;
            startTime = Time.time;
            timeDiff = 0;
            
            while (timeDiff < 2f)
            {
                fixationPoint.transform.position = _hmdTransform.position + _hmdTransform.rotation * keyPositions[i] ;
                fixationPoint.transform.LookAt(_hmdTransform);
                Debug.Log("Fixation point position during hold: " + fixationPoint.transform.position);

                EyeValidationData validationData = GetEyeValidationData();
                
                if (validationData != null)
                {
                    anglesX.Add(validationData.CombinedEyeAngleOffset.x);
                    anglesY.Add(validationData.CombinedEyeAngleOffset.y);
                    anglesZ.Add(validationData.CombinedEyeAngleOffset.z);
                    
                    validationData.EyeValidationError.x = CalculateValidationError(anglesX);
                    validationData.EyeValidationError.y = CalculateValidationError(anglesY);
                    validationData.EyeValidationError.z = CalculateValidationError(anglesZ);

                    _eyeValidationData = validationData;

                    Debug.Log("ValError x" + validationData.EyeValidationError.x);
                    Debug.Log("ValError y" + validationData.EyeValidationError.y);
                    Debug.Log("ValError z" + validationData.EyeValidationError.z);

                    float[] valErrorArray = new float[3];
                    valErrorArray[0] = validationData.EyeValidationError.x;
                    valErrorArray[1] = validationData.EyeValidationError.y;
                    valErrorArray[2] = validationData.EyeValidationError.z;

                    if (gameManager.role == "receiver")

                    {
                        lslReceiverOutlets.lslOValidationError.push_sample(valErrorArray);
                    }	

                    if (gameManager.role == "signaler")
                    {
                        lslSignalerOutlets.lslOValidationError.push_sample(valErrorArray);
                    }
                }
                
                yield return new WaitForEndOfFrame();
                timeDiff = Time.time - startTime;
                
            }
        }

        fixationPoint.transform.position = Vector3.zero;

        _isValidationRunning = false;
        
        fixationPoint.transform.parent = gameObject.transform;


        

        _eyeValidationDataFrames.Add(_eyeValidationData);
        // SaveValidationFile();

  

        fixationPoint.SetActive(false);

        // give feedback whether the error was too large or not
        if (CalculateValidationError(anglesX) > ErrorThreshold || 
            CalculateValidationError(anglesY) > ErrorThreshold ||
            CalculateValidationError(anglesZ) > ErrorThreshold ||
            _eyeValidationData.EyeValidationError == Vector3.zero)
        {
 
            gameManager.SetValidationSuccessStatus(false);

          }
          else
          {
            gameManager.SetValidationSuccessStatus(true); //originally GameManager.Instance.SetValidationSuccessStatus(true)
        }

        // Re-enable head movement
        disableHeadMovement.EnableHeadMovement();

        gameManager.ReturnToCurrentPhase();
    }
    
    private IEnumerator CheckErrorEyeTracker(float delay=5)
    {
        if (_isErrorCheckRunning) yield break;
        _isErrorCheckRunning = true;

        _validationId++;
        
        fixationPoint.transform.parent = mainCamera.gameObject.transform;

        _hmdTransform = Camera.main.transform;

        fixationPoint.transform.position = _hmdTransform.position + _hmdTransform.rotation * new Vector3(0,0,45);

        fixationPoint.transform.LookAt(_hmdTransform);

        
        yield return new WaitForSeconds(.15f);

        fixationPoint.SetActive(true);

        yield return new WaitForSeconds(delay);
        
        var anglesX = new List<float>();
        var anglesY = new List<float>();
        var anglesZ = new List<float>();
        
        EyeValidationData validationData = GetEyeValidationData();
            
        if (validationData != null)
        {
            anglesX.Add(validationData.CombinedEyeAngleOffset.x);
            anglesY.Add(validationData.CombinedEyeAngleOffset.y);
            anglesZ.Add(validationData.CombinedEyeAngleOffset.z);
                    
            validationData.EyeValidationError.x = CalculateValidationError(anglesX);
            validationData.EyeValidationError.y = CalculateValidationError(anglesY);
            validationData.EyeValidationError.z = CalculateValidationError(anglesZ);

            _eyeValidationData = validationData;
        }

        fixationPoint.transform.position = Vector3.zero;

        _isErrorCheckRunning = false;
        
        fixationPoint.transform.parent = gameObject.transform;

        Debug.LogError( "Get validation error" + GetValidationError() + " + " + _eyeValidationData.EyeValidationError);
       

        _eyeValidationDataFrames.Add(_eyeValidationData);
       // SaveValidationFile();


        fixationPoint.SetActive(false);
        
        // give feedback whether the error was too large or not
        if (CalculateValidationError(anglesX) > ErrorThreshold || 
            CalculateValidationError(anglesY) > ErrorThreshold ||
            CalculateValidationError(anglesZ) > ErrorThreshold ||
            _eyeValidationData.EyeValidationError == Vector3.zero)
        {
 
            gameManager.SetValidationSuccessStatus(false);

          }
          else
          {
            gameManager.SetValidationSuccessStatus(true);
        }
    }
    
    private EyeValidationData GetEyeValidationData()
    {
        EyeValidationData eyeValidationData = new EyeValidationData();
        
        EyeData_v2 eye_data = new EyeData_v2();
        Vector3 origin = new Vector3();
        Vector3 direction = new Vector3();


        eyeValidationData.UnixTimestamp = Instance.GetCurrentUnixTimeStamp();
        eyeValidationData.IsErrorCheck = _isErrorCheckRunning;
        
        eyeValidationData.ParticipantID = _participantId;
        eyeValidationData.ValidationID = _validationId;
        eyeValidationData.CalibrationFreq = _calibrationFreq;
        
        eyeValidationData.PointToFocus = fixationPoint.transform.position;
        
        if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.LEFT, out origin, out direction))
        {
            Ray ray = new Ray(origin, direction);
            var angles = Quaternion.FromToRotation((fixationPoint.transform.position - _hmdTransform.position).normalized, _hmdTransform.rotation * ray.direction)
                .eulerAngles;
            
            eyeValidationData.LeftEyeAngleOffset = angles;
        }
        
        if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.RIGHT, out origin, out direction))
        {
            Ray ray = new Ray(origin, direction);
            var angles = Quaternion.FromToRotation((fixationPoint.transform.position - _hmdTransform.position).normalized, _hmdTransform.rotation * ray.direction)
                .eulerAngles;

            eyeValidationData.RightEyeAngleOffset = angles;
        }
        
        if (SRanipal_Eye_v2.GetGazeRay(GazeIndex.COMBINE, out origin, out direction))
        {
            Ray ray = new Ray(origin, direction);
            var angles = Quaternion.FromToRotation((fixationPoint.transform.position - _hmdTransform.position).normalized, _hmdTransform.rotation * ray.direction)
                .eulerAngles;

            eyeValidationData.CombinedEyeAngleOffset = angles;
            rayOrigin = origin;
            rayDirection = direction;
        }

        return eyeValidationData;
    }
    
    private float CalculateValidationError(List<float> angles)
    {
        return angles.Select(f => f > 180 ? Mathf.Abs(f - 360) : Mathf.Abs(f)).Sum() / angles.Count;
    }
    
    #endregion

    #region Public methods

    public void ValidateEyeTracking()
    {
        if(!_isValidationRunning) _runValidationCo = StartCoroutine(ValidateEyeTracker());
    }
    
    public void CheckErrorEyeTracking()
    {
        if(!_isErrorCheckRunning) _runErrorCheckCo = StartCoroutine(CheckErrorEyeTracker());
    }

    public void SetParticipantId(string id)
    {
        _participantId = id;

    }
    
    public void SetSessionId(string id)
    {
        _sessionId = id;
    }
    
    public void NotifyCalibrationFrequency()
    {
        _calibrationFreq++;
    }

    public double GetCurrentUnixTimeStamp()
    {
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        return (System.DateTime.UtcNow - epochStart).TotalSeconds;
    }

    #endregion
}
