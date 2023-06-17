using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Clock : NetworkBehaviour
{
    public EventHandler<float> ClockTick;

    // Update is called once per frame
    void Update()
    {
        if (!IsServer) return;
        //EVENTO PARA ACTUALIZAR RELOJES SUSCRITOS
        ClockTick?.Invoke(this, Time.deltaTime);
    }
}//
