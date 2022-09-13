using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// class TrackCheckpoint
// {
//     public TrackCheckpoint(TrackCheckpointLine trackCheckpointLine)
//     {

//     }
// }

public class RaceLapSector
{
    public readonly int index;
    public readonly float startTime;
    public readonly float prevSectorsSummedTime;
    private float endTime = 0;

    public RaceLapSector(int index, float startTime, float prevSectorsSummedTime)
    {
        this.index = index;
        this.startTime = startTime;
        this.prevSectorsSummedTime = prevSectorsSummedTime;
    }

    public void Finish(float endTime)
    {
        this.endTime = endTime;
    }

    public bool IsFinished()
    {
        return this.endTime > 0;
    }

    public float GetSectorTime()
    {
        if (endTime == 0) return 0;

        return endTime - startTime;
    }

    public float GetSectorSummedTime()
    {
        if (endTime == 0) return 0;

        return prevSectorsSummedTime + GetSectorTime();
    }
}

public class RaceLap
{
    public readonly Race race;
    public readonly int index;
    public readonly float startTime;
    private float endTime = 0;
    private List<RaceLapSector> sectors = new List<RaceLapSector>(); public IReadOnlyList<RaceLapSector> Sectors => sectors;

    public RaceLap(Race race, int index, float startTime)
    {
        this.race = race;
        this.index = index;
        this.startTime = startTime;

        sectors.Add(new RaceLapSector(0, startTime, 0));

        race.checkpointHit += OnCheckpointHit;
    }

    public void Finish(float endTime)
    {
        this.endTime = endTime;

        race.checkpointHit -= OnCheckpointHit;

        sectors[sectors.Count - 1].Finish(endTime);
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

    void OnCheckpointHit(int cpIndex)
    {
        var timeNow = Time.time;

        var prevSector = sectors[sectors.Count - 1];
        prevSector.Finish(timeNow);
        sectors.Add(new RaceLapSector(cpIndex, timeNow, prevSector.GetSectorSummedTime()));
    }
}

public class Race
{
    private Track track; public Track Track => track;

    private float startTime;
    private List<RaceLap> laps = new List<RaceLap>(); public IReadOnlyList<RaceLap> Laps => laps;

    private RaceLap currentLap; public RaceLap CurrentLap => currentLap;
    private RaceLap recordLap; public RaceLap RecordLap => recordLap;
    private TrackCheckpointLine lastCheckpoint;

    public event System.Action<RaceLap, RaceLap> lapStarted;
    public event System.Action<RaceLap> lapFinished;
    public event System.Action<int> checkpointHit;
    // @todo currently needed for UI as checkpointHit is used in RaceLap and there's a race conditions issue - RaceLap should be changed by Race directly
    public event System.Action<int> lapSectorStarted;

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
        var newLap = new RaceLap(this, laps.Count, timeNow);
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

            var cpIndex = track.Checkpoints.IndexOf(trackCheckpointLine);
            checkpointHit?.Invoke(cpIndex);
            lapSectorStarted?.Invoke(cpIndex);

            if (track.IsStartFinishLine(trackCheckpointLine))
            {
                StartNewLap();
            }
        }
    }

    float GetSectorSplitTime(RaceLap lap, int sectorIndex)
    {
        if (recordLap == null) return 0;

        var lapSectorSummedTime = lap.Sectors[sectorIndex].GetSectorSummedTime();
        var recordLapSectorSummedTime = recordLap.Sectors[sectorIndex].GetSectorSummedTime();

        // Debug.Log($"{lapSectorSummedTime} | {recordLapSectorSummedTime}");

        return lapSectorSummedTime - recordLapSectorSummedTime;
    }

    public float GetSplitTime()
    {
        var cpIndex = track.Checkpoints.IndexOf(lastCheckpoint);
        if (cpIndex == 0)
        {
            return GetSectorSplitTime(Laps[Laps.Count - 1], track.Checkpoints.Count - 1);
        }
        else
        {
            return GetSectorSplitTime(currentLap, cpIndex - 1);
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