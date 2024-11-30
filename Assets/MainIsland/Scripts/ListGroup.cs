using System;
using System.Collections.Generic;
using SpatialSys.UnitySDK;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class ListGroup<L> : MonoBehaviour where L : GameList
{
    [SerializeField]
    protected GameObject container;
    public List<L> Lists { get; protected set; }
    [field: ReadOnly]
    protected GameObject grabbed;
    public GameObject Grabbed
    {
        get => grabbed; protected set
        {
            grabbed = value;
            foreach (var list in Lists)
            {
                list.PushMode = value != null;
            }
        }
    }
    protected GameObject warningPrefab;
    protected Material transparentMaterial;

    protected GameObject warningCanvas;
    protected TMP_Text warningText;
    protected GameSlot warned = null;

    protected void Start()
    {
        warningPrefab = AssetManager.Load<GameObject>("Prefabs/Warning");
        transparentMaterial = AssetManager.Load<Material>("Materials/Transparent");

        Lists = Utils.GetChildren<L>(container.transform);

        foreach (var list in Lists)
        {
            list.Start();
            list.onInteract = OnInteract;
        }

        // Warnings
        warningCanvas = Instantiate(warningPrefab);
        warningText = warningCanvas.GetComponentInChildren<TextMeshProUGUI>();
    }

    protected abstract GameObject InstantiateItem();

    protected void Update()
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
    }

    public void ShowWarning(String text, GameSlot slot, Color color)
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
    void OnInteract(GameSlot slot)
    {
        if (Grabbed)
            Release(slot);
        else
            Grab(slot);
    }

    public abstract void Grab(GameSlot slot);
    public abstract void Release(GameSlot slot);
}
