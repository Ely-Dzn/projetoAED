using System.Collections.Generic;
using System.Linq;
using SpatialSys.UnitySDK;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class TestQueueGroup : MonoBehaviour
{
    //TODO
    private bool playing = true;

    public TestQueue queue;
    public List<Color> colors;
    [SerializeField]
    private SpatialTriggerEvent grabArea;
    [SerializeField]
    private GameObject prefab;
    private Material transparentMaterial;

    [SerializeField]
    private GameObject mainBall;
    private LineRenderer mainBallLine;
    [SerializeField]
    private GameObject bowl;
    [SerializeField]
    private Transform bowlSpawn;
    private SpatialInteractable bowlInteractable;
    private Outline bowlOutline;
    private List<Transform> bowlItems = new();
    private GameObject grabbed = null;
    public GameObject Grabbed
    {
        get
        {
            return grabbed;
        }
        set
        {
            grabbed = value;
            //TODO
        }
    }
    private List<Transform> transit = new();
    private int collected = 0;
    private int moves = 0;
    private int level = 0;


    void Start()
    {
        queue.group = this;
        queue.grabArea = grabArea;
        queue.grabGroup = this;

        transparentMaterial = AssetManager.Load<Material>("Materials/Transparent");

        for (int i = 0; i < queue.MaxSize; i++)
        {
            var item = InstantiateItem(colors[i % colors.Count]);
            queue.Push(item, resetTransform: true);
        }

        var ghost = InstantiateItem();
        var r = ghost.GetComponentInChildren<Renderer>();
        r.materials = Enumerable.Repeat(transparentMaterial, r.materials.Length).ToArray();
        queue.BackGhost.Insert(ghost, resetTransform: true);

        mainBallLine = mainBall.GetComponent<LineRenderer>();
        mainBallLine.positionCount = 2;
        mainBallLine.SetPosition(0, mainBall.transform.position);
        mainBallLine.SetPosition(1, mainBall.transform.position);

        bowlInteractable = bowl.GetComponentInChildren<SpatialInteractable>();
        bowlOutline = bowl.GetComponent<Outline>();

        bowlInteractable.onInteractEvent += OnBowlInteract;
    }

    private void Update()
    {
        mainBallLine.enabled = playing;
        if (!playing) return;

        if (Raycast.HasHit)
        {
            mainBallLine.SetPosition(1, Raycast.Hit.point);
        }

        if (Grabbed && Raycast.HasHit && Raycast.Hit.collider.gameObject == bowl)
        {
            bowlOutline.enabled = true;
            bowlInteractable.enabled = true;
        }
        else
        {
            bowlInteractable.enabled = false;
            bowlOutline.enabled = false;
        }

        if (Grabbed)
        {
            Grabbed.transform.position = Vector3.Lerp(Grabbed.transform.position, mainBall.transform.position, 0.5f);
        }

        for (var i = transit.Count - 1; i >= 0; i--)
        {
            var item = transit[i];
            //var rand = Random.insideUnitCircle * 0.25f;
            //item.transform.position = bowlSpawn.position + new Vector3(rand.x, 0, rand.y);
            item.position = Vector3.Lerp(item.position, bowlSpawn.position, 0.5f);
            if (Vector3.Distance(item.position, bowlSpawn.position) < 0.1f)
            {
                transit.RemoveAt(i);
                var col = item.GetComponentInChildren<SphereCollider>(true);
                col.enabled = true;
                col.gameObject.AddComponent<Rigidbody>();
                col.radius /= 2;
            }
        }
    }

    private void OnBowlInteract()
    {
        if (!Grabbed) return;
        //if (is the right ball) {
        //    collected++;
        //    set next wanted ball;
        //}
        moves++;
        Debug.Log($"Collected: {collected}, Moves: {moves}");
        var item = Grabbed;
        Grabbed = null;
        bowlItems.Add(item.transform);
        item.GetComponentInChildren<Outline>().enabled = false;
        item.transform.SetParent(bowl.transform);
        item.layer = Layers.IgnoreRaycast;
        transit.Add(item.transform);
    }

    protected GameObject InstantiateItem(Color c)
    {
        var item = Instantiate(prefab);
        var renderer = item.GetComponent<Renderer>();
        var mat = renderer.material;
        mat.color = c;
        renderer.material = mat;
        // Aumentar raio para melhorar a seleção
        item.GetComponentInChildren<SphereCollider>().radius *= 2;
        return item;
    }
    protected GameObject InstantiateItem()
    {
        return InstantiateItem(colors[Random.Range(0, colors.Count)]);
    }
}
