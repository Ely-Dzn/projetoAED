using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using SpatialSys.UnitySDK;
using System.Collections;

[DisallowMultipleComponent]
public class StackTest : MonoBehaviour
{
    Variables variables;

    void Start()
    {
        variables = gameObject.GetOrAddComponent<Variables>();

    }
    void Update()
    {
        var items = (ArrayList)variables.declarations.Get("items");

        if (SpatialBridge.cameraService.rotationMode != SpatialCameraRotationMode.PointerLock_Locked)
        {
            foreach (var item in items)
            {
                var go = (GameObject)item;
                var interactable = go.GetComponentInParent<SpatialInteractable>();
                if (interactable != null)
                {
                    interactable.enabled = true;
                }
            }
            return;
        }

        var hit = Variables.Scene(gameObject).Get("cam_raycast");
        var target = hit != null ? ((RaycastHit)hit).transform.gameObject : null;
        while (target != null && items.IndexOf(target) == -1)
        {
            var parent = target.transform.parent;
            target = parent != null ? parent.gameObject : null;
        }
        foreach (var item in items)
        {
            var go = (GameObject)item;
            var interactable = go.GetComponentInParent<SpatialInteractable>();
            if (interactable == null) continue;
            if (go == target)
            {
                interactable.enabled = true;
                go.transform.Rotate(0, Time.deltaTime * 100f, 0);
            }
            else
            {
                interactable.enabled = false;
            }
        }
        // var hit = (RaycastHit)Variables.Scene(gameObject).Get("cam_raycast");
        // if (hit)
        // {
        //     var interactable = hit.transform.getComponent<SpatialInteractable>();
        //     Debug.Log("Grabbed");
        // }
    }
}