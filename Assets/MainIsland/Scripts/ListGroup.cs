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
    protected Material transparentMaterial;

    protected void Start()
    {
        transparentMaterial = AssetManager.Load<Material>("Materials/Transparent");

        Lists = Utils.GetChildren<L>(container.transform);

        foreach (var list in Lists)
        {
            list.Start();
            list.onInteract = OnInteract;
        }
    }

    protected abstract GameObject InstantiateItem();

    protected void Update()
    {
        if (Grabbed)
        {
            var player = SpatialBridge.actorService.localActor.avatar;
            var targetPos = GetGrabbedPosition();
            Quaternion targetRot = GetGrabbedRotation();

            Grabbed.transform.SetPositionAndRotation(
                Vector3.Lerp(Grabbed.transform.position, targetPos, Time.deltaTime * 7f),
                Quaternion.Lerp(Grabbed.transform.rotation, targetRot, Time.deltaTime * 8f));
        }
    }

    protected virtual Vector3 GetGrabbedPosition()
    {
        var player = SpatialBridge.actorService.localActor.avatar;
        return player.position + new Vector3(0, 0.9f, 0) + player.rotation * Vector3.forward;
    }
    protected virtual Quaternion GetGrabbedRotation()
    {
        var player = SpatialBridge.actorService.localActor.avatar;
        return player.rotation;
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
