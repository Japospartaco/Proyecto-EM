using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CountdownTimer
{
    private float timer;
    private float alarmTime;
    private bool started;
    public EventHandler Alarm;
    public EventHandler<Match> UpdateUITimeEvent;

    Round round;

    public float Timer
    {
        get { return timer; }
        set { timer = value; }
    }

    public Round Round
	{
        get { return round; }
	}

    public CountdownTimer(float seconds, Round round)
    {
        this.alarmTime = seconds;
        this.timer = seconds;
        this.round = round;

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

        UpdateUITimeEvent?.Invoke(this, round.Match);
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