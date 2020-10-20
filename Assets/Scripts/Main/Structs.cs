using System;
using UnityEngine;

public class Structs
{

}

public struct TileWalls
{
    public TypeWall Left;
    public TypeWall Right;
    public TypeWall Up;
    public TypeWall Down;
}
[Serializable]
public struct SpawnPrefab
{
                    [SerializeField] public GameObject Prefab;
    [Range(0f, 1f)] [SerializeField] public float Chance;
}
[Serializable]
public struct SpawnPortal
{
    [SerializeField] public GameObject EnterPrefab;
    [SerializeField] public GameObject ExitPrefab;
    [Range(0f, 1f)] [SerializeField] public float Chance;
}

[Serializable]
public struct SpawnLockedDoor
{
    [SerializeField] public GameObject DoorPrefab;
    [SerializeField] public GameObject KeyPrefab;
    [Range(0f, 1f)] [SerializeField] public float Chance;
}


[Serializable]
public struct EntityWall
{
    public Transform Point;
    public TypeWall Type;
}
[Serializable]
public struct WallsPoint
{
    public EntityWall Left;
    public EntityWall Right;
    public EntityWall Up;
    public EntityWall Down;

    internal void SetType(TypeWall typeWall)
    {
        Left.Type = typeWall;
        Right.Type = typeWall;
        Up.Type = typeWall;
        Down.Type = typeWall;
    }
}
[Serializable]
public struct PillarsPoint
{
    public Transform LeftUp;
    public Transform LeftDown;
    public Transform RightUp;
    public Transform RightDown;
}