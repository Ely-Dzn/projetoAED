using System;
using System.Collections.Generic;
using SpatialSys.UnitySDK;
using Unity.VisualScripting;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class GameList<T> : MonoBehaviour where T : GameItem
{
    [SerializeField]
    protected GameObject slotsContainer;
    public GameObject slotPrefab;
    protected Vector3 slotOffset;
    public List<GameSlot<T>> Slots { get; protected set; }
    [field: SerializeField]
    public int MaxSize { get; protected set; }
    [field: SerializeField, ReadOnly]
    public int Count { get; protected set; } = 0;
    [field: SerializeField, ReadOnly]
    public GameSlot<T> targetSlot { get; protected set; } = null;
    public delegate void InteractHandler(GameSlot<T> slot, GameList<T> list);
    public event InteractHandler OnInteractEvent;
    public event InteractHandler OnInteractFailEvent;
    protected List<GameSlot<T>> ghostSlots = new();
    protected GameSlot<T> warnedSlot = null;
    public UnityEngine.Object grabGroup;
    public SpatialTriggerEvent grabArea = null;

    protected virtual void Awake()
    {
        grabGroup = this;
        Slots = new();

        if (slotPrefab != null)
        {
            slotOffset = slotsContainer.transform.GetChild(0).localPosition;
            while (Slots.Count < MaxSize)
            {
                //TODO: slots sob demanda
                AddSlot();
            }
        }
        else
        {
            Slots = Utils.GetChildren<GameSlot<T>>(slotsContainer.transform);
            if (Slots.Count == 0)
            {
                throw new System.Exception("Prefab de slot não definido mas não há slots preexistentes");
            }
            foreach (var slot in Slots)
            {
                slot.OnInteractEvent += _OnInteract;
                ResetSlotPosition(slot);
            }
        }

        for (int i = 0; i < Slots.Count; i++)
        {
            var slot = Slots[i];
            if (slot.interactable != null)
                slot.interactable.enabled = false;
            if (slot.outline != null)
                slot.outline.enabled = false;
            slot.index = i;
        }
    }

    protected virtual void Update()
    {
        targetSlot = GetTargetSlot();
        foreach (var slot in Slots)
        {
            UpdateSlot(slot);
            //UpdateSlotPosition(slot);
        }

        UpdateGhosts();

        slotOffset = slotsContainer.transform.GetChild(0).localPosition;
        foreach (var slot in Slots)
        {
            UpdateSlotPosition(slot);
        }
    }
    protected virtual GameSlot<T> GetTargetSlot()
    {
        GameSlot<T> slot = null;
        if (Raycast.HasHit && Raycast.Hit.transform)
        {
            slot = FindSlot(Raycast.Hit.transform.gameObject);
            if (slot != null && slot.interactable && !Utils.IsInteractableInRange(slot.interactable))
            {
                slot = null;
            }
        }
        return slot;
    }
    protected virtual void UpdateSlot(GameSlot<T> slot)
    {
        if (slot == null) return;
        var isTarget = slot == targetSlot;
        if (slot.interactable != null)
            slot.interactable.enabled = isTarget;
        if (slot.outline != null)
            slot.outline.enabled = isTarget;
        UpdateInteractText(slot);
    }

    protected virtual bool PlayerCanRelease()
    {
        var grabbed = GrabManager.Grabbed;
        return grabbed && ReferenceEquals(grabbed.group, grabGroup);
    }

    protected virtual bool OnInteract(GameSlot<T> slot)
    {
        if (slot == null) return false;
        if (GrabManager.Grabbed && !PlayerCanRelease()) return false;
        return true;
    }
    protected void _OnInteract(GameSlot<T> slot)
    {
        if (!OnInteract(slot))
        {
            OnInteractFailEvent?.Invoke(slot, this);
            return;
        }
        OnInteractEvent?.Invoke(slot, this);
    }

    public virtual void UpdateSlotPosition(GameSlot<T> slot)
    {
        if (slotPrefab == null) return;
        var target = slotOffset * slot.index;
        slot.transform.localPosition = Vector3.Lerp(
            slot.transform.localPosition,
            target,
            Time.deltaTime * 7f);
    }
    public virtual void ResetSlotPosition(GameSlot<T> slot)
    {
        if (slotPrefab == null) return;
        slot.transform.localPosition = slotOffset * slot.index;
    }
    protected virtual void UpdateInteractText(GameSlot<T> slot)
    {
        if (slot.interactable != null)
            slot.interactable.interactText = GrabManager.Grabbed ? "push" : "pop";
    }

    public virtual void Clear()
    {
        foreach (var slot in Slots)
        {
            if (slot.IsFilled) slot.Extract().Destroy();
        }
        Count = 0;
        foreach (var slot in Slots)
        {
            UpdateSlot(slot);
            ResetSlotPosition(slot);
        }
        UpdateGhosts();
    }

    protected virtual GameSlot<T> MakeSlot()
    {
        GameSlot<T> slot;
        if (slotPrefab == null)
        {
            if (Slots.Count == 0)
            {
                var go = new GameObject();
                var comp = go.AddComponent<GameSlot>();
                slot = comp.Make<GameSlot<T>, T>(new GameSlot<T>(comp));
            }
            else
            {
                var go = Instantiate(Slots[0].gameObject, slotsContainer.transform);
                Utils.ClearChildren(go.transform);
                var comp = go.GetOrAddComponent<GameSlot>();
                slot = comp.Make<GameSlot<T>, T>(new GameSlot<T>(comp));
            }
        }
        else
        {
            var obj = Instantiate(slotPrefab, slotsContainer.transform);
            var comp = obj.GetOrAddComponent<GameSlot>();
            slot = comp.Make<GameSlot<T>, T>(new GameSlot<T>(comp));
        }
        slot.OnInteractEvent += _OnInteract;
        return slot;
    }

    public virtual GameSlot<T> AddSlot()
    {
        var slot = MakeSlot();
        slot.index = Slots.Count;
        Slots.Add(slot);
        ResetSlotPosition(slot);
        return slot;
    }

    public virtual GameSlot<T> FindSlot(GameObject item)
    {
        var target = item.transform;
        GameSlot slotComponent;
        while ((slotComponent = target.GetComponent<GameSlot>()) == null)
        {
            var parent = target.transform.parent;
            if (parent == null)
            {
                return null;
            }
            target = parent;
        }
        return slotComponent.Get<T>();
        //return Slots.Find(s => s.Item == item);
    }

    public virtual bool Set(int idx, T item, bool resetTransform = false)
    {
        if (idx >= MaxSize) return false;
        GameSlot<T> slot = Slots[idx];
        slot.Insert(item, resetTransform: resetTransform);
        Count = Math.Max(Count, idx + 1);
        UpdateGhosts();
        foreach (var s in Slots)
        {
            UpdateSlotPosition(s);
        }
        item.Transform.GetComponentInChildren<Collider>(true).enabled = true;
        item.Transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        return true;
    }

    protected virtual GameSlot<T> AddGhost(T ghost = null)
    {
        var ghostSlot = MakeSlot();
        if (ghost)
        {
            ghostSlot.Insert(ghost, resetTransform: true);
        }
        ghostSlots.Add(ghostSlot);
        UpdateGhosts();
        return ghostSlot;
    }

    public virtual void UpdateGhosts()
    {
        foreach (var slot in ghostSlots)
        {
            bool canRelease = PlayerCanRelease();
            slot.gameObject.SetActive(slot.enabled && canRelease);
            if (canRelease)
            {
                UpdateSlot(slot);
                UpdateSlotPosition(slot);
            }
        }
    }
    public void ShowWarning(string text, GameSlot<T> slot)
    {
        ClearWarning();
        MessageDisplay.Instance.ShowWarning(text, slot.transform);
        warnedSlot = slot;
        if (warnedSlot.outline != null)
            warnedSlot.outline.OutlineColor = Color.red;
        Invoke(nameof(ClearWarning), 2f);
    }
    public void ClearWarning()
    {
        CancelInvoke(nameof(ClearWarning));
        if (warnedSlot != null)
        {
            if (warnedSlot.outline != null)
                warnedSlot.outline.OutlineColor = Color.white;
            warnedSlot = null;
        }
    }

    public bool IsEmpty()
    {
        return Count == 0;
    }
    public bool IsFull()
    {
        return Count >= MaxSize;
    }
}
