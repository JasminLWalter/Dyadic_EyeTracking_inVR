using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raycast : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    private InputBindings _inputBindings;
    private Ray _ray;

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
    }
}
