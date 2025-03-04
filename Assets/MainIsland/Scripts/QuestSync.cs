using System;
using UnityEngine;
using System.Collections;

public class QuestSync : MonoBehaviour
{
    public static QuestSync Instance { get; private set; }

    [SerializeField]
    private string current = null;
    private Action stopHandler = null;

    void Awake()
    {
        Instance = this;
    }

    public string GetCurrent()
    {
        return current == "" ? null : current;
    }
    public void SetCurrent(string id, Action stopHandler = null)
    {
        if (GetCurrent() == null)
        {
            current = id;
            this.stopHandler = stopHandler;
        }
    }
    public void Clear(string id)
    {
        if (GetCurrent() == id)
        {
            current = null;
            stopHandler = null;
        }
    }
    public void Stop(string id)
    {
        if (GetCurrent() == id)
        {
            StopAny();
        }
    }
    public void StopAny()
    {
        current = null;
        stopHandler?.Invoke();
        stopHandler = null;
    }
}
