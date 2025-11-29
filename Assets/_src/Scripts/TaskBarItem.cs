using UnityEngine;
using TMPro;

public class TaskBarItem : MonoBehaviour
{
    public TextMeshProUGUI label;
    public string GetLabel() => label.text;
}