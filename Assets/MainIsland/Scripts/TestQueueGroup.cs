using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class TestQueueGroup : ListGroup<TestQueue>
{
    public List<TestQueue> Queues => Lists;
    public List<Color> colors;
    [SerializeField]
    private GameObject prefab;
    private Material transparentMaterial;

    new void Start()
    {
        base.Start();

        transparentMaterial = AssetManager.Load<Material>("Materials/Transparent");
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
    protected GameObject InstantiateItem()
    {
        return InstantiateItem(colors[Random.Range(0, colors.Count)]);
    }
}
