using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button btnNewGame;
    [SerializeField] private Button btnContinue;
    [SerializeField] private Button btnSettings;
    [SerializeField] private Button btnExit;

    [Header("Settings UI")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Button btnCloseSettings;

    [Header("Config")]
    [SerializeField] private string gameSceneName = "Game"; // set to your play scene

    private void Awake()
    {
        // Button bindings
        btnNewGame.onClick.AddListener(OnNewGame);
        btnContinue.onClick.AddListener(OnContinue);
        btnSettings.onClick.AddListener(OpenSettings);
        btnExit.onClick.AddListener(ExitGame);

        if (btnCloseSettings != null)
            btnCloseSettings.onClick.AddListener(CloseSettings);

        // Settings defaults
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MASTER_VOL", 1f);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
            bool isFull = PlayerPrefs.GetInt("FULLSCREEN", Screen.fullScreen ? 1 : 0) == 1;
            fullscreenToggle.isOn = isFull;
            Screen.fullScreen = isFull;
        }

        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Continue availability
        RefreshContinueState();

        // Make the first button selected for controller users
        EventSystem.current?.SetSelectedGameObject(btnNewGame.gameObject);
    }

    private void RefreshContinueState()
    {
        bool hasSave = SaveSystem.HasSave();
        btnContinue.interactable = hasSave;
    }

    private async void OnNewGame()
    {
        // Create a fresh save stub and load scene
        SaveSystem.CreateNewSave();
        await LoadGameSceneAsync();
    }

    private async void OnContinue()
    {
        if (!SaveSystem.HasSave())
            return;

        SaveSystem.LoadLatest();
        await LoadGameSceneAsync();
    }

    private void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            // Try to select first control inside settings for controller UX
            if (fullscreenToggle != null)
                EventSystem.current?.SetSelectedGameObject(fullscreenToggle.gameObject);
        }
    }

    private void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            EventSystem.current?.SetSelectedGameObject(btnSettings.gameObject);
        }
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Settings handlers
    private void SetMasterVolume(float v)
    {
        AudioListener.volume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat("MASTER_VOL", AudioListener.volume);
        PlayerPrefs.Save();
    }

    private void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("FULLSCREEN", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Async scene load with a tiny fade hook (optional)
    private System.Threading.Tasks.Task LoadGameSceneAsync()
    {
        // If you want a fade, trigger an Animator here before loading.
        var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
        var op = SceneManager.LoadSceneAsync(gameSceneName, LoadSceneMode.Single);
        op.completed += _ => tcs.SetResult(true);
        return tcs.Task;
    }
}
