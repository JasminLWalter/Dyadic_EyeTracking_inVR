using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    //Signaler
    public List<TMP_Text> TextsPhase0;
    public TMP_Text TextPhase0;
    public List<TMP_Text> TextsPhase2;
    public TMP_Text TextPhase6;
    public TMP_Text Pause;
    public List<TMP_Text> TextsPhase3;

    private bool phase0CoroutineRunning = false;
    private bool phase2CoroutineRunning = false;
    private bool phase3CoroutineRunning = false;
    private bool ShowedStart = false;
    private bool ShowedText1 = false;
    private bool didRun = false;

    //Receiver
    public List<TMP_Text> TextsPhase0Receiver;
    public TMP_Text TextPhase0Receiver;
    public List<TMP_Text> TextsPhase2Receiver;
    public TMP_Text TextPhase6Receiver;
    public TMP_Text PauseReceiver;
    public List<TMP_Text> TextsPhase3Receiver;

    private bool ShowedStartReceiver = false;
    private bool ShowedText1Receiver = false;
    private bool phase0CoroutineRunningReceiver = false;
    private bool phase2CoroutineRunningReceiver = false;
    private bool phase3CoroutineRunningReceiver = false;
    private bool didRunReceiver = false;


    public Player player;
    public GameManager gameManager;
    private InputBindings _inputBindings;
    private int currentTextIndex = 0;





    public GameObject xrOriginSetup;



    private void Start()
    {
        
        _inputBindings = new InputBindings();
        _inputBindings.UI.Enable();
        
        gameManager = FindObjectOfType<GameManager>();

        TextPhase6.gameObject.SetActive(false);
        Pause.gameObject.SetActive(false);

        //TestingText1.gameObject.SetActive(false);
        //TestingText2.gameObject.SetActive(false);
        

        foreach (TMP_Text TextPhase0 in TextsPhase0)
        {
            TextPhase0.gameObject.SetActive(false);
        }
        foreach (TMP_Text TextPhase2 in TextsPhase2)
        {
            TextPhase2.gameObject.SetActive(false);
        }
        foreach (TMP_Text TextPhase3 in TextsPhase3)
        {
            TextPhase3.gameObject.SetActive(false);
        }

        //Receiver
        TextPhase6Receiver.gameObject.SetActive(false);
        PauseReceiver.gameObject.SetActive(false);
        
        foreach (TMP_Text TextPhase0Receiver in TextsPhase0Receiver)
        {
            TextPhase0Receiver.gameObject.SetActive(false);
        }
        foreach (TMP_Text TextPhase2Receiver in TextsPhase2Receiver)
        {
            TextPhase2Receiver.gameObject.SetActive(false);
        }
        foreach (TMP_Text TextPhase3Receiver in TextsPhase3Receiver)
        {
            TextPhase3Receiver.gameObject.SetActive(false);
        }

        if (player == null)
        {
            Debug.LogError("Player reference not set in the Inspector!");
            return;
        }
    }

    void Update()
    {
        if (gameManager.GetCurrentPhase() == 0)
        {
            if (gameManager.role == "signaler" && !phase0CoroutineRunning)
            {
                if (ShowedStart == false)
                {
                    StartCoroutine(ShowStart(TextPhase0));
                    ShowedStart = true;
                }
                else if (ShowedStart == true)
                {
                    StartCoroutine(ShowTexts(TextsPhase0, () => phase0CoroutineRunning = false));
                    phase0CoroutineRunning = true;
                }
            }
            if (gameManager.role == "receiver" && !phase0CoroutineRunningReceiver)
            {
                if (ShowedStartReceiver == false)
                {
                    StartCoroutine(ShowStart(TextPhase0Receiver));
                    ShowedStartReceiver = true;
                }
                else if (ShowedStartReceiver == true)
                {
                    StartCoroutine(ShowTexts(TextsPhase0Receiver, () => phase0CoroutineRunningReceiver = false));
                    phase0CoroutineRunningReceiver = true;
                }
            }
        }
        if (gameManager.GetCurrentPhase() == 2)
        {


        }
        if (gameManager.GetCurrentPhase() == 3)
        {
            if (gameManager.role == "signaler" && !phase3CoroutineRunning)
            {
                if(!didRun)
                {
                    StartCoroutine(ShowTexts(TextsPhase3, () => phase3CoroutineRunning = false));
                    phase3CoroutineRunning = true;
                    didRun = true;
                }
                
            }
            if (gameManager.role == "receiver" && !phase3CoroutineRunningReceiver)
            {
                xrOriginSetup.transform.rotation = Quaternion.Euler(0, 90, 0);

                if(!didRunReceiver)
                {
                    Debug.LogError("hier");
                    StartCoroutine(ShowTexts(TextsPhase3Receiver, () => phase3CoroutineRunningReceiver = false));
                    phase3CoroutineRunningReceiver = true;
                    didRunReceiver = true;
                }
                
            }
        }       
        if (gameManager.GetCurrentPhase() == 4)
        {
            TextPhase6.gameObject.SetActive(true);
        }
        if (_inputBindings.UI.Skip.triggered)
        {
            gameManager.EnterNextPhase();
        }

        //Hides everything if wrong phase
        if (gameManager.GetCurrentPhase() != 0)
        {
            TextPhase0.gameObject.SetActive(false);
            HideLists(TextsPhase0);
        }

        if (gameManager.GetCurrentPhase() != 2)
        {
            HideLists(TextsPhase2);
        }

        if (gameManager.GetCurrentPhase() != 4)
        {
            TextPhase6.gameObject.SetActive(false);

        }

        if (_inputBindings.UI.Pause.triggered)
        {
            gameManager.EnterPausePhase();
            Pause.gameObject.SetActive(true);

        }

        if (_inputBindings.UI.Unpause.triggered)
        {
            gameManager.ReturnToCurrentPhase();
            Pause.gameObject.SetActive(false);
        }

    }
    

    public IEnumerator ShowTexts(List<TMP_Text> textComponents, Action coroutineFinishedCallback)
    {
        int currentTextIndex = 0;
        yield return new WaitForSeconds(3f);
        while (currentTextIndex < textComponents.Count)
        {
            textComponents[currentTextIndex].gameObject.SetActive(true);

            if (_inputBindings.UI.Return.triggered && currentTextIndex > 0)
            {
                textComponents[currentTextIndex].gameObject.SetActive(false);
                currentTextIndex -= 1;
                textComponents[currentTextIndex].gameObject.SetActive(true);
            }
            else if (_inputBindings.UI.Continue.triggered)
            {
                textComponents[currentTextIndex].gameObject.SetActive(false);
                currentTextIndex += 1;
                

                if (currentTextIndex < textComponents.Count)
                {
                    textComponents[currentTextIndex].gameObject.SetActive(true);
                }
                else 
                {
                    // When the last text component is reached
                    textComponents[textComponents.Count - 1].gameObject.SetActive(false);
                    
                    if (gameManager.GetCurrentPhase() == 0 || gameManager.GetCurrentPhase() == 2)
                    {
                        gameManager.EnterNextPhase();
                    }
                }

            }
            yield return null;
        }
    } 

    public IEnumerator ShowStart(TMP_Text Text)
    {
        Text.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        Text.gameObject.SetActive(false);
    }

    void HideLists(List<TMP_Text> texts)
    {
        foreach (TMP_Text textElement in texts)
        {
            // Access each TMP_Text element and its gameObject to hide it
            if (textElement != null)
            {
                textElement.gameObject.SetActive(false); // Hiding gameObject of each TMP_Text
            }
        }
    }
}

