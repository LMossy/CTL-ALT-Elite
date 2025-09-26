using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject settingsPanel;  // assign SettingsPanel
    public Slider volumeSlider;       // assign VolumeSlider

    const string VolumeKey = "masterVolume";

    void Start()
    {
        if (settingsPanel) settingsPanel.SetActive(false);

        float v = PlayerPrefs.GetFloat(VolumeKey, 0.8f);
        if (volumeSlider)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = v;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        ApplyVolume(v);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Main");  // or index 1 if you prefer
    }

    public void OpenSettings(bool open)
    {
        if (settingsPanel) settingsPanel.SetActive(open);
    }

    public void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    public void OnVolumeChanged(float v)
    {
        ApplyVolume(v);
        PlayerPrefs.SetFloat(VolumeKey, v);
    }

    void ApplyVolume(float v)
    {
        AudioListener.volume = Mathf.Clamp01(v);
    }
}
