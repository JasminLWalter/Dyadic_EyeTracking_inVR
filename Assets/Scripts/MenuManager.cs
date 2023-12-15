using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    public List<TMP_Text> TextsPhase1;
    public TMP_Text TextPhase1;
    public List<TMP_Text> TextsPhase3;
    public TMP_Text TextPhase6;

    public Player player;
    public GameManager gameManager;

    private InputBindings _inputBindings;

    private bool phase1CoroutineRunning = false;
    private bool phase3CoroutineRunning = false;
    private bool ShowedStart = false;

    private int currentTextIndex = 0;


    private void Start()
    {
        _inputBindings = new InputBindings();
        _inputBindings.Player.Enable();
        
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
    }

    void Update()
    {
        if (gameManager.GetCurrentPhase() == 1 && !phase1CoroutineRunning)
        {
            if (ShowedStart == false)
            {
                StartCoroutine(ShowStart());
            }
            else
            {
                StartCoroutine(ShowTexts(TextsPhase1, () => phase1CoroutineRunning = false));
                phase1CoroutineRunning = true;
            }
        }
        else if (gameManager.GetCurrentPhase() == 3 && !phase3CoroutineRunning)
        {
            StartCoroutine(ShowTexts(TextsPhase3, () => phase3CoroutineRunning = false));
            phase3CoroutineRunning = true;
        }
        else if (gameManager.GetCurrentPhase() == 6)
        {
            TextPhase6.gameObject.SetActive(true);
        }
        else if (_inputBindings.Player.Skip.triggered)
        {
            gameManager.EnterNextPhase();
        }
    }
    

    IEnumerator ShowTexts(List<TMP_Text> textComponents, Action coroutineFinishedCallback)
    {
        int currentTextIndex = 0;

        while (currentTextIndex < textComponents.Count)
        {
            textComponents[currentTextIndex].gameObject.SetActive(true);

            if (_inputBindings.Player.Return.triggered && currentTextIndex > 0)
            {
                textComponents[currentTextIndex].gameObject.SetActive(false);
                currentTextIndex -= 1;
                textComponents[currentTextIndex].gameObject.SetActive(true);
            }
            else if (_inputBindings.Player.Continue.triggered)
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
        TextPhase1.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        TextPhase1.gameObject.SetActive(false);
        ShowedStart = true;
    }

}

