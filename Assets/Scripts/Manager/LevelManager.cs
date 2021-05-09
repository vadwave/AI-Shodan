using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    ProceduralGenerationLevel level;
    TimerManager timer;

    public Transform exit;
    public Transform start;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] float maxMinutes;
    [SerializeField] Shodan shodan;
    Player player;


    private void Awake()
    {
        level = this.GetComponent<ProceduralGenerationLevel>();
        level.OnLevelBuild += ActivateEnemies;
        level.OnSetStart += SetStart;
        level.OnSetExit += SetExit;
        InstantiatePlayer();
        timer = this.GetComponent<TimerManager>();
        if (timer)
        {
            timer.SetMax(maxMinutes);
            timer.OnEnded += NonEscaped;
            player.OnEndedRespawn += ResetTimer;
        }


        if (shodan)
        {
            SecurityCamera.OnAddedTimeVisible += AddedTimeVisible;
            SecurityCamera.OnFindedTarget += shodan.FindTarget;
            SecurityCamera.OnLostedTarget += shodan.LostTarget;
            player.OnRespawn += shodan.StartEscaping;
            player.OnEscaped += shodan.Result;
            player.OnAddedScore += shodan.CollectInfo;
        }

    }

    private void OnDestroy()
    {
        level.OnLevelBuild -= ActivateEnemies;
        level.OnSetStart -= SetStart;
        level.OnSetExit -= SetExit;
        if(timer)
        timer.OnEnded -= NonEscaped;
        player.OnAddedScore -= UpdateScore;
        player.OnRespawn -= Respawn;
        player.OnEscaped -= DestroyLevel;
        player.OnEndedRespawn -= ResetTimer;

        SecurityCamera.OnAddedTimeVisible -= AddedTimeVisible;
    }

    void InstantiatePlayer()
    {
        GameObject playerObject = playerPrefab;// Instantiate(playerPrefab, this.transform);
        player = playerObject.GetComponentInChildren<Player>();
        player.SetLevel(this);
        player.OnAddedScore += UpdateScore;
        player.OnRespawn += Respawn;
        player.OnEscaped += DestroyLevel;

    }

    private void UpdateScore(float scoreValues)
    {
        if (timer) timer.Score = scoreValues;
    }

    void ActivateEnemies()
    {
       SecurityCamera[] cameras = level.GetComponentsInChildren<SecurityCamera>();
        foreach(SecurityCamera camera in cameras)
        {
            camera.ActivateAI(true);
        }
    }
    void DeactivateEnemies()
    {
        SecurityCamera[] cameras = level.GetComponentsInChildren<SecurityCamera>();
        foreach (SecurityCamera camera in cameras)
        {
            camera.ActivateAI(false);
        }
    }

    void NonEscaped()
    {
        if(player)
        player.ExitLevel(false);
    }
    void ResetTimer()
    {
        timer.ResetTimer();
    }

    void SetExit(Transform exit)
    {
        this.exit = exit;
    }
    void SetStart(Transform start)
    {
        this.start = start;
    }
    void Respawn(Transform positionBody, Transform rotationBody)
    {
        level.Initialize();
        Transform parent = this.start.parent.parent;

        float angle = 90f + parent.eulerAngles.z;
        Vector3 pos = -Vector3.up;
        switch (angle)
        {
            case 180f: pos = -Vector3.up; break;
            case 0f: pos = Vector3.up; break;
            case -90f: pos = -Vector3.right; break;
            case 90f: pos = Vector3.right; break;
        }
     
        rotationBody.rotation = Quaternion.Euler(0f, 0f, angle);
        positionBody.position = this.start.position + pos;
    }
    void DestroyLevel()
    {
        DeactivateEnemies();
        level.DestroyMaze();
    }

    public void SetParameters(EnvironmentParameters resetParams)
    {
        if(resetParams!=null && level)
        level.SetResetParameters(resetParams);
    }
    void AddedTimeVisible(float values)
    {
        timer.TimeVisible += values;
    }
}
