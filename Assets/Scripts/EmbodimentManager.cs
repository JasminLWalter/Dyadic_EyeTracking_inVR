using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmbodimentManager : MonoBehaviour
{
    public Button StartRecording;
    public Button StopRecording;
    public Button ShowRecording;
    public Button Finish;

    public GameObject TV;

    public TMP_Text RecordingText;

    public GameManager gameManager;

    private bool FinishedRecording = false;
    private bool FinishedShow = false;
    private bool End = false;

    private int Counter = 0;

    void Start()
    {
        StartRecording.gameObject.SetActive(false);
        StopRecording.gameObject.SetActive(false);
        ShowRecording.gameObject.SetActive(false);
        Finish.gameObject.SetActive(false);
        RecordingText.gameObject.SetActive(false);

        StartRecording.onClick.AddListener(OnStartRecordingButtonClick);
        StopRecording.onClick.AddListener(OnStopRecordingButtonClick);
        ShowRecording.onClick.AddListener(OnShowRecordingButtonClick);
        Finish.onClick.AddListener(OnFinishButtonClick);

    }

    void Update()
    {
        if (gameManager.GetCurrentPhase() == 1)
        {
            if (!FinishedRecording || FinishedShow)
            {
                StartRecording.gameObject.SetActive(true);
                StopRecording.gameObject.SetActive(true);
                ShowRecording.gameObject.SetActive(false);
            }
            else
            {
                StartRecording.gameObject.SetActive(false);
                StopRecording.gameObject.SetActive(false);
                ShowRecording.gameObject.SetActive(true);
            }
        }

        if (Counter > 2 && !End)
        {
            Finish.gameObject.SetActive(true);
        }
    }

    public void OnStartRecordingButtonClick()
    {

        TV.gameObject.SetActive(false);
        RecordingText.gameObject.SetActive(true);
       // start data collection
       // start task

    }

    public void OnStopRecordingButtonClick()
    {
        RecordingText.gameObject.SetActive(false);
        FinishedRecording = true;
        FinishedShow = false;
        // stop task
        // stop data collection

    }

    public void OnShowRecordingButtonClick()
    {
        //unhide TV & maybe zoom
        //show data simulation...

        //when recording finished:
        FinishedShow = true;
        Counter += 1;
    }

    public void OnFinishButtonClick()
    {
        End = true;
        StartRecording.gameObject.SetActive(false);
        StopRecording.gameObject.SetActive(false);
        Finish.gameObject.SetActive(false);
        gameManager.EnterNextPhase();

    }
}
