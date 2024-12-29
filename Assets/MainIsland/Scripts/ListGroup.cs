using System;
using System.Collections.Generic;
using SpatialSys.UnitySDK;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class ListGroup<L, T> : MonoBehaviour
    where T : GameItem
    where L : GameList<T>
{
    [SerializeField]
    protected GameObject container;
    public List<L> Lists { get; protected set; }
    public Action OnInteractEvent;
    [SerializeField]
    private SpatialTriggerEvent grabArea;
    public SpatialTriggerEvent GrabArea
    {
        get
        {
            return grabArea;
        }
        set
        {
            grabArea = value;
            foreach (var list in Lists)
            {
                list.grabArea = value;
            }
        }
    }

    protected void Start()
    {
        Lists = Utils.GetChildren<L>(container.transform);

        foreach (var list in Lists)
        {
            list.OnInteractEvent += (slot, list) => OnInteractEvent?.Invoke();
            list.grabGroup = this;
            list.grabArea = grabArea;
        }
    }
}
