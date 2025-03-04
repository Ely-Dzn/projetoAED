using UnityEngine;
using System.Collections;
using System;
using SpatialSys.UnitySDK;

public class ToggleButton : MonoBehaviour
{
    public SpatialInteractable interactable;
    public GameObject modelOff;
    public GameObject modelOn;
    [SerializeField]
    private string _textOff = "Off";
    [SerializeField]
    private string _textOn = "On";
    public string TextOff
    {
        get { return _textOff; }
        set
        {
            _textOff = value;
            UpdateText();
        }
    }
    public string TextOn
    {
        get { return _textOn; }
        set
        {
            _textOn = value;
            UpdateText();
        }
    }

    public Action onToggle;

    [SerializeField]
    private bool state = false;
    public bool State
    {
        get { return state; }
        set
        {
            if (state != value)
            {
                state = value;
                UpdateText();
                modelOff.SetActive(!state);
                modelOn.SetActive(state);
            }
        }
    }

    private void Start()
    {
        interactable.onInteractEvent += HandleInteract;
        UpdateText();
    }

    private void UpdateText()
    {
        interactable.interactText = state ? _textOn : _textOff;
    }

    private void HandleInteract()
    {
        State = !State;
        onToggle?.Invoke();
    }
}
