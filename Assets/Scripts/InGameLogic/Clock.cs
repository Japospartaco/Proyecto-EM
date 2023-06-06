using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{
    public EventHandler<float> ClockTick;

    // Update is called once per frame
    void Update()
    {
        ClockTick?.Invoke(this, Time.deltaTime);
    }
}
