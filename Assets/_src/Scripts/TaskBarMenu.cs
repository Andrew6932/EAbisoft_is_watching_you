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

    

    public void AddNewTaskBar(string input)
    {
        GameObject newBar = Instantiate(taskBarPrefab,container);
        TaskBarItem item = newBar.GetComponent<TaskBarItem>();

        item.label.text = input;
        items.Add(item);
    }

    public void RemoveTaskBar(string input)
    {
        for(int i = 0; i < items.Count; i++)
        {
            if(items[i].GetLabel() == input)
            {
                Destroy(items[i].gameObject);
                items.RemoveAt(i);
                return;
            }
        }
    }

    
}