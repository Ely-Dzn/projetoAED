using System;
using System.Collections.Generic;
using System.Linq;
using SpatialSys.UnitySDK;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class TestQueueGroup : ListGroup<GameQueue>
{
    public List<GameQueue> Queues => Lists;
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

        // Warnings
        warningCanvas = Instantiate(warningPrefab);
        warningText = warningCanvas.GetComponentInChildren<TextMeshProUGUI>();
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
            ShowWarning("Não pode pegar X fora da frente", slot, Color.red);
            return;
        }
        var item = slot.Item;
        if (!queue.Pop())
        {
            ShowWarning("Não pode tirar X de uma fila vazia", slot, Color.red);
            return;
        }
        Grabbed = item;
        Grabbed.GetComponentInChildren<Collider>(true).enabled = false;
    }
    public override void Release(GameSlot slot)
    {
        var stack = (TestQueue)slot.Parent;

        if (slot != stack.BackGhost)
        {
            ShowWarning("Não pode colocar o X fora do fundo da fila", slot, Color.red);
            return;
        }
        if (!stack.Push(Grabbed))
        {
            ShowWarning("Não pode colocar livro numa fila já cheia", slot, Color.red);
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
