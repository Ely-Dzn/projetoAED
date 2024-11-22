using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(AssetDictItem))]
public class AssetDictItemDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 16f * 2;
    }
    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        var nameProp = property.FindPropertyRelative("name");
        var valueProp = property.FindPropertyRelative("value");

        EditorGUIUtility.wideMode = true;
        EditorGUIUtility.labelWidth = 70;
        rect.height /= 2;
        nameProp.stringValue = EditorGUI.TextField(rect, nameProp.stringValue);
        rect.y += rect.height;
        EditorGUI.ObjectField(rect, valueProp);
    }
}