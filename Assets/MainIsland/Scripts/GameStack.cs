using UnityEngine;

[DisallowMultipleComponent]
public class GameStack<T> : GameList<T> where T : GameItem
{
    public GameSlot<T> TopGhost { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        TopGhost = AddGhost();
        UpdateGhosts();
    }

    public override void UpdateGhosts()
    {
        if (TopGhost == null) return;

        if (Count <= 0)
        {
            TopGhost.index = 0;
        }
        else
        {
            TopGhost.index = Slots[Count - 1].index + 1;
            TopGhost.transform.localPosition = TopSlot.transform.localPosition + slotOffset;
        }

        base.UpdateGhosts();
    }
    public enum Warning
    {
        Full,
        Empty,
        GrabNotTop,
        ReleaseNotTop,
        WrongGroup
    }
    protected virtual void ShowWarning(Warning w, GameSlot<T> slot)
    {
        var message = w switch
        {
            Warning.Full => "A pilha já está cheia",
            Warning.Empty => "Não há o que retirar",
            Warning.GrabNotTop => "É possível apenas pegar do topo",
            Warning.ReleaseNotTop => "Não pode colocar algo fora do topo",
            Warning.WrongGroup => "Não pode colocar aqui",
        };
        base.ShowWarning(message, slot);
    }
    protected override bool OnInteract(GameSlot<T> slot)
    {
        if (GrabManager.Grabbed)
        {
            if (!ReferenceEquals(GrabManager.Grabbed.group, grabGroup)
                || GrabManager.Grabbed.item is not T)
            {
                ShowWarning(Warning.WrongGroup, slot);
                return false;
            }
            if (TopGhost != slot)
            {
                ShowWarning(Warning.ReleaseNotTop, slot);
                return false;
            }
            if (IsFull())
            {
                ShowWarning(Warning.Full, slot);
                return false;
            }

            var item = (T)GrabManager.Release().item;
            Push(item);
        }
        else
        {
            if (TopSlot != slot)
            {
                ShowWarning(Warning.GrabNotTop, slot);
                return false;
            }
            if (IsEmpty())
            {
                ShowWarning(Warning.Empty, slot);
                return false;
            }
            var item = slot.Item;
            Pop();

            //TODO
            //foreach (var s in Stacks)
            //{
            //    foreach (var ghostSlot in s.ghostSlots)
            //    {
            //        ghostSlot.transform.rotation = GetRandomRotation();
            //    }
            //}

            GrabManager.Grab(new GrabManager.GrabInfo(item)
            {
                group = grabGroup,
                area = grabArea,
                areaExitHandler = () =>
                {
                    GrabManager.Release();
                    Push(item, resetTransform: true);
                }
            });
        }

        return true;
    }

    public virtual bool Push(T item, bool resetTransform = false)
    {
        if (Count >= MaxSize) return false;
        GameSlot<T> slot = Slots[Count];
        slot.Insert(item, resetTransform: resetTransform);
        Count++;
        slot.transform.localPosition = TopGhost.transform.localPosition;
        UpdateGhosts();
        item.Transform.GetComponentInChildren<Collider>(true).enabled = true;
        item.Transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        return true;
    }
    public virtual bool Pop()
    {
        if (Count <= 0) return false;
        var item = Slots[Count - 1].Extract();
        Count--;
        UpdateGhosts();
        item.Transform.GetComponentInChildren<Collider>(true).enabled = false;
        return true;
    }
    public virtual GameObject Top
    {
        get
        {
            if (Count <= 0) return null;
            return Slots[Count - 1].ItemGameObject;
        }
    }
    public virtual GameSlot<T> TopSlot
    {
        get
        {
            if (Count <= 0) return null;
            return Slots[Count - 1];
        }

    }
}