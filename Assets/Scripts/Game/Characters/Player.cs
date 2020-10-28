using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class Player : Agent, IDamageable, IDamageDealer, IMovable, IRotable, IEye, IPocket
{
    [SerializeField] float health = 100f;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotateSpeed = 3f;
    [SerializeField] Rigidbody2D rigidbody;
    [SerializeField] Transform body;

    [Header("IEye")]
    [SerializeField] float viewDistance = 10f;
    [Range(0, 360)]
    [SerializeField] int viewAngle = 20;
    [SerializeField] bool enableVision = false;

    [SerializeField] Transform lastPoint; //Debug
    [SerializeField] List<Transform> visibleTargets = new List<Transform>();



    const float timeDelay = 0.0f;

    public float scores = 0;
    public int keys = 0;


    Coroutine corFind;


    public float Health => health;

    public float Speed => moveSpeed;

    public float SpeedRotate => rotateSpeed;

    public bool OpenEye => throw new System.NotImplementedException();

    public float Radius => throw new System.NotImplementedException();

    public int Angle => throw new System.NotImplementedException();

    public int Keys => keys;

    public float Scores => scores;

    public void DealDamage(IDamageable damageable, int amount)
    {
        throw new System.NotImplementedException();
    }

    public void Find(bool enable)
    {
        if (enable)
        {
            if (corFind == null)
                corFind = StartCoroutine(IEFindTargetsInRadius(timeDelay));
        }
        else 
        { 
            StopCoroutine(corFind);
            corFind = null;
        }
        CheckTargets();
    }

    public void TakeDamage(int amount)
    {
        throw new System.NotImplementedException();
    }



    #region Agent

    public override void Heuristic(float[] actionsOut)
    {
        base.Heuristic(actionsOut);
        //InputControl(ref actionsOut);
        InputControl(ref actionsOut);

    }

    public override void OnActionReceived(float[] vectorAction)
    {
        base.OnActionReceived(vectorAction);
        Move(vectorAction);
        Find(true);
    }

    #endregion

    #region Moving

    public void Move(float[] vectorAction)
    {
        Vector3 dir;
        float angleRotate;
        //GetDirection(vectorAction, out dir, out rotateDir, out angleRotate);
        GetDirection(vectorAction, out dir, out angleRotate);//* moveSpeed
        Vector2 pos = transform.position + dir * Time.deltaTime;
        rigidbody.MovePosition(pos);
        body.rotation = Quaternion.Lerp(body.rotation, Quaternion.Euler(0,0,-angleRotate), rotateSpeed * Time.deltaTime);
    }
    void InputControl(ref float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Vertical");
        actionsOut[1] = Input.GetAxis("Horizontal");

        Vector3 mousePos = Utils.Instance.GetPosMousePosition();
        Vector2 direction = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        actionsOut[2] = angle;
    }
    void GetDirection(float[] vectorAction, out Vector3 dir, out float angleRotate)
    {
        dir = Vector3.zero;
        dir += transform.up * vectorAction[0] * moveSpeed;
        dir += transform.right * vectorAction[1] * moveSpeed;
        angleRotate = vectorAction[2];
    }
    
    #endregion



    #region Coroutines

    IEnumerator IEFindTargetsInRadius(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            GameMath.FindVisibleTargets(body, visibleTargets, viewDistance, viewAngle, true);
        }
    }

    #endregion


    void Start()
    {
        
    }
    void Update()
    {
        
    }


    void CheckTargets()
    {
       foreach(Transform target in visibleTargets)
        {
            if (target.GetComponent<SecurityCamera>())
            {

            }
            else if (target.GetComponent<Guard>())
            {

            }
            else if (target.GetComponent<CollectLogic>())
            {

            }
        }
    }


    public void Move()
    {
        
    }

    public void Alert(bool enable)
    {

    }

    public void Rotate(float angle, float speed, float startAngle)
    {

    }

    public bool UseKey()
    {
        if (Keys > 0)
        {
            keys--;
            return true;
        }
        return false;
    }

    public void AddKey()
    {
        keys++;
    }

    public void Collect()
    {
        scores++;
    }
}
