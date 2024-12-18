using System.Collections.Generic;
using System.Linq;
using SpatialSys.UnitySDK;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class TestQueueGroup : MonoBehaviour
{
    private bool playing = false;

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
    private SpatialInteractable mainBallInteractable;
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
    [SerializeField]
    private RawImage colorDisplay;
    private Color nextColor;
    private Color NextColor
    {
        get => nextColor;
        set
        {
            nextColor = value;
            colorDisplay.color = value;
        }
    }
    private int collected = 0;
    private int moves = 0;
    private int round = 0;

    void Start()
    {
        queue.group = this;
        queue.grabArea = grabArea;
        queue.grabGroup = this;
        queue.OnInteractEvent += (_, _) => moves++;

        transparentMaterial = AssetManager.Load<Material>("Materials/Transparent");

        var ghost = InstantiateItem();
        var r = ghost.GetComponentInChildren<Renderer>();
        r.materials = Enumerable.Repeat(transparentMaterial, r.materials.Length).ToArray();
        queue.BackGhost.Insert(ghost, resetTransform: true);

        mainBallLine = mainBall.GetComponentInChildren<LineRenderer>();
        mainBallLine.positionCount = 2;
        mainBallLine.SetPosition(0, mainBall.transform.position);
        mainBallLine.SetPosition(1, mainBall.transform.position);
        mainBallLine.enabled = false;
        mainBallInteractable = mainBall.GetComponentInChildren<SpatialInteractable>();
        mainBallInteractable.onInteractEvent += StartPlaying;

        bowlInteractable = bowl.GetComponentInChildren<SpatialInteractable>();
        bowlInteractable.onInteractEvent += OnBowlInteract;
        bowlInteractable.enabled = false;
        bowlOutline = bowl.GetComponentInChildren<Outline>();
        bowlOutline.enabled = false;

        colorDisplay.enabled = false;

        grabArea.onExitEvent += StopPlaying;
    }

    private void Update()
    {
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
    }

    private void OnBowlInteract()
    {
        if (!playing || !Grabbed) return;
        var item = Grabbed;
        if (item.GetComponent<Renderer>().material.color == NextColor)
        {
            collected++;
            NextColor = GetNextColor();
        }
        Debug.Log($"Coletados: {collected}, Movimentações: {moves}");
        Grabbed = null;
        bowlItems.Add(item.transform);
        item.GetComponentInChildren<Outline>().enabled = false;
        item.transform.SetParent(bowl.transform);
        item.layer = Layers.IgnoreRaycast;
        transit.Add(item.transform);

        if (queue.Count == 0)
        {
            AvanceRound();
        }
    }

    private void StartPlaying()
    {
        if (playing) return;
        playing = true;
        mainBallInteractable.enabled = false;
        mainBallLine.enabled = true;
        colorDisplay.enabled = true;
        moves = 0;
        collected = 0;
        round = 0;
        AvanceRound();
    }
    private void StopPlaying()
    {
        playing = false;
        round = 0;
        mainBallInteractable.enabled = true;
        mainBallLine.enabled = false;
        bowlInteractable.enabled = false;
        bowlOutline.enabled = false;
        colorDisplay.enabled = false;
        queue.Clear();
        Destroy(Grabbed);
        Grabbed = null;
        foreach (var item in bowlItems)
        {
            if (item) Destroy(item.gameObject);
        }
        bowlItems.Clear();
    }

    private void AvanceRound()
    {
        round++;
        if (round > 4)
        {
            StopPlaying();
            return;
        }
        var n = queue.MaxSize * round / 4;
        for (int i = 0; i < n; i++)
        {
            var item = InstantiateItem(colors[Random.Range(0, colors.Count)]);
            queue.Push(item, resetTransform: true);
        }
        NextColor = GetNextColor();
    }

    private Color GetNextColor()
    {
        List<int> hasColors = new();
        foreach (var slot in queue.Slots)
        {
            if (!slot.IsFilled) continue;
            var renderer = slot.Item.GetComponent<Renderer>();
            if (!renderer) continue;
            hasColors.Add(colors.FindIndex(c => c == renderer.material.color));
        }
        int size = hasColors.Count;
        if (size == 0) return colors[0];
        return colors[hasColors[Random.Range(0, size)]];
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
