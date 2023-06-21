using System;
using UnityEngine;

public class CountdownTimer
{
    private float currentTime; // Tiempo actual del temporizador
    private float alarmTime; // Tiempo de alarma
    private bool started; // Indica si el temporizador ha comenzado
    public EventHandler Alarm; // Evento que se dispara cuando se alcanza el tiempo de alarma
    public EventHandler<Match> UpdateUITimeEvent; // Evento para actualizar el tiempo en la interfaz de usuario

    Round round; // Referencia a la instancia de la clase Round

    public float CurrentTime { get { return currentTime; } set { currentTime = value; } }

    public CountdownTimer(float seconds, Round round)
    {
        this.alarmTime = seconds;
        this.currentTime = seconds;
        this.round = round;

        GameObject.FindGameObjectWithTag("Clock").GetComponent<Clock>().ClockTick += UpdateTimer;
        // Busca un objeto con la etiqueta "Clock" en la escena y se suscribe al evento ClockTick del componente Clock para actualizar el temporizador
    }

    public void StartTimer()
    {
        started = true; // Marca el temporizador como iniciado
    }

    public void ResetTimer()
    {
        started = false; // Marca el temporizador como no iniciado
        currentTime = alarmTime; // Restablece el tiempo actual al tiempo de alarma original
    }

    public void UpdateTimer(object sender, float deltaTime)
    {
        if (!started) return; // Si el temporizador no ha comenzado, no realiza ninguna acción

        currentTime -= deltaTime; // Actualiza el tiempo actual restando el tiempo transcurrido

        UpdateUITimeEvent?.Invoke(this, round.Match); // Invoca el evento UpdateUITimeEvent para notificar a otros componentes sobre el cambio de tiempo en la interfaz de usuario
        CheckAlarm(); // Verifica si se ha alcanzado la alarma
    }

    private void CheckAlarm()
    {
        if (currentTime < 0)
        {
            started = false; // Detiene el temporizador
            Alarm?.Invoke(this, null); // Dispara el evento Alarm para indicar que se ha alcanzado el tiempo de alarma
        }
    }
}
