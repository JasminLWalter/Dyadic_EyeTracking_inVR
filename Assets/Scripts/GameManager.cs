using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UIElements;
using VIVE.OpenXR;
using ViveSR.anipal.Eye;
using System;
public class GameManager : MonoBehaviour
{
    public bool debugging = false;

    [Tooltip("Stores the current condition of the experiment.")]
    [SerializeField] private int condition = 1;  // what condition is meant here? freeze versus free moving condition or milky versus clear glass condition?
    [Tooltip("Phase 0: Welcome & Instruction Embodiment (UI Space); Phase 1: Embodiment (Embodiment Space); Phase 2: Instruction Testing (UI Space); Phase 3: Testing Phase (Testing Space); Phase 4: End Phase (UI Space)")]
    public int phase = 0;

    private ReceiverManager receiverManager;

    private EyetrackingValidation eyetrackingValidation;
    private SignalerManager signalerManager;
    private EmbodimentManager embodimentManager;
    private BoxBehaviour boxBehaviour;
    private MenuManager menuManager;

    public VRRig vRRig;
    public GameObject xrOriginSetup;
    public GameObject signaler;

    public GameObject receiver;
    public GameObject avatarMain;
    public GameObject avatarSecondary;
    public GameObject invisibleObject;
    public GameObject trainingSign;
    public GameObject trainingSignReceiver;
    private InputBindings _inputBindings;
    
    private int score = 0;
    private List<int> shuffledRewards;
    public string role;
    [SerializeField] private TextMeshProUGUI  scoreDisplay;
    [SerializeField] private TextMeshProUGUI roundsDisplay;
    [SerializeField] private TextMeshProUGUI  scoreDisplayReceiver;
    [SerializeField] private TextMeshProUGUI roundsDisplayReceiver;
    [SerializeField] private GameObject TimeExceededTMP;

    [SerializeField] private GameObject TimeExceededTMPReceiver;

    [Tooltip("Must be an even number.")]
    [SerializeField] private int roundsPerCondition;
    public int _currentRound = 0; //only public for debugging
    public bool _startedRound = false;
    private bool _selected = false;
    public bool firstFreeze = false; // Why is this variable needed?
    public bool firstFreezeReceiver = false;  // Why is this variable needed?
    [SerializeField] public List<GameObject> boxes = null;
    

    public int trialNumber = 0;
    public int trialFailedCount = 0;

    Vector3 pauseRoomSignaler = new Vector3();
    Vector3 pauseRoomReceiver = new Vector3();

    private bool firstSelectionMade = false;

    private int countdownTime = 3;
    public TextMeshProUGUI  countdownText;

    public TextMeshProUGUI  countdownTextReceiver;
    public TextMeshProUGUI  timerCountdownText;
    public TextMeshProUGUI  timerCountdownTextReceiver;
    public GameObject milkyGlass;
    public GameObject clearGlass;

    [SerializeField] private List<Vector3> spaceLocationsReceiver = null;
    [SerializeField] private List<Vector3> spaceLocationsSignaler = null;

    [Tooltip("There should be as many rewards as there are inner boxes.")]
    [SerializeField] private List<int> rewards;

    private bool _ValidationSuccessStatus = true; 
    
    public bool TimeExceeded = false;
    public bool running = false;
    public bool calDoneOneTime = false;
    

    public float _timeLimit = 3;
    private float startTime;
    private float _startRoundTime = 0;

    private float probabilityForMilky = 0.5f;

    public LSLReceiverOutlets lslReceiverOutlets;
    public bool countdownRunning = false;
    public bool milkyGlassBool;
    // Stores the boolean values for the clear or milky glass for all the trial rounds
    private bool[] milkyBools;
    private bool trainingEnd = false;
    private int calCounter = 0;

    public bool frozen = false;
    public bool previousFrozen = false;
    public AudioSource soundEffect;

    // Try to fix the frame rate to 90 fps
    void Awake() {
        Application.targetFrameRate = 90;
    }

