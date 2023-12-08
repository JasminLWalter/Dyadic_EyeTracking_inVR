using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raycast : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    private InputBindings _inputBindings;
    private Ray _ray;
    private Collider _lastHit;
    private int _layerMask = 1<<3;  // Only objects on Layer 3 should be considered


    // Start is called before the first frame update
    void Start()
    {
        _inputBindings = new InputBindings();
        _inputBindings.Player.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        var mousePosition = _inputBindings.Player.MouseGaze.ReadValue<Vector2>();
        // Creates a Ray from the mouse position
        _ray = playerCamera.ScreenPointToRay(mousePosition);
        Debug.DrawRay(_ray.origin, _ray.direction * 100);

        RaycastHit hitData;
        if (Physics.Raycast(_ray, out hitData, Mathf.Infinity, _layerMask))
        {   
            if (_lastHit == null)
            {
                _lastHit = hitData.collider;
                _lastHit.gameObject.SendMessage("StarredAt");
            }
            else if (_lastHit != null && _lastHit != hitData.collider)
            {
                Debug.Log("Hit something new: " + hitData.collider.name);
                _lastHit.gameObject.SendMessage("NotLongerStarredAt");
                _lastHit = hitData.collider;
                _lastHit.gameObject.SendMessage("StarredAt");
            }
            else if (_inputBindings.Player.Select.triggered)
            {
                Debug.Log("Selected " + _lastHit);
                _lastHit.gameObject.SendMessage("Selected");
            }
        }
        else 
        {
            if (_lastHit != null)
            {
                Debug.Log("Not longer starred at.");
                _lastHit.gameObject.SendMessage("NotLongerStarredAt");
                _lastHit = null;
            }
        }

    }
}
