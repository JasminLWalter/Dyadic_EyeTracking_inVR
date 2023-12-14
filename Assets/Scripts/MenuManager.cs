using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
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
    private int isCoroutineRunning = 0;

    private bool continueButtonPressed = false;
    private int continueCounter = 0;

    void Update()
    {
        if (gameManager.GetCurrentPhase() == 1 && isCoroutineRunning == 0)
        {
            StartCoroutine(ShowTextTemporarily(TextsPhase1, 3f));
        }
        else if (gameManager.GetCurrentPhase() == 3 && isCoroutineRunning == 1)
        {
            StartCoroutine(ShowTextTemporarily(TextsPhase3, 3f));
        }
    }

    IEnumerator ShowTextTemporarily(List<TMP_Text> textComponents, float duration)
    {
        isCoroutineRunning += 1; // Set the flag to indicate the coroutine is running

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
        continueButtonPressed = true;

        textComponents[1].gameObject.SetActive(false);
        textComponents[2].gameObject.SetActive(true);

        while (textComponents[textComponents.Count - 1].gameObject.activeSelf)
        {
            yield return null; // Wait until textComponent[2] becomes visible
            if (_inputBindings.Player.Continue.triggered)
            {
                textComponents[2].gameObject.SetActive(false);
                gameManager.EnterNextPhase();
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

