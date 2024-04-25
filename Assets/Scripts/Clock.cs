using UnityEngine;
using Wave.Essence;

public class ClockManager : MonoBehaviour
{
    private int seconds = 0;


    public GameObject pointerSeconds;

    [SerializeField] private GameObject clock;
    public GameManager gameManager;

    // -- time speed factor
    private float clockSpeed = 12f; // 1.0f = realtime, < 1.0f = slower, > 1.0f = faster

    // -- internal vars
    float msecs = 0;
    public bool isRunning = false; // Flag to control whether the clock is running or not
    //public bool TimeExceeded = false;
    float totalSeconds = 0; // Total seconds elapsed

    void Start()
    {
        clock.gameObject.SetActive(false);
        Debug.Log("Start in clockManager");

    }

    void Update()
    {
        if (isRunning)
        {
            // -- calculate time
            msecs += Time.deltaTime * clockSpeed;
            if (msecs >= 1.0f)
            {
                msecs -= 1.0f;
                seconds++;
                totalSeconds++;
                /*
                if (seconds >= 60)
                {
                    seconds = 0;
                }
                */
                // -- Stop the clock after 45 seconds
                if (totalSeconds >= 60)
                {
                    isRunning = false;
                    gameManager.StartCoroutine(gameManager.ShowTimeExceeded());
                    //gameManager.TimeExceeded = true;
                    //TimeExceeded = true;

                    
                    Debug.Log("Clock stopped after 45 seconds.");
                }
            }

            // -- calculate pointer angles
            float rotationSeconds = (360.0f / 60.0f) * seconds; // Invert rotation for starting from the top
           

            // -- draw pointers
            pointerSeconds.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationSeconds);
        }
    }
}
