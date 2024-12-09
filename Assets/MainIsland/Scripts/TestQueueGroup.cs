using System;
using System.Collections.Generic;
using System.Linq;
using SpatialSys.UnitySDK;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class TestQueueGroup : ListGroup<TestQueue>
{
    public List<TestQueue> Queues => Lists;
    public List<Color> colors;
    [SerializeField]
    private GameObject prefab;

    new void Start()
    {
        base.Start();

        //prefab = AssetManager.Load<GameObject>("Prefabs/Livro");

        for (int i = 0; i < Queues[0].MaxSize; i++)
        {
            var item = InstantiateItem(colors[i % colors.Count]);
            Queues[0].Push(item, resetTransform: true);
        }

        // Ghost
        var ghost = InstantiateItem();
        foreach (var c in ghost.GetComponentsInChildren<Collider>())
            c.gameObject.layer = Layers.Ghost;
        var r = ghost.GetComponentInChildren<Renderer>();
        r.materials = Enumerable.Repeat(transparentMaterial, r.materials.Length).ToArray();
        foreach (var queue in Queues)
        {
            queue.FrontGhost.Insert(Instantiate(ghost), resetTransform: true);
            queue.BackGhost.Insert(Instantiate(ghost), resetTransform: true);
        }
        Destroy(ghost);
    }

    protected GameObject InstantiateItem(Color c)
    {
        var item = Instantiate(prefab);
        var renderer = item.GetComponent<Renderer>();
        var mat = renderer.material;
        mat.color = c;
        renderer.material = mat;
        return item;
    }
    protected override GameObject InstantiateItem()
    {
        return InstantiateItem(colors[Random.Range(0, colors.Count)]);
    }

    public override void Grab(GameSlot slot)
    {
        var queue = (TestQueue)slot.Parent;

        if (slot != queue.FrontSlot)
        {
            queue.ShowWarning("Não pode pegar X fora da frente", slot);
            return;
        }
        var item = slot.Item;
        if (!queue.Pop())
        {
            queue.ShowWarning("Não pode tirar X de uma fila vazia", slot);
            return;
        }
        Grabbed = item;
        Grabbed.GetComponentInChildren<Collider>(true).enabled = false;
    }
    public override void Release(GameSlot slot)
    {
        var queue = (TestQueue)slot.Parent;

        if (slot != queue.BackGhost)
        {
            queue.ShowWarning("Não pode colocar o X fora do fundo da fila", slot);
            return;
        }
        if (!queue.Push(Grabbed, resetTransform: true))
        {
            queue.ShowWarning("Não pode colocar X numa fila já cheia", slot);
            return;
        }

        Grabbed.GetComponentInChildren<Collider>(true).enabled = true;
        Grabbed = null;
        //TODO: colocar animação lerp
        //TODO: manter colisao desativada durante animação lerp
    }
}
