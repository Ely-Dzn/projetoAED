using System.Linq;
using UnityEngine;

using Item = StackQueueGroup.Item;

public class SQQueue : GameQueue<Item>
{
    protected override void Awake()
    {
        base.Awake();

        FrontGhost.Insert(Item.MakeGhost(), resetTransform: true);
        BackGhost.Insert(Item.MakeGhost(), resetTransform: true);
    }
}
