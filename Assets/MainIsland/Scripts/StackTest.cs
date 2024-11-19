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
        var slots = (ArrayList)variables.declarations.Get("slots");
        var items = (ArrayList)variables.declarations.Get("items");
        var cam = SpatialBridge.cameraService;

#if !UNITY_EDITOR
        if (cam.rotationMode != SpatialCameraRotationMode.PointerLock_Locked)
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
#endif

        var hit = Variables.Scene(gameObject).Get("cam_raycast");
        var target = hit != null ? ((RaycastHit)hit).transform.gameObject : null;
        while (target != null && items.IndexOf(target) == -1)
        {
            var parent = target.transform.parent;
            target = parent != null ? parent.gameObject : null;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = (GameObject)slots[i];
            var interactable = slot.GetComponentInChildren<SpatialInteractable>();
            if (interactable == null) continue;
            if (i < items.Count)
            {
                var item = (GameObject)items[i];
                var outline = item.GetComponentInChildren<Outline>();
                if (item == target)
                {
                    interactable.enabled = true;
                    outline.enabled = true;
                    continue;
                }
                outline.enabled = false;
            }
            interactable.enabled = false;
        }
    }
}
