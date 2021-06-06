using System;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProceduralGenerationLevel : MonoBehaviour
{
    enum TypeLevel
    {
        Speed,
        Defence,
        Filled
    }

    [SerializeField] Vector2Int sizeMaze;
    [SerializeField] int seed;
    [SerializeField] bool isRandom = false;
    [SerializeField] TypeLevel typeLevel;
    [Range(1, 9)] [SerializeField] int difficultyLevel = 1;

    [Header("Prefabs")]
    [SerializeField] TileEntity prefab;
    [SerializeField] float tileSize = 3.84f;

    [Header("Element Walls")]
    [SerializeField] GameObject Wall;
    [Header("Element Enters")]
    [SerializeField] SpawnPrefab Passage;
    [SerializeField] SpawnPrefab Door;
    [Header("Element Enemies")]
    [SerializeField] int countEnemies = 5;
    [SerializeField] SpawnPrefab Camera;
    [SerializeField] SpawnPrefab Guard;
    [Header("Element Collectibles")]
    [SerializeField] int countCollectables = 5;
    [SerializeField] SpawnPrefab InfoFile;
    [Header("Element Logics")]
    [SerializeField] GameObject ExitDoor;
    [SerializeField] GameObject StartDoor;
    [SerializeField] SpawnPortal Portal;
    [SerializeField] SpawnLockedDoor LockedDoor;


    private GameObject mazeParent;
    private TileEntity[,] tiles;
    private List<TileEntity> unvisited;
    private TileEntity current;

    private bool isReset = false;

    public GameObject Level => mazeParent;

    public event System.Action OnLevelBuild;
    public event Action<Transform> OnSetExit;
    public event Action<Transform> OnSetStart;

    public string CurrentTypeLevel => typeLevel.ToString();
    public string CurrentDiffLevel => difficultyLevel.ToString();
    void SetParameters()
    {
        int diff = 1 - (1 / difficultyLevel);
        Passage.Chance = diff;
        Camera.Chance = diff;
        InfoFile.Chance = diff;

        if(typeLevel == TypeLevel.Defence)
        {
            countEnemies = 5;//5-9
            countCollectables = 3;//3-5

            Passage.Chance = 0;
        }
        else if (typeLevel == TypeLevel.Speed)
        {
            countEnemies = 2;//1-3
            countCollectables = 0;//0-3

            Passage.Chance = 0;
            Camera.Chance = 0;
            InfoFile.Chance = 0;
        }
        else if (typeLevel == TypeLevel.Filled)
        {
            countEnemies = 3;//3-5
            countCollectables = 7;//5-7
        }
    }
    private void GenerateEnemies()
    {
        for (int i = 0; i < countEnemies; i++)
        {
            int x = Random.Range(0, sizeMaze.x - 1);
            int y = Random.Range(0, sizeMaze.y - 1);

            if (tiles[x, y].Type == TypeTile.Null)
            {
                GameObject enemy = Camera.Prefab;
                tiles[x, y].SpawnOnWall(enemy);
                tiles[x, y].gameObject.name += " Enemy";
                tiles[x, y].Type = TypeTile.Enemy;
            }
            else i--;
        }
    }

    private void GenerateCollectables()
    {
        for (int i = 0; i < countCollectables; i++)
        {
            int x = Random.Range(0, sizeMaze.x - 1);
            int y = Random.Range(0, sizeMaze.y - 1);

            if (tiles[x, y].Type == TypeTile.Null || tiles[x, y].Type == TypeTile.Enemy)
            {
                GameObject collect = InfoFile.Prefab;
                tiles[x, y].SpawnOnFloor(collect);
                tiles[x, y].gameObject.name += " Collectable";
                tiles[x, y].Type = TypeTile.Collectable;
            }
            else i--;
        }
    }

    void GenerateRewardCollectables(Vector2Int sizeMaze, TileEntity[,] tiles, TypeTile setType, GameObject rewardCollectableObject, int countCollectables)
    {
        if (sizeMaze == null || tiles == null)
        {
            Debug.LogError("Отсутствует игровое поле для установки игровых обьектов для награждения!");
        }
        else if(rewardCollectableObject == null)
        {
            Debug.LogError("Требуемый шаблон игрового обьекта для награждения отсутствует!");
        }
        else if ((sizeMaze.x * sizeMaze.y) < countCollectables || countCollectables <= 0)
        {
            Debug.LogError("Количество награждаемых игровых объектов должен быть в пределах от 1 до площади игрового пространства!");
        }
        else
        {
            for (int i = 0; i < countCollectables; i++)
            {
                int x = Random.Range(0, sizeMaze.x - 1);
                int y = Random.Range(0, sizeMaze.y - 1);

                if (tiles[x, y].Type == TypeTile.Null)
                {
                    GameObject collect = rewardCollectableObject;
                    tiles[x, y].SpawnOnFloor(collect);
                    tiles[x, y].gameObject.name += (" " + setType.ToString());
                    tiles[x, y].Type = setType;
                }
                else i--;
            }
            Debug.Log("Установка награждаемых игровых объектов выполнена!");
        }

    }

    internal void DestroyMaze()
    {
        if (isRandom) { Destroy(mazeParent); isReset = false; }
        else ResetTiles();
    }
    void ResetTiles()
    {
        isReset = true;
        for (int x = 0; x < sizeMaze.x; x++)
        {
            for (int y = 0; y < sizeMaze.y; y++)
            {
                tiles[x, y].EnableTile();
                switch (tiles[x, y].Type)
                {
                    case TypeTile.Collectable:
                        {
                            tiles[x, y].Type = TypeTile.Null;
                            tiles[x, y].ClearObjects();
                            tiles[x, y].gameObject.name.Replace(" Enemy", "");
                            tiles[x, y].gameObject.name.Replace(" Collectable", "");
                        }
                        break;
                    case TypeTile.Key:
                        {

                        }
                        break;
                    case TypeTile.Enemy:
                        {
                            tiles[x, y].Type = TypeTile.Null;
                            tiles[x, y].ClearObjects();
                            tiles[x, y].gameObject.name.Replace(" Enemy","");
                            tiles[x, y].gameObject.name.Replace(" Collectable", "");
                        }
                        break;
                }
            }
        }
    }


    void InitializeParent()
    {
        if(isRandom)
        seed = Random.Range(100000, 999999);
        Random.InitState(seed);
        mazeParent = new GameObject();
        mazeParent.transform.parent = this.transform;
        mazeParent.name = "Level";
        tiles = new TileEntity[sizeMaze.x, sizeMaze.y];
        unvisited = new List<TileEntity>();
    }

    public void Initialize()
    {
        SetParameters();
        if (!isReset)
        {
            InitializeParent();
            CreateLayout();
        }
        GenerateCollectables();
        GenerateEnemies();
        OnLevelBuild.Invoke();
    }

    public void SetResetParameters(EnvironmentParameters resetParams)
    {
        if(resetParams!= null)
        {
            countEnemies = (int)resetParams.GetWithDefault("enemies", countEnemies);
            countCollectables = (int)resetParams.GetWithDefault("collectables", countCollectables);

            float random = (isRandom) ? 1f : 0f;
            random = resetParams.GetWithDefault("randomLevel", random);
            isRandom = (random == 0) ? false : true;

            sizeMaze.x = (int)resetParams.GetWithDefault("sizeLevel_X", sizeMaze.x);
            sizeMaze.y = (int)resetParams.GetWithDefault("sizeLevel_Y", sizeMaze.y);

            Passage.Chance = resetParams.GetWithDefault("chancePassage", Passage.Chance);
            Door.Chance = resetParams.GetWithDefault("chanceDoor", Door.Chance);
        }
    }

    public void CreateLayout()
    {
        float startPosX = -(tileSize * (sizeMaze.x * 0.5f)) + (tileSize * 0.5f);
        float startPosY = -(tileSize * (sizeMaze.y * 0.5f)) + (tileSize * 0.5f);
        Vector2 startPos = new Vector2(startPosX, startPosY);
        Vector2 spawnPos = startPos;

        for (int x = 0; x < sizeMaze.x; x++)
        {
            for (int y = 0; y < sizeMaze.y; y++)
            {
                GenerateTile(spawnPos, new Vector2Int(x, y));
                spawnPos.y += tileSize;
            }
            spawnPos.y = startPos.y;
            spawnPos.x += tileSize;
        }

        CreateCenter();
        RunAlgorithm();
        MakeExit();
    }

    private void MakeExit()
    {
        Vector2Int[] tilesRandom = new Vector2Int[4];

        tilesRandom[0] = new Vector2Int(0, Random.Range(0, sizeMaze.y - 1));
        tilesRandom[1] = new Vector2Int(sizeMaze.x - 1, Random.Range(0, sizeMaze.y - 1));
        tilesRandom[2] = new Vector2Int(Random.Range(0, sizeMaze.x - 1), 0);
        tilesRandom[3] = new Vector2Int(Random.Range(0, sizeMaze.x - 1), sizeMaze.y - 1);

        Vector2Int random = tilesRandom[Random.Range(0, 3)];

        TileEntity newCell = tiles[random.x, random.y];
        newCell.name += " Exit";
        newCell.Type = TypeTile.Exit;

        if (newCell.Position.x == 0) 
        {
            CreateExit(newCell, Direction.Left);
        }
        else if (newCell.Position.x == sizeMaze.x - 1) 
        {
            CreateExit(newCell, Direction.Right);
        }
        else if (newCell.Position.y == sizeMaze.y - 1) 
        {
            CreateExit(newCell, Direction.Up);
        }
        else 
        {
            CreateExit(newCell, Direction.Down);
        }

    }

    public void GenerateTile(Vector2 spawnPos, Vector2Int keyPos)
    {
       
        Vector2 posInMaze = new Vector2(transform.position.x, transform.position.y) + spawnPos;

        TileEntity newCell = Instantiate(prefab, posInMaze, Quaternion.identity, mazeParent.transform);
        newCell.Position = keyPos; 


        ChangeWall(newCell, GetWalls());
        newCell.name = "Tile - X:" + (keyPos.x) + " Y:" + (keyPos.y);

        tiles[keyPos.x, keyPos.y] = newCell;
        unvisited.Add(newCell);
    }
    public void CreateCenter()
    {
        TileEntity[] tilesCenter = new TileEntity[4];

        int centerX = (int)(sizeMaze.x * 0.5f) - 1;
        int centerY = (int)(sizeMaze.y * 0.5f) - 1;
        Vector2Int sizeCenter = new Vector2Int(centerX, centerY);

        //
        tilesCenter[0] = tiles[centerX, centerY + 1];
        RemoveWall(tilesCenter[0], Direction.Right);
        RemoveWall(tilesCenter[0], Direction.Down);
        //
        tilesCenter[1] = tiles[centerX + 1, centerY + 1];
        RemoveWall(tilesCenter[1], Direction.Left);
        RemoveWall(tilesCenter[1], Direction.Down);
        //
        tilesCenter[2] = tiles[centerX , centerY];
        RemoveWall(tilesCenter[2], Direction.Right);
        RemoveWall(tilesCenter[2], Direction.Up);
        //
        tilesCenter[3] = tiles[centerX + 1, centerY];
        RemoveWall(tilesCenter[3], Direction.Left);
        RemoveWall(tilesCenter[3], Direction.Up);

        foreach (TileEntity tile in tilesCenter)
            tile.Type = TypeTile.Start;

        int randomTile = 0; // Random.Range(0, tilesCenter.Length - 1);
        Direction randomDir = Direction.Up;
        CreateStart(tilesCenter[randomTile], randomDir);

        List<int> rndList = new List<int> { 0, 1, 2, 3 };
        int startCell = rndList[Random.Range(0, rndList.Count)];
        rndList.Remove(startCell);
        current = tilesCenter[startCell];
        foreach (int c in rndList)
        {
            unvisited.Remove(tilesCenter[c]);
        }
    }

    TileWalls GetWalls()
    {
        TileWalls walls = new TileWalls();
        walls.Left = TypeWall.Wall;
        walls.Right = TypeWall.Wall;
        walls.Down = TypeWall.Wall;
        walls.Up = TypeWall.Wall;

        return walls;
    }


    public void RunAlgorithm()
    {
        List<TileEntity> stack = new List<TileEntity>();
        TileEntity checkTile;
        unvisited.Remove(current);
        while (unvisited.Count > 0)
        {
            List<TileEntity> unvisitedNeighbours = GetUnvisitedNeighbours(current);
            if (unvisitedNeighbours.Count > 0)
            {
                checkTile = unvisitedNeighbours[Random.Range(0, unvisitedNeighbours.Count)];
                stack.Add(current);
                CompareWalls(current, checkTile);
                current = checkTile;
                //Debug.Log("sas " + current.name);
                unvisited.Remove(current);
            }
            else if (stack.Count > 0)
            {
                current = stack[stack.Count - 1];
                stack.Remove(current);
            }
        }
    }

    private void CompareWalls(TileEntity current, TileEntity neighbour)
    {
        if (neighbour.Position.x < current.Position.x)
        {
            RemoveWall(neighbour, Direction.Right);
            RemoveWall(current, Direction.Left);
        }
        else if (neighbour.Position.x > current.Position.x)
        {
            RemoveWall(neighbour, Direction.Left);
            RemoveWall(current, Direction.Right);
        }
        else if (neighbour.Position.y > current.Position.y)
        {
            RemoveWall(neighbour, Direction.Down);
            RemoveWall(current, Direction.Up);
        }
        else if (neighbour.Position.y < current.Position.y)
        {
            RemoveWall(neighbour, Direction.Up);
            RemoveWall(current, Direction.Down);
        }
    }
    public void RemoveWall(TileEntity tile, Direction dir)
    {
        EntityWall wall = SelectEnter();
        GameObject wallObj = (wall.Point == null) ? null: wall.Point.gameObject;
        switch (dir)
        {
            case Direction.Left: tile.ChangeWall(Direction.Left, wallObj, wall.Type); break;
            case Direction.Right: tile.ChangeWall(Direction.Right, wallObj, wall.Type); break;
            case Direction.Up: tile.ChangeWall(Direction.Up, wallObj, wall.Type); break;
            case Direction.Down: tile.ChangeWall(Direction.Down, wallObj, wall.Type); break;
        }
    }
    public void CreateExit(TileEntity tile, Direction dir)
    {
        switch (dir)
        {
            case Direction.Left: tile.ChangeWall(Direction.Left, ExitDoor, TypeWall.Exit); break;
            case Direction.Right: tile.ChangeWall(Direction.Right, ExitDoor, TypeWall.Exit); break;
            case Direction.Up: tile.ChangeWall(Direction.Up, ExitDoor, TypeWall.Exit); break;
            case Direction.Down: tile.ChangeWall(Direction.Down, ExitDoor, TypeWall.Exit); break;
        }
        Transform exit = GameMath.FindComponentInChildWithTag<Transform>(tile.gameObject, "Finish");
        OnSetExit?.Invoke(exit);
    }
    public void CreateStart(TileEntity tile, Direction dir)
    {
        switch (dir)
        {
            case Direction.Left: tile.ChangeWall(Direction.Left, StartDoor, TypeWall.Start); break;
            case Direction.Right: tile.ChangeWall(Direction.Right, StartDoor, TypeWall.Start); break;
            case Direction.Up: tile.ChangeWall(Direction.Up, StartDoor, TypeWall.Start); break;
            case Direction.Down: tile.ChangeWall(Direction.Down, StartDoor, TypeWall.Start); break;
        }
        Transform start = GameMath.FindComponentInChildWithTag<Transform>(tile.gameObject, "Respawn");
        OnSetStart?.Invoke(start);
    }
    public EntityWall SelectEnter()
    {
        SpawnPrefab[] list = GetSortPrefabs(new SpawnPrefab[] { Door, Passage });
        float random = Random.value;
        GameObject prefabWall = null;
        foreach (SpawnPrefab prefab in list)
        {
            if (random <= prefab.Chance)
            {
                prefabWall = prefab.Prefab; break;
            }
                
        }
        EntityWall entityWall;
        entityWall.Point = (prefabWall == null) ? null : prefabWall.transform;
        entityWall.Type = (prefabWall == null) ? TypeWall.Null : TypeWall.Passage;
        return entityWall;
    }
    public GameObject SelectEnemy()
    {
        SpawnPrefab[] list = GetSortPrefabs(new SpawnPrefab[]{ Camera, Guard });
        float random = Random.value;
        foreach (SpawnPrefab prefab in list)
        {
            if (random <= prefab.Chance)
                return prefab.Prefab;
        }
        return null;
    }


    SpawnPrefab[] GetSortPrefabs(SpawnPrefab[] list)
    {
        SpawnPrefab Enter; 
        Enter.Chance = 1f; 
        Enter.Prefab = null;

        Array.Resize(ref list, list.Length + 1);
        list[list.Length - 1] = Enter;
        Array.Sort(list, (x, y) => x.Chance.CompareTo(y.Chance));
        return list;
    }


    private List<TileEntity> GetUnvisitedNeighbours(TileEntity current)
    {
        Vector2Int[] neighbourPositions = new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
        List<TileEntity> neighbours = new List<TileEntity>();
        TileEntity curTile = current;
        Vector2Int curPos = current.Position;

        foreach (Vector2Int pos in neighbourPositions)
        {
            Vector2Int neighbourPos = curPos + pos;
            if (ConstainsPosition(neighbourPos)) 
            { 
                curTile = tiles[neighbourPos.x, neighbourPos.y]; 
            }
            if (unvisited.Contains(curTile)) 
            { 
                neighbours.Add(curTile); 
            }
        }
        return neighbours;
    }
    bool ConstainsPosition(Vector2Int neighbourPos)
    {
        bool isMoreNull = (neighbourPos.x >= 0 && neighbourPos.y >= 0);
        bool isLimitMaze = (sizeMaze.x > neighbourPos.x && sizeMaze.y > neighbourPos.y);

        return isLimitMaze && isMoreNull;
    }

    public void ChangeWall(TileEntity tileEntity, TileWalls walls)
    {
        tileEntity.CreateTile(GetWall(walls.Left), GetWall(walls.Right), GetWall(walls.Up), GetWall(walls.Down));
    }

    GameObject GetWall(TypeWall type)
    {
        switch (type)
        {
            case TypeWall.Null: return null;
            case TypeWall.Passage: return Passage.Prefab;
            case TypeWall.Door: return Door.Prefab;
            case TypeWall.Wall: return Wall;
            default: return null;
        }
    }
}

