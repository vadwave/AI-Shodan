using UnityEngine;

public class Utils : Singleton<Utils>
{
    [SerializeField] LayerMask targetMask;
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] Camera main;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public Vector3 GetPosMousePosition()
    {
        return main.ScreenToWorldPoint(Input.mousePosition);
    }

    protected override void AwakeSingleton()
    {
        GameMath.Initialize(obstacleMask,targetMask);
    }
}
