using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BallQueueGroup : ListGroup<BallQueue, BallQueue.Item>
{
    public List<BallQueue> Queues => Lists;
    public List<Color> colors;
    public static readonly int[][] defaultItems = {
        new int[]{ 0, 1, 2, 3, 4 },
        new int[]{ 3, 1 },
        new int[]{ 2, 4, 0 },
    };

    new void Start()
    {
        base.Start();

        Populate(defaultItems);
    }

    public void Clear()
    {
        if (GrabManager.Grabbed && GrabManager.Grabbed.item is BookStack.Item)
        {
            GrabManager.Release().item.Destroy();
        }

        foreach (var q in Queues)
        {
            q.Clear();
        }
    }
    public void Populate(int[][] items)
    {
        for (int i = 0; i < items.Length; i++)
        {
            for (int j = 0; j < items[i].Length; j++)
            {
                var item = new BallQueue.Item(colors[items[i][j]]);
                Queues[i].Push(item, resetTransform: true);
            }
        }
    }
}
