using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BallQueueGroup : ListGroup<BallQueue, BallQueue.Item>
{
    public List<BallQueue> Queues => Lists;
    public List<Color> colors;

    new void Start()
    {
        base.Start();

        for (int i = 0; i < Queues[0].MaxSize; i++)
        {
            Queues[0].Push(new BallQueue.Item(colors[i % colors.Count]), resetTransform: true);
        }

        foreach (var queue in Queues)
        {
            queue.FrontGhost.Insert(BallQueue.Item.MakeGhost(), resetTransform: true);
            queue.BackGhost.Insert(BallQueue.Item.MakeGhost(), resetTransform: true);
        }
    }
}
