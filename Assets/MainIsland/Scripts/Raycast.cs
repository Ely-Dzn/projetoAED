using SpatialSys.UnitySDK;
using Unity.VisualScripting;
using UnityEngine;
using SCRM = SpatialSys.UnitySDK.SpatialCameraRotationMode;

public class Raycast : MonoBehaviour
{
    public static RaycastHit Hit { get; private set; }
    public static bool HasHit { get; private set; } = false;
    private VariableDeclarations sceneVariables;

    void Start()
    {
        sceneVariables = Variables.Scene(gameObject);
    }

    void Update()
    {
        var cam = SpatialBridge.cameraService;
#if UNITY_EDITOR
        var rotationMode = SCRM.PointerLock_Unlocked;
#else
        var rotationMode = cam.rotationMode;
#endif

        Ray ray;
        if (rotationMode == SCRM.PointerLock_Locked)
            ray = new(cam.position, cam.forward);
        else
            ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(
            ray, out RaycastHit hit, 10,
            ~(Layers.AvatarLocalMask | Layers.AvatarRemoteMask | Layers.IgnoreRaycastMask),
            QueryTriggerInteraction.Ignore))
        {
            Hit = hit;
            HasHit = true;
            sceneVariables.Set("cam_raycast", hit);
            transform.position = hit.point;
        }
        else
        {
            ResetRaycast();
        }
    }
    void ResetRaycast()
    {
        HasHit = false;
        sceneVariables.Set("cam_raycast", null);
        transform.position = Vector3.zero;
    }
}