    void Start()
    {
        _inputBindings = new InputBindings();
        _inputBindings.Player.Enable();

        _inputBindings = new InputBindings();
        _inputBindings.UI.Enable();

        startTime = Time.time;
        menuManager = FindObjectOfType<MenuManager>();
        signalerManager = FindObjectOfType<SignalerManager>();
        receiverManager = FindObjectOfType<ReceiverManager>();
        embodimentManager = FindObjectOfType<EmbodimentManager>();
        eyetrackingValidation = FindObjectOfType<EyetrackingValidation>();
        boxBehaviour = FindObjectOfType<BoxBehaviour>();
        lslReceiverOutlets = avatarMain.GetComponent<LSLReceiverOutlets>();

        trainingSign.gameObject.SetActive(false);
        trainingSignReceiver.gameObject.SetActive(false);
        TimeExceededTMP.gameObject.SetActive(false);
        TimeExceededTMPReceiver.gameObject.SetActive(false);

        xrOriginSetup.transform.rotation =  Quaternion.Euler(new Vector3(0, 180, 0)); 
        
        score = 0;

        // Prepare the milky glass bools
        ShowMilkyGlassRandom();

        StartCoroutine(TriggerEyetrackingCalibration());
    }


    // Update is called once per frame
    void Update()
    {
        Debug.Log("Phase" + phase);

        // Make the glass either milky or clear according to current condition
        if(milkyGlassBool)
        {
            clearGlass.SetActive(false);
            milkyGlass.SetActive(true);
        }
        if(!milkyGlassBool)
        {
            clearGlass.SetActive(true);
            milkyGlass.SetActive(false);
        }

        if (frozen != previousFrozen)
        {
            // Play the audio when the boolean changes
            PlayAudio();
            previousFrozen = frozen;
        }
    
        
        // According to the role of the main player, set up the secondary avatar
        // The secondary avatar has to put to this position every frame, since otherwise it falls down infinitely.
        if (role == "receiver")
        {
            avatarSecondary.transform.position = new Vector3(-3f,3.4f,4.8f); 
        }
        if (role == "signaler")
        {
            avatarSecondary.transform.position = new Vector3(-3f,3.4f,-10f);
        }


        #region Experimental process 

        // Phase 0: Welcome & Instruction Embodiment (UI Space)
        if (phase == 0)
        {
            // Role assignment 
            if (_inputBindings.UI.Signaler.triggered) // "s" on keyboard
            {
                role = "signaler";
                receiver.GetComponent<ReceiverManager>().enabled = false;
                signaler.GetComponent<SignalerManager>().enabled = true;
                signalerManager.Teleport(spaceLocationsSignaler.ElementAt(0), xrOriginSetup);
                avatarSecondary.transform.position = new Vector3(-3f,3.4f,-10f);
                
                // Enable the Eye data scripts of the signaler and disable the ones of the receiver
                avatarMain.GetComponent<SRanipal_AvatarEyeSample_v2>().enabled = true;
                avatarMain.GetComponent<SRanipal_AvatarEyeSample_v2_modified>().enabled = false;
                avatarMain.GetComponent<SignalerEyeDataSender>().enabled = true;
                
                avatarSecondary.GetComponent<SRanipal_AvatarEyeSample_v2>().enabled = false;
                avatarSecondary.GetComponent<SRanipal_AvatarEyeSample_v2_modified>().enabled = true;

                // Enable the LSL Streams of the signaler and disable the ones of the receiver
                avatarMain.GetComponent<LSLSignalerOutlets>().enabled = true;
                avatarMain.GetComponent<LSLSignalerInlets>().enabled = true; 
                avatarMain.GetComponent<LSLReceiverInlets>().enabled = false;
                avatarMain.GetComponent<LSLReceiverOutlets>().enabled = false;

                // Show or disable displays of score and round number
                scoreDisplay.gameObject.SetActive(true);
                roundsDisplay.gameObject.SetActive(true);
                scoreDisplayReceiver.gameObject.SetActive(false);
                roundsDisplayReceiver.gameObject.SetActive(false);

            }
            else if (_inputBindings.UI.Receiver.triggered) // "r" on keyboard
            {
                role = "receiver";
                receiver.GetComponent<ReceiverManager>().enabled = true;
                signaler.GetComponent<SignalerManager>().enabled = false;
                receiverManager.Teleport(spaceLocationsReceiver.ElementAt(0), xrOriginSetup); 
                avatarSecondary.transform.rotation =Quaternion.Euler(new Vector3(0, -180, 0));
                avatarSecondary.transform.position = new Vector3(-3f,3.4f,4.8f);

                // Enable the Eye data scripts of the receiver and disable the ones of the signaler
                avatarMain.GetComponent<SRanipal_AvatarEyeSample_v2>().enabled = true;
                avatarMain.GetComponent<SRanipal_AvatarEyeSample_v2_modified>().enabled = false;
                avatarMain.GetComponent<ReceiverEyeDataSender>().enabled = true;
                
                avatarSecondary.GetComponent<SRanipal_AvatarEyeSample_v2>().enabled = false;
                avatarSecondary.GetComponent<SRanipal_AvatarEyeSample_v2_modified>().enabled = true;

                // Enable the LSL Streams of the receiver and disable the ones of the signaler
                avatarMain.GetComponent<LSLReceiverInlets>().enabled = true;
                avatarMain.GetComponent<LSLReceiverOutlets>().enabled = true;  
                avatarMain.GetComponent<LSLSignalerOutlets>().enabled = false;
                avatarMain.GetComponent<LSLSignalerInlets>().enabled = false;

                // Show or disable displays of score and round number
                scoreDisplayReceiver.gameObject.SetActive(true);
                roundsDisplayReceiver.gameObject.SetActive(true);
                scoreDisplay.gameObject.SetActive(false);
                roundsDisplay.gameObject.SetActive(false);
            }
        }
        // Phase 1: Embodiment (Embodiment Space)
        else if (phase == 1)
        {
            Debug.LogError("Embodiment Phase");
        }
        // Phase 2: Instruction Testing (UI Space)
        else if (phase == 2)
        {
            Debug.LogError("Phase 2");
        }
        // Phase 3: First Condition (Experiment Room)
        else if (phase == 3)
        {   
           // Change the rotation of the receiver's avatar every frame, otherwise it is weirdly oriented, don't know why
            if (role == "receiver")
            {
                vRRig.headBodyOffset =   new Vector3(-0.0299999993f,-5.32000017f,-0.94f);
                xrOriginSetup.transform.rotation =  Quaternion.Euler(new Vector3(0, 0, 0)); 
            }
            // Update the display that shows the current round number
            if(role == "signaler")
            {
                roundsDisplay.text = "Round: " + _currentRound;
            } 
            else if(role == "receiver")
            {
                roundsDisplayReceiver.text = "Round: " + _currentRound;
            }
            // While the training phase is ongoing, show a respective sign
            if (_currentRound > 0 && _currentRound < 4 && !trainingEnd)
            {
                if (role == "signaler")
                {
                    trainingSign.gameObject.SetActive(true);
                }   
                if (role == "receiver")
                {
                    trainingSignReceiver.gameObject.SetActive(true);
                }
            } 
            // If the training phase is over, reset the score and remove the training sign         
            if (_currentRound == 4 && !trainingEnd) //this order could cause problems
            {    
                ResetScoreRound();
                if (role == "signaler")
                {
                    trainingSign.gameObject.SetActive(false);
                }   
                if (role == "receiver")
                {
                    trainingSignReceiver.gameObject.SetActive(false);
                }
                trainingEnd = true;
            }
            // If enough rounds per condition have been played, go to the next phase
            else if (_currentRound == roundsPerCondition)
            { 
                EnterNextPhase();
            }
        }
       
        // Phase 4: End Phase (UI Space)
        else
        {
           if (role == "receiver")
           {
            vRRig.headBodyOffset =   new Vector3(-0.0299999993f,-5.32000017f,1.22000003f);
            xrOriginSetup.transform.rotation =  Quaternion.Euler(new Vector3(0, 90, 0));
            avatarMain.transform.rotation =  Quaternion.Euler(new Vector3(0, 180, 0));
           }
           if (role == "signaler")
           {
            signalerManager.invisibleObjectSignaler = invisibleObject; // Why?
           }
        }
        #endregion
    }

