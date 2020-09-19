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

    Vector3 lastPos;

    bool isReverse = false;
    float startAngle = 0;

    void Start()
    {

    }
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
        if (enable) corRotate = StartCoroutine(IEPatrolRotate());
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



    IEnumerator IEPatrolRotate()
    {
        isEscaped = false;
        while (true)
        {
            Rotate(targetAngle, SpeedRotate, startAngle);
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
        yield return IEWaitPosition(waitTime * 0.5f, lastPos);
    }

    public void Rotate(float targetAngle, float speedRotate, float middleAngle)
    {
        GameMath.RotateLookAround(body, speedRotate, middleAngle, targetAngle, ref isReverse);
    }

    IEnumerator IETargetLock()
    {
        Vector3 pos = GetTarget().position;
        isEscaped = false;
        float speedMultiplier = 1.5f;
        float speed = SpeedRotate * speedMultiplier;
        while (true)
        {
            if (GetTarget())
            {
                GameMath.RotateToPosition(body, pos, speed);
                pos = GetTarget().position;
            }
            else
            {
                SetLastPosition(pos);
                if (GameMath.CheckRotatingToTarget(pos, body))
                    yield return IEWaitPosition(3f, pos);
            }       
            yield return null;
        }
    }
    void SetLastPosition(Vector3 position)
    {
        lastPos = position;
        lastPoint.position = position;
    }

    IEnumerator IEWaitPosition(float waitTime, Vector3 pos)
    {
        float counter = 0;
        float speedMultiplier = 1.5f;
        float speed = SpeedRotate * speedMultiplier;
        while (counter < waitTime)
        {
            GameMath.RotateToPosition(body, pos, speed);
            counter += Time.deltaTime;
            if (GetTarget())
            {
                yield break;
            }         
            yield return null;
        }
        if (!GetTarget()) isEscaped = true;
    }

    IEnumerator IEFindTargetsInRadius(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            GameMath.FindVisibleTargets(body, visibleTargets, radiusEye, angleEye);
        }
    }
}

