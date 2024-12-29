using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BallItem : GameItem
{
    private Color color;
    public Color Color => color;
    public BallItem(Color color) : base(Object.Instantiate(AssetManager.Load<GameObject>("Prefabs/Ball")))
    {
        this.color = color;
        GameObject.GetComponentInChildren<Renderer>(true).material.color = color;
    }
}
public class GhostBallItem : GameItem
{
    public GhostBallItem() : base(Object.Instantiate(AssetManager.Load<GameObject>("Prefabs/Ball")))
    {
        var renderer = GameObject.GetComponentInChildren<Renderer>(true);
        var transparent = AssetManager.Load<Material>("Materials/Transparent");
        renderer.materials = Enumerable.Repeat(transparent, renderer.materials.Length).ToArray();
    }
}

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
            Queues[0].Push(new BallItem(colors[i % colors.Count]), resetTransform: true);
        }

        foreach (var queue in Queues)
        {
            queue.FrontGhost.Insert(new GhostBallItem(), resetTransform: true);
            queue.BackGhost.Insert(new GhostBallItem(), resetTransform: true);
        }
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
