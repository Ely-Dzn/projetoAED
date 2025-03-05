using System.Linq;
using UnityEngine;

public class BookStack : GameStack<BookStack.Item>
{
    public class Item : GameItem, IColoredItem
    {
        public bool isGhost = false;
        private Color color;
        public Color Color => color;
        public Item(Color color) : base(Instantiate(AssetManager.Load<GameObject>("Prefabs/Livro")))
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

    protected override void Awake()
    {
        base.Awake();

        // Livro fantasma do topo da pilha
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

    protected override void ShowWarning(Warning w, GameSlot<BookStack.Item> slot)
    {
        var message = w switch
        {
            Warning.Full => "A pilha já está cheia",
            Warning.Empty => "Não há o que retirar",
            Warning.GrabNotTop => "Apenas o livro do topo consegue ser retirado",
            Warning.ReleaseNotTop => "Não pode colocar o livro fora do topo",
            Warning.WrongGroup => "Não pode colocar isso aqui",
        };
        base.ShowWarning(message, slot);
    }
}
