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
    [Tooltip("Phase 0: Welcome & Instruction Embodiment (UI Space); Phase 1: Embodiment (Embodiment Space); Phase 2: Instruction Testing (UI Space); Phase 3: First Condition (Experiment Room); Phase 4: Instructions Second Condition (UI Space); Phase 5: Second Condition (Experiment Room); Phase 6: End Phase (UI Space)")]
    public int phase = 0;
    private Player player;
    private Player player2; // don't need "player2" as we run it on two devices and only distinguish between roles, right?
    private InputBindings _inputBindings;
    
    private int score;
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

    private MenuManager menuManager;

    private bool firstSelectionMade = false;

    public TMP_Text TestingText3;
    public TMP_Text TestingText4;

    [Tooltip("The locations of the embodiment, start, condition 1, break, condition 2 and end space.")]
    [SerializeField] private List<Vector3> spaceLocations = null;

    [Tooltip("There should be as many rewards as there are inner boxes.")]
    [SerializeField] private List<int> rewards;

    private bool _ValidationSuccessStatus = true;
    
    //Jojo Timer/Clock 
    public ClockManager clockManager;
    [SerializeField] private GameObject clock;
    public bool TimeExceeded = false;

    // Start is called before the first frame update
    void Start()
    {
        menuManager = FindObjectOfType<MenuManager>();
        player = FindObjectOfType<Player>();
        score = 0;
        _inputBindings = new InputBindings();
        _inputBindings.Player.Enable();

        TestingText3.gameObject.SetActive(false);
        TestingText4.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        // Phase 0: Welcome & Instruction Embodiment (UI Space)
        if (phase == 0)
        {
            player.Teleport(spaceLocations.ElementAt(0));
            player.role = "";
            Debug.Log("Phase 0");
            // TODO: let the function be called from the menu manager or an embodiment phase manager 
            // EnterNextPhase();
        }
        // Phase 1: Embodiment (Embodiment Space)
        else if (phase == 1)
        {
            player.role = "";
            // Debug.Log("Phase 1");
            // TODO: let the function be called from the menu manager or an embodiment phase manager 
            //EnterNextPhase();
        }
        // Phase 2: Instruction Testing (UI Space)
        else if (phase == 2)
        {
            player.role = "";
            Debug.Log("Phase 2");
        }
        // Phase 3: First Condition (Experiment Room)
        else if (phase == 3)
        {
            //assign role here?
           // player.role = "receiver";
            //Debug.Log("Phase 3");

            if (player.role == "receiver")
            {
                player.Teleport(spaceLocations.ElementAt(7));
            }

            if (_currentRound < roundsPerCondition)
            {   
                if (_startedRound == false)
                    StartCoroutine(Condition1());
                trialNumber++;
            } 
            else 
            {
                EnterNextPhase();
            }
        }
        // Phase 4: Second Condition Instructions (UI Space)
        else if (phase == 4)
        {
           
            Debug.Log("Phase 4");
        }
        // Phase 5: Second Condition (Experiment Room)
        else if (phase == 5)
        {
            //Debug.Log("Phase 4");
            if (_currentRound < roundsPerCondition)
            {   
                if (_startedRound == false)
                    StartCoroutine(Condition2());
                trialNumber++;

            } 
            else 
            {
                EnterNextPhase();
            }
        }
        // Phase 6: End Phase (UI Space)
        else
        {
            Debug.Log("Phase 6");
        }

        if (_ValidationSuccessStatus == false) 
        {
            SRanipal_Eye_v2.LaunchEyeCalibration();
            _ValidationSuccessStatus = true;
        }

        
    }

    
    public void EnterNextPhase()
    {
        phase += 1;
        player.Teleport(spaceLocations.ElementAt(phase));
    }

    private int currentPhase = 0;
   
    public void EnterPausePhase()
    {
        pauseRoom = new Vector3(-50, -26, 1);
        player.Teleport(pauseRoom);
    }
    public void ReturnToCurrentPhase()
    {
        currentPhase = GetCurrentPhase();
        player.Teleport(spaceLocations.ElementAt(currentPhase));

    }


    /* public void EnterPausePhase()
     {
         if (_inputBindings.UI.Pause.triggered)
         {
             CurrentPhase = GetCurrentPhase();
             player.Teleport(spaceLocations.ElementAt(0));
             if (_inputBindings.UI.Unpause.triggered)
             {
                 player.Teleport(spaceLocations.ElementAt(CurrentPhase));
             }
         }
     } */
    public int GetCurrentPhase()
    {
        return phase;
    }

    private IEnumerator Condition1()
    {
        _startedRound = true;
        _selected = false;
        Timer();
        Debug.Log("Timer started");
        roundsDisplay.text = "Round: " + _currentRound;
        // 1. Freeze receiver 
        // 2. Shuffle rewards
        ShuffleRewards();
        // 3. Unfreeze signaller
        player.Unfreeze();
        // wait for a certain amount of time / the signaller pressing a button
        yield return new WaitWhile(() => _inputBindings.UI.Select.triggered == false);
        player.Freeze();
        if (!firstSelectionMade)
        {
            // Show the text
            StartCoroutine(menuManager.ShowTwoTexts(TestingText3, TestingText4)); //might need to assign the TMP's to GameManager script
            // Set the flag to true
            firstSelectionMade = true;
        }
        // player2.Unfreeze();
        // 4. Receiver chooses box 
        yield return new WaitWhile(() => _selected == false);
        // Reward is added up
        
    }

    private IEnumerator Condition2()
    {
        _startedRound = true;
        _selected = false;
        roundsDisplay.text = "Round: " + _currentRound;
        ShuffleRewards();
        yield return new WaitWhile(() => _selected == false);
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


}
