using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameCompletionBar : MonoBehaviour
{
    [SerializeField]
    private Image progressImage;
    [SerializeField]
    private float defaultSpeed = 1f;
    [SerializeField]
    private UnityEvent<float> onProgress;
    [SerializeField]
    private UnityEvent onCompleted;

    private Coroutine animationCoroutine;

    private void Start(){
        if(progressImage.type != Image.Type.Filled){
            Debug.LogError($"{name}'s progressImage is not of type Filled");
            this.enabled = false;
        }
    }

    public void setProgress(float progress){
        setProgress(progress, defaultSpeed);
    }

    public void setProgress(float progress, float speed){
        if(progress < 0 || progress > 1){
            Debug.LogWarning($"Invalid progress passed, value out of Bounds! (must range from 0 to 1)");
            progress = Mathf.Clamp01(progress);
        }
        if(progress != progressImage.fillAmount){
            if(animationCoroutine!=null){
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(AnimateProgress(progress, speed));
        }
    }

    private IEnumerator AnimateProgress(float progress, float speed){
        float time = 0;
        float initalProgress = progressImage.fillAmount;

        while(time < 1){
            progressImage.fillAmount = Mathf.Lerp(initalProgress, progress, time);
            time += Time.deltaTime * speed;

            onProgress?.Invoke(progressImage.fillAmount);
            yield return null;
        }

        progressImage.fillAmount = progress;
        onProgress?.Invoke(progress);
        onCompleted?.Invoke();
    }
}
