using System;
using SpatialSys.UnitySDK;
using UnityEngine;

public class GameSlot : MonoBehaviour
{
    [SerializeField]
    private SpatialInteractable interactable;
    [SerializeField]
    private Outline outline;
    private object slot;
    public GameSlot<T> Get<T>() where T : GameItem
    {
        try
        {
            return (GameSlot<T>)slot;
        }
        catch
        {
            return null;
        }
    }

    public S Make<S, T>(S slot)
        where T : GameItem
        where S : GameSlot<T>
    {
        this.slot = slot;

        if (interactable != null)
            slot.interactable = interactable;
        else
            slot.interactable = GetComponentInChildren<SpatialInteractable>(true);
        if (slot.interactable != null)
        {
            slot.interactable.onInteractEvent += slot.SendOnInteract;
            _OnDestroy += () =>
            {
                slot.interactable.onInteractEvent -= slot.SendOnInteract;
            };
        }

        if (outline != null)
            slot.outline = outline;
        else
            slot.outline = GetComponentInChildren<Outline>(true);

        return slot;
    }

    Action _OnDestroy;
    void OnDestroy()
    {
        _OnDestroy?.Invoke();
    }
}

public class GameSlot<T> where T : GameItem
{
    public int index = 0;
    public bool enabled = true;
    private readonly GameSlot component;
    [field: SerializeField]
    public T Item { get; protected set; }
    public GameObject gameObject => component.gameObject;
    public Transform transform => component.transform;
    public bool IsFilled => Item != null;
    public SpatialInteractable interactable = null;
    public Outline outline = null;
    public delegate void InteractHandler(GameSlot<T> slot);
    public event InteractHandler OnInteractEvent;

    public GameSlot(GameSlot component)
    {
        this.component = component;
    }

    public void SendOnInteract()
    {
        OnInteractEvent?.Invoke(this);
    }

    public void Insert(T item, bool resetTransform = false)
    {
        Item = item;
        if (resetTransform)
            item.Transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        item.Transform.SetParent(transform, !resetTransform);
        if (outline == null)
            outline = transform.GetComponentInChildren<Outline>(true);
    }

    public GameItem Extract()
    {
        var oldItem = Item;
        Item = null;
        if (outline != null && outline.gameObject != gameObject)
            outline = null;
        return oldItem;
    }
}
