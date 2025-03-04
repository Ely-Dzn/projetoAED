using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.YamlDotNet.Core;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class StackQueueGroup : ListGroup<GameList<StackQueueGroup.Item>, StackQueueGroup.Item>
{
    public class Item : GameItem
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

    public GameStack<Item> stack1;
    public GameStack<Item> stack2;
    public GameQueue<Item> queue1;
    public GameQueue<Item> queue2;
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
        stack1 = Lists[0] as GameStack<Item>;
        stack2 = Lists[1] as GameStack<Item>;
        queue1 = Lists[2] as GameQueue<Item>;
        queue2 = Lists[3] as GameQueue<Item>;

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
                item.Transform.rotation = GetRandomRotation();
            }
        }
    }

    private Quaternion GetRandomRotation()
    {
        return Quaternion.Euler(0, (Random.value - 0.5f) * 90f, 0);
    }

    //protected override Quaternion GetGrabbedRotation()
    //{
    //    var player = SpatialBridge.actorService.localActor.avatar;
    //    return Utils.QuaternionFromEuler(player.rotation.eulerAngles + new Vector3(90, 90, 90));
    //}

}
