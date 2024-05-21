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

    public TMP_Text TestingText1;
    public TMP_Text TestingText2;

    private bool phase0CoroutineRunning = false;
    private bool phase2CoroutineRunning = false;
    private bool ShowedStart = false;
    private bool ShowedText1 = false;

    //Receiver
    public List<TMP_Text> TextsPhase0Receiver;
    public TMP_Text TextPhase0Receiver;
    public List<TMP_Text> TextsPhase2Receiver;
    public TMP_Text TextPhase6Receiver;
    public TMP_Text PauseReceiver;
   
    public TMP_Text TestingText1Receiver;
    public TMP_Text TestingText2Receiver;

    private bool ShowedStartReceiver = false;
    private bool ShowedText1Receiver = false;
    private bool phase0CoroutineRunningReceiver = false;
    private bool phase2CoroutineRunningReceiver = false;


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

        TestingText1.gameObject.SetActive(false);
        TestingText2.gameObject.SetActive(false);
        

        foreach (TMP_Text TextPhase0 in TextsPhase0)
        {
            TextPhase0.gameObject.SetActive(false);
        }

        foreach (TMP_Text TextPhase2 in TextsPhase2)
        {
            TextPhase2.gameObject.SetActive(false);
        }

        //Receiver
        TextPhase6Receiver.gameObject.SetActive(false);
        PauseReceiver.gameObject.SetActive(false);

        TestingText1Receiver.gameObject.SetActive(false);
        TestingText2Receiver.gameObject.SetActive(false);
        

        foreach (TMP_Text TextPhase0Receiver in TextsPhase0Receiver)
        {
            TextPhase0Receiver.gameObject.SetActive(false);
        }

        foreach (TMP_Text TextPhase2Receiver in TextsPhase2Receiver)
        {
            TextPhase2Receiver.gameObject.SetActive(false);
        }

        if (player == null)
        {
            Debug.LogError("Player reference not set in the Inspector!");
            return;
        }
    }

    void Update()
    {
        if (gameManager.GetCurrentPhase() == 0 && !phase0CoroutineRunning && !phase0CoroutineRunningReceiver)
        {
            if (gameManager.role == "signaler")
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
            if (gameManager.role == "receiver")
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
                    //gameManager.EnterNextPhase();
                }
            }

        }
        if (gameManager.GetCurrentPhase() == 2 && !phase2CoroutineRunning && !phase2CoroutineRunningReceiver)
        {
            if (gameManager.role == "signaler")
                if (!ShowedText1)
                {
                    StartCoroutine(ShowTwoTexts2(TestingText1, TestingText2));
                    ShowedText1 = true;
                }
            if (gameManager.role == "receiver")
            {
                xrOriginSetup.transform.rotation = Quaternion.Euler(0, 90, 0);

                if (!ShowedText1Receiver && gameManager.role == "receiver")
                {
                    StartCoroutine(ShowTwoTexts2(TestingText1Receiver, TestingText2Receiver));
                    ShowedText1Receiver = true;
                }
            }
            else 
            phase2CoroutineRunning = true;
            phase2CoroutineRunningReceiver = true;
            gameManager.EnterNextPhase();

        }
        if (gameManager.GetCurrentPhase() == 3)
        {
            StartCoroutine(ShowTwoTexts2(TestingText1, TestingText2));
            
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
    

    IEnumerator ShowTexts(List<TMP_Text> textComponents, Action coroutineFinishedCallback)
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
                Debug.LogError("Current text index: " + currentTextIndex);

                if (currentTextIndex < textComponents.Count)
                {
                    textComponents[currentTextIndex].gameObject.SetActive(true);
                }
                else 
                {
                    // When the last text component is reached
                    textComponents[textComponents.Count - 1].gameObject.SetActive(false);
                    gameManager.EnterNextPhase();
                    coroutineFinishedCallback?.Invoke(); // Callback to signal coroutine completion
                }

            }

            yield return null;
        }
    } 

    private IEnumerator ShowStart(TMP_Text Text)
    {
        Text.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        Text.gameObject.SetActive(false);
        //Show = true;
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

    
    public IEnumerator ShowTwoTexts(TMP_Text textObject1, TMP_Text textObject2)
    {
        textObject1.gameObject.SetActive(true);
        if (_inputBindings.UI.Continue.triggered)
        {
            textObject1.gameObject.SetActive(false);
            textObject2.gameObject.SetActive(true);
            yield return new WaitForSeconds(5f);
            textObject2.gameObject.SetActive(false);
        }
        else if (_inputBindings.UI.Return.triggered)
        {
            textObject1.gameObject.SetActive(true);
            textObject2.gameObject.SetActive(false);
        }
    } 

        public IEnumerator ShowTwoTexts2(TMP_Text textObject1, TMP_Text textObject2)
    {
        textObject1.gameObject.SetActive(true);
        textObject2.gameObject.SetActive(false);

        if (_inputBindings.UI.Continue.triggered)
        {
            textObject1.gameObject.SetActive(false);
            textObject2.gameObject.SetActive(true);
            yield return new WaitForSeconds(5f);
            textObject2.gameObject.SetActive(false);
        }
        else if (_inputBindings.UI.Return.triggered)
        {
            textObject1.gameObject.SetActive(true);
            textObject2.gameObject.SetActive(false);
        }
    } 
}

