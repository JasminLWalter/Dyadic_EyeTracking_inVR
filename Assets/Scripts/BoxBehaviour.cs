using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoxBehaviour : MonoBehaviour
{
    // In scene hierarchy: Box folder (empty Gameobject) > Box GameObject > Reward Display Canvas
    // Assign this script to the Box GameObject 
    private GameManager gameManager;
    private Material defaultMaterial;

    [Tooltip("How the box should appear, when it is starred at.")]
    [SerializeField] private Material highlightMaterial;

    private TextMeshPro rewardText;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        defaultMaterial = FindObjectOfType<MeshRenderer>().material;
        rewardText = gameObject.transform.GetComponentInChildren<TextMeshPro>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeReward(int reward)
    {
        rewardText.text = reward.ToString();
    }

    // When the box is starred at (= when the x-ray collides with the box)
    private void OnTriggerEnter(Collider xray)
    {
        FindObjectOfType<MeshRenderer>().material = highlightMaterial;
    }

    // While the box is starred at (= while the x-ray collides with the box)
    private void OnTriggerStay(Collider xray)
    {
        // TODO: 
        // If box is actually selected
        // gameManager.UpdateScore(int.Parse(rewardText.text));
    }

    // When the box is not longer starred at (= when the x-ray does not collide with the box anymore)
    private void OnTriggerExit(Collider xray)
    {
        FindObjectOfType<MeshRenderer>().material = defaultMaterial;
    }
}
