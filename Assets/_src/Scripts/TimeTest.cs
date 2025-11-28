using System.Collections;
using UnityEngine;

public class TimeTest : MonoBehaviour{
    public TimeBar timebar;

    private void Start(){
        StartCoroutine(TestRoutine());
    }

    private IEnumerator TestRoutine()
    {
        timebar.SetProgress(0f);
        yield return new WaitForSeconds(5f);

        timebar.SetProgress(0.5f, 0.5f);
        yield return new WaitForSeconds(5f);

        timebar.SetProgress(0f, 1f);

    }
}