    #region Phase functions
    public void EnterNextPhase()
    {
        phase += 1;
        if (role == "receiver")
        {
            receiverManager.Teleport(spaceLocationsReceiver.ElementAt(phase), xrOriginSetup);
        }
        if (role == "signaler") 
        {
            signalerManager.Teleport(spaceLocationsSignaler.ElementAt(phase), xrOriginSetup);
        }
    }



    public void FreezeSignaler()
    {
        frozen = true;
        signalerManager.simpleCrosshair.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = Color.red;
        Debug.Log("Freeze method");
    }

    public void UnfreezeSignaler()
    {
        frozen = false;
        signalerManager.simpleCrosshair.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = Color.white;
        Debug.Log("Unfreeze method");
    }
   
    public void EnterPausePhase()
    {
        pauseRoomSignaler = new Vector3(0, -26, 55);  //former y value: -31.54
        pauseRoomReceiver = new Vector3(-114, -27, 1);
        if (role == "receiver")
        {
            receiverManager.Teleport(pauseRoomReceiver, xrOriginSetup);
        }
        if (role == "signaler") 
        {
            signalerManager.Teleport(pauseRoomSignaler, xrOriginSetup);
        }
    }

    public void ReturnToCurrentPhase()
    {
        if (role == "receiver")
        {
            receiverManager.Teleport(spaceLocationsReceiver.ElementAt(phase), xrOriginSetup);
        }
        if (role == "signaler") 
        {
            signalerManager.Teleport(spaceLocationsSignaler.ElementAt(phase), xrOriginSetup);
        }

    }


