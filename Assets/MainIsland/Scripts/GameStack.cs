using UnityEngine;
using static UnityEditor.Progress;

[DisallowMultipleComponent]
public class GameStack : GameList
{
    public GameSlot TopGhost { get; protected set; }

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
    protected virtual void ShowWarning(Warning w, GameSlot slot)
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
    protected override bool OnInteract(GameSlot slot)
    {
        if (GrabManager.Grabbed)
        {
            if (!ReferenceEquals(GrabManager.Grabbed.group, grabGroup))
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

            var item = GrabManager.Release().transform;
            Push(item.gameObject);
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
                    Push(GrabManager.Release().gameObject, resetTransform: true);
                }
            });
        }

        return true;
    }

    public virtual bool Push(GameObject item, bool resetTransform = false)
    {
        if (Count >= MaxSize) return false;
        GameSlot slot = Slots[Count];
        slot.Insert(item, resetTransform: resetTransform);
        Count++;
        slot.transform.localPosition = TopGhost.transform.localPosition;
        UpdateGhosts();
        item.GetComponentInChildren<Collider>(true).enabled = true;
        item.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        return true;
    }
    public virtual bool Pop()
    {
        if (Count <= 0) return false;
        var item = Slots[Count - 1].Extract();
        Count--;
        UpdateGhosts();
        item.GetComponentInChildren<Collider>(true).enabled = false;
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