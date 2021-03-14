using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Rewards = Constants.Scores.Agent;

public class Player : Agent, IDamageable, IDamageDealer, IMovable, IRotable, IEye, IPocket
{
    [SerializeField] float health = 100f;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotateSpeed = 3f;
    [SerializeField] Rigidbody2D rigBody;
    [SerializeField] Transform body;

    [Header("IEye")]
    [SerializeField] float viewDistance = 10f;
    [Range(0, 360)]
    [SerializeField] int viewAngle = 20;
    [SerializeField] bool enableVision = false;
    [Header("DEBUG")]
    [SerializeField] Transform lastPoint; 
    [SerializeField] List<Transform> visibleTargets = new List<Transform>();

    const float timeDelay = 0.0f;
    private bool isWaiting = false;
    float scores = 0;
    int keys = 0;


    Coroutine corFind;
    Coroutine corCheckPos;

    LevelManager level;

    EnvironmentParameters resetParams;


    public float Health => health;
    public float Speed => moveSpeed;
    public float SpeedRotate => rotateSpeed;
    public bool OpenEye => enableVision;
    public float Radius => viewDistance;
    public int Angle => viewAngle;
    public int Keys => keys;
    public float Scores => scores;


    public event Action<Transform, Transform> OnRespawn;
    public event Action OnEndedRespawn;
    public event Action OnEscaped;
    public event Action<float> OnAddedScore;
    #region Agent

    public override void Initialize()
    {
        base.Initialize();
        resetParams = Academy.Instance.EnvironmentParameters;
        SetResetParameters();
    }
    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        keys = 0;
        scores = 0;
        SetResetParameters();
        Respawn();
    }
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
    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
        if (sensor != null)
        {
            if (rigBody)
            {
                sensor.AddObservation(rigBody.velocity.normalized.x);
                sensor.AddObservation(rigBody.velocity.normalized.y);

            }
            sensor.AddObservation(body.rotation.eulerAngles.normalized.z);
            if (level.exit) sensor.AddObservation((rigBody.transform.position - level.exit.position).normalized);
        }
      

    }

    void SetResetParameters()
    {
        if (resetParams != null)
        {
            moveSpeed = resetParams.GetWithDefault("moveSpeed", moveSpeed);
            rotateSpeed = resetParams.GetWithDefault("rotateSpeed", rotateSpeed);

            viewAngle = (int)resetParams.GetWithDefault("viewAngle", viewAngle);
            viewDistance = resetParams.GetWithDefault("viewDistance", viewDistance);
            if (level)
            {
                level.SetParameters(resetParams);
            }
            else
            {
                this.transform.root.GetComponent<LevelManager>().SetParameters(resetParams);
            }
        }

    }

    #endregion


    #region Moving

    public void Move(float[] vectorAction)
    {
        if (isWaiting) return;
        Vector3 dir;
        float angleRotate;
        GetDirection(vectorAction, out dir, out angleRotate);
        Vector2 pos = rigBody.transform.position + dir * Time.deltaTime;
        rigBody.MovePosition(pos);
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
        dir += rigBody.transform.up * vectorAction[0] * moveSpeed;
        dir += rigBody.transform.right * vectorAction[1] * moveSpeed;
        angleRotate = vectorAction[2];
    }

    public void Move()
    {

    }
    public void Rotate(float angle, float speed, float startAngle)
    {

    }


    #endregion

    #region Vision

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
    public void Alert(bool enable)
    {

    }

    void CheckTargets()
    {
        foreach (Transform target in visibleTargets)
        {
            if (target.GetComponent<SecurityCamera>())
            {
                AddReward(- Rewards.Check);
                Debug.Log("Find Camera score: -" + Rewards.Check);
            }
            else if (target.GetComponent<Guard>())
            {
                AddReward(- Rewards.Check);
                Debug.Log("Find Guard score: -" + Rewards.Check);
            }
            else if (target.GetComponent<CollectLogic>())
            {
                AddReward(Rewards.Check);
                Debug.Log("Find CollectLogic score: +" + Rewards.Check);
            }
            else if (target.GetComponent<KeyLogic>())
            {
                AddReward(Rewards.Check);
                Debug.Log("Find KeyLogic score: +" + Rewards.Check);
            }
        }
    }
    void OnVisiblePlayer()
    {
        AddReward(-Rewards.Visible);
    }

    #endregion

    #region HealthAndAttack

    public void DealDamage(IDamageable damageable, int amount)
    {
        throw new System.NotImplementedException();
    }
    public void TakeDamage(int amount)
    {
        throw new System.NotImplementedException();
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
    IEnumerator IEWaitingAfterRespawn(float delay)
    {
        isWaiting = true;
        yield return new WaitForSeconds(delay);
        isWaiting = false;
        OnEndedRespawn?.Invoke();
        yield return null;
        if(corCheckPos==null)
        corCheckPos = StartCoroutine(IECheckPosition(1f));
    }

    IEnumerator IECheckPosition(float delay)
    {
        while (true)
        {
            Vector3 lastPos = body.position;
            yield return new WaitForSeconds(delay);
            Vector3 currentPos = body.position;
            Vector2 minSize = new Vector2(lastPos.x-1, lastPos.y-1);
            Vector2 maxSize = new Vector2(lastPos.x+1, lastPos.y+1);
            if (((minSize.x <= currentPos.x) && (currentPos.x <= maxSize.x)) &&
                ((minSize.y <= currentPos.y) && (currentPos.y <= maxSize.y)))
            {
                AddReward(-Rewards.Check);
                //Debug.Log("ALARM! Change Position!");
            }
            yield return null;
        }
    }

    #endregion


    #region LevelLogic

    public void ExitLevel(bool success = true)
    {
        if(corCheckPos!=null)
        StopCoroutine(corCheckPos);
        corCheckPos = null;
        OnEscaped?.Invoke();
        float tempReward = (success) ? Rewards.Win : -Rewards.Win;
        SetReward(tempReward);
        EndEpisode();
    }

    public void Respawn()
    {
        rigBody.velocity = default(Vector2);
        rigBody.angularVelocity = 0;

        OnRespawn?.Invoke(rigBody.transform, body);

        StartCoroutine(IEWaitingAfterRespawn(1f));
    }

    public void SetLevel(LevelManager levelManager)
    {
        this.level = levelManager;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Finish")
        {
            ExitLevel();
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Wall")
        {
            AddReward(-(Rewards.Check * 2));
           // Debug.Log("Collision score: -" + Rewards.Check*2);
        }


    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.tag == "Wall")
        {
            AddReward(Rewards.Check);
           // Debug.Log("Collision score: +" + Rewards.Check);
        }
    }

    #endregion

    #region ItemActions
    public bool UseKey()
    {
        if (Keys > 0)
        {
            keys--;
            AddReward(Rewards.Key);
            return true;
        }
        return false;
    }

    public void AddKey()
    {
        keys++;
        AddReward(Rewards.Key);
    }

    public void Collect()
    {
        scores++;
        AddReward(Rewards.Collectable);
        OnAddedScore?.Invoke(scores);
    }

    #endregion
}
