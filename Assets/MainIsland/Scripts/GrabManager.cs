using System;
using SpatialSys.UnitySDK;
using UnityEngine;

//TODO: suporte a multiplayer
public class GrabManager : MonoBehaviour
{
    public static GrabManager Instance { get; private set; }
    public class GrabInfo
    {
        public Transform transform;
        public GameObject gameObject => transform != null ? transform.gameObject : null;
        public object extra = null;
        public object group = null;
        /// <summary>
        ///  Limites do objeto a ser agarrado.
        ///  Caso o jogador esteja fora dessa área, o objeto é solto.
        /// </summary>
        public SpatialTriggerEvent area = null;
        public Action areaExitHandler = null;
        //TODO
        //public Vector3 position = Vector3.zero;
        //public Quaternion rotation = Quaternion.identity;
        //public Vector3 scale = Vector3.one;
        public GrabInfo(Transform target)
        {
            this.transform = target;
        }
        public GrabInfo(GameObject target)
        {
            this.transform = target.transform;
        }
        public static implicit operator bool(GrabInfo info)
        {
            return info is not null;
        }
    }

    public GrabInfo grabbed { get; private set; } = null;
    public static GrabInfo Grabbed
    {
        get
        {
            if (!Instance) return null;
            return Instance.grabbed;
        }
    }

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        //TODO: suporte a VR
        if (Grabbed != null)
        {
            var player = SpatialBridge.actorService.localActor.avatar;
            var targetPos = player.position + new Vector3(0, 0.9f, 0) + player.rotation * Vector3.forward;
            var targetRot = player.rotation;

            Grabbed.transform.SetPositionAndRotation(
                Vector3.Lerp(Grabbed.transform.position, targetPos, Time.deltaTime * 7f),
                Quaternion.Lerp(Grabbed.transform.rotation, targetRot, Time.deltaTime * 8f));
        }
    }

    public static bool Grab(GrabInfo info, bool lerp = true)
    {
        if (Grabbed != null || info == null || info.transform == null) return false;
        Instance.grabbed = info;
        Grabbed.transform.SetParent(Instance.transform, worldPositionStays: true);
        if (Grabbed.area != null)
        {
            Grabbed.area.onExitEvent += HandleAreaExit;
        }
        if (!lerp)
        {
            Grabbed.transform.SetLocalPositionAndRotation(
                Vector3.zero,
                Quaternion.identity);
        }
        return true;
    }
    public static GrabInfo Release()
    {
        if (Grabbed == null) return null;
        if (Grabbed.area != null)
        {
            Grabbed.area.onExitEvent -= HandleAreaExit;
        }
        var item = Grabbed;
        Instance.grabbed = null;
        return item;
    }
    private static void HandleAreaExit()
    {
        Debug.Log("HandleAreaExit");
        if (Grabbed != null)
        {
            if (Grabbed.areaExitHandler != null)
            {
                Grabbed.areaExitHandler();
            }
            else
            {
                Release();
            }
        }
    }
}
