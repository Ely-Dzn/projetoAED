using System;
using System.Collections.Generic;
using SpatialSys.UnitySDK;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class GameList : MonoBehaviour
{
    protected bool initialized = false;
    [SerializeField]
    protected GameObject slotsContainer;
    public GameObject slotPrefab;
    protected Vector3 slotOffset;
    public List<GameSlot> Slots { get; protected set; }
    [field: SerializeField]
    public int MaxSize { get; protected set; }
    [field: SerializeField, ReadOnly]
    public int Count { get; protected set; } = 0;
    public GameSlot targetSlot = null;
    public delegate void OnInteract(GameSlot slot);
    public OnInteract onInteract;

    public List<GameSlot> ghostSlots = new();
    protected bool pushMode = false;

    protected GameSlot warnedSlot = null;
    public virtual bool PushMode
    {
        get => pushMode;
        set
        {
            pushMode = value;
            UpdateGhosts();
        }
    }

    public virtual void Start()
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
            Slots = Utils.GetChildren<GameSlot>(slotsContainer.transform);
            if (Slots.Count == 0)
            {
                throw new System.Exception("Prefab de slot não definido nem há slots preexistentes");
            }
            foreach (var slot in Slots)
            {
                slot.onInteract += (s) => onInteract?.Invoke(s);
                ResetSlotPosition(slot);
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

        PushMode = false;
    }

    protected virtual void Update()
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

        if (pushMode)
        {
            UpdateGhosts();
        }

        slotOffset = slotsContainer.transform.GetChild(0).localPosition;
        foreach (var slot in Slots)
        {
            UpdateSlotPosition(slot);
        }
    }
    protected virtual GameSlot GetTargetSlot()
    {
        var player = SpatialBridge.actorService.localActor.avatar;
        var playerPos = player.position;
        GameSlot slot = null;
        if (Raycast.HasHit)
        {
            slot = FindSlot(Raycast.Hit.transform.gameObject);
            if (slot != null && slot.interactable && !Utils.IsInteractableInRange(slot.interactable, playerPos))
            {
                slot = null;
            }
        }
        return slot;
    }
    protected virtual void UpdateSlot(GameSlot slot)
    {
        if (slot == null) return;
        var isTarget = slot == targetSlot;
        if (slot.interactable != null)
            slot.interactable.enabled = isTarget;
        if (slot.outline != null)
            slot.outline.enabled = isTarget;
        UpdateInteractText(slot);
    }

    protected virtual void DefaultOnInteract(GameSlot slot)
    {
        //TODO
    }

    public virtual void UpdateSlotPosition(GameSlot slot)
    {
        if (slotPrefab == null) return;
        var target = slotOffset * slot.index;
        slot.transform.localPosition = Vector3.Lerp(
            slot.transform.localPosition,
            target,
            Time.deltaTime);
    }
    public virtual void ResetSlotPosition(GameSlot slot)
    {
        if (slotPrefab == null) return;
        slot.transform.localPosition = slotOffset * slot.index;
    }
    protected virtual void UpdateInteractText(GameSlot slot)
    {
        if (slot.interactable != null)
            slot.interactable.interactText = pushMode ? "push" : "pop";
    }

    protected virtual GameSlot MakeSlot()
    {
        GameSlot slot;
        if (slotPrefab == null)
        {
            if (Slots.Count == 0)
            {
                var go = new GameObject();
                slot = go.AddComponent<GameSlot>();
            }
            else
            {
                var go = Instantiate(Slots[0].gameObject, slotsContainer.transform);
                Utils.ClearChildren(go.transform);
                slot = GetComponent<GameSlot>();
            }
        }
        else
        {
            slot = Instantiate(slotPrefab, slotsContainer.transform).GetComponent<GameSlot>();
            slot.gameObject.SetActive(true);
        }
        slot.Parent = this;
        slot.onInteract += (s) => onInteract?.Invoke(s);
        return slot;
    }

    public virtual GameSlot AddSlot()
    {
        var slot = MakeSlot();
        slot.index = Slots.Count;
        Slots.Add(slot);
        ResetSlotPosition(slot);
        return slot;
    }

    public virtual GameSlot FindSlot(GameObject item)
    {
        var target = item.transform;
        GameSlot slot;
        while ((slot = target.GetComponent<GameSlot>()) == null)
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

    protected virtual GameSlot AddGhost(GameObject ghost = null)
    {
        var ghostSlot = MakeSlot();
        if (ghost)
        {
            ghostSlot.Insert(ghost, resetTransform: true);
        }
        ghostSlot.gameObject.SetActive(true);
        ghostSlots.Add(ghostSlot);
        UpdateGhosts();
        return ghostSlot;
    }

    public virtual void UpdateGhosts()
    {
        foreach (var slot in ghostSlots)
        {
            UpdateSlot(slot);
            UpdateSlotPosition(slot);
            slot.gameObject.SetActive(pushMode);
        }
    }
    public void ShowWarning(String text, GameSlot slot)
    {
        ClearWarning();
        MessageDisplay.ShowWarning(text, slot.transform);
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
}