    public int GetCurrentPhase()
    {
        return phase;
    }

    #endregion

    // TODO: make this function more clean
    public IEnumerator Condition1()
    {
        milkyGlassBool = milkyBools[_currentRound];
        
        string milkyGlassBoolString = milkyGlassBool.ToString();
        lslReceiverOutlets.lslOmilkyGlassBool.push_sample(new string[] {milkyGlassBoolString});
        // _startedRound = true;
        _selected = false;

        // 1. Freeze receiver 
            	// --> eye data is not sent to the signaler at this point
        // 2. Shuffle rewards
        ShuffleRewards();
        // 3. Unfreeze signaler
            // --> signalers gaze is unlocked and can look around
        // signalerManager.Unfreeze();
        //Timer starts
        _startRoundTime = Time.time;

        // 4. Signaler freezes themselves by button press or after the timer runs out
        yield return new WaitWhile(() => ((_inputBindings.Player.Freeze.triggered == false)||(Time.time - _startRoundTime < _timeLimit)));
        if(role == "signaler" && _inputBindings.Player.Freeze.triggered && !firstSelectionMade)
        {
            Debug.LogError("firstSelectionMade");
            firstSelectionMade = true;
        }
        
        //signalerManager.Freeze();
        if (firstSelectionMade) 
        {

        }
        // receiverManager.Unfreeze();
        // 5. Receiver chooses box 

        if (Time.time - _startRoundTime < _timeLimit){
            Debug.LogError("Time exceeded. We're at the end of the round. Receiver did not select");
        }
        
        yield return new WaitWhile(() => _selected == false);
        // Reward is added up
        
    }


