using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using SpatialSys.UnitySDK;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class BookStackGroup : MonoBehaviour
{
    [SerializeField]
    private GameObject stacksContainer;
    public List<BookStack> Stacks { get; private set; }
    [field: ReadOnly]
    private GameObject grabbed;
    public GameObject Grabbed
    {
        get => grabbed; private set
        {
            grabbed = value;
            foreach (var stack in Stacks)
            {
                stack.PushMode = value != null;
            }
        }
    }
    public List<Color> colors;
    private static GameObject bookPrefab;
    private static GameObject warningPrefab;
    private static Material transparentMaterial;

    private GameObject warningCanvas;
    private TMP_Text warningText;
    private StackSlot warned = null;

    void Start()
    {
        if (bookPrefab == null)
        {
            bookPrefab = AssetManager.Load<GameObject>("Prefabs/Livro");
            warningPrefab = AssetManager.Load<GameObject>("Prefabs/Warning");
            transparentMaterial = AssetManager.Load<Material>("Materials/Transparent");
        }

        Stacks = Utils.GetChildren<BookStack>(stacksContainer.transform);

        foreach (var stack in Stacks)
        {
            stack.Start();
            stack.onInteract = OnInteract;
        }


        for (int i = 0; i < Stacks[0].MaxSize; i++)
        {
            var book = InstantiateBook(colors[i % colors.Count]);
            book.transform.rotation = GetRandomRotation();
            Stacks[0].Push(book, resetTransform: true);
        }

        // Ghost Book
        var ghostBook = InstantiateBook();
        ghostBook.layer = Layers.Ghost;
        foreach (var c in ghostBook.GetComponentsInChildren<Collider>())
            c.gameObject.layer = Layers.Ghost;
        var r = ghostBook.GetComponentInChildren<Renderer>();
        r.materials = Enumerable.Repeat(transparentMaterial, r.materials.Length).ToArray();
        foreach (var stack in Stacks)
            stack.SetGhost(Instantiate(ghostBook));
        Destroy(ghostBook);

        // Warnings
        warningCanvas = Instantiate(warningPrefab);
        warningText = warningCanvas.GetComponentInChildren<TextMeshProUGUI>();
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

    void Update()
    {
        var cam = SpatialBridge.cameraService;
        if (Grabbed)
        {
            var player = SpatialBridge.actorService.localActor.avatar;
            Grabbed.transform.position = player.position + new Vector3(0, 0.9f, 0) + player.rotation * Vector3.forward;
            if (cam.zoomDistance < 1f)
                Grabbed.transform.eulerAngles = cam.rotation.eulerAngles + new Vector3(90, 90, 90);
            else
                Grabbed.transform.eulerAngles = player.rotation.eulerAngles + new Vector3(90, 90, 90);
        }

        DebugUtils.Log(cam.zoomDistance.ToString());
    }

    public void ShowWarning(String text, StackSlot slot, Color color)
    {
        ClearWarning();
        warned = slot;
        warningText.text = text;
        var player = SpatialBridge.actorService.localActor.avatar;
        var dir = player.position - slot.transform.position;
        dir.y = 0;
        dir.Normalize();
        warningCanvas.transform.SetPositionAndRotation(
            slot.transform.position + dir,
            Quaternion.LookRotation(-dir));
        warningCanvas.SetActive(true);
        if (warned.outline != null)
            warned.outline.OutlineColor = color;
        Invoke(nameof(ClearWarning), 2f);
    }
    public void ClearWarning()
    {
        CancelInvoke(nameof(ClearWarning));
        if (warned != null)
        {
            if (warned.outline != null)
                warned.outline.OutlineColor = Color.white;
            warningCanvas.SetActive(false);
            warned = null;
        }
    }
    void OnInteract(StackSlot slot)
    {
        if (Grabbed)
            Release(slot);
        else
            Grab(slot);
    }

    public void Grab(StackSlot slot)
    {
        if (slot.index != slot.Parent.Count - 1)
        {
            ShowWarning("Não pode pegar livro fora do topo", slot, Color.red);
            return;
        }
        var item = slot.Item;
        if (!slot.Parent.Pop())
        {
            ShowWarning("Não pode tirar livro de uma pilha vazia", slot, Color.red);
            return;
        }

        foreach (var stack in Stacks)
        {
            stack.ghostSlot.transform.rotation = GetRandomRotation();
        }
        Grabbed = item;
        Grabbed.GetComponentInChildren<Collider>(true).enabled = false;
    }
    public void Release(StackSlot slot)
    {
        GameStack stack = slot.Parent;
        if (slot.index != stack.Count)
        {
            ShowWarning("Não pode colocar o livro fora do topo", slot, Color.red);
            return;
        }
        if (!stack.Push(Grabbed))
        {
            ShowWarning("Não pode colocar livro numa pilha já cheia", slot, Color.red);
            return;
        }

        Grabbed.GetComponentInChildren<Collider>(true).enabled = true;
        Grabbed.transform.localPosition = Vector3.zero;
        Grabbed.transform.rotation = ((BookStack)stack).ghostSlot.transform.rotation;
        Grabbed = null;
        //TODO: colocar animação lerp
        //TODO: manter colisao desativada durante animação lerp
    }
}
