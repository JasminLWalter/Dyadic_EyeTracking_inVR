using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BoxBehaviour : MonoBehaviour
{
    // In scene hierarchy: Box folder (empty Gameobject) > Box GameObject > Reward Display Canvas
    // Assign this script to the Box GameObject 
    private GameManager gameManager;

    // Appearance variables
    private Material defaultMaterial;
    [Tooltip("How the box should appear, when it is pointed at by the receiver.")]
    [SerializeField] private Material highlightMaterial;
    private TextMeshPro rewardText;

    // Start is called before the first frame update
    // Used to assign the still missing attributes
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        defaultMaterial = gameObject.GetComponent<MeshRenderer>().material;
        rewardText = gameObject.transform.GetComponentInChildren<TextMeshPro>();
    }


    // This function is called by the GameManager.
    // It changes the reward display on the current box.
    public void ChangeReward(int reward)
    {
        rewardText.text = reward.ToString();
    }

    // This function is called by the ReceiverManager, when the box is being pointed at.
    // It changes the material of the box to the highlight material.
    public void PointedAt()
    {
        gameObject.GetComponent<MeshRenderer>().material = highlightMaterial;
    }

    // This function is called by the ReceiverManager, when the box is selected by the receiver.
    // It stores the reward and updates the score.
    public void Selected()
    {
        gameManager.lslReceiverOutlets.lslOScore.push_sample( new int[] { int.Parse(rewardText.text) } );
        gameManager.UpdateScore(int.Parse(rewardText.text));
    }

    // This function is called by the ReceiverManager, when the box is not longer being pointed at.
    // It changes the material of the box back to the default material.
    public void NotLongerPointedAt()
    {
        gameObject.GetComponent<MeshRenderer>().material = defaultMaterial;
    }
}
