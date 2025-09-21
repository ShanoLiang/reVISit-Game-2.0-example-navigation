using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MMapManager : MonoBehaviour
{
    // ...existing code...
    // Disable M_Map camera for test state
    public void DisableMMapCamera()
    {
        if (mmapCamera != null)
            mmapCamera.gameObject.SetActive(false);
        if (mmapObject != null)
            mmapObject.SetActive(false);
    }
    // Total time spent with map open
    public float totalMapOpenTime = 0f;
    [SerializeField] private GameObject mmapObject; // GameObject for the M_Map.
    [SerializeField] private Camera mmapCamera; // Camera rendering the minimap
    [SerializeField] private string playerIconLayerName = "PlayerIcon"; // Layer name for player icon

    // Track map visibility
    private bool isMapOpen = false;
    // Track player icon visibility in minimap camera
    public bool isPlayerIconVisible = true;
    // Track guiding line visibility
    public bool isGuidingLineVisible = true; // Default: guiding line shown
    [SerializeField] private string guidingLineLayerName = "Minimap"; // Layer name for guiding line
    // Track when the map was opened
    private float mapOpenStartTime = -1f;
    // List to log open times and durations
    private List<MapOpenEvent> mapOpenLog = new List<MapOpenEvent>();

    void Start()
    {

    }

    void Update()
    {
        // Press M to toggle map
        if (Input.GetKeyDown(KeyCode.M))
        {
            isMapOpen = !isMapOpen;
            if (mmapObject.activeSelf != isMapOpen)
                mmapObject.SetActive(isMapOpen);
            if (isMapOpen)
            {
                // Log when map is opened
                mapOpenStartTime = Time.timeSinceLevelLoad;
            }
            else
            {
                // Log duration when map is closed
                if (mapOpenStartTime >= 0f)
                {
                    float duration = Time.timeSinceLevelLoad - mapOpenStartTime;
                    mapOpenLog.Add(new MapOpenEvent(mapOpenStartTime, duration));
                    totalMapOpenTime += duration;
                    mapOpenStartTime = -1f;
                }
            }
        }
    }

    // Call WebGLBridge to post all map open events as JSON
    public void PostMapOpenEventsToWebGL()
    {
    string json = JsonUtility.ToJson(new MapOpenEventList(mapOpenLog));
    WebGLBridge.PostJSON("M_Map", json);
    }

    [System.Serializable]
    private class MapOpenEventList
    {
        public List<MapOpenEvent> items;
        public MapOpenEventList(List<MapOpenEvent> log)
        {
            items = new List<MapOpenEvent>(log);
        }
    }

    [System.Serializable]
    public class MapOpenEvent
    {
        public float openTime;
        public float duration;
        public MapOpenEvent(float openTime, float duration)
        {
            this.openTime = openTime;
            this.duration = duration;
        }
    }
    public List<MapOpenEvent> GetMapOpenLog()
    {
        return mapOpenLog;
    }

    // Public method to set whether minimap camera includes PlayerIcon layer
    public void SetPlayerIconVisible(bool visible)
    {
        isPlayerIconVisible = visible;
        if (mmapCamera != null)
        {
            int layer = LayerMask.NameToLayer(playerIconLayerName);
            if (layer < 0) return;
            int layerMask = mmapCamera.cullingMask;
            if (visible)
            {
                // Include PlayerIcon layer
                layerMask |= (1 << layer);
            }
            else
            {
                // Exclude PlayerIcon layer
                layerMask &= ~(1 << layer);
            }
            mmapCamera.cullingMask = layerMask;
        }
    }

    // Public method to set guiding line visibility
    public void SetGuidingLineVisible(bool visible)
    {
        isGuidingLineVisible = visible;
        if (mmapCamera != null)
        {
            int layer = LayerMask.NameToLayer(guidingLineLayerName);
            if (layer < 0) return;
            int layerMask = mmapCamera.cullingMask;
            if (visible)
            {
                // Include Minimap layer (guiding line)
                layerMask |= (1 << layer);
            }
            else
            {
                // Exclude Minimap layer (guiding line)
                layerMask &= ~(1 << layer);
            }
            mmapCamera.cullingMask = layerMask;
        }
    }
}
