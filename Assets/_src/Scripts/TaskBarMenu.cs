using UnityEngine;
using System;
using TMPro;
using Unity.VisualScripting;
using System.Collections.Generic;

public class TaskBarMenu : MonoBehaviour
{
    public Transform container;
    public GameObject taskBarPrefab;

    private List<TaskBarItem> items = new();
    public List<TaskBarItem> GetItems()
    {
        return items;
    }
    public void AddNewTaskBar(string input)
    {
        GameObject newBar = Instantiate(taskBarPrefab, container);
        TaskBarItem item = newBar.GetComponent<TaskBarItem>();

        item.label.text = input;

        // Если это задача менеджера, включаем красное мигание и обратный отсчет
        if (input == "Consult with Manager")
        {
            item.EnableRedBlinking();
            item.StartCountdown(10f); // 10 секунд обратного отсчета
        }

        items.Add(item);
    }

    public void RemoveTaskBar(string input)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].GetLabel() == input)
            {
                Destroy(items[i].gameObject);
                items.RemoveAt(i);
                return;
            }
        }
    }

    // Метод для принудительного удаления по компоненту
    public void RemoveTaskBar(TaskBarItem item)
    {
        if (items.Contains(item))
        {
            Destroy(item.gameObject);
            items.Remove(item);
        }
    }
}