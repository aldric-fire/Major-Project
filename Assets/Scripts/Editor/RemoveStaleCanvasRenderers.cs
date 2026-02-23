using UnityEngine;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor utility to remove unnecessary CanvasRenderer components from TextMeshPro (world-space) objects.
/// Run via menu: Tools > TMP > Remove Stale CanvasRenderers
/// </summary>
public static class RemoveStaleCanvasRenderers
{
    [MenuItem("Tools/TMP/Remove Stale CanvasRenderers")]
    public static void RemoveAll()
    {
        int removed = 0;

        // Find ALL TextMeshPro (world-space, NOT TextMeshProUGUI) objects in the scene
        TextMeshPro[] tmps = Object.FindObjectsByType<TextMeshPro>(FindObjectsSortMode.None);

        foreach (var tmp in tmps)
        {
            CanvasRenderer cr = tmp.GetComponent<CanvasRenderer>();
            if (cr != null)
            {
                Undo.DestroyObjectImmediate(cr);
                removed++;
                Debug.Log($"Removed CanvasRenderer from [{tmp.gameObject.name}]");
            }
        }

        if (removed > 0)
        {
            EditorUtility.DisplayDialog("Done",
                $"Removed {removed} stale CanvasRenderer component(s).\nSave your scene to keep the changes.",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Done", "No stale CanvasRenderer components found.", "OK");
        }
    }
}
