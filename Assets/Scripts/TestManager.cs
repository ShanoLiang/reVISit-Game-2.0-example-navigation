using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TestManager : MonoBehaviour
{
    [Header("Test Setup")]
    [Tooltip("Assign reference photos (sprites) for each target.")]
    public List<Sprite> referencePhotos;

    [Header("Managers")]
    public MiniMapManager miniMapManager;
    public MMapManager mMapManager;

    void Start()
    {
        SetupTestState();
    }

    public void SetupTestState()
    {
        // Disable M_Map and minimap cameras (if present)
        if (mMapManager != null)
            mMapManager.DisableMMapCamera();
        if (miniMapManager != null)
            miniMapManager.DisableMinimapCamera();

        // Assign reference photos to targets (loop if not enough photos)
        var targetBehaviour = FindObjectOfType<TargetBehaviour>();
        if (targetBehaviour != null && referencePhotos != null && referencePhotos.Count > 0)
        {
            // Assign reference photo for the current target
            int photoIndex = targetBehaviour.currentTargetIndex % referencePhotos.Count;
            targetBehaviour.SetReferencePhoto(referencePhotos[photoIndex]);
        }
    }
}
