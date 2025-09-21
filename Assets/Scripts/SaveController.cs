using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Save : MonoBehaviour
{
    public PlayerController playerController;
    private List<Vector3> positionHistory = new List<Vector3>();
    private List<KeyEventData> keyEvents = new List<KeyEventData>();
    private Coroutine recordCoroutine;
    private bool isRecording = false;

    private void Awake()
    {
        SaveManager.Init();
    }

    void Update()
    {
        if (isRecording)
        {
            // Record key events
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RecordKeyEvent("Space");
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                RecordKeyEvent("M");
            }
        }
    }

    // Record key event with timestamp and position
    private void RecordKeyEvent(string key)
    {
        var evt = new KeyEventData
        {
            time = Time.time,
            position = playerController.transform.position,
            key = key
        };
        keyEvents.Add(evt);

        // Optional: Log the key event for debugging
        Debug.Log($"Key {key} pressed at {evt.position} (t={evt.time})");
    }

    public void StartRecording()
    {
        positionHistory.Clear();
        keyEvents.Clear();
        isRecording = true;
        recordCoroutine = StartCoroutine(RecordPositionCoroutine());
        Debug.Log("Start recording");
    }

    // Stop recording and save trajectory
    public void StopRecordingAndSave()
    {
        isRecording = false;
        if (recordCoroutine != null)
        {
            StopCoroutine(recordCoroutine);
        }
        SaveTrajectory();
        Debug.Log("Stop recording and save");
    }

    // Recording methods
    private IEnumerator RecordPositionCoroutine()
    {
        while (true)
        {
            positionHistory.Add(playerController.transform.position);
            // Set the interval length for each recording (1f = 1 second)
            yield return new WaitForSeconds(0.5f);

            // Optional: Log the recorded position for debugging
            //Debug.Log("Recording position: " + playerController.transform.position);
        }
    }

    // Save trajectory to JOSN file
    private void SaveTrajectory()
    {
        SaveData saveData = new SaveData
        {
            playerPositions = new List<Vector3>(positionHistory),
            keyEvents = new List<KeyEventData>(keyEvents)
        };
        string json = JsonUtility.ToJson(saveData, true);
        SaveManager.Save(json);
        Debug.Log("Save trajectory");
    }

    // Data structure for key events
    [System.Serializable]
    public class KeyEventData
    {
        public float time;
        public Vector3 position;
        public string key;
    }

    // Data structure for saving data (positions and key events)
    [System.Serializable]
    private class SaveData
    {
        public List<Vector3> playerPositions;
        public List<KeyEventData> keyEvents;
    }
}
