using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class BookStackGroup : ListGroup<BookStack, BookStack.Item>
{
    public List<BookStack> Stacks => Lists;
    public List<Color> colors;
    public static readonly int[][] defaultBooks = {
        new int[]{ 0, 1, 2, 3, 4 },
        new int[]{ 3, 1 },
        new int[]{ 2, 4, 0 },
    };

    new void Start()
    {
        base.Start();

        Populate(defaultBooks);
    }

    public void Clear()
    {
        if (GrabManager.Grabbed && GrabManager.Grabbed.item is BookStack.Item)
        {
            GrabManager.Release().item.Destroy();
        }

        foreach (var stack in Stacks)
        {
            stack.Clear();
        }
    }
    public void Populate(int[][] books)
    {
        for (int i = 0; i < books.Length; i++)
        {
            for (int j = 0; j < books[i].Length; j++)
            {
                var book = new BookStack.Item(colors[books[i][j]]);
                Stacks[i].Push(book, resetTransform: true);
                book.Transform.rotation = GetRandomRotation();
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
