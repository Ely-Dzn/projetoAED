using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class BookStackGroup : ListGroup<GameStack>
{
    public List<GameStack> Stacks => Lists;
    public List<Color> colors;
    private GameObject bookPrefab;
    private Material transparentMaterial;

    new void Start()
    {
        base.Start();

        transparentMaterial = AssetManager.Load<Material>("Materials/Transparent");
        bookPrefab = AssetManager.Load<GameObject>("Prefabs/Livro");

        for (int i = 0; i < Stacks[0].MaxSize; i++)
        {
            var book = InstantiateBook(colors[i % colors.Count]);
            Stacks[0].Push(book, resetTransform: true);
            book.transform.rotation = GetRandomRotation();
        }

        // Livro fantasma do topo da pilha
        var ghostBook = InstantiateBook();
        var r = ghostBook.GetComponentInChildren<Renderer>();
        r.materials = Enumerable.Repeat(transparentMaterial, r.materials.Length).ToArray();
        foreach (var stack in Stacks)
            stack.TopGhost.Insert(Instantiate(ghostBook), resetTransform: true);
        Destroy(ghostBook);
    }

    private Quaternion GetRandomRotation()
    {
        return Quaternion.Euler(0, (Random.value - 0.5f) * 90f, 0);
    }

    GameObject InstantiateBook(Color? coverColor = null, Color? pageColor = null)
    {
        var book = Instantiate(bookPrefab);

        if (coverColor != null)
        {
            book.GetComponentInChildren<Renderer>().material.color = (Color)coverColor;
        }
        if (pageColor != null)
        {
            book.GetComponentInChildren<Renderer>().materials[1].color = (Color)pageColor;
        }

        return book;
    }

    //protected override Quaternion GetGrabbedRotation()
    //{
    //    var player = SpatialBridge.actorService.localActor.avatar;
    //    return Utils.QuaternionFromEuler(player.rotation.eulerAngles + new Vector3(90, 90, 90));
    //}

}
