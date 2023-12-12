using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public TMP_Text textToShow1;
    public TMP_Text textToShow2;
    public TMP_Text Instructions2;

    public Player player;
    public GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        textToShow2.gameObject.SetActive(false);
        Instructions2.gameObject.SetActive(false);

        if (player == null)
        {
            Debug.LogError("Player reference not set in the Inspector!");
            return;
        }
    }

    void Update()
    {
        if (player != null && gameManager.GetCurrentPhase() == 0)
        {
            StartCoroutine(ShowTextTemporarily(textToShow1, 3f, textToShow2));

            if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                ShowText(Instructions2);
            }
        }
    }

    void ShowText(TMP_Text textComponent)
    {
        textComponent.gameObject.SetActive(true);
    }

    IEnumerator ShowTextTemporarily(TMP_Text textComponent, float duration, TMP_Text textComponent2)
    {
        yield return new WaitForSeconds(duration);
        textComponent.gameObject.SetActive(false);
        ShowText(textComponent2);
    }
}
