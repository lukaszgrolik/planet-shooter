using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// class TrackCheckpoint
// {
//     public TrackCheckpoint(TrackCheckpointLine trackCheckpointLine)
//     {

//     }
// }

class RaceLap
{
    public readonly int index;
    public readonly float startTime;
    private float endTime = 0;

    public RaceLap(int index, float startTime)
    {
        this.index = index;
        this.startTime = startTime;
    }

    public void Finish(float endTime)
    {
        this.endTime = endTime;
    }

    public bool IsFinished()
    {
        return this.endTime > 0;
    }

    public float GetLapTime()
    {
        if (endTime == 0) return 0;

        return endTime - startTime;
    }
}

class Race
{
    private Track track; public Track Track => track;

    private float startTime;
    private List<RaceLap> laps = new List<RaceLap>();

    private RaceLap currentLap;
    private RaceLap recordLap; public RaceLap RecordLap => recordLap;
    private TrackCheckpointLine lastCheckpoint;

    public event System.Action<RaceLap, RaceLap> lapStarted;
    public event System.Action<RaceLap> lapFinished;
    public event System.Action<int> checkpointHit;

    public Race(Track track)
    {
        this.track = track;

        track.checkpointHit += OnCheckpointHit;
    }

    void StartNewLap()
    {
        var timeNow = Time.time;

        if (currentLap != null)
        {
            currentLap.Finish(timeNow);

            if (IsNewRecordLap(currentLap))
            {
                recordLap = currentLap;
            }
        }

        var prevLap = currentLap;
        var newLap = new RaceLap(laps.Count, timeNow);
        currentLap = newLap;

        laps.Add(newLap);

        if (prevLap != null) lapFinished?.Invoke(prevLap);
        lapStarted?.Invoke(newLap, prevLap);
    }

    bool IsNewRecordLap(RaceLap lap)
    {
        if (lap.IsFinished() == false) return false;
        if (recordLap == null) return true;

        return lap.GetLapTime() < recordLap.GetLapTime();
    }

    void OnCheckpointHit(TrackCheckpointLine trackCheckpointLine)
    {
        if (lastCheckpoint == null || track.IsNextCheckpoint(lastCheckpoint, trackCheckpointLine))
        {
            lastCheckpoint = trackCheckpointLine;

            checkpointHit?.Invoke(track.Checkpoints.IndexOf(trackCheckpointLine));

            if (track.IsStartFinishLine(trackCheckpointLine))
            {
                StartNewLap();
            }
        }
    }
}

public class Track : MonoBehaviour
{
    private List<TrackCheckpointLine> checkpoints = new List<TrackCheckpointLine>(); public List<TrackCheckpointLine> Checkpoints => checkpoints;

    public event System.Action<TrackCheckpointLine> checkpointHit;

    public void Setup()
    {
        var trackCheckpointLines = gameObject.GetComponentsInChildren<TrackCheckpointLine>();
        // Debug.Log(trackCheckpointLines.Length);
        if (trackCheckpointLines.Length == 0) throw new System.Exception("track has no checkpoints");

        foreach (var line in trackCheckpointLines)
        {
            // var cp = new TrackCheckpoint(
            //     trackCheckpointLine: line
            // );
            checkpoints.Add(line);

            line.collided += OnCollided;
        }
    }

    void OnCollided(TrackCheckpointLine trackCheckpointLine)
    {
        checkpointHit?.Invoke(trackCheckpointLine);
    }

    public bool IsNextCheckpoint(TrackCheckpointLine lastCp, TrackCheckpointLine newCp)
    {
        var lastCpIndex = checkpoints.IndexOf(lastCp);
        var newCpIndex = checkpoints.IndexOf(newCp);

        return newCpIndex == lastCpIndex + 1 || (newCpIndex == 0 && lastCpIndex == checkpoints.Count - 1);
    }

    public bool IsStartFinishLine(TrackCheckpointLine newCp)
    {
        return checkpoints.IndexOf(newCp) == 0;
    }
}