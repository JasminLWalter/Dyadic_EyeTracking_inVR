using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class EmbodimentManager : MonoBehaviour
{
    public Button StartRecording;
    public Button StopRecording;
    public Button ShowRecording;
    public Button Finish;

    public GameObject TV;
    public GameObject TV1;
    public GameObject TV2;
    public Material Invisible;
    public Material Screen_off;

    public TMP_Text RecordingText;

    public GameManager gameManager;

    public GameObject playerEyes;

    public GameObject recordedEyes;

    private bool FinishedRecording = false;
    private bool FinishedShow = false;
    private bool End = false;

    private int Counter = 0;

    private Queue<Quaternion> storedRotations;

    void Start()
    {
        StartRecording.gameObject.SetActive(false);
        StopRecording.gameObject.SetActive(false);
        ShowRecording.gameObject.SetActive(false);
        Finish.gameObject.SetActive(false);
        RecordingText.gameObject.SetActive(false);
        TV.gameObject.SetActive(false);

        StartRecording.onClick.AddListener(OnStartRecordingButtonClick);
        StopRecording.onClick.AddListener(OnStopRecordingButtonClick);
        ShowRecording.onClick.AddListener(OnShowRecordingButtonClick);
        Finish.onClick.AddListener(OnFinishButtonClick);

        storedRotations = new Queue<Quaternion>();
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

        if (gameManager.GetCurrentPhase() != 1)
        {
            StartRecording.gameObject.SetActive(false);
            StopRecording.gameObject.SetActive(false);
            ShowRecording.gameObject.SetActive(false);
            Finish.gameObject.SetActive(false);
            RecordingText.gameObject.SetActive(false);
        }
    }

    public void OnStartRecordingButtonClick()
    {
        
        RecordingText.gameObject.SetActive(true);
       // start data collection
       // start task
        StartCoroutine(StoreRotations());
    }

    private IEnumerator StoreRotations()
    {   
        while (!FinishedRecording)
        {
            storedRotations.Enqueue(playerEyes.transform.rotation);
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void OnStopRecordingButtonClick()
    {
        RecordingText.gameObject.SetActive(false);
        FinishedRecording = true;
        FinishedShow = false;
        TV.gameObject.SetActive(true);
        ChangeColor(TV1, Screen_off);
        ChangeColor(TV2, Screen_off);

    }

    public void OnShowRecordingButtonClick()
    {
        //show data simulation...
        StartCoroutine(ApplyRotations());
        ChangeColor(TV1, Invisible);
        ChangeColor(TV2, Invisible);


    }

    private IEnumerator ApplyRotations()
    {
        Debug.Log("Coroutine to show recording");
        while (storedRotations.Count != 0)
        {
            recordedEyes.transform.rotation = storedRotations.Dequeue();
            yield return new WaitForSeconds(0.2f);
        }
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


    private void ChangeColor(GameObject obj, Material newMaterial)
    {
        Renderer objectRenderer = obj.GetComponent<Renderer>();

        if (objectRenderer != null)
        {
            Material[] materials = objectRenderer.materials;

            if (materials.Length > 1)
            {
                materials[1] = newMaterial; // Assuming you want to change the second material (index 1)
                objectRenderer.materials = materials; // Assign the modified materials array back to the Renderer
                Debug.LogError("Material changed");
            }
            else
            {
                Debug.LogError("Object does not have enough materials.");
            }
        }
        else
        {
            Debug.LogError("Object does not have a Renderer component.");
        }
    }


}
