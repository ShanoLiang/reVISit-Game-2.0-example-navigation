using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReplayBar : MonoBehaviour
{
    public ReplayManager replayManager;
    public Button playPauseButton;
    public Slider progressBar;
    public TextMeshProUGUI timeText;

    private bool isPlaying = false;
    private int currentIndex = 0;
    private float interval = 0.5f;

    void Start()
    {
        playPauseButton.onClick.AddListener(TogglePlayPause);
        progressBar.onValueChanged.AddListener(OnSliderChanged);

        InitProgressBar();
        UpdateUI(0, replayManager.GetReplayCount());
    }

    void InitProgressBar()
    {
        int count = replayManager != null ? replayManager.GetReplayCount() : 0;
        progressBar.minValue = 0f;
        progressBar.maxValue = 1f;
        progressBar.value = 0f;
        progressBar.interactable = count > 1;
    }

    void TogglePlayPause()
    {
        if (isPlaying)
            Pause();
        else
            Play();
    }

    void Play()
    {
        isPlaying = true;
        replayManager.StopReplay();
        replayManager.StartReplay(OnReplayProgress);
        // Optionally switch button icon to pause
    }

    void Pause()
    {
        isPlaying = false;
        replayManager.StopReplay();
        // Optionally switch button icon to play
    }

    void OnSliderChanged(float value)
    {
        int count = replayManager.GetReplayCount();
        if (count < 2) return;
        int index = Mathf.Clamp(Mathf.RoundToInt(value * (count - 1)), 0, count - 1);
        replayManager.SetPlayerToIndex(index);
        UpdateUI(index, count);
    }

    void OnReplayProgress(int index, int count)
    {
        currentIndex = index;
        UpdateUI(index, count);
    }

    void UpdateUI(int index, int count)
    {
        // Update progress bar
        progressBar.value = count > 1 ? (float)index / (count - 1) : 0f;

        // Update time text
        if (timeText != null)
        {
            if (count <= 1)
            {
                timeText.text = "0.0s / 0.0s";
            }
            else
            {
                float total = (count - 1) * interval;
                float current = Mathf.Min(index * interval, total);
                timeText.text = $"{current:F1}s / {total:F1}s";
            }
        }
    }
}