using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BallQueueGroup : ListGroup<GameQueue>
{
    public List<GameQueue> Queues => Lists;
    public List<Color> colors;
    private GameObject prefab;
    private Material transparentMaterial;

    new void Start()
    {
        base.Start();

        transparentMaterial = AssetManager.Load<Material>("Materials/Transparent");
        prefab = AssetManager.Load<GameObject>("Prefabs/Ball");

        for (int i = 0; i < Queues[0].MaxSize; i++)
        {
            var book = InstantiateBall(colors[i % colors.Count]);
            Queues[0].Push(book, resetTransform: true);
        }

        var ghost = InstantiateBall();
        var r = ghost.GetComponentInChildren<Renderer>();
        r.materials = Enumerable.Repeat(transparentMaterial, r.materials.Length).ToArray();
        foreach (var queue in Queues)
        {
            queue.FrontGhost.Insert(Instantiate(ghost), resetTransform: true);
            queue.BackGhost.Insert(Instantiate(ghost), resetTransform: true);
        }
        Destroy(ghost);
    }
    GameObject InstantiateBall(Color? color = null)
    {
        var ball = Instantiate(prefab);

        if (color != null)
        {
            ball.GetComponentInChildren<Renderer>().material.color = (Color)color;
        }

        return ball;
    }

    void Update()
    {

    }
}
