using UnityEngine;

[DisallowMultipleComponent]
public class GameStack : GameList
{
    public GameSlot TopGhost { get; protected set; }

    public override void Start()
    {
        if (initialized) return;
        base.Start();
        TopGhost = AddGhost();
        UpdateGhosts();
    }

    public override void UpdateGhosts()
    {
        if (TopGhost) TopGhost.index = Count;
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
        Slots[Count - 1].Remove();
        Count--;
        UpdateGhosts();
        return true;
    }
    public virtual GameObject Top
    {
        get
        {
            if (Count <= 0) return null;
            return Slots[Count - 1].Item;
        }
    }
    public virtual GameSlot TopSlot
    {
        get
        {
            if (Count <= 0) return null;
            return Slots[Count - 1];
        }

    }
}