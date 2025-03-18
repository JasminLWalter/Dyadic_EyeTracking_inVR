using UnityEngine;
using System.Collections;

public class ClockManager : MonoBehaviour
{
    private int seconds = 0;
    public GameObject pointerSeconds;
    [SerializeField] private GameObject clock;
    public GameManager gameManager;
    private float clockSpeed = 12f; // 1.0f = realtime, < 1.0f = slower, > 1.0f = faster
    float msecs = 0;
    public bool isRunning = false;
    float totalSeconds = 0;

    void Start()
    {
        clock.gameObject.SetActive(false);
        Debug.Log("Start in ClockManager");
    }

    void Update()
    {
    }

    public void StartClock()
    {
        // Start the coroutine to run the clock
        StartCoroutine(RunClock());
    }

    IEnumerator RunClock()
    {
            msecs += Time.deltaTime * clockSpeed;
            if (msecs >= 1.0f)
            {
                Debug.LogError("Timer started startclock");
                msecs -= 1.0f;
                seconds++;
                totalSeconds++;

                if (totalSeconds >= 60)
                {
                    isRunning = false;
                    gameManager.StartCoroutine(gameManager.ShowTimeExceeded());
                    Debug.Log("Clock stopped after 60 seconds.");
                }

                float rotationSeconds = (360.0f / 60.0f) * seconds;
                pointerSeconds.transform.localEulerAngles = new Vector3(0.0f, 0.0f, rotationSeconds);
            }

            yield return null;
        
    }
}
