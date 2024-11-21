using System.Collections.Generic;
using SpatialSys.UnitySDK;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class GameStack : MonoBehaviour
{
    protected bool initialized = false;
    [SerializeField]
    protected GameObject slotsContainer;
    public GameObject slotPrefab;
    protected Vector3 slotOffset;
    public List<StackSlot> Slots { get; protected set; }
    [field: SerializeField]
    public int MaxSize { get; protected set; }
    [field: SerializeField, ReadOnly]
    public int Count { get; protected set; } = 0;
    public StackSlot targetSlot = null;
    public delegate void OnInteract(StackSlot slot);
    public OnInteract onInteract;

    public void Start()
    {
        if (initialized) return;
        initialized = true;

        if (slotPrefab != null)
        {
            Slots = new();
            slotOffset = slotsContainer.transform.GetChild(0).localPosition;
            while (Slots.Count < MaxSize)
            {
                AddSlot();
            }
        }
        else
        {
            Slots = Utils.GetChildren<StackSlot>(slotsContainer.transform);
            foreach (var slot in Slots)
            {
                slot.onInteract += (s) => onInteract?.Invoke(s);
                UpdateSlotPosition(slot);
            }
        }

        for (int i = 0; i < Slots.Count; i++)
        {
            var slot = Slots[i];
            slot.Parent = this;
            if (slot.interactable != null)
                slot.interactable.enabled = false;
            if (slot.outline != null)
                slot.outline.enabled = false;
            slot.index = i;
        }

        onInteract ??= DefaultOnInteract;
    }

    protected void Update()
    {
        //StackSlot newTargetSlot = GetTargetSlot();
        //if (targetSlot != newTargetSlot)
        //{
        //    var _targetSlot = targetSlot;
        //    targetSlot = newTargetSlot;
        //    UpdateSlot(_targetSlot);
        //    UpdateSlot(newTargetSlot);
        //}

        targetSlot = GetTargetSlot();
        foreach (var slot in Slots)
        {
            UpdateSlot(slot);
            //UpdateSlotPosition(slot);
        }
    }
    protected StackSlot GetTargetSlot()
    {
        var player = SpatialBridge.actorService.localActor.avatar;
        var playerPos = player.position;
        StackSlot slot = null;
        if (Raycast.HasHit)
        {
            slot = FindSlot(Raycast.Hit.transform.gameObject);
            if (slot != null && !Utils.IsInteractableInRange(slot.interactable, playerPos))
            {
                slot = null;
            }
        }
        return slot;
    }
    protected void UpdateSlot(StackSlot slot)
    {
        if (slot == null) return;
        var isTarget = slot == targetSlot;
        if (slot.interactable != null)
            slot.interactable.enabled = isTarget;
        if (slot.outline != null)
            slot.outline.enabled = isTarget;
    }

    protected void DefaultOnInteract(StackSlot slot)
    {
        //TODO
    }

    public bool Push(GameObject item, bool resetTransform = false)
    {
        if (Count >= MaxSize) return false;
        StackSlot slot = Slots[Count];
        slot.Insert(item, resetTransform);
        Count++;
        return true;
    }
    public bool Pop()
    {
        if (Count <= 0) return false;
        Slots[Count - 1].Remove();
        Count--;
        return true;
    }
    public GameObject Top
    {
        get
        {
            if (Count <= 0) return null;
            return Slots[Count - 1].Item;
        }
    }
    public StackSlot TopSlot
    {
        get
        {
            if (Count <= 0) return null;
            return Slots[Count - 1];
        }
    }

    public void UpdateSlotPosition(StackSlot slot)
    {
        if (slotPrefab == null) return;
        slot.transform.localPosition = slotOffset * slot.index;
    }

    protected StackSlot MakeSlot()
    {
        StackSlot slot;
        if (slotPrefab == null)
        {
            if (Slots.Count == 0)
            {
                var go = new GameObject();
                slot = go.AddComponent<StackSlot>();
            }
            else
            {
                var go = Instantiate(Slots[0].gameObject, slotsContainer.transform);
                Utils.ClearChildren(go.transform);
                slot = GetComponent<StackSlot>();
            }
        }
        else
        {
            slot = Instantiate(slotPrefab, slotsContainer.transform).GetComponent<StackSlot>();
            slot.gameObject.SetActive(true);
        }
        slot.Parent = this;
        slot.onInteract += (s) => onInteract?.Invoke(s);
        return slot;
    }

    public StackSlot AddSlot()
    {
        var slot = MakeSlot();
        slot.index = Slots.Count;
        Slots.Add(slot);
        UpdateSlotPosition(slot);
        return slot;
    }

    public StackSlot FindSlot(GameObject item)
    {
        var target = item.transform;
        StackSlot slot;
        while ((slot = target.GetComponent<StackSlot>()) == null)
        {
            var parent = target.transform.parent;
            if (parent == null)
            {
                return null;
            }
            target = parent;
        }
        return slot;
        //return Slots.Find(s => s.Item == item);
    }
}
