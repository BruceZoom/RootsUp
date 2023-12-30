//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.7.0
//     from Assets/Scripts/Controls/EditInputActions.inputactions
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

public partial class @EditInputActions: IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @EditInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""EditInputActions"",
    ""maps"": [
        {
            ""name"": ""Edit"",
            ""id"": ""1d010e53-cffb-4396-b5a1-cd66b4435ea6"",
            ""actions"": [
                {
                    ""name"": ""MouseClick"",
                    ""type"": ""Button"",
                    ""id"": ""317534d7-eee7-4038-b554-bbe24cdb37c3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""MousePosition"",
                    ""type"": ""Value"",
                    ""id"": ""2c3e8aad-f725-4970-83cf-e2432234de31"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""c2459245-9a51-4211-8d96-445da1aaf0b5"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""93c04665-bcd5-4756-9d33-85c135909839"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MousePosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Edit
        m_Edit = asset.FindActionMap("Edit", throwIfNotFound: true);
        m_Edit_MouseClick = m_Edit.FindAction("MouseClick", throwIfNotFound: true);
        m_Edit_MousePosition = m_Edit.FindAction("MousePosition", throwIfNotFound: true);
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

    // Edit
    private readonly InputActionMap m_Edit;
    private List<IEditActions> m_EditActionsCallbackInterfaces = new List<IEditActions>();
    private readonly InputAction m_Edit_MouseClick;
    private readonly InputAction m_Edit_MousePosition;
    public struct EditActions
    {
        private @EditInputActions m_Wrapper;
        public EditActions(@EditInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @MouseClick => m_Wrapper.m_Edit_MouseClick;
        public InputAction @MousePosition => m_Wrapper.m_Edit_MousePosition;
        public InputActionMap Get() { return m_Wrapper.m_Edit; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(EditActions set) { return set.Get(); }
        public void AddCallbacks(IEditActions instance)
        {
            if (instance == null || m_Wrapper.m_EditActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_EditActionsCallbackInterfaces.Add(instance);
            @MouseClick.started += instance.OnMouseClick;
            @MouseClick.performed += instance.OnMouseClick;
            @MouseClick.canceled += instance.OnMouseClick;
            @MousePosition.started += instance.OnMousePosition;
            @MousePosition.performed += instance.OnMousePosition;
            @MousePosition.canceled += instance.OnMousePosition;
        }

        private void UnregisterCallbacks(IEditActions instance)
        {
            @MouseClick.started -= instance.OnMouseClick;
            @MouseClick.performed -= instance.OnMouseClick;
            @MouseClick.canceled -= instance.OnMouseClick;
            @MousePosition.started -= instance.OnMousePosition;
            @MousePosition.performed -= instance.OnMousePosition;
            @MousePosition.canceled -= instance.OnMousePosition;
        }

        public void RemoveCallbacks(IEditActions instance)
        {
            if (m_Wrapper.m_EditActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IEditActions instance)
        {
            foreach (var item in m_Wrapper.m_EditActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_EditActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    public EditActions @Edit => new EditActions(this);
    public interface IEditActions
    {
        void OnMouseClick(InputAction.CallbackContext context);
        void OnMousePosition(InputAction.CallbackContext context);
    }
}