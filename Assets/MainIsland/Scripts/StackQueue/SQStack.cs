using System.Linq;
using UnityEngine;

using Item = StackQueueGroup.Item;

public class SQStack : GameStack<Item>
{
    protected override void Awake()
    {
        base.Awake();

        TopGhost.Insert(Item.MakeGhost(), resetTransform: true);
        TopGhost.transform.rotation = Quaternion.Euler(0, Random.value * 360, 0);
    }
    public override void UpdateSlotPosition(GameSlot<Item> slot)
    {
        ResetSlotPosition(slot);
    }

    protected override bool OnInteract(GameSlot<Item> slot)
    {
        var isPush = !!GrabManager.Grabbed;
        var ok = base.OnInteract(slot);
        if (ok && isPush)
        {
            TopSlot.transform.rotation = TopGhost.transform.rotation;
            TopGhost.transform.rotation = Quaternion.Euler(0, Random.value * 360, 0);
        }
        return ok;
    }
}
