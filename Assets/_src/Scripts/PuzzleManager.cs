//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class GlobalPuzzleManager : MonoBehaviour
//{
//    [Header("Puzzle Settings")]
//    public int activePuzzlesCount = 3;
//    public float puzzleCooldown = 10f;

//    [Header("Debug")]
//    [SerializeField] private List<ObjectHighlighter> allPuzzleObjects = new List<ObjectHighlighter>();
//    [SerializeField] private List<ObjectHighlighter> activePuzzles = new List<ObjectHighlighter>();
//    [SerializeField] private List<ObjectHighlighter> completedPuzzles = new List<ObjectHighlighter>();

//    void Start()
//    {
//        Debug.Log("GlobalPuzzleManager запущен");
//        FindAllPuzzleObjects();
//        ActivateRandomPuzzles();
//    }

//    void FindAllPuzzleObjects()
//    {
//        // Исправляем устаревший метод
//        ObjectHighlighter[] allHighlighters = FindObjectsByType<ObjectHighlighter>(FindObjectsSortMode.None);
//        Debug.Log($"Найдено ObjectHighlighter'ов: {allHighlighters.Length}");

//        foreach (ObjectHighlighter highlighter in allHighlighters)
//        {
//            if (HasPuzzle(highlighter))
//            {
//                allPuzzleObjects.Add(highlighter);
//                //highlighter.StopHighlight();
//                highlighter.SetPuzzleManager(this);
//                Debug.Log($"Добавлен объект с пазлом: {highlighter.gameObject.name}");
//            }
//            else
//            {
//                Debug.Log($"Пропущен объект без пазлов: {highlighter.gameObject.name}");
//            }
//        }
//    }

//    bool HasPuzzle(ObjectHighlighter highlighter)
//    {
//        bool hasPuzzle = highlighter.codePuzzle != null ||
//                        highlighter.waitPuzzle != null ||
//                        highlighter.mathPuzzle != null ||
//                        highlighter.colorSequencePuzzle != null;

//        Debug.Log($"Объект {highlighter.gameObject.name} имеет пазл: {hasPuzzle}");
//        return hasPuzzle;
//    }

//    void ActivateRandomPuzzles()
//    {
//        Debug.Log($"Активация пазлов. Доступно: {allPuzzleObjects.Count}, Запрошено: {activePuzzlesCount}");

//        // Выключаем все текущие активные пазлы
//        foreach (ObjectHighlighter active in activePuzzles)
//        {
//            //active.StopHighlight();
//        }
//        activePuzzles.Clear();

//        // Выбираем случайные объекты для активации
//        List<ObjectHighlighter> availableObjects = new List<ObjectHighlighter>(allPuzzleObjects);

//        // Убираем уже завершенные пазлы
//        foreach (ObjectHighlighter completed in completedPuzzles)
//        {
//            availableObjects.Remove(completed);
//        }

//        Debug.Log($"Доступно для активации: {availableObjects.Count}");

//        // Активируем случайные объекты
//        int countToActivate = Mathf.Min(activePuzzlesCount, availableObjects.Count);
//        for (int i = 0; i < countToActivate; i++)
//        {
//            if (availableObjects.Count == 0) break;

//            int randomIndex = Random.Range(0, availableObjects.Count);
//            ObjectHighlighter selected = availableObjects[randomIndex];

//            Debug.Log($"Активируем пазл: {selected.gameObject.name}");
//            selected.StartHighlight();
//            activePuzzles.Add(selected);
//            availableObjects.RemoveAt(randomIndex);
//        }

//        Debug.Log($"Активировано пазлов: {activePuzzles.Count}");
//    }

//    public void OnPuzzleCompleted(ObjectHighlighter completedPuzzle)
//    {
//        Debug.Log($"Пазл завершен: {completedPuzzle.gameObject.name}");

//        if (!completedPuzzles.Contains(completedPuzzle))
//        {
//            completedPuzzles.Add(completedPuzzle);
//        }

//        activePuzzles.Remove(completedPuzzle);
//        //completedPuzzle.StopHighlight();

//        StartCoroutine(ActivateNewPuzzleAfterDelay());
//    }

//    IEnumerator ActivateNewPuzzleAfterDelay()
//    {
//        yield return new WaitForSeconds(puzzleCooldown);
//        ActivateRandomPuzzles();
//    }

//    [ContextMenu("Перезапустить пазлы")]
//    public void RestartPuzzles()
//    {
//        Debug.Log("Принудительный перезапуск пазлов");
//        completedPuzzles.Clear();
//        ActivateRandomPuzzles();
//    }

//    [ContextMenu("Принудительно активировать пазлы")]
//    public void ForceActivatePuzzles()
//    {
//        ActivateRandomPuzzles();
//    }
//}