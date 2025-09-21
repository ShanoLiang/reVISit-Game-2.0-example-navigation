using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetBehaviour : MonoBehaviour
{
    // RawImage slot for reference photo (used only in test mode)
    [Header("Test Reference Photo")]
    public UnityEngine.UI.RawImage referencePhotoSlot;

    // Called by TestManager to set the reference photo for the current target
    public void SetReferencePhoto(Sprite referencePhoto)
    {
        if (referencePhotoSlot != null && referencePhoto != null)
            referencePhotoSlot.texture = referencePhoto.texture;
    }
    // Assign a reference photo to the target's minimap RawImage slot
    public void SetReferencePhoto(int targetIndex, Sprite referencePhoto)
    {
        if (targets == null || targetIndex < 0 || targetIndex >= targets.Count)
            return;
        var rawImage = targets[targetIndex].GetComponentInChildren<UnityEngine.UI.RawImage>();
        if (rawImage != null && referencePhoto != null)
            rawImage.texture = referencePhoto.texture;
    }
    public int TotalTargetCount => targets != null ? targets.Count : 0;
    public int currentTargetIndex = 0; // Index of the current target.
    [Header("Assign Target Objects")]
    [SerializeField] private List<GameObject> targets;

    private void Start()
    {
        SetTargetPosition(currentTargetIndex);
    }

    public void NextTargetLocation()
    {
        currentTargetIndex++;

        // If reached the last target...
    if (currentTargetIndex >= targets.Count)
        {
            // Reset target index.
            currentTargetIndex = 0;

            // Stop the game.
            GameManager gm = GameObject.FindObjectOfType<GameManager>();
            gm.StopGame();
        }

        SetTargetPosition(currentTargetIndex);
        // If TestManager is present, update reference photo
        var testManager = FindObjectOfType<TestManager>();
        if (testManager != null && testManager.referencePhotos != null && testManager.referencePhotos.Count > 0)
        {
            int photoIndex = currentTargetIndex % testManager.referencePhotos.Count;
            SetReferencePhoto(testManager.referencePhotos[photoIndex]);
        }
    }

    private void SetTargetPosition(int index)
    {
        if (targets == null || targets.Count == 0)
            return;
        if (index >= targets.Count)
            return;

        transform.position = targets[index].transform.position;
        // Player should NOT be moved between target tasks
    }

}
