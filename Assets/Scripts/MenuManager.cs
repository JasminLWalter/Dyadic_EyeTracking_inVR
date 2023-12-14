using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    public List<TMP_Text> TextsPhase1;
    public List<TMP_Text> TextsPhase3;



    public Player player;
    public GameManager gameManager;

    private InputBindings _inputBindings;

    private void Start()
    {
        _inputBindings = new InputBindings();
        _inputBindings.Player.Enable();
        //_inputBindings.Player.Return.performed += ctx => ReturnToPreviousText();

        gameManager = FindObjectOfType<GameManager>();

        foreach (TMP_Text TextPhase1 in TextsPhase1)
        {
            TextPhase1.gameObject.SetActive(false);
        }

        foreach (TMP_Text TextPhase3 in TextsPhase3)
        {
            TextPhase3.gameObject.SetActive(false);
        }

        if (player == null)
        {
            Debug.LogError("Player reference not set in the Inspector!");
            return;
        }

        // Subscribe to the 'Continue' action performed event
        //_inputBindings.Player.Continue.performed += OnContinuePerformed;
    }
    //private int isCoroutineRunning = 0;

  
    //private int continueCounter = 0;

    private bool phase1CoroutineRunning = false;
    private bool phase3CoroutineRunning = false;

    void Update()
    {
        if (gameManager.GetCurrentPhase() == 1 && !phase1CoroutineRunning)
        {
            StartCoroutine(ShowTextTemporarily(TextsPhase1, 3f, () => phase1CoroutineRunning = false));
            phase1CoroutineRunning = true;
        }
        else if (gameManager.GetCurrentPhase() == 3 && !phase3CoroutineRunning)
        {
            StartCoroutine(ShowTextTemporarily(TextsPhase3, 3f, () => phase3CoroutineRunning = false));
            phase3CoroutineRunning = true;
        }
    }

    IEnumerator ShowTextTemporarily(List<TMP_Text> textComponents, float duration, Action coroutineFinishedCallback)
    {
        if (textComponents.Count < 3)
        {
            Debug.LogError("Insufficient text components in the list.");
            //isCoroutineRunning = false; // Reset the flag before exiting the coroutine
            yield break; // Exit the coroutine if there aren't enough components
        }

        textComponents[0].gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);

        textComponents[0].gameObject.SetActive(false);
        textComponents[1].gameObject.SetActive(true);

        yield return new WaitWhile(() => _inputBindings.Player.Continue.triggered == false);
        //continueButtonPressed = true;

        textComponents[1].gameObject.SetActive(false);
        textComponents[2].gameObject.SetActive(true);

        while (textComponents[2].gameObject.activeSelf)
        {
            yield return null; // Wait until the last text component becomes visible
            if (_inputBindings.Player.Continue.triggered)
            {
                textComponents[2].gameObject.SetActive(false);
                gameManager.EnterNextPhase();
                coroutineFinishedCallback?.Invoke(); // Callback to signal coroutine completion
            }
        }

    }


    /*private int currentTextIndex = 0;

    void ReturnToPreviousText()
    {
        if (currentTextIndex > 0)
        {
            textsToShow[currentTextIndex].gameObject.SetActive(false);
            currentTextIndex--;
            textsToShow[currentTextIndex].gameObject.SetActive(true);
        }
    }*/


}

