using SpatialSys.UnitySDK;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public bool lookAtCamera = false;
    public bool invert = false;
    public bool lockY = false;
    public float rotateSpeed = 2.0f;

    void Update()
    {
        Vector3 direction;
        if (lookAtCamera)
        {
            var cam = SpatialBridge.cameraService;
            direction = cam.position - transform.position;
        }
        else
        {
            var player = SpatialBridge.actorService.localActor.avatar;
            direction = player.position - transform.position;
        }
        if (lockY) direction.y = 0;
        if (invert) direction = -direction;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
    }
}