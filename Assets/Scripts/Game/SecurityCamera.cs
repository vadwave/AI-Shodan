using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityCamera : MonoBehaviour, IEye, IPatroling, IRotable
{
    ExtensibleStateMachine stateMachineEx;
    StateMachine stateMachine;

    [Header("IRotable")]
    [SerializeField] float speedRotate = 1f;
    [SerializeField] Transform body;


    [Header("IEye")]
    [SerializeField] float radiusEye = 10f;
    [Range(0,360)]
    [SerializeField] int angleEye = 20;
    [SerializeField] bool openedEye = false;
    [SerializeField] LayerMask targetMask;
    [SerializeField] LayerMask obstacleMask;

    [SerializeField] Transform lastPoint;


    [SerializeField] List<Transform> visibleTargets = new List<Transform>();

    float targetAngle = 90f;
    const float timeDelay = 0.0f;

    Coroutine corRotate;
    Coroutine corFind;
    private bool isEscaped;

    public float Radius => radiusEye;
    public int Angle => angleEye;
    public bool OpenEye => openedEye;

    public float SpeedRotate => speedRotate;


    bool isReverse = false;
    float startAngle = 0;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {  
        stateMachineEx.Tick();
    }
    private void Awake()
    {
        startAngle = body.localRotation.eulerAngles.z;
        InitializeExtensibleStateMachine();
    }

    void InitializeStateMachine()
    {
        this.gameObject.AddComponent(typeof(StateMachine));
        stateMachine = this.gameObject.GetComponent<StateMachine>();

        var states = new Dictionary<Type, IState>();
        states.Add(typeof(AllertState), new AllertState(this));
        states.Add(typeof(IdleState), new IdleState(this));

        stateMachine.SetStates(states);
    }
    void InitializeExtensibleStateMachine()
    {
        this.gameObject.AddComponent(typeof(ExtensibleStateMachine));
        stateMachineEx = this.gameObject.GetComponent<ExtensibleStateMachine>();

        var idle = new IdleState(this);
        var patrol = new PatrolState(this);
        var allert = new AllertState(this);
        var search = new SearchState(this);

       
        AtAny(idle, Disable());
        At(idle, patrol, Enable());

        At(patrol, allert, HasTarget());

        At(allert, search, NotTarget());

        At(search, allert, HasTarget());
        At(search, patrol, NotTarget());
        stateMachineEx.SetState(idle);

        void At(IState _to, IState _from, Func<bool> _condition) => stateMachineEx.AddTransition(_to, _from, _condition);
        void AtAny(IState _state, Func<bool> _conditionstate) => stateMachineEx.AddAnyTransition(_state, _conditionstate);

        Func<bool> HasTarget() => () => (GetTarget() != null);
        Func<bool> NotTarget() => () => isEscaped;
        Func<bool> Enable() => () => OpenEye;
        Func<bool> Disable() => () => !OpenEye;
    }
    public Transform GetTarget()
    {
        if (visibleTargets.Count != 0)
        {
            return visibleTargets[0];
        }
        return null;
    }


    public void Patrol(bool enable)
    {
        if (enable) corRotate = StartCoroutine(IEPatrolRotate(targetAngle,SpeedRotate));
        else StopCoroutine(corRotate);
        Find(enable);
    }
    public void Search(bool enable)
    {
        if (enable) corRotate = StartCoroutine(IESearchRotate(targetAngle * 0.5f, SpeedRotate * 1.5f, 5f));
        else StopCoroutine(corRotate);
        Find(enable);
    }
    public void Alert(bool enable)
    {
        if (enable) corRotate = StartCoroutine(IETargetLock());
        else StopCoroutine(corRotate);
        Find(enable);
    }
    public void Find(bool enable)
    {
        if (enable) corFind = StartCoroutine(IEFindTargetsInRadius(timeDelay));
        else StopCoroutine(corFind);
    }



    IEnumerator IEPatrolRotate(float targetAngle, float speedRotate)
    {
        isEscaped = false;
        while (true)
        {
            Rotate(targetAngle, speedRotate);
            yield return null;
        }
    }
    IEnumerator IESearchRotate(float targetAngle, float speedRotate, float waitTime)
    {
        isEscaped = false;
        float counter = 0;
        float curAngle = body.localRotation.eulerAngles.z;
        while (counter < waitTime)
        {
            Rotate(targetAngle, speedRotate, curAngle);
            counter += Time.deltaTime;
            yield return null;
        }
        yield return IEWaitLastPos(waitTime * 0.5f, lastPoint.position);
    }

    public void Rotate(float targetAngle, float speedRotate)
    {
        Rotate(targetAngle, speedRotate, startAngle);
    }
    public void Rotate(float targetAngle, float speedRotate ,float curAngle)
    {
        targetAngle = (isReverse) ? -targetAngle : targetAngle;
        float angle = curAngle - targetAngle;
        Quaternion QtargetAngle = Quaternion.Euler(0f, 0f, angle);
        if (CheckAngle(QtargetAngle, body.localRotation)) isReverse = !isReverse;
        body.localRotation = Quaternion.RotateTowards(body.localRotation, QtargetAngle, speedRotate * Time.deltaTime);
    }

    bool CheckAngle(Quaternion targetRotation, Quaternion bodyRotation)
    {
        const float precision = 0.9999f;
        float oper = Mathf.Abs(Quaternion.Dot(bodyRotation, targetRotation));
        return (oper > precision);
    }

    IEnumerator IETargetLock()
    {
        Vector3 pos = GetTarget().position;
        isEscaped = false;
        while (true)
        {
            if (GetTarget())
            {
                RotateToTarget(pos);
                pos = GetTarget().position;
            }
            else
            {
                lastPoint.position = pos;
                if (CheckRotatingToTarget(pos))
                    yield return IEWaitLastPos(3f, pos);
            }       
            yield return null;
        }
    }

    bool CheckRotatingToTarget(Vector3 targetPosition)
    {
        Vector2 dirToTarget = (targetPosition - body.position).normalized;
        float angle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
        Quaternion QtargetAngle = Quaternion.Euler(0f, 0f, angle);
        return !CheckAngle(QtargetAngle, body.localRotation);
    }


    IEnumerator IEWaitLastPos(float waitTime, Vector3 pos)
    {
        float counter = 0;
        while (counter < waitTime)
        {
            RotateToTarget(pos);
            counter += Time.deltaTime;
            if (GetTarget())
            {
                yield break;
            }         
            yield return null;
        }
        if (!GetTarget()) isEscaped = true;
    }

    void RotateToTarget(Vector3 targetPos)
    {
        Vector2 dirToTarget = (targetPos - body.position).normalized;
        float angle = Mathf.Atan2(dirToTarget.x, dirToTarget.y) * Mathf.Rad2Deg;
        body.rotation = Quaternion.RotateTowards(body.rotation, Quaternion.Euler(0f, 0f, -angle), (SpeedRotate*1.5f) * Time.deltaTime); //Quaternion.Euler(0f, 0f, -angle);
    }

    IEnumerator IEFindTargetsInRadius(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }
    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(body.position, radiusEye, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector2 dirToTarget = (target.position - body.position).normalized;
            Debug.DrawRay(body.position, dirToTarget, Color.red);
            float angle = Vector2.Angle(body.up, dirToTarget);
            if (angle < angleEye)
            {
                float dstToTarget = Vector2.Distance(body.position, target.position);
                if (!Physics2D.Raycast(body.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }
}

