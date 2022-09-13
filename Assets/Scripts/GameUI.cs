using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// class Clock
// {
//     private float speedMeter_updateInterval = .1f;
//     private float speedMeter_lastUpdateTime = -Mathf.Infinity;
// }

public class GameUI : MonoBehaviour
{
    [SerializeField] private TMP_Text speedMeter;
    [SerializeField] private TMP_Text cpCounter;
    [SerializeField] private TMP_Text lapCounter;
    [SerializeField] private TMP_Text lastLapTime;
    [SerializeField] private TMP_Text bestLapTime;
    [SerializeField] private TMP_Text splitTime;
    [SerializeField] private TMP_Text currentLapTime;

    private IShipSpeed ship;
    private Race race;

    private float speedMeter_updateInterval = .1f;
    private float speedMeter_lastUpdateTime = -Mathf.Infinity;
    private float currentLapTime_updateInterval = .1f;
    private float currentLapTime_lastUpdateTime = -Mathf.Infinity;

    public void Setup(
        IShipSpeed ship,
        Race race
    )
    {
        this.ship = ship;
        this.race = race;

        race.lapSectorStarted += OnRaceLapSectorStarted;
        race.lapStarted += OnRaceLapStarted;
        race.lapFinished += OnRaceLapFinished;

        SetSpeedMeter(0);

        SetCPCounter(0);
        SetLapCounter(0);
        SetLastLapTime(-1);
        SetBestLapTime(-1);

        SetSplitTime();
        SetCurrentLapTime(-1);
    }

    public void OnUpdate()
    {
        if (race.Laps.Count == 0) return;

        if (Time.time - speedMeter_lastUpdateTime >= speedMeter_updateInterval)
        {
            SetSpeedMeter(ship.Speed);

            speedMeter_lastUpdateTime = Time.time;
        }

        if (Time.time - currentLapTime_lastUpdateTime >= currentLapTime_updateInterval)
        {
            SetCurrentLapTime(Time.time - race.CurrentLap.startTime);

            currentLapTime_lastUpdateTime = Time.time;
        }
    }

    string FormatTime(float time, bool secondsOnly = false, int prec = 2)
    {
        var s = Mathf.FloorToInt(time);
        var fr = (int)((time - s) * Mathf.Pow(10, prec));

        var frStr = fr.ToString();
        var frStrDiff = prec - frStr.Length;
        if (frStrDiff > 0) frStr = $"{new string('0', frStrDiff)}{frStrDiff}";

        if (secondsOnly)
        {
            return $"{s}.{frStr}";
        }
        else
        {
            var m = Mathf.FloorToInt(time / 60);

            var sStr = s.ToString();
            if (sStr.Length < 2) sStr = "0" + sStr;

            return $"{m}:{sStr}.{frStr}";
        }
    }

    void OnRaceLapSectorStarted(int cpIndex)
    {
        SetCPCounter(cpIndex + 1);
        SetSplitTime();
    }

    void OnRaceLapStarted(RaceLap newLap, RaceLap prevLap)
    {
        SetLapCounter(newLap.index + 1);
        SetCurrentLapTime(0);
    }

    void OnRaceLapFinished(RaceLap lap)
    {
        SetLastLapTime(lap.GetLapTime());

        if (race.RecordLap == lap)
        {
            SetBestLapTime(lap.GetLapTime());
        }
    }

    void SetSpeedMeter(float speed)
    {
        speedMeter.text = $"{Mathf.Round(Ship.MpsToKmph(speed))} km/h";
    }

    void SetCPCounter(int cpIndex)
    {
        cpCounter.text = $"CP {cpIndex}/{race.Track.Checkpoints.Count}";
    }

    void SetLapCounter(int lapIndex)
    {
        lapCounter.text = $"Lap {lapIndex}";
    }

    void SetLastLapTime(float time)
    {
        // if (time == -1)
        // {
        //     lastLapTime.gameObject.SetActive(false);
        // }
        // else
        // {
        //     if (lastLapTime.gameObject.activeSelf == false) lastLapTime.gameObject.SetActive(true);
        // }
        lastLapTime.text = $"Last {FormatTime(time)}";
    }

    void SetBestLapTime(float time)
    {
        bestLapTime.text = $"Best {FormatTime(time)}";
    }

    void SetSplitTime()
    {
        if (race.RecordLap == null) return;

        var time = race.GetSplitTime();

        splitTime.text = $"{(time > 0 ? "+" : "")}{FormatTime(time, true)}";

        var col = time > 0 ? Color.red : Color.green;
        col.a = .5f;
        splitTime.color = col;
    }

    void SetCurrentLapTime(float time)
    {
        currentLapTime.text = FormatTime(time, false, 1);
    }
}
