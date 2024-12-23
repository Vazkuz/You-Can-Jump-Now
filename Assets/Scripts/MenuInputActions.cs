//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.8.1
//     from Assets/Scripts/MenuInputActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine;

public partial class @MenuInputActions: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @MenuInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""MenuInputActions"",
    ""maps"": [
        {
            ""name"": ""MenuActions"",
            ""id"": ""56cfacea-cb50-491b-9615-74f7947650fe"",
            ""actions"": [
                {
                    ""name"": ""toggleMenu"",
                    ""type"": ""Button"",
                    ""id"": ""0afba417-c668-499d-9c71-1caf7c4d7823"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""d05e1066-3a77-46ca-b421-e4a78ca10370"",
                    ""path"": ""<Gamepad>/start"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""toggleMenu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""be7f5ca5-44a3-4a17-8b46-5c401cb39283"",
                    ""path"": ""<Keyboard>/escape"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""toggleMenu"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // MenuActions
        m_MenuActions = asset.FindActionMap("MenuActions", throwIfNotFound: true);
        m_MenuActions_toggleMenu = m_MenuActions.FindAction("toggleMenu", throwIfNotFound: true);
    }

    ~@MenuInputActions()
    {
        Debug.Assert(!m_MenuActions.enabled, "This will cause a leak and performance issues, MenuInputActions.MenuActions.Disable() has not been called.");
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // MenuActions
    private readonly InputActionMap m_MenuActions;
    private List<IMenuActionsActions> m_MenuActionsActionsCallbackInterfaces = new List<IMenuActionsActions>();
    private readonly InputAction m_MenuActions_toggleMenu;
    public struct MenuActionsActions
    {
        private @MenuInputActions m_Wrapper;
        public MenuActionsActions(@MenuInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @toggleMenu => m_Wrapper.m_MenuActions_toggleMenu;
        public InputActionMap Get() { return m_Wrapper.m_MenuActions; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(MenuActionsActions set) { return set.Get(); }
        public void AddCallbacks(IMenuActionsActions instance)
        {
            if (instance == null || m_Wrapper.m_MenuActionsActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_MenuActionsActionsCallbackInterfaces.Add(instance);
            @toggleMenu.started += instance.OnToggleMenu;
            @toggleMenu.performed += instance.OnToggleMenu;
            @toggleMenu.canceled += instance.OnToggleMenu;
        }

        private void UnregisterCallbacks(IMenuActionsActions instance)
        {
            @toggleMenu.started -= instance.OnToggleMenu;
            @toggleMenu.performed -= instance.OnToggleMenu;
            @toggleMenu.canceled -= instance.OnToggleMenu;
        }

        public void RemoveCallbacks(IMenuActionsActions instance)
        {
            if (m_Wrapper.m_MenuActionsActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IMenuActionsActions instance)
        {
            foreach (var item in m_Wrapper.m_MenuActionsActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_MenuActionsActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public MenuActionsActions @MenuActions => new MenuActionsActions(this);
    public interface IMenuActionsActions
    {
        void OnToggleMenu(InputAction.CallbackContext context);
    }
}