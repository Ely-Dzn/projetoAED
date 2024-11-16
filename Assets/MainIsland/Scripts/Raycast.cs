using Unity.VisualScripting;
using UnityEngine;
using SpatialSys.UnitySDK;

using SCRM = SpatialSys.UnitySDK.SpatialCameraRotationMode;

public class Raycast : MonoBehaviour
{
    public const int AvatarLocalLayerMask = 30;
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
            Reset();
            return;
        }

        Ray ray;
        if (rotationMode == SCRM.PointerLock_Locked)
            ray = new(cam.position, cam.forward);
        else
            ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 10, ~AvatarLocalLayerMask))
        {
            sceneVariables.Set("cam_raycast", hit);
            transform.position = hit.point;
        }
        else
        {
            Reset();
        }
    }
    void Reset()
    {
        sceneVariables.Set("cam_raycast", null);
        transform.position = Vector3.zero;
    }
}
