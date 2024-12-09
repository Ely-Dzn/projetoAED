using System;
using System.Collections.Generic;
using System.Linq;
using SpatialSys.UnitySDK;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class BookStackGroup : ListGroup<GameStack>
{
    public List<GameStack> Stacks => Lists;
    public List<Color> colors;
    private GameObject bookPrefab;

    new void Start()
    {
        base.Start();

        bookPrefab = AssetManager.Load<GameObject>("Prefabs/Livro");

        for (int i = 0; i < Stacks[0].MaxSize; i++)
        {
            var book = InstantiateBook(colors[i % colors.Count]);
            Stacks[0].Push(book, resetTransform: true);
            book.transform.rotation = GetRandomRotation();
        }

        // Ghost Book
        var ghostBook = InstantiateBook();
        foreach (var c in ghostBook.GetComponentsInChildren<Collider>())
            c.gameObject.layer = Layers.Ghost;
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
    protected override GameObject InstantiateItem() => InstantiateBook();

    protected override Quaternion GetGrabbedRotation()
    {
        var player = SpatialBridge.actorService.localActor.avatar;
        return Utils.QuaternionFromEuler(player.rotation.eulerAngles + new Vector3(90, 90, 90));
    }

    public override void Grab(GameSlot slot)
    {
        var stack = (BookStack)slot.Parent;

        if (slot.index != stack.Count - 1)
        {
            stack.ShowWarning("Não pode pegar livro fora do topo", slot);
            return;
        }
        var item = slot.Item;
        if (!stack.Pop())
        {
            stack.ShowWarning("Não pode tirar livro de uma pilha vazia", slot);
            return;
        }

        foreach (var s in Stacks)
        {
            foreach (var ghostSlot in s.ghostSlots)
            {
                ghostSlot.transform.rotation = GetRandomRotation();
            }
        }
        Grabbed = item;
        Grabbed.GetComponentInChildren<Collider>(true).enabled = false;
    }
    public override void Release(GameSlot slot)
    {
        var stack = (BookStack)slot.Parent;

        if (slot.index != stack.Count)
        {
            stack.ShowWarning("Não pode colocar o livro fora do topo", slot);
            return;
        }
        if (!stack.Push(Grabbed))
        {
            stack.ShowWarning("Não pode colocar livro numa pilha já cheia", slot);
            return;
        }

        Grabbed.GetComponentInChildren<Collider>(true).enabled = true;
        Grabbed.transform.localPosition = Vector3.zero;
        Grabbed.transform.rotation = slot.transform.rotation;
        Grabbed = null;
        //TODO: colocar animação lerp
        //TODO: manter colisao desativada durante animação lerp
    }
}
