using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using System.Collections.Generic;

/* Phases: 
/// Phase 0: Welcoming
- Welcome to the Experiment!
- To continue, press the trackpad. 
To register your chosen position during your task, press the trigger button.
Signaler:
- You have the role of the Signaler. You can signal a preferred box to your partner by fixating it with your eyes and pressing the trigger button to register your response. 
- Keep in Mind that you point with your eyes. 
Receiver: 
- You have the role of the Receiver. Your goal is to choose the box your partner was signaling you to choose with their eyes.
- By pointing to the box, the box will highlight and you will be able to choose the box by pressing the trigger button.
- After you registered your box, you will have to wait for your partner to signal you the next box. 


/// Phase 1: Embodiment (not done yet)

/// Phase 2: End of embodiment phase (not done yet)

/// Phase 6: End of experiment
Congratulations!
You finished the Experiment.

Thank you, for your participation!
*/

public class MenuManager : MonoBehaviour
{
    //Signaler
    public List<TMP_Text> TextsPhase0;
    public List<TMP_Text> TextsPhase2;
    public TMP_Text TextPhase6;
    public TMP_Text Pause;
    public List<TMP_Text> TextsPhase3;
    public TMP_Text textGazeFixed;

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
    




    public GameManager gameManager;
    private ReceiverManager receiverManager;
    private SignalerManager signalerManager;
    public EyetrackingValidation eyetrackingValidation;
    private ReceiverEyeDataSender receiverEyeDataSender;
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
        receiverEyeDataSender = FindObjectOfType<ReceiverEyeDataSender>();
        

        // Signaler
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

        // Receiver
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

    }

    void Update()
    {
        if (gameManager.GetCurrentPhase() == 0)
        {
            if (gameManager.role == "signaler" && !phase0CoroutineRunning)
            {
                    StartCoroutine(ShowTexts(TextsPhase0, () => phase0CoroutineRunning = false)); // Once the coroutine is done, phase0CoroutineRunning will turn false
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

        //Hides everything if wrong phase // TODO: re-work this
        if (gameManager.GetCurrentPhase() != 0)
        {
            HideLists(TextsPhase0);
        }

        if (gameManager.GetCurrentPhase() != 2)
        {
            HideLists(TextsPhase2);
        }

        if (gameManager.GetCurrentPhase() != 4)  // TODO: make the phases match
        {
            TextPhase6.gameObject.SetActive(false);

        }

        // Better to put this in GameManager?
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
        
        // Loop over all texts in the current phase
        while (currentTextIndex < textComponents.Count)
        {
            // Show current text
            textComponents[currentTextIndex].gameObject.SetActive(true);

            // If the participant wants to go back in the instructions
            if (_inputBindings.UI.Return.triggered && currentTextIndex > 0)
            {
                textComponents[currentTextIndex].gameObject.SetActive(false);
                currentTextIndex -= 1;
                textComponents[currentTextIndex].gameObject.SetActive(true);
            }
            // If the participant wants to read the next text
            else if (_inputBindings.UI.Continue.triggered)
            {
                textComponents[currentTextIndex].gameObject.SetActive(false);
                currentTextIndex += 1;
                

                if (currentTextIndex < textComponents.Count)
                {
                    textComponents[currentTextIndex].gameObject.SetActive(true);
                }
                else if (currentTextIndex >= textComponents.Count)  // If the end of this part's instructions is reached; TODO: re-work this part
                { 
                    // I think this initiates the Second Part of the instructions
                    if (gameManager.GetCurrentPhase() == 3 && signalerManager.signalerReady == false && !countdownRunning && receiverManager.receiverReady == false)
                    {
                        //StartCoroutine(gameManager.Countdown());
                        countdownRunning = true;
                        Debug.Log("second part did run");
                        
                    } 
                    //and this the countdown
                    if (gameManager.GetCurrentPhase() == 3 && signalerManager.signalerReady == true && countdownRunning && receiverManager.receiverReady == true && receiverManager.secondCheck)
                    {
                        receiverEyeDataSender.waitReceiver.gameObject.SetActive(true);
                        StartCoroutine(gameManager.Countdown());
                        Debug.Log("second part did run, second if");
                        
                    } 
                    else if (gameManager.GetCurrentPhase() == 0) // || gameManager.GetCurrentPhase() == 2)
                    {
                        gameManager.EnterNextPhase();
                        gameManager.EnterNextPhase();
                        gameManager.EnterNextPhase();



                    }
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
                textElement.gameObject.SetActive(false); 
            }
        }
    }
}

