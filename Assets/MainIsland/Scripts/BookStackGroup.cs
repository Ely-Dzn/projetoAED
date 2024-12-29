using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BookItem : GameItem
{
    private Color color;
    public Color Color => color;
    public BookItem(Color color) : base(Object.Instantiate(AssetManager.Load<GameObject>("Prefabs/Livro")))
    {
        this.color = color;
        GameObject.GetComponentInChildren<Renderer>(true).material.color = color;
    }
}
public class GhostBookItem : GameItem
{
    public GhostBookItem() : base(Object.Instantiate(AssetManager.Load<GameObject>("Prefabs/Livro")))
    {
        var renderer = GameObject.GetComponentInChildren<Renderer>(true);
        var transparent = AssetManager.Load<Material>("Materials/Transparent");
        renderer.materials = Enumerable.Repeat(transparent, renderer.materials.Length).ToArray();
    }
}

[DisallowMultipleComponent]
public class BookStackGroup : ListGroup<GameStack>
{
    public List<GameStack> Stacks => Lists;
    public List<Color> colors;

    new void Start()
    {
        base.Start();

        int colorIndex = 0;
        for (int i = 0; i < 5; i++)
        {
            var book = new BookItem(colors[(colorIndex++) % colors.Count]);
            Stacks[0].Push(book, resetTransform: true);
            book.Transform.rotation = GetRandomRotation();
        }
        for (int i = 0; i < 2; i++)
        {
            var book = new BookItem(colors[(colorIndex++) % colors.Count]);
            Stacks[1].Push(book, resetTransform: true);
            book.Transform.rotation = GetRandomRotation();
        }
        for (int i = 0; i < 3; i++)
        {
            var book = new BookItem(colors[(colorIndex++) % colors.Count]);
            Stacks[2].Push(book, resetTransform: true);
            book.Transform.rotation = GetRandomRotation();
        }

        // Livro fantasma do topo da pilha
        foreach (var stack in Stacks)
        {
            stack.TopGhost.Insert(new GhostBookItem(), resetTransform: true);
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
