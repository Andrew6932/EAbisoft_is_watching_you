using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TimeBar : MonoBehaviour
{
    [SerializeField]
    private Image ProgressImage;
    [SerializeField]
    private float DefaultSpeed = -1f;
    [SerializeField]
    private UnityEvent<float> OnProgress;
    [SerializeField]
    private bool isEmpty = false;

    private Coroutine AnimationCoroutine;

    private void Start(){
        if(ProgressImage.type != Image.Type.Filled){
            Debug.LogError($"{name}'s progressImage is not of type Filled");
            this.enabled = false;
        }
    }

    public bool getIsEmpty()
    {
        return isEmpty;
    }

    public void setIsEmpty(bool input)
    {
        isEmpty = input;
    }

    public void SetProgress(float progress){
        SetProgress(progress, DefaultSpeed);
    }

    public void SetProgress(float progress, float speed){
        if(progress < 0 || progress > 1){
            Debug.LogWarning($"Invalid progress passed, value out of Bounds! (must range from 0 to 1)");
            progress = Mathf.Clamp01(progress);
        }
        if(progress != ProgressImage.fillAmount){
            if(AnimationCoroutine!=null){
                StopCoroutine(AnimationCoroutine);
            }
            AnimationCoroutine = StartCoroutine(AnimateProgress(progress, speed));
        }
    }
    public float getProgress()
    {
        if(ProgressImage.fillAmount < 0)
        {
            return 0f;
        }else if(ProgressImage.fillAmount > 1)
        {
            return 1f;
        }
        return ProgressImage.fillAmount;
    }

    private IEnumerator AnimateProgress(float progress, float speed){
        float time = 0;
        float initalProgress = ProgressImage.fillAmount;

        if(Mathf.Approximately(progress, 0f))
        {
            isEmpty = true;
        }
        else
        {
            isEmpty = false;
        }
        
        while(time < 1){
            ProgressImage.fillAmount = Mathf.Lerp(initalProgress, progress, time);
            time += Time.deltaTime * speed;

            OnProgress?.Invoke(ProgressImage.fillAmount);
            yield return null;
        }

        ProgressImage.fillAmount = progress;
        OnProgress?.Invoke(progress);
        if(Mathf.Approximately(progress, 0f)){
            isEmpty = true;
        }
    }

    public void setInstantProgressToOne()
    {
        if(AnimationCoroutine != null)
        {
            StopCoroutine(AnimationCoroutine);
            AnimationCoroutine = null;
        }

        ProgressImage.fillAmount = 1;
        isEmpty = false;
        OnProgress?.Invoke(1);
    }
}
