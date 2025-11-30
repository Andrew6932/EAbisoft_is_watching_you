using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AutoSoundSettings : MonoBehaviour
{
    [Header("Settings")]
    public float defaultMusicVolume = 0.5f;
    public float defaultSFXVolume = 0.7f;
    public string panelTitle = "Sound Settings";

    private AudioManager audioManager;
    private GameObject settingsPanel;
    private Canvas canvas;

    void Start()
    {
        audioManager = AudioManager.Instance;

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(ShowSettingsPanel);
        }
    }

    public void ShowSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            return;
        }

        CreateSettingsPanel();
    }

    private void CreateSettingsPanel()
    {
        CreateCanvas();

        settingsPanel = CreatePanel("SoundSettingsPanel");
        settingsPanel.transform.SetParent(canvas.transform, false);

        Image panelImage = settingsPanel.GetComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        VerticalLayoutGroup panelLayout = settingsPanel.AddComponent<VerticalLayoutGroup>();
        panelLayout.padding = new RectOffset(20, 20, 20, 20);
        panelLayout.spacing = 15f;
        panelLayout.childAlignment = TextAnchor.UpperCenter;
        panelLayout.childControlWidth = true;
        panelLayout.childControlHeight = false;
        panelLayout.childForceExpandWidth = true;
        panelLayout.childForceExpandHeight = false;

        ContentSizeFitter panelFitter = settingsPanel.AddComponent<ContentSizeFitter>();
        panelFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;


        CreateTitle("Sound Settings");


        CreateVolumeControl("Music Volume", "MusicVolume", defaultMusicVolume, OnMusicSliderChanged);


        CreateVolumeControl("SFX Volume", "SFXVolume", defaultSFXVolume, OnSFXSliderChanged);


        CreateControlButtons();


        RectTransform panelRect = settingsPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
    }

    private void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("SoundSettingsCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay; 
        canvas.sortingOrder = 1000; 


        canvasObj.AddComponent<GraphicRaycaster>();


        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    private GameObject CreatePanel(string name)
    {
        GameObject panel = new GameObject(name);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 300);

        Image image = panel.AddComponent<Image>();
        return panel;
    }

    private void CreateTitle(string titleText)
    {
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(settingsPanel.transform, false);

        TextMeshProUGUI text = titleObj.AddComponent<TextMeshProUGUI>();
        text.text = titleText;
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        RectTransform rect = titleObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 40);


        LayoutElement layout = titleObj.AddComponent<LayoutElement>();
        layout.preferredHeight = 40;
    }

    private void CreateVolumeControl(string label, string name, float defaultValue, UnityEngine.Events.UnityAction<float> onValueChanged)
    {

        GameObject controlContainer = new GameObject(name + "Container");
        controlContainer.transform.SetParent(settingsPanel.transform, false);


        HorizontalLayoutGroup layout = controlContainer.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 5, 5);
        layout.spacing = 15f;
        layout.childAlignment = TextAnchor.MiddleLeft;

        LayoutElement containerLayout = controlContainer.AddComponent<LayoutElement>();
        containerLayout.preferredHeight = 40;


        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(controlContainer.transform, false);

        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 18;
        labelText.color = Color.white;
        labelText.alignment = TextAlignmentOptions.Left;

        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(120, 30);

        LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 120;


        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(controlContainer.transform, false);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = defaultValue;
        slider.onValueChanged.AddListener(onValueChanged);

        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(150, 20);

        LayoutElement sliderLayout = sliderObj.AddComponent<LayoutElement>();
        sliderLayout.preferredWidth = 150;


        GameObject background = new GameObject("Background");
        background.transform.SetParent(sliderObj.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.75f, 0.75f, 0.75f, 0.5f);

        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillRect = fillArea.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0.25f);
        fillRect.anchorMax = new Vector2(1, 0.75f);
        fillRect.sizeDelta = new Vector2(-20, 0);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.6f, 1f, 1f);

        RectTransform fillImageRect = fill.GetComponent<RectTransform>();
        fillImageRect.anchorMin = Vector2.zero;
        fillImageRect.anchorMax = Vector2.one;
        fillImageRect.sizeDelta = Vector2.zero;

        slider.fillRect = fillImageRect;


        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleRect = handleArea.AddComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.one;
        handleRect.sizeDelta = new Vector2(-20, 0);


        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;

        RectTransform handleImageRect = handle.GetComponent<RectTransform>();
        handleImageRect.sizeDelta = new Vector2(20, 20);

        slider.targetGraphic = handleImage;
        slider.handleRect = handleImageRect;


        GameObject valueObj = new GameObject("ValueText");
        valueObj.transform.SetParent(controlContainer.transform, false);

        TextMeshProUGUI valueText = valueObj.AddComponent<TextMeshProUGUI>();
        valueText.text = $"{defaultValue * 100:0}%";
        valueText.fontSize = 16;
        valueText.color = Color.white;
        valueText.alignment = TextAlignmentOptions.Right;

        RectTransform valueRect = valueObj.GetComponent<RectTransform>();
        valueRect.sizeDelta = new Vector2(50, 30);

        LayoutElement valueLayout = valueObj.AddComponent<LayoutElement>();
        valueLayout.preferredWidth = 50;

        slider.onValueChanged.AddListener((value) => {
            valueText.text = $"{value * 100:0}%";
        });
    }

    private void CreateControlButtons()
    {

        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(settingsPanel.transform, false);


        HorizontalLayoutGroup layout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.spacing = 20f;
        layout.childAlignment = TextAnchor.MiddleCenter;

        LayoutElement containerLayout = buttonContainer.AddComponent<LayoutElement>();
        containerLayout.preferredHeight = 50;

        CreateButton("CloseButton", "Close", buttonContainer.transform, HideSettingsPanel);
    }

    private void CreateButton(string name, string buttonText, Transform parent, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);


        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(action);


        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        button.colors = colors;


        RectTransform rect = buttonObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(80, 40);

        LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
        layout.preferredWidth = 80;
        layout.preferredHeight = 40;


        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = buttonText;
        text.fontSize = 16;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
    }

    private void OnMusicSliderChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetMusicVolume(value);
        }
    }

    private void OnSFXSliderChanged(float value)
    {
        if (audioManager != null)
        {
            audioManager.SetSFXVolume(value);
        }
    }

    private void ApplySettings()
    {

        Debug.Log("Sound settings applied");
    }

    private void ResetSettings()
    {

        Slider[] sliders = settingsPanel.GetComponentsInChildren<Slider>();
        foreach (Slider slider in sliders)
        {
            if (slider.name.Contains("Music"))
            {
                slider.value = defaultMusicVolume;
            }
            else if (slider.name.Contains("SFX"))
            {
                slider.value = defaultSFXVolume;
            }
        }
    }

    public void HideSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private float GetSFXVolume()
    {

        if (audioManager != null && audioManager.sfxSource != null)
        {
            return audioManager.sfxSource.volume;
        }
        return defaultSFXVolume;
    }
}