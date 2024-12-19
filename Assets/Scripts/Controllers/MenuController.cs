using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    private MenuInputActions menuActions;
    // Start is called before the first frame update
    protected void Awake()
    {
        menuActions = new MenuInputActions();
    }

    protected void OnEnable()
    {

        menuActions.Enable();
        menuActions.MenuActions.toggleMenu.performed += (InputAction.CallbackContext context) =>
        {
            print("abrir menu");
        };
    }
}
