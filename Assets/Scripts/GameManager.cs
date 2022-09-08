using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameplayManagerPrefabs
{
    GameObject ProjectilePrefab { get; }
}

public class GameManager : MonoBehaviour, IGameplayManagerPrefabs
{
    private PlayerController playerController;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject projectilePrefab; public GameObject ProjectilePrefab => projectilePrefab;

    [SerializeField] private Ship ship;

    private Race race;

    void Awake() {
        var cameraFollow = GetComponent<CameraFollow>();
        cameraFollow.Setup(mainCamera, ship.transform);

        playerController = GetComponent<PlayerController>();
        playerController.Setup(ship);

        ship.Setup(this);
    }

    void Start()
    {
        var track = FindObjectOfType<Track>();
        track.Setup();

        this.race = new Race(track: track);

        race.checkpointHit += OnRaceCheckpointHit;
        race.lapStarted += OnRaceLapStarted;
        race.lapFinished += OnRaceLapFinished;
    }

    void OnRaceCheckpointHit(int cpIndex)
    {
        Debug.Log($"checkpoint #{cpIndex + 1} hit");
    }

    void OnRaceLapStarted(RaceLap newLap, RaceLap prevLap)
    {
        Debug.Log($"lap #{newLap.index + 1} started");
    }

    void OnRaceLapFinished(RaceLap lap)
    {
        var lapRecordStr = race.RecordLap == lap ? " (new lap record)" : "";
        var lapTimeStr = lap.GetLapTime().ToString("F2");

        Debug.Log($"lap #{lap.index + 1} finished ({lapTimeStr}s){lapRecordStr}");
    }
}
