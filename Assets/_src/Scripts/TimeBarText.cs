using TMPro;
using UnityEngine;

public class TimeBarText : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI text;

    public void setText(float progress)
    {
        text.SetText($"{(progress * 100).ToString("N2")}%");
    }
}