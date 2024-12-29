using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class BookStackGroup : ListGroup<BookStack, BookStack.Item>
{
    public List<BookStack> Stacks => Lists;
    public List<Color> colors;

    new void Start()
    {
        base.Start();

        int colorIndex = 0;
        for (int i = 0; i < 5; i++)
        {
            var book = new BookStack.Item(colors[(colorIndex++) % colors.Count]);
            Stacks[0].Push(book, resetTransform: true);
            book.Transform.rotation = GetRandomRotation();
        }
        for (int i = 0; i < 2; i++)
        {
            var book = new BookStack.Item(colors[(colorIndex++) % colors.Count]);
            Stacks[1].Push(book, resetTransform: true);
            book.Transform.rotation = GetRandomRotation();
        }
        for (int i = 0; i < 3; i++)
        {
            var book = new BookStack.Item(colors[(colorIndex++) % colors.Count]);
            Stacks[2].Push(book, resetTransform: true);
            book.Transform.rotation = GetRandomRotation();
        }

        // Livro fantasma do topo da pilha
        foreach (var stack in Stacks)
        {
            stack.TopGhost.Insert(BookStack.Item.MakeGhost(), resetTransform: true);
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
