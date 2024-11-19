using Unity.VisualScripting;
using UnityEngine;
using SpatialSys.UnitySDK;

using SCRM = SpatialSys.UnitySDK.SpatialCameraRotationMode;

public class Raycast : MonoBehaviour
{
    public const int AvatarLocalLayerMask = 30;
    public static RaycastHit RaycastHit { get; private set; }
    public static bool Hit { get; private set; } = false;
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

        if (rotationMode != SCRM.PointerLock_Locked && rotationMode != SCRM.PointerLock_Unlocked)
        {
            ResetRaycast();
            return;
        }

        Ray ray;
        if (rotationMode == SCRM.PointerLock_Locked)
            ray = new(cam.position, cam.forward);
        else
            ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 10, ~AvatarLocalLayerMask))
        {
            RaycastHit = hit;
            Hit = true;
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
        Hit = false;
        sceneVariables.Set("cam_raycast", null);
        transform.position = Vector3.zero;
    }
}
