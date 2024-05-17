using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    public List<TMP_Text> TextsPhase0;
    public TMP_Text TextPhase0;
    public List<TMP_Text> TextsPhase2;
    public TMP_Text TextPhase6;
    public TMP_Text Pause;

    public Player player;
    public GameManager gameManager;

    private InputBindings _inputBindings;

    private bool phase0CoroutineRunning = false;
    private bool phase2CoroutineRunning = false;
    private bool ShowedStart = false;
    private bool ShowedText1 = false;

    private int currentTextIndex = 0;

    public TMP_Text TestingText1;
    public TMP_Text TestingText2;

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

        if (player == null)
        {
            Debug.LogError("Player reference not set in the Inspector!");
            return;
        }
    }

    void Update()
    {
        if (gameManager.GetCurrentPhase() == 0 && !phase0CoroutineRunning)
        {
            if (ShowedStart == false)
            {
                StartCoroutine(ShowStart());
            }
            else
            {
                StartCoroutine(ShowTexts(TextsPhase0, () => phase0CoroutineRunning = false));
                phase0CoroutineRunning = true;
            }

        }
        if (gameManager.GetCurrentPhase() == 2 && !phase2CoroutineRunning)
        {
            if (!ShowedText1)
            {
                StartCoroutine(ShowTwoTexts(TestingText1, TestingText2));
                ShowedText1 = true;
            }
            phase2CoroutineRunning = true;
            gameManager.EnterNextPhase();
            if (gameManager.role == "receiver")
            {
                xrOriginSetup.transform.rotation = Quaternion.Euler(0, 90, 0);
            }
        }
        if (gameManager.GetCurrentPhase() == 3)
        {
            StartCoroutine(ShowTwoTexts(TestingText1, TestingText2));
            
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
                    gameManager.EnterNextPhase();
                    coroutineFinishedCallback?.Invoke(); // Callback to signal coroutine completion
                }
            }

            yield return null;
        }
    } 

    private IEnumerator ShowStart()
    {
        TextPhase0.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        TextPhase0.gameObject.SetActive(false);
        ShowedStart = true;
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
        yield return new WaitForSeconds(2f);
        textObject1.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        textObject1.gameObject.SetActive(false);
        yield return new WaitForSeconds(1f);
        textObject2.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        textObject2.gameObject.SetActive(false);
        Debug.LogError("Finished showing two texts");  
        
    }

}

