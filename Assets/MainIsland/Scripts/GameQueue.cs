using UnityEngine;

public abstract class GameQueue : GameList
{
    public GameSlot FrontGhost { get; protected set; }
    public GameSlot BackGhost { get; protected set; }

    protected override void Awake()
    {
        base.Awake();
        FrontGhost = AddGhost();
        BackGhost = AddGhost();
        UpdateGhosts();
    }

    public override void UpdateGhosts()
    {
        // Caso esta classe ainda não tenha sido inicializada
        if (FrontGhost == null || BackGhost == null) return;

        FrontGhost.enabled = Count > 0;
        if (Count <= 0)
        {
            BackGhost.index = 0;
        }
        else
        {
            FrontGhost.index = Slots[0].index - 1;
            BackGhost.index = Slots[Count - 1].index + 1;
            FrontGhost.transform.localPosition = FrontSlot.transform.localPosition - slotOffset;
            BackGhost.transform.localPosition = BackSlot.transform.localPosition + slotOffset;
        }

        base.UpdateGhosts();

    }
    public enum Warning
    {
        Full,
        Empty,
        GrabNotFront,
        ReleaseNotBack,
        WrongGroup
    }
    protected virtual void ShowWarning(Warning w, GameSlot slot)
    {
        var message = w switch
        {
            Warning.Full => "A fila já está cheia",
            Warning.Empty => "Não há o que retirar",
            Warning.GrabNotFront => "É possível apenas pegar da frente",
            Warning.ReleaseNotBack => "Não pode colocar algo fora do fim da fila",
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
            if (BackGhost != slot)
            {
                ShowWarning(Warning.GrabNotFront, slot);
                return false;
            }
            if (IsFull())
            {
                ShowWarning(Warning.Full, slot);
                return false;
            }

            var item = GrabManager.Release().transform;
            Push(item.gameObject);

            item.GetComponentInChildren<Collider>(true).enabled = true;
            item.localPosition = Vector3.zero;
            item.rotation = slot.transform.rotation;
        }
        else
        {
            if (FrontSlot != slot)
            {
                ShowWarning(Warning.ReleaseNotBack, slot);
                return false;
            }
            if (IsEmpty())
            {
                ShowWarning(Warning.Empty, slot);
                return false;
            }
            var item = slot.Item;
            Pop();

            GrabManager.Grab(new GrabManager.GrabInfo(item)
            {
                group = grabGroup,
                area = grabArea,
                areaExitHandler = () =>
                {
                    Push(GrabManager.Release().gameObject, resetTransform: true);
                }
            });
            item.GetComponentInChildren<Collider>(true).enabled = false;
        }

        return true;
    }

    public virtual bool Push(GameObject item, bool resetTransform = false)
    {
        if (Count >= MaxSize) return false;
        GameSlot slot = Slots[Count];
        slot.Insert(item, resetTransform: resetTransform);
        Count++;
        slot.transform.localPosition = BackGhost.transform.localPosition;
        UpdateGhosts();
        return true;
    }
    public virtual bool Pop()
    {
        if (Count <= 0) return false;
        var slot = Slots[0];
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
