using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    ProceduralGenerationLevel level;

    public Transform exit;
    public Transform start;
    [SerializeField] GameObject playerPrefab;

    Player player;


    private void Awake()
    {
        level = this.GetComponent<ProceduralGenerationLevel>();
        level.OnLevelBuild += ActivateEnemies;
        level.OnSetStart += SetStart;
        level.OnSetExit += SetExit;
        InstantiatePlayer();
    }

    private void OnDestroy()
    {
        level.OnLevelBuild -= ActivateEnemies;
        level.OnSetStart -= SetStart;
        level.OnSetExit -= SetExit;
        player.OnRespawn -= Respawn;
        player.OnEscaped -= DestroyLevel;
    }

    void InstantiatePlayer()
    {
        GameObject playerObject = Instantiate(playerPrefab, this.transform);
        player = playerObject.GetComponentInChildren<Player>();
        player.SetLevel(this);
        player.OnRespawn += Respawn;
        player.OnEscaped += DestroyLevel;
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
        level.SetResetParameters(resetParams);
    }
}
