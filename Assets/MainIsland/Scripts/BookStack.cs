using UnityEngine;

public class BookStack : GameStack
{
    public StackSlot ghostSlot;
    private bool pushMode = false;
    public bool PushMode
    {
        get => pushMode;
        set
        {
            pushMode = value;
            foreach (var slot in Slots)
            {
                if (slot.interactable != null)
                    slot.interactable.interactText = pushMode ? "Colocar" : "Retirar";
            }
            UpdateGhost();
        }
    }

    protected new void Start()
    {
        base.Start();
        PushMode = false;
    }

    protected new void Update()
    {
        base.Update();
        if (pushMode)
        {
            UpdateGhost();
        }
    }

    public void SetGhost(GameObject book)
    {
        ghostSlot = MakeSlot();
        ghostSlot.Insert(book, resetTransform: true);
        ghostSlot.gameObject.SetActive(true);
        UpdateGhost();
    }

    public void UpdateGhost()
    {
        if (ghostSlot == null) return;
        if (pushMode)
        {
            ghostSlot.index = Count;
            UpdateSlot(ghostSlot);
            UpdateSlotPosition(ghostSlot);
        }
        ghostSlot.gameObject.SetActive(pushMode);
    }


    public new bool Push(GameObject item, bool resetTransform = false)
    {
        try { return base.Push(item, resetTransform); } finally { UpdateGhost(); }
    }
    public new bool Pop()
    {
        try { return base.Pop(); } finally { UpdateGhost(); }
    }

}
