using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PrefabColorSetting
{
    [Header("Prefab to Color")]
    public GameObject prefab; // Drag the prefab reference here
    [Header("Target Color")]
    public Color color = Color.white;
}

public class KeyEventManager : MonoBehaviour
{
    [Header("Material to Modify")]
    public Material targetMaterial;

    [Header("Prefab and Color Settings")]
    public List<PrefabColorSetting> prefabColorSettings = new List<PrefabColorSetting>();

    [Header("Tag to Color")]
    public string targetTag = "KeyEventPrefabs"; // The tag to color

    void Start()
    {
        ApplyColorToAllPrefabs();
    }

    [ContextMenu("Apply Color To All Prefabs")]
    public void ApplyColorToAllPrefabs()
    {
        if (targetMaterial == null)
        {
            Debug.LogWarning("No material specified!");
            return;
        }

        foreach (var setting in prefabColorSettings)
        {
            if (setting.prefab == null) continue;

            // Find all instances in the scene with the same name as the prefab
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                // Only process active objects whose name starts with the prefab's name
                if (obj.activeInHierarchy && obj.name.StartsWith(setting.prefab.name))
                {
                    // Find all child objects with the target tag under this prefab instance
                    Renderer[] childRenderers = obj.GetComponentsInChildren<Renderer>(true);
                    foreach (Renderer renderer in childRenderers)
                    {
                        if (renderer.gameObject.tag == targetTag)
                        {
                            Material[] mats = renderer.materials; // Automatically instantiates a copy
                            for (int i = 0; i < mats.Length; i++)
                            {
                                if (mats[i].name.StartsWith(targetMaterial.name))
                                {
                                    mats[i].color = setting.color;
                                }
                            }
                            renderer.materials = mats;
                        }
                    }
                }
            }
        }
    }
}