    // Shuffles the reward values and assigns them to the boxes.
    private void ShuffleRewards()
    {
        // Initialize the rewards list

        rewards = new List<int> { 100, 0, 0, 0, 0, 0, -25, -25 };

        // Initialize the random number generator
        System.Random rng = new System.Random();

        // Create a new list with 8 elements, all initialized to -1 (a placeholder value)
        shuffledRewards = Enumerable.Repeat(-1, 8).ToList();

        // Increase the range to include the edges but with a small probability
        int mainRewardIndex;
        double probability = rng.NextDouble();

        if (probability < 0.1) // 10% chance for the main reward to be on the edges
        {
            mainRewardIndex = (rng.Next(2) == 0) ? 0 : 7;
        }
        else
        {
            mainRewardIndex = rng.Next(1, 7); // 90% chance for the main reward to be between 1 and 5
        }
       
        // Insert the main reward (100) at the generated index
        shuffledRewards[mainRewardIndex] = 100;

        // Remove the main reward from the original list
        rewards.Remove(100);

        // Shuffle the remaining rewards and insert them into the shuffledRewards list
        foreach (int reward in rewards)
        {
            int index;
            do
            {
                index = rng.Next(shuffledRewards.Count);
            } while (shuffledRewards[index] != -1); // Ensure we place only in empty slots

            shuffledRewards[index] = reward;
        }

        float[] floatArray = shuffledRewards.Select(i => (float)i).ToArray();
        lslReceiverOutlets.lslORewardValues.push_sample(floatArray);

        Debug.Log("Shuffled rewards: " + string.Join(", ", shuffledRewards));
        // Assign the shuffled rewards to the boxes
        for (int i = 0; i < boxes.Count; i++)
        {
            BoxBehaviour currentBox = boxes.ElementAt(i).GetComponent<BoxBehaviour>();
            currentBox.ChangeReward(shuffledRewards.ElementAt(i));
        }
    }


    // Adds the newly achieved reward to the score
    public void UpdateScore(int reward)
    {
        score += reward;
        scoreDisplayReceiver.text = "Score: " + score;
        scoreDisplay.text = "Score: " + score;
        _currentRound += 1;
        _selected = true;
        // _startedRound = false;
        receiverManager.boxSelected = false;

    }

    public void ResetScoreRound()
    {
        score = 0;
        _currentRound = 0;
        scoreDisplayReceiver.text = "Score: " + score;
        roundsDisplayReceiver.text = "Round: " + _currentRound;

        scoreDisplay.text = "Score: " + score;
        roundsDisplay.text = "Round: " + _currentRound;

    }

    // Trigger eyetracking calibration every 30 rounds
    private IEnumerator TriggerEyetrackingCalibration()
    {
        Debug.Log("started eye coroutine");
        // Once phase 3 has been entered start the eye tracking calibration cycle
        bool FirstCal() => phase == 3;
        yield return new WaitUntil(FirstCal);
        Debug.Log("before while");

        while(!debugging)
        {
            Debug.Log("while ");
            //SRanipal_Eye_v2.LaunchEyeCalibration();
            EnterPausePhase();
            eyetrackingValidation.ValidateEyeTracking();
            // Do as many calibrations as needed to reduce the validation error to below 1
            
            // Debug // TODO: solve the problem
            _ValidationSuccessStatus = true;

            // Wait for 30 rounds
            bool NextVal() => (_ValidationSuccessStatus == false || (_currentRound % 30 == 0 && _currentRound != 0));
            yield return new WaitUntil(NextVal); 
        }
    }

    

    public void SetValidationSuccessStatus(bool success)
    {
        if (success)
        {
            _ValidationSuccessStatus = true;
        }
        else
        {
            _ValidationSuccessStatus = false;
        }
    }
    

    public IEnumerator ShowTimeExceeded()
    {
        Debug.Log("ShowTimeExceeded");
        if(role == "signaler")
        {
            TimeExceededTMP.gameObject.SetActive(true);
            yield return new WaitForSeconds(2);
            TimeExceededTMP.gameObject.SetActive(false);
        }
        if(role == "receiver")
        {
            TimeExceededTMPReceiver.gameObject.SetActive(true);
            yield return new WaitForSeconds(2);
            TimeExceededTMPReceiver.gameObject.SetActive(false);
        }
        
    }

