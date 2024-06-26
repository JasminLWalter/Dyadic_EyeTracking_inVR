using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using VIVE.OpenXR;
using ViveSR.anipal.Eye;

public class GameManager : MonoBehaviour
{
    [Tooltip("Stores the current condition of the experiment.")]
    [SerializeField] private int condition = 0;
    [Tooltip("Phase 0: Welcome & Instruction Embodiment (UI Space); Phase 1: Embodiment (Embodiment Space); Phase 2: Instruction Testing (UI Space); Phase 3: Testing Phase (Testing Space); Phase 4: End Phase (UI Space)")]
    public int phase = 0;
    private int currentPhase = 0;

    private ReceiverManager receiverManager;
    private SignalerManager signalerManager;
    private BoxBehaviour boxBehaviour;
    private MenuManager menuManager;
    public GameObject xrOriginSetup;
    public GameObject avatar;

    private InputBindings _inputBindings;
    
    private int score;
    public string role;
    [SerializeField] private TextMeshPro scoreDisplay;
    [SerializeField] private TextMeshPro roundsDisplay;
    [SerializeField] private TextMeshPro TimeExceededTMP;

    [SerializeField] private int roundsPerCondition = 0;
    private int _currentRound = 0;
    private bool _startedRound = false;
    private bool _selected = false;
    [SerializeField] private List<GameObject> boxes = null;
    

    public int trialNumber = 0;
    public int trialFailedCount = 0;

    Vector3 pauseRoom = new Vector3();
    Vector3 pauseRoomReceiver = new Vector3();

    private bool firstSelectionMade = false;

    private int countdownTime = 3;
    public TextMeshProUGUI  countdownText;
    public TextMeshProUGUI  countdownTextReceiver;

    public GameObject milkyGlass;
    public GameObject clearGlass;

    //[Tooltip("The locations of the embodiment, start, condition 1, break, condition 2 and end space.")]

    [SerializeField] private List<Vector3> spaceLocationsReceiver = null;
    [SerializeField] private List<Vector3> spaceLocationsSignaler = null;

    [Tooltip("There should be as many rewards as there are inner boxes.")]
    [SerializeField] private List<int> rewards;

    public bool _ValidationSuccessStatus = true;
    
    //Jojo Timer/Clock 
    public ClockManager clockManager;
    [SerializeField] private GameObject clock;
    public bool TimeExceeded = false;

    public float _timeLimit = 3;
    private float startTime;
    private float _startRoundTime = 0;

    private float probabilityForOne = 0.5f;
    public Camera mainCamera;
    
    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
        menuManager = FindObjectOfType<MenuManager>();
        signalerManager = FindObjectOfType<SignalerManager>();
        receiverManager = FindObjectOfType<ReceiverManager>();



        role = "signaler";
        xrOriginSetup.transform.rotation =  Quaternion.Euler(new Vector3(0, 90, 0));
        //role = "receiver";
        if (role == "receiver") 
        {
            xrOriginSetup.GetComponent<ReceiverManager>().enabled = true;
            xrOriginSetup.GetComponent<SignalerManager>().enabled = false;
            receiverManager.Teleport(spaceLocationsReceiver.ElementAt(0));
            
        }
        if (role == "signaler")
        {
            xrOriginSetup.GetComponent<ReceiverManager>().enabled = false;
            xrOriginSetup.GetComponent<SignalerManager>().enabled = true;
            signalerManager.Teleport(spaceLocationsSignaler.ElementAt(0));
        }

        score = 0;

        _inputBindings = new InputBindings();
        _inputBindings.Player.Enable();

