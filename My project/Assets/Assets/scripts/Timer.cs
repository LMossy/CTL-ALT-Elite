using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class UITimer_TimeSurvived : MonoBehaviour
{
    [Header("UI Wiring")]
    [SerializeField] private TextMeshProUGUI timeSurvivedLabel;

    [Header("Behaviour")]
    [SerializeField] private bool autoStart = true;
    [Tooltip("Show hundredths: mm:ss.ff")]
    [SerializeField] private bool showHundredths = false;
    [Tooltip("Time scale multiplier (1 = real time)")]
    [SerializeField] private float timeScale = 1f;

    [Header("Colours (HEX)")]
    [Tooltip("Normal state colour (HEX). Examples: #E6E6E6, #FFFFFF, #B22222")]
    [SerializeField] private string normalHex = "#E6E6E6";
    [Tooltip("Danger state colour (HEX). Examples: #FF2E2E, #FF4500")]
    [SerializeField] private string dangerHex = "#FF2E2E";

    [Header("Danger State")]
    [Tooltip("When elapsed time >= this value (seconds), switch to danger style")]
    [SerializeField] private float dangerTime = 60f;
    [Tooltip("Pulse the label while in danger state")]
    [SerializeField] private bool pulseOnDanger = true;
    [Tooltip("How fast the pulse animates")]
    [SerializeField] private float pulseSpeed = 6f;
    [Tooltip("How strong the pulse tint is (0..1). 0.25–0.5 is subtle, 1 is heavy.")]
    [SerializeField, Range(0f, 1f)] private float pulseIntensity = 0.35f;

    private float elapsedTime;
    private bool running;
    private Color normalColor = Color.white;
    private Color dangerColor = new Color(1f, 0.25f, 0.25f);
    private bool parsedColours;

    // ===== Unity Lifecycle =====
    private void Awake()
    {
        ParseHexColours();
        if (!timeSurvivedLabel)
        {
            timeSurvivedLabel = GetComponentInChildren<TextMeshProUGUI>();
        }
        UpdateLabel(); // draw initial 00:00
    }

    private void Start()
    {
        if (autoStart) StartTimer();
    }

    private void Update()
    {
        if (!running) return;

        elapsedTime += Time.deltaTime * Mathf.Max(0f, timeScale);
        UpdateLabel();
    }

    private void OnValidate()
    {
        // Keep colours synced when edited in inspector
        ParseHexColours();
        if (Application.isPlaying == false) UpdateLabel();
    }

    // ===== Public Controls =====
    public void StartTimer() => running = true;
    public void PauseTimer() => running = false;
    public void ResumeTimer() => running = true;

    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateLabel();
    }

    public float CurrentSeconds => elapsedTime;
    public bool IsRunning => running;

    // ===== Internals =====
    private void UpdateLabel()
    {
        if (!timeSurvivedLabel) return;

        // Text
        timeSurvivedLabel.text = "Time Survived: " + FormatTime(elapsedTime, showHundredths);

        // Colour & pulse behaviour
        if (elapsedTime >= dangerTime)
        {
            if (pulseOnDanger)
            {
                // Pulse between dangerColor and a brighter variant
                var t = (Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f) * pulseIntensity;
                var bright = Color.Lerp(dangerColor, Color.white, 0.35f); // slight highlight
                timeSurvivedLabel.color = Color.Lerp(dangerColor, bright, t);
            }
            else
            {
                timeSurvivedLabel.color = dangerColor;
            }
        }
        else
        {
            timeSurvivedLabel.color = normalColor;
        }
    }

    private static string FormatTime(float seconds, bool hundredths)
    {
        seconds = Mathf.Max(0f, seconds);
        int mins = Mathf.FloorToInt(seconds / 60f);
        float secs = seconds % 60f;
        return hundredths ? $"{mins:00}:{secs:00.00}" : $"{mins:00}:{Mathf.FloorToInt(secs):00}";
    }

    private void ParseHexColours()
    {
        parsedColours = true;

        if (!TryParseHex(normalHex, out normalColor))
        {
            normalColor = Color.white;
            parsedColours = false;
        }

        if (!TryParseHex(dangerHex, out dangerColor))
        {
            dangerColor = new Color(1f, 0.25f, 0.25f);
            parsedColours = false;
        }
    }

    private static bool TryParseHex(string hex, out Color c)
    {
        if (string.IsNullOrWhiteSpace(hex)) { c = Color.white; return false; }
        // Accept with or without '#'
        if (hex[0] != '#') hex = "#" + hex.Trim();
        return ColorUtility.TryParseHtmlString(hex, out c);
    }
}
