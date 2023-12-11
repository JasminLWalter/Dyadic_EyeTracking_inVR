using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [Tooltip("Stores the current condition of the experiment.")]
    [SerializeField] private int condition = 0;
    [Tooltip("Phase 1: Embodiment Phase; Phase 2: Instruction Phase; Phase 3: Testing Phase Condition 1; Phase 4: Second Instruction Phase; Phase 5: Testing Phase Condition 2; Phase 6: End Phase")]
    [SerializeField] private int phase = 0;
    private Player player;
    private Player player2;
    private InputBindings _inputBindings;
    
    private int score;
    [SerializeField] private TextMeshPro scoreDisplay;
    [SerializeField] private TextMeshPro roundsDisplay;

    [SerializeField] private int roundsPerCondition = 0;
    private int _currentRound = 0;
    private bool _startedRound = false;
    private bool _selected = false;
    [SerializeField] private List<GameObject> boxes = null;

    private MenuManager menuManager;

    [Tooltip("The locations of the embodiment, start, condition 1, break, condition 2 and end space.")]
    [SerializeField] private List<Vector3> spaceLocations = null;

    [Tooltip("There should be as many rewards as there are inner boxes.")]
    [SerializeField] private List<int> rewards;


    // Start is called before the first frame update
    void Start()
    {
        menuManager = FindObjectOfType<MenuManager>();
        player = FindObjectOfType<Player>();
        score = 0;
        _inputBindings = new InputBindings();
        _inputBindings.Player.Enable();
    }

    // Update is called once per frame
    void Update()
    {
       // Embodiment Phase
        if (phase == 0)
        { 
            Debug.Log("Phase 0");
            // TODO: let the function be called from the menu manager or an embodiment phase manager 
            EnterNextPhase();
        }
        // Instruction Phase
        else if (phase == 1)
        {
            Debug.Log("Phase 1");
            // TODO: let the function be called from the menu manager or an embodiment phase manager 
            EnterNextPhase();
        }
        // Testing Phase
        else if (phase == 2)
        {
            //Debug.Log("Phase 2");
            if (_currentRound < roundsPerCondition)
            {   
                if (_startedRound == false)
                    StartCoroutine(Condition1());
            } 
            else 
            {
                EnterNextPhase();
            }
        }
        // End Phase
        else if (phase > 2)
        {
            Debug.Log("Phase 3");
        }
    }

    
    public void EnterNextPhase()
    {
        phase += 1;
        player.Teleport(spaceLocations.ElementAt(phase));
    }

    public int GetCurrentPhase()
    {
        return phase;
    }

    private IEnumerator Condition1()
    {
        _startedRound = true;
        _selected = false;
        roundsDisplay.text = "Round: " + _currentRound;
        // 1. Freeze receiver 
        // 2. Shuffle rewards
        ShuffleRewards();
        // 3. Unfreeze signaller
        player.Unfreeze();
        // wait for a certain amount of time / the signaller pressing a button
        yield return new WaitWhile(() => _inputBindings.Player.Select.triggered == false);
        player.Freeze();
        // player2.Unfreeze();
        // 4. Receiver chooses box 
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

}
