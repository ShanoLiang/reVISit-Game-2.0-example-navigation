using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class ButtonAnimation : MonoBehaviour
{
    [Header("Button Sprites")]
    public Sprite normalSprite;
    public Sprite pressedSprite;
    public Sprite toggledSprite;
    public Sprite untogglePressedSprite;

    [Header("Audio")]
    public AudioClip pressSound;

    private Image image;
    private Button button;
    private AudioSource audioSource;
    private bool isToggled = false;

    void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();

        // Add AudioSource if not present
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        button.onClick.AddListener(OnButtonClick);

        // Initialize as normal
        if (normalSprite != null)
            image.sprite = normalSprite;
    }

    public void OnButtonClick()
    {
        StartCoroutine(HandleButtonPress());
    }

    private IEnumerator HandleButtonPress()
    {
        // Play sound
        if (pressSound != null)
            audioSource.PlayOneShot(pressSound);

        // Show pressed sprite depending on toggle state
        if (!isToggled)
        {
            if (pressedSprite != null)
                image.sprite = pressedSprite;
        }
        else
        {
            if (untogglePressedSprite != null)
                image.sprite = untogglePressedSprite;
        }

        yield return new WaitForSeconds(0.1f);

        if (!isToggled)
        {
            // Switch to toggled
            if (toggledSprite != null)
                image.sprite = toggledSprite;
            isToggled = true;
        }
        else
        {
            // Switch back to normal
            if (normalSprite != null)
                image.sprite = normalSprite;
            isToggled = false;
        }
    }
}
