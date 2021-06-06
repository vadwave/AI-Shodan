using UnityEngine;

public class Utils : Singleton<Utils>
{
    [SerializeField] LayerMask targetMask;
    [SerializeField] LayerMask guardMask;
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] LayerMask itemMask;
    [SerializeField] Camera main;
    [SerializeField] bool isDebug;

    public bool DebugMode => isDebug;

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
        GameMath.Initialize(obstacleMask, targetMask, guardMask, itemMask);
    }
}
