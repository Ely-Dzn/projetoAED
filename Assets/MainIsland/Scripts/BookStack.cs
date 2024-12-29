using System.Linq;
using UnityEngine;

public class BookStack : GameStack<BookStack.Item>
{
    public class Item : GameItem
    {
        private Color color;
        public Color Color => color;
        public Item(Color color) : base(Object.Instantiate(AssetManager.Load<GameObject>("Prefabs/Livro")))
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

    protected override void ShowWarning(Warning w, GameSlot<BookStack.Item> slot)
    {
        var message = w switch
        {
            Warning.Full => "A pilha já está cheia",
            Warning.Empty => "Não há o que retirar",
            Warning.GrabNotTop => "É possível apenas pegar o livro do topo",
            Warning.ReleaseNotTop => "Não pode colocar o livro fora do topo",
            Warning.WrongGroup => "Não pode colocar aqui",
        };
        base.ShowWarning(message, slot);
    }
}
