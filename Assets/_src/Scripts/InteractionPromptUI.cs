using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("Text Appearance")]
    public Color textColor = new Color(1, 0.9f, 0.2f); 
    public Color shadowColor = new Color(0, 0, 0, 0.7f);
    public int fontSize = 30;
    public float characterSize = 0.08f;

    [Header("Effects")]
    public bool enablePulse = true;
    public float pulseSpeed = 2f;
    public float pulseIntensity = 0.1f;
    public bool enableFloat = true;
    public float floatDistance = 0.2f;

    [Header("Shadow")]
    public bool enableShadow = true;
    public Vector2 shadowOffset = new Vector2(0.05f, -0.05f);

    private TextMesh mainText;
    private TextMesh shadowText;
    private Vector3 basePosition;
    private GameObject textGroup;

    public void Setup(string promptText)
    {
        basePosition = Vector3.zero;

        textGroup = new GameObject("TextGroup");
        textGroup.transform.SetParent(transform);
        textGroup.transform.localPosition = basePosition;


        if (enableShadow)
        {
            CreateShadowText(promptText);
        }

        CreateMainText(promptText);

        Hide();
    }

    void CreateShadowText(string text)
    {
        GameObject shadowObj = new GameObject("ShadowText");
        shadowObj.transform.SetParent(textGroup.transform);
        shadowObj.transform.localPosition = new Vector3(shadowOffset.x, shadowOffset.y, 0.01f);

        shadowText = shadowObj.AddComponent<TextMesh>();
        shadowText.text = text;
        shadowText.color = shadowColor;
        shadowText.fontSize = fontSize;
        shadowText.characterSize = characterSize;
        shadowText.anchor = TextAnchor.MiddleCenter;
        shadowText.alignment = TextAlignment.Center;
        shadowText.fontStyle = FontStyle.Bold;

        shadowObj.GetComponent<MeshRenderer>().sortingOrder = 1000;
    }

    void CreateMainText(string text)
    {
        GameObject mainTextObj = new GameObject("MainText");
        mainTextObj.transform.SetParent(textGroup.transform);
        mainTextObj.transform.localPosition = Vector3.zero;

        mainText = mainTextObj.AddComponent<TextMesh>();
        mainText.text = text;
        mainText.color = textColor;
        mainText.fontSize = fontSize;
        mainText.characterSize = characterSize;
        mainText.anchor = TextAnchor.MiddleCenter;
        mainText.alignment = TextAlignment.Center;
        mainText.fontStyle = FontStyle.Bold;

        mainTextObj.GetComponent<MeshRenderer>().sortingOrder = 1001;
    }

    void Update()
    {
        if (textGroup == null) return;


        if (enableFloat)
        {
            float floatY = Mathf.Sin(Time.time * pulseSpeed * 0.5f) * floatDistance;
            textGroup.transform.localPosition = basePosition + new Vector3(0, floatY, 0);
        }

        if (enablePulse)
        {
            float pulseScale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            textGroup.transform.localScale = Vector3.one * pulseScale;
        }
    }

    public void Show()
    {
        if (textGroup != null)
        {
            textGroup.SetActive(true);
        }
    }

    public void Hide()
    {
        if (textGroup != null)
        {
            textGroup.SetActive(false);
        }
    }

    public void UpdateText(string newText)
    {
        if (mainText != null)
        {
            mainText.text = newText;
        }
        if (shadowText != null)
        {
            shadowText.text = newText;
        }
    }
}