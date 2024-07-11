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
    private EmbodimentManager embodimentManager;
    private BoxBehaviour boxBehaviour;
    private MenuManager menuManager;
    private SimpleCrosshair simpleCrosshair;

    public VRRig vRRig;
    public GameObject xrOriginSetup;
    public GameObject signaler;
    public GameObject avatar;
    public GameObject avatarSecondary;
    public GameObject invisibleObject;
    public GameObject crosshair;
    public GameObject trainingSign;
    public Material invisible;
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
    public bool firstFreeze = false;
    public bool firstFreezeReceiver = false;
    [SerializeField] private List<GameObject> boxes = null;
    

    public int trialNumber = 0;
    public int trialFailedCount = 0;

    Vector3 pauseRoom = new Vector3();
    Vector3 pauseRoomReceiver = new Vector3();

    private bool firstSelectionMade = false;

    private int countdownTime = 3;
    public TextMeshProUGUI  countdownText;

    public TextMeshProUGUI  countdownTextReceiver;
    public TextMeshProUGUI  timerCountdownText;
    public TextMeshProUGUI  timerCountdownTextReceiver;
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
    //private bool startedTimer = false;
    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
        menuManager = FindObjectOfType<MenuManager>();
        signalerManager = FindObjectOfType<SignalerManager>();
        receiverManager = FindObjectOfType<ReceiverManager>();
        embodimentManager = FindObjectOfType<EmbodimentManager>();
        simpleCrosshair = FindObjectOfType<SimpleCrosshair>();

        trainingSign.gameObject.SetActive(false);

        //role = "signaler";
        xrOriginSetup.transform.rotation =  Quaternion.Euler(new Vector3(0, 90, 0));
        role = "receiver";
        if (role == "receiver") 
        {
            //signaler.GetComponent<ReceiverManager>().enabled = true;
            xrOriginSetup.GetComponent<ReceiverManager>().enabled = true;
            xrOriginSetup.GetComponent<SignalerManager>().enabled = false;
            receiverManager.Teleport(spaceLocationsReceiver.ElementAt(0));
            
        }
        if (role == "signaler")
        {
            xrOriginSetup.GetComponent<ReceiverManager>().enabled = false;
            signaler.GetComponent<SignalerManager>().enabled = true;
            signalerManager.Teleport(spaceLocationsSignaler.ElementAt(0), xrOriginSetup);
            signalerManager.Teleport(new Vector3(-99.5999985f,-105.760002f,66.6399994f), avatarSecondary);
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
        Debug.LogError("avatar position secondary" + avatarSecondary.transform.position);
        signalerManager.Teleport(new Vector3(-99.5999985f,-99f,66.6399994f), avatarSecondary);
        #region Experimental process 
        // Phase 0: Welcome & Instruction Embodiment (UI Space)
        if (phase == 0)
        {
            if (role == "receiver")
            {
                receiverManager.Teleport(spaceLocationsReceiver.ElementAt(phase));
            }
            if (role == "signaler") 
            {
                signalerManager.Teleport(spaceLocationsSignaler.ElementAt(phase), xrOriginSetup);
            }
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
            if (role == "receiver")
            {
                vRRig.headBodyOffset =   new Vector3(-0.0299999993f,-5.32000017f,-0.94f);
                xrOriginSetup.transform.rotation =  Quaternion.Euler(new Vector3(0, 270, 0));
                avatar.transform.rotation =  Quaternion.Euler(new Vector3(0, 0, 0));
            }
            if (_currentRound < roundsPerCondition)
            {   
                if (_startedRound == false && firstFreeze == true)
                {
                    StartCoroutine(Condition1());
                    StartCoroutine(CountdownTimer(timerCountdownText));
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
           if (role == "receiver")
           {
            
            vRRig.headBodyOffset =   new Vector3(-0.0299999993f,-5.32000017f,1.22000003f);
            xrOriginSetup.transform.rotation =  Quaternion.Euler(new Vector3(0, 90, 0));
            avatar.transform.rotation =  Quaternion.Euler(new Vector3(0, 180, 0));
            Debug.LogError("turn around");
           }
           if (role == "signaler")
           {
            signalerManager.invisibleObject = invisibleObject;
           }
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
            signalerManager.Teleport(spaceLocationsSignaler.ElementAt(phase), xrOriginSetup);
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
            signalerManager.Teleport(pauseRoom, xrOriginSetup);
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
            signalerManager.Teleport(spaceLocationsSignaler.ElementAt(currentPhase), xrOriginSetup);
        }

    }


    public int GetCurrentPhase()
    {
        return phase;
    }

    #endregion
    private IEnumerator Condition1()
    {
        if (_currentRound > 0 && _currentRound < 4)
        {
            trainingSign.gameObject.SetActive(true);
        }
        else
        {
            trainingSign.gameObject.SetActive(false);
        }
        ShowMilkyGlassRandom();
        _startedRound = true;
        _selected = false;
        //Timer();
        
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

        // Ensure there are 8 boxes
        // if (boxes.Count != 8)
        // {
        //     Debug.LogError("There must be exactly 8 boxes.");
        //     return;
        // }

        // Initialize the rewards list

        rewards = new List<int> { 100, 0, 0, 0, 0, 0, -25, -25 };

        // Console.WriteLine("Original rewards: " + string.Join(", ", rewards));

        // Initialize the random number generator
        System.Random rng = new System.Random();

        // Create a new list with 8 elements, all initialized to -1 (a placeholder value)
        List<int> shuffledRewards = Enumerable.Repeat(-1, 8).ToList();

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



        // rewards = new List<int> { 100, 0, 0, 0, 0, 0, -25 };
        
        // Debug.Log("Shuffled rewards: " + string.Join(", ", rewards));

        // // Shuffle rewards except for the main reward (100)
        // List<int> shuffledRewards = new List<int>();
        // Debug.Log("Shuffled rewards: " + string.Join(", ", shuffledRewards));

        // System.Random rng = new System.Random();
        // Debug.Log("Shuffled rewards: " + shuffledRewards);
        // // Ensure the main reward is not on the edges (index 0 or 7)
        // int mainRewardIndex = rng.Next(1, 6); // Generate a random index between 1 and 6 for the main reward
        // shuffledRewards.
        // // Shuffle the remaining rewards
        // rewards.Remove(100); // Remove the main reward from the original list

        // while (rewards.Count > 0)
        // {
        //     int index = rng.Next(rewards.Count);
        //     shuffledRewards.Add(rewards.ElementAt(index));
        //     rewards.RemoveAt(index);
        // }

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
        
        //if (_startedRound && !_selected)
        //{
            
            clock.gameObject.SetActive(true);
            clockManager.StartClock();
            
            if (TimeExceeded == true)
            {
                Debug.LogError("Time Exceeded == true");
                clock.gameObject.SetActive(false);
                StartCoroutine(ShowTimeExceeded());
                trialFailedCount++;

            }
        /*}
        else
        {
            clock.gameObject.SetActive(false);

        }*/
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
            signalerManager.invisibleObject = crosshair;
            
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
            countdownTextReceiver.gameObject.SetActive(false);
            //StartCoroutine(CountdownTimer(timerCountdownTextReceiver));
            
            
            
        }
        
    }

public IEnumerator CountdownTimer(TextMeshProUGUI CDT)
{
    yield return new WaitForSeconds(1);
    int count = 10;
    while (count > 0)
    {
        if ((signalerManager.frozen && signalerManager.freezeCounter > 2)  || (_inputBindings.Player.SelectBox.triggered && receiverManager.selectCounter > 2))
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
    
    //firstFreezeReceiver = true;
}

}
