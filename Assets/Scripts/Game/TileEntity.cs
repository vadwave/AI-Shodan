using UnityEngine;

public class TileEntity : MonoBehaviour
{
    [SerializeField] WallsPoint walls;
    [SerializeField] PillarsPoint pillars;
    [SerializeField] Transform floor;
    [SerializeField] Transform ParentOjects;

    [HideInInspector] public Vector2Int Position;
    [HideInInspector] public TypeTile Type;
    public void CreateTile(GameObject left, GameObject right, GameObject up, GameObject down)
    {
        ClearWalls();
        ClearPillars();
        if (left) Instantiate(left, this.walls.Left.Point);
        if (right) Instantiate(right, this.walls.Right.Point);
        if (up) Instantiate(up, this.walls.Up.Point);
        if (down) Instantiate(down, this.walls.Down.Point);
        this.walls.SetType(TypeWall.Wall);
    }


    public void ChangeWall(Direction direction, GameObject wall, TypeWall type)
    {
        switch (direction)
        {
            case Direction.Left:
                {
                    ClearChildrens(this.walls.Left.Point);
                    if (wall) Instantiate(wall, this.walls.Left.Point);
                    this.walls.Left.Type = type;
                }
                break;
            case Direction.Right:
                {
                    ClearChildrens(this.walls.Right.Point);
                    if (wall) Instantiate(wall, this.walls.Right.Point);
                    this.walls.Right.Type = type;
                }
                break;
            case Direction.Up:
                {
                    ClearChildrens(this.walls.Up.Point);
                    if (wall) Instantiate(wall, this.walls.Up.Point);
                    this.walls.Up.Type = type;
                }
                break;
            case Direction.Down:
                {
                    ClearChildrens(this.walls.Down.Point);
                    if (wall) Instantiate(wall, this.walls.Down.Point);
                    this.walls.Down.Type = type;
                }
                break;
        }
        ShowPillars(direction);
    }

    public void ShowPillars(Direction direction)
    {
        switch (direction)
        {
            case Direction.Left:
                {
                    if (walls.Down.Type == TypeWall.Null) pillars.LeftDown.gameObject.SetActive(false);
                    if (walls.Up.Type == TypeWall.Null) pillars.LeftUp.gameObject.SetActive(false);
                }
                break;
            case Direction.Right:
                {
                    if (walls.Down.Type == TypeWall.Null) pillars.RightDown.gameObject.SetActive(false);
                    if (walls.Up.Type == TypeWall.Null) pillars.RightUp.gameObject.SetActive(false);
                }
                break;
            case Direction.Up:
                {
                    if (walls.Left.Type == TypeWall.Null) pillars.LeftUp.gameObject.SetActive(false);
                    if (walls.Right.Type == TypeWall.Null) pillars.RightUp.gameObject.SetActive(false);
                }
                break;
            case Direction.Down:
                {
                    if (walls.Left.Type == TypeWall.Null) pillars.LeftDown.gameObject.SetActive(false);
                    if (walls.Right.Type == TypeWall.Null) pillars.RightDown.gameObject.SetActive(false);
                }
                break;
        }
    }

    public bool SpawnOnWall(GameObject prefab)
    {
        float maxPos = pillars.LeftDown.localPosition.x;
        float halfPos = maxPos * 0.5f;
        Vector3 position= new Vector3(0, 0, 0);
        Vector3 rotation = new Vector3(0, 0, 0);
        bool isChange = false;
        if(walls.Left.Type == TypeWall.Wall)
        {
            position = walls.Left.Point.localPosition; isChange = true;
            rotation = new Vector3(0, 0, -90);
        }
        else if (walls.Right.Type == TypeWall.Wall)
        {
            position = walls.Right.Point.localPosition; isChange = true;
            rotation = new Vector3(0, 0, 90);
        }
        else if (walls.Up.Type == TypeWall.Wall)
        {
            position = walls.Up.Point.localPosition; isChange = true;
            rotation = new Vector3(0, 0, 0);
        }
        else if (walls.Down.Type == TypeWall.Wall)
        {
            position = walls.Down.Point.localPosition; isChange = true;
            rotation = new Vector3(0, 0, 180);
        }
        if (isChange)
        {
            GameObject enemy = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity, ParentOjects);
            enemy.transform.localPosition = position;
            enemy.transform.localRotation = Quaternion.Euler(rotation);
            return true;
        }
        return false;
    }


    void ClearWalls()
    {
        if (this.walls.Left.Point.childCount > 0) ClearChildrens(this.walls.Left.Point);
        if (this.walls.Right.Point.childCount > 0) ClearChildrens(this.walls.Right.Point);
        if (this.walls.Up.Point.childCount > 0) ClearChildrens(this.walls.Up.Point);
        if (this.walls.Down.Point.childCount > 0) ClearChildrens(this.walls.Down.Point);
        this.walls.SetType(TypeWall.Null);
    }
    void ClearPillars()
    {
        pillars.LeftDown.gameObject.SetActive(true);
        pillars.LeftUp.gameObject.SetActive(true);
        pillars.RightDown.gameObject.SetActive(true);
        pillars.RightUp.gameObject.SetActive(true);
    }

    void ClearChildrens(Transform parent)
    {
        foreach (Transform child in parent) GameObject.Destroy(child.gameObject);
    }
}
