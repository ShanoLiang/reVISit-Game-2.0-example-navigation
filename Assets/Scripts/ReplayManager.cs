using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]

// Helper struct to serialize Vector3
public struct Vector3Data
{
    public float x, y, z;
    public Vector3 ToVector3() => new Vector3(x, y, z);
}

// Class to hold the saved data structure
[System.Serializable]
public class SaveData
{
    public List<Vector3Data> playerPositions;
}

public class ReplayManager : MonoBehaviour
{
    public LineRenderer fullTrajectoryLine;   // Trajectory for full path display
    public LineRenderer replayTrajectoryLine; // Trajectory for replay display
    public GameObject player;

    // List to hold the recorded positions
    private List<Vector3> replayPositions = new List<Vector3>();
    private bool isReplaying = false;

    // Start is called before the first frame update
    void Start()
    {
        LoadReplayData();
        DrawFullTrajectory();
    }

    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.P) && !isReplaying)
        {
            StartCoroutine(ReplayCoroutine());
        }
        */
    }

    void LoadReplayData()
    {
        // Load the replay data from a JSON file
        string path = Application.dataPath + "/Saves/save_1.json";
        if (File.Exists(path))
        {
            // Read and parse the JSON file
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            // Clear previous positions
            replayPositions.Clear();

            // Convert Vector3Data to Vector3 and store in the list
            foreach (var v in data.playerPositions)
            {
                replayPositions.Add(v.ToVector3());
            }
        }
        else
        {
            Debug.LogWarning("Replay file not found: " + path);
        }
    }

    // Draw the full trajectory using LineRenderer
    void DrawFullTrajectory()
    {
        if (fullTrajectoryLine == null || replayPositions.Count == 0) return;
        fullTrajectoryLine.positionCount = replayPositions.Count;
        fullTrajectoryLine.SetPositions(replayPositions.ToArray());
    }

    // Coroutine to replay the player's movement, draw the replay trajectory, and support progress callback
    public IEnumerator ReplayCoroutine(System.Action<int, int> onProgress = null)
    {
        isReplaying = true;
        if (player == null || replayPositions.Count == 0 || replayTrajectoryLine == null)
        {
            isReplaying = false;
            yield break;
        }

        replayTrajectoryLine.positionCount = 0;
        List<Vector3> walkedPositions = new List<Vector3>();

        // Start replaying from the first position
        walkedPositions.Add(replayPositions[0]);
        walkedPositions.Add(replayPositions[0]);
        replayTrajectoryLine.positionCount = 2;
        replayTrajectoryLine.SetPositions(walkedPositions.ToArray());
        onProgress?.Invoke(0, replayPositions.Count);
        yield return new WaitForSeconds(0.5f);

        // Move through each recorded position with a delay
        for (int i = 1; i < replayPositions.Count; i++)
        {
            player.transform.position = replayPositions[i];
            walkedPositions.Add(replayPositions[i]);
            replayTrajectoryLine.positionCount = walkedPositions.Count;
            replayTrajectoryLine.SetPositions(walkedPositions.ToArray());
            onProgress?.Invoke(i, replayPositions.Count);
            yield return new WaitForSeconds(0.5f);
            if (!isReplaying) break;
        }

        isReplaying = false;
    }

    // Get the total number of recorded positions
    public int GetReplayCount()
    {
        return replayPositions.Count;
    }

    // Set the player's position to a specific index in the replay data
    public void SetPlayerToIndex(int index)
    {
        if (player != null && index >= 0 && index < replayPositions.Count)
            player.transform.position = replayPositions[index];
    }

    public void StopReplay()
    {
        isReplaying = false;
        StopAllCoroutines();
    }

    public void StartReplay(System.Action<int, int> onProgress = null)
    {
        if (!isReplaying)
            StartCoroutine(ReplayCoroutine(onProgress));
    }
}