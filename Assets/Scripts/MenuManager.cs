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
    public List<TMP_Text> TextsPhase2Receiver;
    public TMP_Text TextPhase6Receiver;
    public TMP_Text PauseReceiver;
    public List<TMP_Text> TextsPhase3Receiver;

    private bool ShowedStartReceiver = false;
    private bool ShowedText1Receiver = false;
    private bool phase0CoroutineRunningReceiver = false;
    private bool phase2CoroutineRunningReceiver = false;
    private bool phase3CoroutineRunningReceiver = false;
    public bool didRunReceiver = false;
    public bool countdownRunning = false;
    



    public Player player;
    public GameManager gameManager;
    private ReceiverManager receiverManager;
    private SignalerManager signalerManager;
    public EyetrackingValidation eyetrackingValidation;
    //public SRanipal_Eye_v2 sRanipal_Eye_v2;
    private InputBindings _inputBindings;
    private int currentTextIndex = 0;





    public GameObject xrOriginSetup;



    private void Start()
    {
        
        _inputBindings = new InputBindings();
        _inputBindings.UI.Enable();
        
        gameManager = FindObjectOfType<GameManager>();
        eyetrackingValidation = FindObjectOfType<EyetrackingValidation>();
        receiverManager = FindObjectOfType<ReceiverManager>();
        signalerManager = FindObjectOfType<SignalerManager>();
        //sRanipal_Eye_v2 = FindObjectOfType<SRanipal_Eye_v2>();
        

        TextPhase6.gameObject.SetActive(false);
        Pause.gameObject.SetActive(false);
        

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
                    StartCoroutine(ShowTexts(TextsPhase0, () => phase0CoroutineRunning = false));
                    phase0CoroutineRunning = true;
                    
            }
            if (gameManager.role == "receiver" && !phase0CoroutineRunningReceiver)
            {
                    StartCoroutine(ShowTexts(TextsPhase0Receiver, () => phase0CoroutineRunningReceiver = false));
                    phase0CoroutineRunningReceiver = true;
            }
        }
        if (gameManager.GetCurrentPhase() == 2)
        {
            if (gameManager.role == "signaler" && !phase2CoroutineRunning)
            {
                    StartCoroutine(ShowTexts(TextsPhase2, () => phase2CoroutineRunning = false));
                    phase2CoroutineRunning = true;
            }
            if (gameManager.role == "receiver" && !phase2CoroutineRunningReceiver)
            {
                    StartCoroutine(ShowTexts(TextsPhase2Receiver, () => phase2CoroutineRunningReceiver = false));
                    phase2CoroutineRunningReceiver = true;
            }

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

                    foreach (TMP_Text TextPhase3Receiver in receiverManager.TextsPhase3Receiver)
                    {
                        TextPhase3Receiver.gameObject.SetActive(false);
                    }
                }
                
            }
            if (gameManager.role == "receiver" && !phase3CoroutineRunningReceiver)
            {
                xrOriginSetup.transform.rotation = Quaternion.Euler(0, 90, 0);

                if(!didRunReceiver)
                {
                    StartCoroutine(ShowTexts(TextsPhase3Receiver, () => phase3CoroutineRunningReceiver = false));
                    phase3CoroutineRunningReceiver = true;
                    didRunReceiver = true;

                    foreach (TMP_Text TextPhase3 in TextsPhase3)
                    {
                        TextPhase3.gameObject.SetActive(false);
                    }

                }
            }
        }       
        if (gameManager.GetCurrentPhase() == 4)
        {
            if (gameManager.role == "signaler")
            {
                TextPhase6.gameObject.SetActive(true);
            }
            if (gameManager.role == "receiver")
            {
                TextPhase6Receiver.gameObject.SetActive(true);
            }
        }
        if (_inputBindings.UI.Skip.triggered)
        {
            gameManager.EnterNextPhase();
        }

        //Hides everything if wrong phase
        if (gameManager.GetCurrentPhase() != 0)
        {
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
        //yield return new WaitForSeconds(3f);
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
                else if (currentTextIndex >= textComponents.Count)
                { 
                    if (gameManager.GetCurrentPhase() == 3 && signalerManager.signalerReady == false && !countdownRunning && receiverManager.receiverReady == false)
                    {
                        //StartCoroutine(gameManager.Countdown());
                        countdownRunning = true;
                        Debug.Log("second part did run");
                        
                    } 
                    if (gameManager.GetCurrentPhase() == 3 && signalerManager.signalerReady == true && countdownRunning && receiverManager.receiverReady == true && receiverManager.secondCheck)
                    {
                        StartCoroutine(gameManager.Countdown());
                        Debug.Log("second part did run");
                        
                    } 
                    if (gameManager.GetCurrentPhase() == 0) // || gameManager.GetCurrentPhase() == 2)
                    {
                        gameManager.EnterNextPhase();



                    }/*  
                        //sRanipal_Eye_v2.LaunchEyeCalibration();
                        //eyetrackingValidation.ValidateEyeTracking();
                        //if(gameManager._ValidationSuccessStatus == true) //could cause problems because _ValidationSuccessStatus is initiated as true
                        //{
                            gameManager.EnterNextPhase();
                        //}
                        
                    }*/

                }

            }
            yield return null;
        }
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

