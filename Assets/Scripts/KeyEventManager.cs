using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PrefabColorSetting
{
    [Header("Prefab to Color")]
    public GameObject prefab;
    [Header("Target Color")]
    public Color color = Color.white;
}

[System.Serializable]
public class ButtonToggleTarget
{
    [Header("UI Button")]
    public Button button;
    [Header("Target GameObject")]
    public GameObject target;
    [HideInInspector]
    public bool isActive = true;
}

public class KeyEventManager : MonoBehaviour
{
    [Header("Material to Modify")]
    public Material targetMaterial;

    [Header("Prefab and Color Settings")]
    public List<PrefabColorSetting> prefabColorSettings = new List<PrefabColorSetting>();

    [Header("Tag to Color")]
    public string targetTag = "KeyEventPrefabs";

    [Header("Button-Target Toggle Settings")]
    public List<ButtonToggleTarget> buttonToggleTargets = new List<ButtonToggleTarget>();

    void Start()
    {
        ApplyColorToAllPrefabs();

        // Add listeners for all toggle buttons
        foreach (var toggle in buttonToggleTargets)
        {
            if (toggle.button != null && toggle.target != null)
            {
                // Cache local reference for closure
                GameObject targetObj = toggle.target;
                toggle.button.onClick.AddListener(() =>
                {
                    bool newState = !targetObj.activeSelf;
                    targetObj.SetActive(newState);
                });
            }
        }
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

            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.activeInHierarchy && obj.name.StartsWith(setting.prefab.name))
                {
                    Renderer[] childRenderers = obj.GetComponentsInChildren<Renderer>(true);
                    foreach (Renderer renderer in childRenderers)
                    {
                        if (renderer.gameObject.tag == targetTag)
                        {
                            Material[] mats = renderer.materials;
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