using UnityEngine;

public class TestQueue : GameList
{
    public TestQueueGroup group;
    public GameSlot BackGhost { get; protected set; }


    protected override void Awake()
    {
        base.Awake();
        BackGhost = AddGhost();
        UpdateGhosts();
    }

    public override void UpdateGhosts()
    {
        // Caso esta classe ainda não tenha sido inicializada
        if (BackGhost == null) return;

        if (Count <= 0)
        {
            BackGhost.index = 0;
        }
        else
        {
            BackGhost.index = Slots[Count - 1].index + 1;
            BackGhost.transform.localPosition = BackSlot.transform.localPosition + slotOffset;
        }

        base.UpdateGhosts();

    }
    protected override bool OnInteract(GameSlot slot)
    {
        if (group.Grabbed)
        {
            if (BackGhost != slot)
            {
                return false;
            }
            if (IsFull())
            {
                return false;
            }

            var item = group.Grabbed.transform;
            group.Grabbed = null;
            Push(item.gameObject);

            item.GetComponentInChildren<Collider>(true).enabled = true;
            item.localPosition = Vector3.zero;
            item.rotation = slot.transform.rotation;
        }
        else
        {
            if (IsEmpty())
            {
                return false;
            }
            var item = slot.Item;
            Remove(slot);

            group.Grabbed = item;
            item.GetComponentInChildren<Collider>(true).enabled = false;
        }

        return true;
    }

    protected override bool PlayerCanRelease()
    {
        return group != null ? group.Grabbed : null != null;
    }
    protected override void UpdateInteractText(GameSlot slot)
    {
        if (slot.interactable != null)
            slot.interactable.interactText = PlayerCanRelease() ? "push" : "pop";
    }

    public virtual bool Push(GameObject item, bool resetTransform = false)
    {
        if (Count >= MaxSize) return false;
        GameSlot slot = Slots[Count];
        slot.Insert(item, resetTransform);
        Count++;
        slot.transform.localPosition = BackGhost.transform.localPosition;
        UpdateGhosts();
        return true;
    }
    public virtual bool Remove(GameSlot slot)
    {
        if (Count <= 0) return false;
        slot.Extract();
        Slots.Remove(slot);
        Slots.Add(slot);
        Count--;
        for (int i = 0; i < Slots.Count; i++)
        {
            Slots[i].index = i;
        }
        //slot.index = Slots[Count - 1].index + 1;
        UpdateGhosts();
        return true;
    }

    public virtual GameObject Front
    {
        get
        {
            if (Count <= 0) return null;
            return Slots[0].Item;
        }
    }
    public virtual GameSlot FrontSlot
    {
        get
        {
            if (Count <= 0) return null;
            return Slots[0];
        }
    }
    public virtual GameObject Back
    {
        get
        {
            if (Count <= 0) return null;
            return Slots[Count - 1].Item;
        }
    }
    public virtual GameSlot BackSlot
    {
        get
        {
            if (Count <= 0) return null;
            return Slots[Count - 1];
        }
    }
}