         _inputBindings = new InputBindings();
        _inputBindings.UI.Enable();
    }

    // Update is called once per frame
    void Update()
    {

        Debug.LogError(avatar.transform.position - mainCamera.transform.position); 
        #region Experimental process 
        // Phase 0: Welcome & Instruction Embodiment (UI Space)
        if (phase == 0)
        {
            //player.Teleport(spaceLocations.ElementAt(0));
            signalerManager.Teleport(spaceLocationsSignaler.ElementAt(0));
            //receiverManager.Teleport(spaceLocationsReceiver.ElementAt(0));
           // player.role = "";
            
            // TODO: let the function be called from the menu manager or an embodiment phase manager 
            // EnterNextPhase();
        }
        // Phase 1: Embodiment (Embodiment Space)
        else if (phase == 1)
        {
            //player.role = "";

            // TODO: let the function be called from the menu manager or an embodiment phase manager 
            //EnterNextPhase();
            Debug.LogError("Phase 1");
            
        }
        // Phase 2: Instruction Testing (UI Space)
        else if (phase == 2)
        {
           // player.role = "";
            Debug.LogError("Phase 2");
        }
        // Phase 3: First Condition (Experiment Room)
        else if (phase == 3)
        {
            //assign role here?
            //player.role = "receiver";
            Debug.LogError("Phase 3");

            if (_currentRound < roundsPerCondition)
            {   
                if (_startedRound == false)
                {
                    StartCoroutine(Condition1());
                }
                trialNumber++;
            } 
            else 
            {
                EnterNextPhase();
            }
        }
       
        // Phase 4: End Phase (UI Space)
        else
        {
           // Debug.Log("Phase 4");
        }

        if (_ValidationSuccessStatus == false) 
        {
            //SRanipal_Eye_v2.LaunchEyeCalibration();
            _ValidationSuccessStatus = true;
        }
        #endregion
    }

    #region Phase functions
    public void EnterNextPhase()
    {
        phase += 1;
        if (role == "receiver")
        {
            receiverManager.Teleport(spaceLocationsReceiver.ElementAt(phase));
        }
        if (role == "signaler") 
        {
            signalerManager.Teleport(spaceLocationsSignaler.ElementAt(phase));
        }
       // player.Teleport(spaceLocations.ElementAt(phase));
    }
   
    public void EnterPausePhase()
    {
        pauseRoom = new Vector3(0, -26, 55);
        pauseRoomReceiver = new Vector3(-114, -27, 1);
        if (role == "receiver")
        {
            receiverManager.Teleport(pauseRoomReceiver);
        }
        if (role == "signaler") 
        {
            signalerManager.Teleport(pauseRoom);
        }

    }
    public void ReturnToCurrentPhase()
    {
        currentPhase = GetCurrentPhase();

        if (role == "receiver")
        {
            receiverManager.Teleport(spaceLocationsReceiver.ElementAt(currentPhase));
        }
        if (role == "signaler") 
        {
            signalerManager.Teleport(spaceLocationsSignaler.ElementAt(currentPhase));
        }

    }


    public int GetCurrentPhase()
    {
        return phase;
    }

    #endregion
    private IEnumerator Condition1()
    {
        
        ShowMilkyGlassRandom();
        _startedRound = true;
        _selected = false;
        Timer();
        Debug.Log("Timer started");
        roundsDisplay.text = "Round: " + _currentRound;
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
        yield return new WaitWhile(() => ((_inputBindings.Player.Freeze.triggered == false)^(Time.time - _startRoundTime < _timeLimit)));
        if(_inputBindings.Player.Freeze.triggered && !firstSelectionMade)
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
        if(receiverManager.boxSelected == true)
        {
            boxBehaviour.Selected();
        }

        if (Time.time - _startRoundTime < _timeLimit){
            Debug.LogError("Time exceeded. We're at the end of the round. Receiver did not select");
        }
        
        yield return new WaitWhile(() => _selected == false);
        // Reward is added up
        
    }


    // Shuffles the reward values and assigns them to the boxes.
    private void ShuffleRewards()
    {   
        Debug.Log("Inside ShuffleRewards");
        // Shuffle the rewards of the inner boxes
        List<int> shuffledRewards = new List<int>();
    
        while (rewards.Count > 0)
        {
            int index = Random.Range(0, rewards.Count - 1); 
            shuffledRewards.Add(rewards.ElementAt(index));
            rewards.RemoveAt(index);
        }

        rewards = shuffledRewards;

        // Assign the shuffled rewards to the boxes
        for (int i = 0; i < boxes.Count; i++) 
        {
            BoxBehaviour currentBox = boxes.ElementAt(i).GetComponent<BoxBehaviour>();
            currentBox.ChangeReward(rewards.ElementAt(i));
        }
    }


    // Adds the newly achieved reward to the score
    public void UpdateScore(int reward)
    {
        score += reward;
        scoreDisplay.text = "Score: " + score;
        _currentRound += 1;
        _selected = true;
        _startedRound = false;
    }

    // TODO:
    // Continuously stores measured data to a file.
    private void StoreData()
    {

    }

    // TODO:
    // When the application is quit, the data storage file is closed and saved.
    private void OnApplicationQuit()
    {

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
    
    private void Timer()
    { 
        if (_startedRound && !_selected)
        {
            
            clock.gameObject.SetActive(true);
            clockManager.isRunning = true;

            if (TimeExceeded == true)
            {
                Debug.LogError("Time Exceeded == true");
                clock.gameObject.SetActive(false);
                ShowTimeExceeded();
                trialFailedCount++;

            }
        }
        else
        {
            clock.gameObject.SetActive(false);

        }
    }

    public IEnumerator ShowTimeExceeded()
    {
        // Show the object
        TimeExceededTMP.gameObject.SetActive(true);
        Debug.LogError("ShowTimeExceeded");
        // Wait for the specified duration
        yield return new WaitForSeconds(2);

        // Hide the object after the duration
        TimeExceededTMP.gameObject.SetActive(false);
        _selected = true;
    }

    private void ShowMilkyGlassRandom()
    {
        float randomValue = Random.value;
         
         if(randomValue < probabilityForOne){
            milkyGlass.SetActive(true);
            clearGlass.SetActive(false);
            Debug.LogError("ShowMilkyGlass");
         } 
         else 
         {
            clearGlass.SetActive(true);
            milkyGlass.SetActive(false);
            Debug.LogError("ShowClearGlass");
         }

    }
    public IEnumerator Countdown()
    {
        int count = countdownTime;
        yield return new WaitForSeconds(3);

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
            countdownTextReceiver.gameObject.SetActive(false);
            
        }
        
    }
}
