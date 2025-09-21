using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapManager : MonoBehaviour
{
    // ...existing code...
    // Disable minimap camera for test state
    public void DisableMinimapCamera()
    {
        if (minimapCameraObject != null)
            minimapCameraObject.SetActive(false);
        // Do NOT disable minimapGameObject (canvas/UI)
    }
    [Header("Minimap Settings")]
    [SerializeField] private GameObject minimapGameObject; // Assign the minimap UI GameObject in inspector
    [SerializeField] private GameObject minimapCameraObject; // Assign the minimap camera GameObject in inspector
    [SerializeField] private string playerTargetLayerName = "PlayerTarget"; // Layer name for player target

    public bool minimapVisible = true;
    public bool showPlayerTarget = true;
    public bool showGuidingLine = false; // Toggle for guiding line on minimap
    [SerializeField] private string guidingLineLayerName = "Minimap"; // Layer name for guiding line
    private int guidingLineLayer;

    private Camera minimapCamera;
    private int playerTargetLayer;

    void Awake()
    {
        if (minimapCameraObject != null)
        {
            minimapCamera = minimapCameraObject.GetComponent<Camera>();
        }
        playerTargetLayer = LayerMask.NameToLayer(playerTargetLayerName);
    guidingLineLayer = LayerMask.NameToLayer(guidingLineLayerName);
    }

    void Start()
    {
        // Default states
        SetMinimapVisibility(minimapVisible);
        SetPlayerTargetVisibility(showPlayerTarget);
    SetGuidingLineVisibility(showGuidingLine);
    }

    // Toggle minimap visibility
    public void ToggleMinimapVisibility()
    {
        minimapVisible = !minimapVisible;
        SetMinimapVisibility(minimapVisible);
    }

    // Toggle guiding line visibility
    public void ToggleGuidingLineVisibility()
    {
        showGuidingLine = !showGuidingLine;
        SetGuidingLineVisibility(showGuidingLine);
    }

    // Set guiding line visibility by changing camera culling mask
    public void SetGuidingLineVisibility(bool show)
    {
        showGuidingLine = show;
        if (minimapCamera != null && guidingLineLayer >= 0)
        {
            if (show)
            {
                minimapCamera.cullingMask |= (1 << guidingLineLayer);
            }
            else
            {
                minimapCamera.cullingMask &= ~(1 << guidingLineLayer);
            }
        }
    }

    // Set minimap visibility
    public void SetMinimapVisibility(bool visible)
    {
        minimapVisible = visible;
        if (minimapGameObject != null)
            minimapGameObject.SetActive(visible);
        if (minimapCameraObject != null)
            minimapCameraObject.SetActive(visible);
    }

    // Toggle player target visibility
    public void TogglePlayerTargetVisibility()
    {
        showPlayerTarget = !showPlayerTarget;
        SetPlayerTargetVisibility(showPlayerTarget);
    }

    // Set player target visibility by changing camera culling mask
    public void SetPlayerTargetVisibility(bool show)
    {
        showPlayerTarget = show;
        if (minimapCamera != null && playerTargetLayer >= 0)
        {
            if (show)
            {
                minimapCamera.cullingMask |= (1 << playerTargetLayer);
            }
            else
            {
                minimapCamera.cullingMask &= ~(1 << playerTargetLayer);
            }
        }
    }
}
