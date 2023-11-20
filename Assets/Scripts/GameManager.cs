using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Tooltip("Stores the current condition of the experiment.")]
    [SerializeField] private int condition = 0;
    private int phase = 0;
    [SerializeField] private Player player;
    private int score = 0;

    [SerializeField] private int roundsPerCondition = 0;
    [SerializeField] private List<GameObject> boxes = None;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ShowStartMenu()
    {

    }

    // Continuously stores measured data to a file.
    private void StoreData()
    {

    }

    // Shuffles the rewards values and assigns them to the boxes.
    private void ShuffleRewards()
    {

    }

    private void ShowBreakMenu()
    {

    }

    // When the application is quit, the data storage file is closed and saved.
    private void OnApplicationQuit()
    {

    }

}
