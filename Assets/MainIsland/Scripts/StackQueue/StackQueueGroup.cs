using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.YamlDotNet.Core;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class StackQueueGroup : ListGroup<GameList<StackQueueGroup.Item>, StackQueueGroup.Item>
{
    public class Item : GameItem, IColoredItem
    {
        public bool isGhost = false;
        private Color color;
        public Color Color => color;
        public Item(Color color) : base(Instantiate(AssetManager.Load<GameObject>("Prefabs/Ball")))
        {
            this.color = color;
            GameObject.GetComponentInChildren<Renderer>(true).material.color = color;
        }

        public static Item MakeGhost()
        {
            var item = new Item(Color.clear);
            item.isGhost = true;
            var renderer = item.GameObject.GetComponentInChildren<Renderer>(true);
            var transparent = AssetManager.Load<Material>("Materials/Transparent");
            renderer.materials = Enumerable.Repeat(transparent, renderer.materials.Length).ToArray();
            return item;
        }
    }

    public SQStack stack1;
    public SQStack stack2;
    public SQQueue queue1;
    public SQQueue queue2;
    public List<Color> colors;
    public static readonly int[][] defaultItems = {
        new int[]{ 0, 1, 2, 3, 4 },
        new int[]{ 3, 1 },
        new int[]{ 2, 4, 0 },
        new int[]{ 2, 4, 0 },
    };

    new void Start()
    {
        base.Start();
        stack1 = Lists[0] as SQStack;
        stack2 = Lists[1] as SQStack;
        queue1 = Lists[2] as SQQueue;
        queue2 = Lists[3] as SQQueue;

        Populate(defaultItems);
    }

    public void Clear()
    {
        if (GrabManager.Grabbed && GrabManager.Grabbed.item is BookStack.Item)
        {
            GrabManager.Release().item.Destroy();
        }

        foreach (var list in Lists)
        {
            list.Clear();
        }
    }
    public void Populate(int[][] items)
    {
        for (int i = 0; i < items.Length; i++)
        {
            for (int j = 0; j < items[i].Length; j++)
            {
                var item = new Item(colors[items[i][j]]);
                Lists[i].Set(j, item, resetTransform: true);
            }
        }
    }

}
