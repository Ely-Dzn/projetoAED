using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[DisallowMultipleComponent]
public class VSOutlineUpdater : MonoBehaviour
{
    VariableDeclarations variables;
    Outline outline;

    void Start()
    {
        variables = gameObject.GetOrAddComponent<Variables>().declarations;
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
        return GetVarSafe("outline_enabled", true);
    }
    Outline.Mode GetMode()
    {
        int val = GetVarSafe("outline_mode", 0);
        if (!Enum.IsDefined(typeof(Outline.Mode), val))
        {
            val = 0;
        }
        return (Outline.Mode)val;
    }
    T GetVarSafe<T>(string variable, T defaultValue)
    {
        try
        {
            return variables.Get<T>(variable);
        }
        catch { }
        return defaultValue;
    }
}