    // Called once in the start function
    private void ShowMilkyGlassRandom()
    {
        /*
        float randomValue = UnityEngine.Random.value;
         
         if(randomValue < probabilityForMilky){
            milkyGlassBool = true;
         } 
         else 
         {
            milkyGlassBool = false;
         }

        string milkyGlassBoolString = milkyGlassBool.ToString();
        lslReceiverOutlets.lslOmilkyGlassBool.push_sample(new string[] {milkyGlassBoolString});
        */

        // Create an array to store the boolean values of whether the glass is cear or milky in the respective round
        milkyBools = new bool[roundsPerCondition];
        // Fill the first half of the array with 'true' values
        int cut = (int) Math.Round(roundsPerCondition*probabilityForMilky);
        for (int i = 0; i < cut; i++)
        {
            milkyBools[i] = true;
        }
        // Fill the second half of the array with 'false' values
        for (int i = cut; i < roundsPerCondition; i++)
        {
            milkyBools[i] = false;
        }
        // Shuffle the array randomly using the Knuth shuffle algorithm
        for (int i = 0; i < milkyBools.Length; i++)
        {
            bool currentValue = milkyBools[i];
            int rand = UnityEngine.Random.Range(i, milkyBools.Length);
            milkyBools[i] = milkyBools[rand];
            milkyBools[rand] = currentValue;
        }
    }


    public IEnumerator Countdown()
    {
        running = true;
        int count = countdownTime;
        yield return new WaitForSeconds(1f);

        if (role == "signaler")
        {
            while (count > 0)
            {
                countdownText.text = count.ToString();
                yield return new WaitForSeconds(1);
                count--;
            }

            countdownText.text = "Go!";
            yield return new WaitForSeconds(1);
            countdownText.gameObject.SetActive(false);
            //signalerManager.invisibleObjectSignaler = crosshair;
            
            StartCoroutine(CountdownTimer(timerCountdownText));
            
        }
        if (role == "receiver")
        {
            while (count > 0)
            {
                countdownTextReceiver.text = count.ToString();
                yield return new WaitForSeconds(1);
                count--;
            }

            countdownTextReceiver.text = "Go!";
            yield return new WaitForSeconds(1);
            countdownTextReceiver.text = "Wait!";
            yield return new WaitForSeconds(19);
            countdownTextReceiver.text = "Start!";
            yield return new WaitForSeconds(1);
            
            
            countdownTextReceiver.gameObject.SetActive(false);
            
            
            
            
        }
        
    }

public IEnumerator CountdownTimer(TextMeshProUGUI CDT)
{
    countdownRunning = true;
    receiverManager.CountdownStarted = true;
    yield return new WaitForSeconds(1);
    int count = 20;
    while (count > 0)
    {
        if ((frozen && signalerManager.freezeCounter > 1)  || (_inputBindings.Player.SelectBox.triggered && receiverManager.selectCounter > 1))
        {
            CDT.text = string.Empty; 
            Debug.LogError("stopped everything");
            yield break; 
        }
        if (count <= 3)
        {
            CDT.text = $"<b><color=red>{count}</color></b>";
        }
        else
        {
            CDT.text = count.ToString();
        }
        yield return new WaitForSeconds(1);
        count--;
    }
    // Optionally, clear the countdown text after the loop ends
    CDT.text = string.Empty;
    StartCoroutine(ShowTimeExceeded());
    trialFailedCount++;
    UpdateScore(-20);
    countdownRunning = false;
}
    public void PlayAudio()
    {
        // Play the audio
        if (soundEffect.isPlaying)
        {
            soundEffect.Stop();  // Optional: Stop current audio if it's still playing
        }
        soundEffect.Play();  // Play the audio
    }

}
