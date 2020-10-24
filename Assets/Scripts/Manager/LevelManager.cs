using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    ProceduralGenerationLevel level;
    private void Awake()
    {
        level = this.GetComponent<ProceduralGenerationLevel>();
        level.OnLevelBuild += EnableEnemies;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void OnDestroy()
    {
        level.OnLevelBuild -= EnableEnemies;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void EnableEnemies()
    {
       SecurityCamera[] cameras = level.GetComponentsInChildren<SecurityCamera>();
        foreach(SecurityCamera camera in cameras)
        {
            camera.ActivateAI(true);
        }
    }
}
