using UnityEngine;
using UnityEngine.UI;

public class EmbodimentManager : MonoBehaviour
{
    public Button StartRecording;
    public Button StopRecording;
    public Button ShowRecording;
    public GameObject Light;
    public Image barImage;

    public GameManager gameManager;

    private bool Finished = false;

    void Start()
    {
        StartRecording.gameObject.SetActive(false);
        StopRecording.gameObject.SetActive(false);
        ShowRecording.gameObject.SetActive(false);

        StartRecording.onClick.AddListener(OnStartRecordingButtonClick);
        StopRecording.onClick.AddListener(OnStopRecordingButtonClick);
        ShowRecording.onClick.AddListener(OnShowRecordingButtonClick);
    }

    void Update()
    {
        if (gameManager.GetCurrentPhase() == 1)
        {
            if (!Finished)
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
    }

    public void OnStartRecordingButtonClick()
    {
        Renderer objectRenderer = Light.GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            objectRenderer.material.color = Color.green;
            
        }
    }

    public void OnStopRecordingButtonClick()
    {
        Renderer objectRenderer = Light.GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            objectRenderer.material.color = Color.red;
            Finished = true;
        }
    }

    public void OnShowRecordingButtonClick()
    {
        // Logic for handling ShowRecording button click
        // Add your functionality here if needed
    }
}
