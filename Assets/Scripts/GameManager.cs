using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Random;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [Tooltip("Stores the current condition of the experiment.")]
    [SerializeField] private int condition = 0;
    [Tooltip("Phase 1: Embodiment Phase; Phase 2: Instruction Phase; Phase 3: Testing Phase; Phase 4: End Phase")]
    [SerializeField] private int phase = 0;
    private Player player;
    private Player player2;
    private int score;

    [SerializeField] private int roundsPerCondition = 0;
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
    }

    // Update is called once per frame
    void Update()
    {
        // Embodiment Phase
        if (phase == 0)
        { 
            
            // TODO: let the function be called from the menu manager or an embodiment phase manager 
            EnterNextPhase();
        }
        // Instruction Phase
        else if (phase == 1)
        {
           
        }
        // Testing Phase
        else if (phase == 2)
        {
            // 1. Freeze receiver 
            // 2. Shuffle rewards
            ShuffleRewards();
            // 3. Unfreeze signaller
        }
        // End Phase
        else if (phase > 2)
        {
            
        }
    }

    
    public void EnterNextPhase()
    {
        phase += 1;
        player.Teleport(spaceLocations.ElementAt(phase));
    }



    // Shuffles the rewards values and assigns them to the boxes.
    private void ShuffleRewards()
    {
        // Shuffle the rewards of the inner boxes
        List<int> shuffledRewards = new List<int>();
    
        while (rewards.Count > 0)
        {
            int index = Random.Range(0, rewards.Count - 1); 
            shuffledRewards.Add(rewards.ElementAt(index));
            rewards.RemoveAt(index);
        }

        rewards = shuffledRewards;

        // Assign the shuffled rewards to the inner boxes
        for (int i = 1; i < boxes.Count - 1; i++) 
        {
            BoxBehaviour currentBox = boxes.ElementAt(i).GetComponent<BoxBehaviour>();
            currentBox.ChangeReward(rewards.ElementAt(i - 1));
        }
    }



    // Adds the newly achieved reward to the score
    public void UpdateScore(int reward)
    {
        score += reward;
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
