using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    public TMP_Text textToShow1; // Reference to the first TMP_Text component in the Inspector
    public TMP_Text textToShow2; // Reference to the second TMP_Text component in the Inspector

    public Player player; // Reference to the PlayerTest script


    void Start()
    {

        textToShow2.gameObject.SetActive(false);

        // Ensure the playerTest variable is assigned in the Inspector or via code
        if (player == null)
        {
            Debug.LogError("PlayerTest reference not set in the Inspector!");
            return;
        }
    }

    void Update()
    {


        // Check if the player has reached the specific position using a method in the PlayerTest script
        if (player != null && player.IsAtSpecificPosition())
        {
            StartCoroutine(ShowTextTemporarily(textToShow1, 3f, textToShow2));
            Debug.Log("Start Coroutine.");
            Debug.Log("Player position: " + player.transform.position);
        }
    }

    // Function to show a text without any time constraints
    void ShowText(TMP_Text textComponent2)
    {
        textComponent2.gameObject.SetActive(true);
    }

    // Function to show a text temporarily for a specified duration
    IEnumerator ShowTextTemporarily(TMP_Text textComponent, float duration, TMP_Text textComponent2)
    {
        yield return new WaitForSeconds(duration);
        textComponent.gameObject.SetActive(false);
        ShowText(textComponent2);
    }
}
