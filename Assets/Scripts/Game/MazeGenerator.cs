using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    #region Variables:
    // ------------------------------------------------------
    // User defined variables - set in editor:
    // ------------------------------------------------------

    [Header("Element Walls")]
    [SerializeField] GameObject Wall;
    [SerializeField] GameObject Passage;
    [SerializeField] GameObject Door;

    [SerializeField] float tileSize = 3.84f;


    [Header("Maze generation values:")]
    [Tooltip("How many cells tall is the maze. MUST be an even number. " +
        "If number is odd, it will be reduced by 1.\n\n" +
        "Minimum value of 4.")]
    public int mazeRows;
    [Tooltip("How many cells wide is the maze. Must be an even number. " +
        "If number is odd, it will be reduced by 1.\n\n" +
        "Minimum value of 4.")]
    public int mazeColumns;

    [Header("Maze object variables:")]
    [Tooltip("Cell prefab object.")]
    [SerializeField]
    private GameObject cellPrefab;

    [Tooltip("If you want to disable the main sprite so the cell has no background, set to TRUE. This will create a maze with only walls.")]
    public bool disableCellSprite;

    private int centreSize = 2;

    private Dictionary<Vector2, TileEntity> allCells = new Dictionary<Vector2, TileEntity>();
    private List<TileEntity> unvisited = new List<TileEntity>();
    private List<TileEntity> stack = new List<TileEntity>();

    private TileEntity[] centreCells = new TileEntity[4];

    private TileEntity currentCell;
    private TileEntity checkCell;

    private Vector2[] neighbourPositions = new Vector2[] { new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, -1) };
    private float cellSize;

    private GameObject mazeParent;
    #endregion

    private void Start()
    {
        GenerateMaze(mazeRows, mazeColumns);
    }

    private void GenerateMaze(int rows, int columns)
    {
        if (mazeParent != null) DeleteMaze();

        mazeRows = rows;
        mazeColumns = columns;
        CreateLayout();
    }

    // Creates the grid of cells.
    public void CreateLayout()
    {
        InitValues();

        // Set starting point, set spawn point to start.
        Vector2 startPos = new Vector2(-(cellSize * (mazeColumns / 2)) + (cellSize / 2), -(cellSize * (mazeRows / 2)) + (cellSize / 2));
        Vector2 spawnPos = startPos;

        for (int x = 1; x <= mazeColumns; x++)
        {
            for (int y = 1; y <= mazeRows; y++)
            {
                GenerateCell(spawnPos, new Vector2(x, y));

                // Increase spawnPos y.
                spawnPos.y += cellSize;
            }

            // Reset spawnPos y and increase spawnPos x.
            spawnPos.y = startPos.y;
            spawnPos.x += cellSize;
        }

        CreateCentre();
        RunAlgorithm();
        MakeExit();
    }

    // This is where the fun stuff happens.
    public void RunAlgorithm()
    {
        // Get start cell, make it visited (i.e. remove from unvisited list).
        unvisited.Remove(currentCell);

        // While we have unvisited cells.
        while (unvisited.Count > 0)
        {
            List<TileEntity> unvisitedNeighbours = GetUnvisitedNeighbours(currentCell);
            if (unvisitedNeighbours.Count > 0)
            {
                // Get a random unvisited neighbour.
                checkCell = unvisitedNeighbours[Random.Range(0, unvisitedNeighbours.Count)];
                // Add current cell to stack.
                stack.Add(currentCell);
                // Compare and remove walls.
                CompareWalls(currentCell, checkCell);
                // Make currentCell the neighbour cell.
                currentCell = checkCell;
                // Mark new current cell as visited.
                unvisited.Remove(currentCell);
            }
            else if (stack.Count > 0)
            {
                // Make current cell the most recently added Cell from the stack.
                currentCell = stack[stack.Count - 1];
                // Remove it from stack.
                stack.Remove(currentCell);
            }
        }
    }

    public void MakeExit()
    {
        // Create and populate list of all possible edge cells.
        List<TileEntity> edgeCells = new List<TileEntity>();

        foreach (KeyValuePair<Vector2, TileEntity> cell in allCells)
        {
            if (cell.Key.x == 0 || cell.Key.x == mazeColumns || cell.Key.y == 0 || cell.Key.y == mazeRows)
            {
                edgeCells.Add(cell.Value);
            }
        }

        // Get edge cell randomly from list.
        TileEntity newCell = edgeCells[Random.Range(0, edgeCells.Count)];

        // Remove appropriate wall for chosen edge cell.
        if (newCell.Position.x == 0) RemoveWall(newCell, 1);
        else if (newCell.Position.x == mazeColumns) RemoveWall(newCell, 2);
        else if (newCell.Position.y == mazeRows) RemoveWall(newCell, 3);
        else RemoveWall(newCell, 4);

        Debug.Log("Maze generation finished.");
    }

    public List<TileEntity> GetUnvisitedNeighbours(TileEntity curCell)
    {
        // Create a list to return.
        List<TileEntity> neighbours = new List<TileEntity>();
        // Create a Cell object.
        TileEntity nCell = curCell;
        // Store current cell grid pos.
        Vector2 cPos = curCell.Position;

        foreach (Vector2 p in neighbourPositions)
        {
            // Find position of neighbour on grid, relative to current.
            Vector2 nPos = cPos + p;
            // If cell exists.
            if (allCells.ContainsKey(nPos)) nCell = allCells[nPos];
            // If cell is unvisited.
            if (unvisited.Contains(nCell)) neighbours.Add(nCell);
        }

        return neighbours;
    }

    // Compare neighbour with current and remove appropriate walls.
    public void CompareWalls(TileEntity cCell, TileEntity nCell)
    {
        // If neighbour is left of current.
        if (nCell.Position.x < cCell.Position.x)
        {
            RemoveWall(nCell, 2);
            RemoveWall(cCell, 1);
        }
        // Else if neighbour is right of current.
        else if (nCell.Position.x > cCell.Position.x)
        {
            RemoveWall(nCell, 1);
            RemoveWall(cCell, 2);
        }
        // Else if neighbour is above current.
        else if (nCell.Position.y > cCell.Position.y)
        {
            RemoveWall(nCell, 4);
            RemoveWall(cCell, 3);
        }
        // Else if neighbour is below current.
        else if (nCell.Position.y < cCell.Position.y)
        {
            RemoveWall(nCell, 3);
            RemoveWall(cCell, 4);
        }
    }

    // Function disables wall of your choosing, pass it the script attached to the desired cell
    // and an 'ID', where the ID = the wall. 1 = left, 2 = right, 3 = up, 4 = down.
    public void RemoveWall(TileEntity cScript, int wallID, bool isRandom = true)
    {
        if (wallID == 1)
        {
            cScript.ChangeWall(Direction.Left, SelectPassage(isRandom),TypeWall.Null);
        }
        else if (wallID == 2)
        {
            cScript.ChangeWall(Direction.Right, SelectPassage(isRandom), TypeWall.Null);
        }
        else if (wallID == 3)
        {
            cScript.ChangeWall(Direction.Up, SelectPassage(isRandom), TypeWall.Null);
        }
        else if (wallID == 4) 
        {
            cScript.ChangeWall(Direction.Down, SelectPassage(isRandom), TypeWall.Null); 
        }
    }
    public GameObject SelectPassage(bool isRandom)
    {
        if (!isRandom) return null;
        int random = Random.Range(0, 2);
        switch (random)
        {
            case 0: return null;
            case 1: return Door;
            case 2: return Passage;
            default: return null;
        }

    }

    public void CreateCentre()
    {
        // Get the 4 centre cells using the rows and columns variables.
        // Remove the required walls for each.
        centreCells[0] = allCells[new Vector2((mazeColumns / 2), (mazeRows / 2) + 1)];
        RemoveWall(centreCells[0], 4, false);
        RemoveWall(centreCells[0], 2, false);
        centreCells[1] = allCells[new Vector2((mazeColumns / 2) + 1, (mazeRows / 2) + 1)];
        RemoveWall(centreCells[1], 4, false);
        RemoveWall(centreCells[1], 1, false);
        centreCells[2] = allCells[new Vector2((mazeColumns / 2), (mazeRows / 2))];
        RemoveWall(centreCells[2], 3, false);
        RemoveWall(centreCells[2], 2, false);
        centreCells[3] = allCells[new Vector2((mazeColumns / 2) + 1, (mazeRows / 2))];
        RemoveWall(centreCells[3], 3, false);
        RemoveWall(centreCells[3], 1, false);

        // Create a List of ints, using this, select one at random and remove it.
        // We then use the remaining 3 ints to remove 3 of the centre cells from the 'unvisited' list.
        // This ensures that one of the centre cells will connect to the maze but the other three won't.
        // This way, the centre room will only have 1 entry / exit point.
        List<int> rndList = new List<int> { 0, 1, 2, 3 };
        int startCell = rndList[Random.Range(0, rndList.Count)];
        rndList.Remove(startCell);
        currentCell = centreCells[startCell];
        foreach (int c in rndList)
        {
            unvisited.Remove(centreCells[c]);
        }
    }

    public void GenerateCell(Vector2 pos, Vector2 keyPos)
    {
        GameObject newGameObj= Instantiate(cellPrefab, pos, cellPrefab.transform.rotation);
        TileEntity newCell = newGameObj.GetComponent<TileEntity>();
        newCell.CreateTile(Wall, Wall, Wall, Wall);
        newCell.Position = new Vector2Int((int)keyPos.x, (int)keyPos.y);
        if (mazeParent != null) newCell.transform.parent = mazeParent.transform;
        newCell.name = "Cell - X:" + keyPos.x + " Y:" + keyPos.y;
        if (disableCellSprite) newCell.GetComponent<SpriteRenderer>().enabled = false;
        allCells[keyPos] = newCell;
        unvisited.Add(newCell);
    }

    public void DeleteMaze()
    {
        if (mazeParent != null) Destroy(mazeParent);
    }

    public void InitValues()
    {
        if (IsOdd(mazeRows)) mazeRows--;
        if (IsOdd(mazeColumns)) mazeColumns--;

        if (mazeRows <= 3) mazeRows = 4;
        if (mazeColumns <= 3) mazeColumns = 4;

        cellSize = cellPrefab.transform.localScale.x;
        cellSize = tileSize;
        mazeParent = new GameObject();
        mazeParent.transform.position = Vector2.zero;
        mazeParent.name = "Maze";
    }

    public bool IsOdd(int value)
    {
        return value % 2 != 0;
    }
}
