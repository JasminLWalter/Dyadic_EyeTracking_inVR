using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VRMap
{
    public Transform vrTarget;
    public Transform rigTarget;
    public Vector3 trackingPositionOffset;
    public Vector3 trackingRotationOffset;
    private Vector3 offset = new Vector3(-57.7999992f,-0.810000002f,-0.419999987f);

    public void Map()
    {

        rigTarget.position = vrTarget.TransformPoint(trackingPositionOffset);
        rigTarget.rotation = vrTarget.rotation * Quaternion.Euler(trackingRotationOffset);
    }
}
public class VRRig : MonoBehaviour
{
    public float turnSmoothness = 1.0f;
    public VRMap head;
    public VRMap bodyHip;
    public VRMap leftHand;
    public VRMap rightHand;

    public Transform headConstraint;
    public Transform bodyConstraint;

    public Vector3 headBodyOffset;
 
    
    
    // Start is called before the first frame update
    void Start()
    {
        headBodyOffset =   new Vector3(-0.0299999993f,-5.32000017f,1.22000003f);
        transform.position = headConstraint.position + headBodyOffset;
        transform.forward = Vector3.Lerp(transform.forward, Vector3.ProjectOnPlane(headConstraint.forward, Vector3.up).normalized, Time.deltaTime*turnSmoothness);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = headConstraint.position + headBodyOffset;
        Debug.LogError("HeadBodyOffset" + headBodyOffset);
        //transform.rotation = headConstraint.up;
        //transform.forward = Vector3.ProjectOnPlane(headConstraint.forward, Vector3.up).normalized;
        //transform.forward = Vector3.Lerp(transform.forward, Vector3.ProjectOnPlane(headConstraint.up, Vector3.up).normalized, Time.deltaTime*turnSmoothness);
        //transform.forward = Vector3.Lerp(transform.forward, Vector3.ProjectOnPlane(headConstraint.forward, Vector3.up).normalized, Time.deltaTime*turnSmoothness);


        head.Map();
        leftHand.Map();
        rightHand.Map();
    }
}
