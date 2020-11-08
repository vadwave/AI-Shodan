using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerManager : MonoBehaviour
{
    [SerializeField] Text timeCounter;
    [SerializeField] Text timePreviousCounter;
    [SerializeField] Text timeBestCounter;

    float starttime;
    float maxMinutes = 999f;
    TimeEntity bestTime = new TimeEntity { min = 10, sec = 10, fraction = 10 };
    TimeEntity time = new TimeEntity { min = 11, sec = 10, fraction = 10 };

    public event System.Action OnEnded;

    // Update is called once per frame
    void Update()
    {
        UpdateTime();
    }

    public void SetMax(float maxMinutes)
    {
        this.maxMinutes = maxMinutes;
    }

    public void ResetTimer()
    {
        BestTimeUpdate();
        if (timeCounter && timePreviousCounter) timePreviousCounter.text = timeCounter.text;
        starttime = Time.time;
    }
    void BestTimeUpdate()
    {
        if (time.min == bestTime.min)
        {
            if (time.sec == bestTime.sec)
            {
                if (time.fraction <= bestTime.fraction)
                    bestTime = time;
            }
            else if (time.sec < bestTime.sec)
                bestTime = time;

        }
        else if (time.min < bestTime.min)
            bestTime = time;

        if(bestTime.min == 0f && bestTime.sec <= 1.6f)
        {
            bestTime.min = 10;
        }

        if (timeBestCounter) timeBestCounter.text = String.Format("{0:00}:{1:00}:{2:00}", bestTime.min, bestTime.sec, bestTime.fraction);

    }
    void UpdateTime()
    {
        float timecount = Time.time - starttime;
        float min = Mathf.Floor((timecount / 60f));
        float sec = (timecount % 60f);
        float fraction = ((timecount * 10) % 10);
        time = new TimeEntity { min = min, sec = sec, fraction = fraction };

        if(timeCounter) timeCounter.text = String.Format("{0:00}:{1:00}:{2:00}", time.min, time.sec, time.fraction);

        if (time.min >= maxMinutes)
        {
            OnEnded?.Invoke();
        }
    }

    [Serializable]
    public class TimeEntity
    {
        public float min;
        public float sec;
        public float fraction;
    }
}
