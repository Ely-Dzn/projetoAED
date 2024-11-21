﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpatialSys.UnitySDK;
using System.Diagnostics.CodeAnalysis;

public class WorldSpaceOverlayUI : MonoBehaviour
{
    private const string shaderTestMode = "unity_GUIZTestMode";
    [SuppressMessage("ApiDesign", "RS0030:Avoid APIs that are flagged for banning")]
    [SerializeField] UnityEngine.Rendering.CompareFunction desiredUIComparison = UnityEngine.Rendering.CompareFunction.Always;
    [Tooltip("Set to blank to automatically populate from the child UI elements")]
    [SerializeField] Graphic[] uiGraphicsToApplyTo;
    [Tooltip("Set to blank to automatically populate from the child UI elements")]
    [SerializeField] TextMeshProUGUI[] uiTextsToApplyTo;
    //Allows us to reuse materials
    private Dictionary<Material, Material> materialMappings = new();
    protected virtual void Start()
    {
        if (uiGraphicsToApplyTo == null || uiGraphicsToApplyTo.Length == 0)
        {
            uiGraphicsToApplyTo = gameObject.GetComponentsInChildren<Graphic>();
        }
        if (uiTextsToApplyTo == null || uiTextsToApplyTo.Length == 0)
        {
            uiTextsToApplyTo = gameObject.GetComponentsInChildren<TextMeshProUGUI>();
        }
        foreach (var graphic in uiGraphicsToApplyTo)
        {
            Material material = graphic.materialForRendering;
            if (material == null)
            {
                Debug.LogError($"{nameof(WorldSpaceOverlayUI)}: skipping target without material {graphic.name}.{graphic.GetType().Name}");
                continue;
            }
            if (!materialMappings.TryGetValue(material, out Material materialCopy))
            {
                materialCopy = new Material(material);
                materialMappings.Add(material, materialCopy);
            }
            materialCopy.SetInt(shaderTestMode, (int)desiredUIComparison);
            graphic.material = materialCopy;
        }
        foreach (var text in uiTextsToApplyTo)
        {
            Material material = text.fontMaterial;
            if (material == null)
            {
                Debug.LogError($"{nameof(WorldSpaceOverlayUI)}: skipping target without material {text.name}.{text.GetType().Name}");
                continue;
            }
            if (!materialMappings.TryGetValue(material, out Material materialCopy))
            {
                materialCopy = new Material(material);
                materialMappings.Add(material, materialCopy);
            }
            materialCopy.SetInt(shaderTestMode, (int)desiredUIComparison);
            text.fontMaterial = materialCopy;
        }
    }
}
