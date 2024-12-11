using SpatialSys.UnitySDK;
using UnityEngine;

//TODO: suporte a multiplayer
public class GrabManager : MonoBehaviour
{
    private static GrabManager INSTANCE;
    public class GrabInfo
    {
        public Transform target;
        public object extra = null;
        public object group = null;
        /// <summary>
        ///  Limites do objeto a ser agarrado.
        ///  Caso o jogador esteja fora dessa área, o objeto é solto.
        /// </summary>
        public Collider area;
        //TODO
        //public Vector3 position = Vector3.zero;
        //public Quaternion rotation = Quaternion.identity;
        //public Vector3 scale = Vector3.one;
        public GrabInfo(Transform target)
        {
            this.target = target;
        }
        public GrabInfo(GameObject target)
        {
            this.target = target.transform;
        }
        public static implicit operator bool(GrabInfo info)
        {
            return info is not null;
        }
    }

    public static GrabInfo Grabbed { get; private set; } = null;

    void Start()
    {
        INSTANCE = this;
    }

    void Update()
    {
        //TODO: suporte a VR
        if (Grabbed != null)
        {
            var player = SpatialBridge.actorService.localActor.avatar;
            var targetPos = player.position + new Vector3(0, 0.9f, 0) + player.rotation * Vector3.forward;
            var targetRot = player.rotation;

            Grabbed.target.SetPositionAndRotation(
                Vector3.Lerp(Grabbed.target.position, targetPos, Time.deltaTime * 7f),
                Quaternion.Lerp(Grabbed.target.rotation, targetRot, Time.deltaTime * 8f));
        }
    }

    public static bool Grab(GrabInfo info, bool lerp = true)
    {
        if (Grabbed != null) return false;
        Grabbed = info;
        Grabbed.target.SetParent(INSTANCE.transform, worldPositionStays: true);
        if (!lerp)
        {
            Grabbed.target.SetLocalPositionAndRotation(
                Vector3.zero,
                Quaternion.identity);
        }
        return true;
    }
    public static Transform Release()
    {
        if (Grabbed == null) return null;
        var x = Grabbed.target;
        Grabbed = null;
        return x;
    }
}
