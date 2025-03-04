using System.Linq;
using UnityEngine;

public class BallQueue : GameQueue<BallQueue.Item>
{
    public class Item : GameItem
    {
        private Color color;
        public Color Color => color;
        public Item(Color color) : base(Object.Instantiate(AssetManager.Load<GameObject>("Prefabs/Ball")))
        {
            this.color = color;
            GameObject.GetComponentInChildren<Renderer>(true).material.color = color;
        }
        public static Item MakeGhost()
        {
            var item = new Item(Color.clear);
            var renderer = item.GameObject.GetComponentInChildren<Renderer>(true);
            var transparent = AssetManager.Load<Material>("Materials/Transparent");
            renderer.materials = Enumerable.Repeat(transparent, renderer.materials.Length).ToArray();
            return item;
        }
    }
    protected override void Awake()
    {
        base.Awake();

        FrontGhost.Insert(Item.MakeGhost(), resetTransform: true);
        BackGhost.Insert(Item.MakeGhost(), resetTransform: true);
    }
}
