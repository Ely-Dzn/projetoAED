using System;
using Unity.VisualScripting;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Outline))]
public class VSOutlineUpdater : MonoBehaviour
{
    Variables variables;
    Outline outline;

    void Start()
    {
        variables = gameObject.GetOrAddComponent<Variables>();
        outline = GetComponent<Outline>();
        outline.OutlineMode = GetMode();
        outline.enabled = GetEnabled();
    }
    void Update()
    {
        var enabled = GetEnabled();
        if (enabled != outline.enabled)
        {
            outline.enabled = enabled;
        }
        var mode = GetMode();
        if (mode != outline.OutlineMode)
        {
            outline.OutlineMode = mode;
        }
    }

    bool GetEnabled()
    {
        return (bool)GetVarSafe("outline_enabled", true);
    }
    Outline.Mode GetMode()
    {
        int val = (int)GetVarSafe("outline_mode", 0);
        if (!Enum.IsDefined(typeof(Outline.Mode), val))
        {
            val = 0;
        }
        return (Outline.Mode)val;
    }
    object GetVarSafe(string variable, object defaultValue)
    {
        try
        {
            return variables.declarations.Get(variable);
        }
        catch { }
        return defaultValue;
    }
}