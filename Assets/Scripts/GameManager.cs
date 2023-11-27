using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [Tooltip("Stores the current condition of the experiment.")]
    [SerializeField] private int condition = 0;
    private int phase = 0;
    private Player player1;
    private Player player2;
    private int score = 0;

    [SerializeField] private int roundsPerCondition = 0;
    [SerializeField] private List<GameObject> boxes = null;

    private MenuManager menuManager;

    [Tooltip("The locations of the embodiment, start, condition 1, break, condition 2 and end space.")]
    [SerializeField] private Dictionary<string, Position> spaceLocations = null;

    // Start is called before the first frame update
    void Start()
    {
        menuManager = FindObjectOfType<MenuManager>();
        player = FindObjectOfType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

        

    // Shuffles the rewards values and assigns them to the boxes.
    private void ShuffleRewards()
    {

    }

    // Adds the newly achieved reward to the score
    public void UpdateScore(int reward)
    {
        score += reward;
    }


    // Continuously stores measured data to a file.
    private void StoreData()
    {

    }


    // When the application is quit, the data storage file is closed and saved.
    private void OnApplicationQuit()
    {

    }

}
