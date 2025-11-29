using TMPro;
using UnityEngine;

public class TextUpdater : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI text;

    public void setText(float progress)
    {
        text.SetText($"{(progress * 100).ToString("N2")}%");
    }
}