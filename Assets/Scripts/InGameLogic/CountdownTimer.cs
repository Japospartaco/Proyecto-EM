using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountdownTimer
{
    private float timer;
    private float alarmTime;
    private bool started;
    public EventHandler Alarm;

    public float Timer
    {
        get { return timer; }
    }

    public CountdownTimer(float seconds)
    {
        this.alarmTime = seconds;
        this.timer = seconds;
        GameObject.FindGameObjectWithTag("Clock").GetComponent<Clock>().ClockTick += UpdateTimer;
    }

    public void StartTimer()
    {
        started = true;
    }

    public void ResetTimer()
    {
        started = false;
        timer = alarmTime;
    }

    public void UpdateTimer(object sender, float deltaTime)
    {
        if (!started) return;

        timer -= deltaTime;
        CheckAlarm();
    }

    private void CheckAlarm()
    {
        if (timer < 0)
        {
            started = false;
            Alarm?.Invoke(this, null);
        }
    }
}