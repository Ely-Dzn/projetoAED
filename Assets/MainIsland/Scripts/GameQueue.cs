using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameQueue : GameList
{
    public GameSlot FrontGhost { get; protected set; }
    public GameSlot BackGhost { get; protected set; }

    public override void Start()
    {
        if (initialized) return;
        base.Start();
        FrontGhost = AddGhost();
        BackGhost = AddGhost();
        UpdateGhosts();
    }

    public override void UpdateGhosts()
    {
        if (FrontGhost && BackGhost)
        {
            FrontGhost.enabled = Count > 0;
            if (Count <= 0)
            {
                BackGhost.index = 0;
            }
            else
            {
                FrontGhost.index = Slots[0].index - 1;
                BackGhost.index = Slots[Count - 1].index + 1;
            }
        }
        base.UpdateGhosts();
    }

    public virtual bool Push(GameObject item, bool resetTransform = false)
    {
        if (Count >= MaxSize) return false;
        GameSlot slot = Slots[Count];
        slot.Insert(item, resetTransform);
        Count++;
        UpdateGhosts();
        return true;
    }
    public virtual bool Pop()
    {
        if (Count <= 0) return false;
        var slot = Slots[0];
        slot.Remove();
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
