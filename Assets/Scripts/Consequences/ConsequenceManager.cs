using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages environmental narrative consequences when players fail challenges.
/// Triggers visual, audio, and UI effects across the office.
/// Place on the same GameObject as ChallengeManager.
/// </summary>
public class ConsequenceManager : MonoBehaviour
{
    public static ConsequenceManager Instance { get; private set; }

    [Header("Consequence UI")]
    [Tooltip("Full-screen overlay panel for consequence alerts.")]
    public GameObject consequenceOverlayPanel;

    [Tooltip("Text component for the consequence narrative.")]
    public TMPro.TextMeshProUGUI consequenceText;

    [Header("Office Environment References")]
    [Tooltip("Renderers on office monitors to swap to ransomware texture.")]
    public Renderer[] officeMonitorRenderers;

    [Tooltip("Material to apply to monitors during ransomware consequence.")]
    public Material ransomwareMaterial;

    [Tooltip("Original material to restore on monitors.")]
    public Material normalMonitorMaterial;

    [Tooltip("Lights in the office that can flicker or change color.")]
    public Light[] officeLights;

    [Header("Audio")]
    [Tooltip("AudioSource for alarm / dramatic sounds.")]
    public AudioSource consequenceAudioSource;

    [Tooltip("Alarm sound clip.")]
    public AudioClip alarmClip;

    [Tooltip("Dramatic stinger sound clip.")]
    public AudioClip stingerClip;

    [Header("Timing")]
    [Tooltip("How long the consequence effect persists before fading.")]
    public float consequenceDuration = 12f;

    [Header("Events")]
    public UnityEvent<ChallengeData> OnConsequenceTriggered;
    public UnityEvent OnConsequenceEnded;

    private Coroutine activeConsequence;
    private Color[] originalLightColors;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Store original light colors
        if (officeLights != null && officeLights.Length > 0)
        {
            originalLightColors = new Color[officeLights.Length];
            for (int i = 0; i < officeLights.Length; i++)
            {
                if (officeLights[i] != null)
                    originalLightColors[i] = officeLights[i].color;
            }
        }

        // Hide overlay on start
        if (consequenceOverlayPanel != null)
            consequenceOverlayPanel.SetActive(false);
    }

    /// <summary>
    /// Triggers an environmental consequence based on the failed challenge data.
    /// </summary>
    public void TriggerConsequence(ChallengeData failedChallenge)
    {
        if (activeConsequence != null)
            StopCoroutine(activeConsequence);

        activeConsequence = StartCoroutine(ConsequenceRoutine(failedChallenge));
        OnConsequenceTriggered?.Invoke(failedChallenge);
    }

    private IEnumerator ConsequenceRoutine(ChallengeData data)
    {
        // --- PHASE 1: Dramatic consequence effect ---

        // Show overlay with narrative text
        if (consequenceOverlayPanel != null)
        {
            consequenceOverlayPanel.SetActive(true);
            if (consequenceText != null)
                consequenceText.text = data.consequenceNarrative;
        }

        // Play appropriate audio
        PlayConsequenceAudio(data.consequenceType);

        // Apply environmental effects based on type
        switch (data.consequenceType)
        {
            case ConsequenceType.RansomwareScreen:
                ApplyRansomwareScreens();
                break;

            case ConsequenceType.NetworkDown:
                yield return StartCoroutine(FlickerLights(3f));
                break;

            case ConsequenceType.DataBreach:
                TintLights(new Color(1f, 0.3f, 0.3f)); // red tint
                break;

            case ConsequenceType.PhysicalBreach:
                TintLights(new Color(1f, 0.6f, 0f)); // orange warning
                break;

            case ConsequenceType.GenericAlert:
            default:
                TintLights(new Color(1f, 0.2f, 0.2f)); // deep red
                break;
        }

        // Hold for duration
        yield return new WaitForSeconds(consequenceDuration);

        // --- PHASE 2: Cleanup and debrief ---
        RestoreEnvironment();

        // Hide consequence overlay
        if (consequenceOverlayPanel != null)
            consequenceOverlayPanel.SetActive(false);

        // Show debrief panel
        DebriefPanel debrief = FindFirstObjectByType<DebriefPanel>();
        if (debrief != null)
        {
            debrief.Show(data);
        }
        else
        {
            // If no debrief panel, just notify ChallengeManager to release player
            ChallengeManager.Instance?.OnChallengeUIClosed();
        }

        OnConsequenceEnded?.Invoke();
        activeConsequence = null;
    }

    private void PlayConsequenceAudio(ConsequenceType type)
    {
        if (consequenceAudioSource == null) return;

        AudioClip clip = type == ConsequenceType.RansomwareScreen ? stingerClip : alarmClip;
        if (clip != null)
        {
            consequenceAudioSource.PlayOneShot(clip);
        }
    }

    private void ApplyRansomwareScreens()
    {
        if (officeMonitorRenderers == null || ransomwareMaterial == null) return;

        foreach (var renderer in officeMonitorRenderers)
        {
            if (renderer != null)
                renderer.material = ransomwareMaterial;
        }
    }

    private void TintLights(Color tintColor)
    {
        if (officeLights == null) return;

        foreach (var light in officeLights)
        {
            if (light != null)
                light.color = tintColor;
        }
    }

    private IEnumerator FlickerLights(float duration)
    {
        if (officeLights == null || officeLights.Length == 0) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            foreach (var light in officeLights)
            {
                if (light != null)
                    light.enabled = !light.enabled;
            }
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
            elapsed += 0.15f;
        }

        // Ensure lights end on
        foreach (var light in officeLights)
        {
            if (light != null)
                light.enabled = true;
        }
    }

    private void RestoreEnvironment()
    {
        // Restore monitor materials
        if (officeMonitorRenderers != null && normalMonitorMaterial != null)
        {
            foreach (var renderer in officeMonitorRenderers)
            {
                if (renderer != null)
                    renderer.material = normalMonitorMaterial;
            }
        }

        // Restore light colors
        if (officeLights != null && originalLightColors != null)
        {
            for (int i = 0; i < officeLights.Length; i++)
            {
                if (officeLights[i] != null && i < originalLightColors.Length)
                    officeLights[i].color = originalLightColors[i];
            }
        }
    }
}
