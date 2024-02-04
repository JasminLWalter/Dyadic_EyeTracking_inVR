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
    public TMP_Text TextPhase4;
    public TMP_Text TextPhase6;

    public Player player;
    public GameManager gameManager;

    private InputBindings _inputBindings;

    private bool phase0CoroutineRunning = false;
    private bool phase2CoroutineRunning = false;
    private bool ShowedStart = false;

    private int currentTextIndex = 0;


    private void Start()
    {
        _inputBindings = new InputBindings();
        _inputBindings.UI.Enable();
        
        gameManager = FindObjectOfType<GameManager>();

        TextPhase4.gameObject.SetActive(false);
        TextPhase6.gameObject.SetActive(false);

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
            StartCoroutine(ShowTexts(TextsPhase2, () => phase2CoroutineRunning = false));
            phase2CoroutineRunning = true;
        }
        if (gameManager.GetCurrentPhase() == 4)
        {
            TextPhase4.gameObject.SetActive(true);
            if (_inputBindings.UI.Continue.triggered)
            {
                gameManager.EnterNextPhase();
                TextPhase4.gameObject.SetActive(false);
            }
        }
        if (gameManager.GetCurrentPhase() == 6)
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
            Debug.Log("Hides");
        }

        if (gameManager.GetCurrentPhase() != 4)
        {
            TextPhase4.gameObject.SetActive(false);

        }

        if (gameManager.GetCurrentPhase() != 6)
        {
            TextPhase6.gameObject.SetActive(false);

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

}

