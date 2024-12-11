using SpatialSys.UnitySDK;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GrabManager))]
public class GrabManagerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        GUI.enabled = false;
        var Grabbed = GrabManager.Grabbed;
        if (!Grabbed)
        {
            EditorGUILayout.LabelField("Nenhum objeto sendo segurado");
        }
        else
        {
            EditorGUILayout.LabelField("Segurando objeto");
            EditorGUILayout.ObjectField("Objeto", Grabbed.gameObject, typeof(GameObject), allowSceneObjects: true);
            EditorGUILayout.ObjectField("Área", Grabbed.area, typeof(SpatialTriggerEvent), allowSceneObjects: true);
            GameObject group = null;
            if (Grabbed.group is GameObject)
            {
                group = Grabbed.group as GameObject;
            }
            else if (Grabbed.group is Component)
            {
                group = (Grabbed.group as Component).gameObject;
            }
            if (group != null)
            {
                EditorGUILayout.ObjectField("Grupo", group, typeof(GameObject), allowSceneObjects: true);
            }
            else
            {
                EditorGUILayout.LabelField("Grupo", "Nenhum");
            }
        }
        GUI.enabled = true;
    }
}
