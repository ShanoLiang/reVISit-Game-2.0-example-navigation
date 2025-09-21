using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public struct RouteCompletionEvent {
        public int routeIndex;
        public float timeSpent;
    }

    [System.Serializable]
    public struct SummaryEvent {
        public string scene;
        public string assistance;
        public float mapReviewTime;
        public float distanceTravelled;
        public float completionTime;
    }

    private List<RouteCompletionEvent> routeCompletionEvents = new List<RouteCompletionEvent>();
    private float routeStartTime = 0f;
    [SerializeField] private GameObject endGameObject; // UI GameObject to show at the end of the game.

    void Awake()
    {
        // Get references to managers at runtime
        mMapManager = FindObjectOfType<MMapManager>();
        miniMapManager = FindObjectOfType<MiniMapManager>();
        navigator = FindObjectOfType<Navigator>();

        // Parse assistance type from URL if running in WebGL
        #if UNITY_WEBGL && !UNITY_EDITOR
        string url = Application.absoluteURL;
        if (!string.IsNullOrEmpty(url))
        {
            int idx = url.IndexOf("assistance=");
            if (idx >= 0)
            {
                int start = idx + "assistance=".Length;
                int end = url.IndexOf('&', start);
                string value = (end > start) ? url.Substring(start, end - start) : url.Substring(start);
                int assistInt = 0;
                int.TryParse(value, out assistInt);
                switch (assistInt)
                {
                    case 0:
                        stateSelected = AssistanceState.None;
                        break;
                    case 1:
                        stateSelected = AssistanceState.Moderate;
                        break;
                    case 2:
                        stateSelected = AssistanceState.Strong;
                        break;
                    case 3:
                        stateSelected = AssistanceState.NA;
                        break;
                }
            }
        }
        #endif
    }

    public enum AssistanceState {
    None, Moderate, Strong, NA // NA for test state
    }

    // UI COMPONENTS
    [SerializeField] private GameObject nextRoundGameObject; // UI GameObject for when the next round begins.

    // ASSISTANCE
    public AssistanceState stateSelected; // The assistance state (none, moderate, strong)
    [SerializeField] private MMapManager mMapManager; // Reference to the MMapManager script/component.
    [SerializeField] private MiniMapManager miniMapManager; // Reference to the MiniMapManager script/component.
    [SerializeField] private Navigator navigator; // Reference to the Navigator script/component.

    // TIMER-RELATED
    public static bool timerRunning = false; // Is the timer currently running?
    float timerValue = 90; // What is the timer value?
    public float maxTimerValue = 90; // What is the maximum timer value?

    [SerializeField] private TextMeshProUGUI timerValueText; // Get the text object that displays the timer value.

    // TASKS-RELATED
    public static int rounds = 0; // How many rounds have been completed?

    [SerializeField] private TextMeshProUGUI tasksValueText; // Get the text object that displays the tasks value.

    // Start is called before the first frame update
    void Start()
    {
        // Hide the end game object at the beginning
        if (endGameObject != null) endGameObject.SetActive(false);

        // Send game started event
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string assistanceType = stateSelected.ToString();
        // Update the display text.
        UpdateTasksText();
        UpdateTimerText();
        SetAssistanceState(stateSelected);

        // Start timing first route
        routeStartTime = Time.timeSinceLevelLoad;
    }

    // Update is called once per frame
    void Update()
    {
        // Start updating the time if the timer is running.
        if (timerRunning) UpdateTimer();

        // Update the tasks text.
        UpdateTasksText();

        // Detect if the player has started moving.
        if (!timerRunning)
        {
            DetectMovement();
        }
    }

    // Advance to the next task.
    public void AdvanceTask()
    {
        float timeSpent = Time.timeSinceLevelLoad - routeStartTime;
        routeCompletionEvents.Add(new RouteCompletionEvent {
            routeIndex = rounds,
            timeSpent = Mathf.Min(timeSpent, maxTimerValue)
        });

        if (rounds < 8)
        {
            rounds++;
            ResetTimer();
            NextRoundNotification();
            routeStartTime = Time.timeSinceLevelLoad;
        }
        else
        {
            StopGame();
        }
    }

    // Set the assistance state for the game.
    private void SetAssistanceState(AssistanceState newState)
    {
        // Save the new state
        stateSelected = newState;

        switch (stateSelected)
        {
            case AssistanceState.None:
                // NO player locator on M_Map
                if (mMapManager != null) {
                    mMapManager.SetPlayerIconVisible(false);
                    mMapManager.SetGuidingLineVisible(false);
                }
                // NO minimap
                if (miniMapManager != null) miniMapManager.SetMinimapVisibility(false);
                // NO minimap guiding line
                if (miniMapManager != null) miniMapManager.SetGuidingLineVisibility(false);
                // NO guiding lines
                if (navigator != null) navigator.SetGuidingLinesActive(false);
                break;
            case AssistanceState.Moderate:
                // Player locator on M_Map
                if (mMapManager != null) {
                    mMapManager.SetPlayerIconVisible(true);
                    mMapManager.SetGuidingLineVisible(false);
                }
                // Minimap
                if (miniMapManager != null) miniMapManager.SetMinimapVisibility(true);
                // NO minimap guiding line
                if (miniMapManager != null) miniMapManager.SetGuidingLineVisibility(false);
                // NO guiding lines
                if (navigator != null) navigator.SetGuidingLinesActive(false);
                break;
            case AssistanceState.Strong:
                // Player locator on M_Map
                if (mMapManager != null) {
                    mMapManager.SetPlayerIconVisible(true);
                    mMapManager.SetGuidingLineVisible(true);
                }
                // Minimap
                if (miniMapManager != null) miniMapManager.SetMinimapVisibility(true);
                // Minimap guiding line ON
                if (miniMapManager != null) miniMapManager.SetGuidingLineVisibility(true);
                // Guiding lines
                if (navigator != null) navigator.SetGuidingLinesActive(true);
                break;
            case AssistanceState.NA:
                // Test state: delegate camera and minimap setup to TestManager
                var testManager = FindObjectOfType<TestManager>();
                if (testManager != null)
                {
                    testManager.SetupTestState();
                }
                // Guiding lines off
                if (navigator != null) navigator.SetGuidingLinesActive(false);
                break;
        }
    }

    // Detect if the player has started moving.
    private void DetectMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // Check if input values have changed for vertical or horizontal input values
        if (Mathf.Abs(x) > 0 || Mathf.Abs(y) > 0)
        {
            // Movement detected!! Do something
            timerRunning = true;
        }
    }

    // Reset the timer value.
    private void ResetTimer()
    {
        timerValue = maxTimerValue;
    }

    // Update the timer value.
    private void UpdateTimer()
    {
        // Decrease the timer value every frame.
        timerValue -= Time.deltaTime;

        // Update the text.
        UpdateTimerText();

        // When time is up...
        if (timerValue <= 0)
        {
            // If all rounds are completed...
            if (rounds >= 8)
            {
                // Stop running the timer.
                timerRunning = false;

                // Stop the game.
                StopGame();
                return;
            }

            // Reset the timer value.
            ResetTimer();

            // Go to the next target.
            TargetBehaviour tb = GameObject.FindObjectOfType<TargetBehaviour>();
            tb.NextTargetLocation();

            // Advance to the next task.
            AdvanceTask();
        }
    }

    // Update the timer text display.
    private void UpdateTimerText()
    {
        // Display the value on the text.
        timerValueText.SetText(((int) timerValue) + "s");
    }

    // Update the tasks text display.
    private void UpdateTasksText()
    {
    // Display the value on the text.
        int totalTargets = 8;
        TargetBehaviour tb = GameObject.FindObjectOfType<TargetBehaviour>();
        if (tb != null)
            totalTargets = tb.TotalTargetCount;
        tasksValueText.SetText((rounds+1) + "/" + totalTargets);
    }

    // Show the notification for the next round.
    private void NextRoundNotification()
    {
        nextRoundGameObject.SetActive(true);
        Invoke("HideNextRoundNotification", 3);
    }

    // Hide the notification for the next round.
    private void HideNextRoundNotification()
    {
        nextRoundGameObject.SetActive(false);
    }

    // Stop running the game.
    public void StopGame()
    {
        // Ensure last route completion is recorded
        if (routeCompletionEvents.Count == rounds) {
            float timeSpent = Time.timeSinceLevelLoad - routeStartTime;
            routeCompletionEvents.Add(new RouteCompletionEvent {
                routeIndex = rounds,
                timeSpent = Mathf.Min(timeSpent, maxTimerValue)
            });
        }

        // Prepare and send summary event data
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string assistanceType = stateSelected.ToString();
        float totalMapReviewTime = mMapManager != null ? mMapManager.totalMapOpenTime : 0f;
        PlayerController player = FindObjectOfType<PlayerController>();
        float totalDistanceTravelled = player != null ? player.totalDistanceTravelled : 0f;
        float totalCompletionTime = 0f;
        foreach (var route in routeCompletionEvents) {
            totalCompletionTime += route.timeSpent;
        }
        SummaryEvent summary = new SummaryEvent {
            scene = sceneName,
            assistance = assistanceType,
            mapReviewTime = totalMapReviewTime,
            distanceTravelled = totalDistanceTravelled,
            completionTime = totalCompletionTime
        };
        string summaryJson = JsonUtility.ToJson(summary);
        WebGLBridge.PostJSON("summary_complete", summaryJson);

        // Show the end game object
        if (endGameObject != null) endGameObject.SetActive(true);
        // Send route completion data to WebGL
        string routeJson = JsonUtility.ToJson(new RouteCompletionEventList { items = routeCompletionEvents.ToArray() });
        WebGLBridge.PostJSON("route_complete", routeJson);
        // Send map open events data
        if (mMapManager != null) mMapManager.PostMapOpenEventsToWebGL();
        // Send player distance data
        if (player != null) player.PostTotalDistanceToWebGL();

        // Stop recording
        var saveController = FindObjectOfType<Save>();
        if (saveController != null)
        {
            saveController.StopRecordingAndSave();
        }
        else 
        {
            Debug.LogWarning("SaveController not found. Unable to stop recording and save.");
        }
    }

    [System.Serializable]
    public class RouteCompletionEventList { public RouteCompletionEvent[] items; }
}
