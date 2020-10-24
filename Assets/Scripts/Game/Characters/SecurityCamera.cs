using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityCamera : MonoBehaviour, IEye, IPatroling, IRotable
{
    ExtensibleStateMachine stateMachineEx;
    StateMachine stateMachine;

    [Header("IRotable")]
    [SerializeField] float targetAngle = 90f;
    [SerializeField] float speedRotate = 1f;
    [SerializeField] Transform body;


    [Header("IEye")]
    [SerializeField] float viewDistance = 10f;
    [Range(0,360)]
    [SerializeField] int viewAngle = 20;
    [SerializeField] bool enableVision = false;

    [SerializeField] Transform lastPoint; //Debug
    [SerializeField] List<Transform> visibleTargets = new List<Transform>();

    const float timeDelay = 0.0f;

    Coroutine corRotate;
    Coroutine corFind;

    bool isEscaped;
    bool isReverse = false;

    public float Radius => viewDistance;
    public int Angle => viewAngle;
    public bool OpenEye => enableVision;
    public float SpeedRotate => speedRotate;

    Vector3 lastPos;
    float startAngle = 0;

    void Start()
    {

    }
    void Update()
    {  
        stateMachineEx.Tick();
    }
    void Awake()
    {
        if (!lastPoint)
        {
            lastPoint = GameObject.Find("LastPoint").transform;
        }
        startAngle = body.localRotation.eulerAngles.z;
        InitializeExtensibleStateMachine();
    }

    public void ActivateAI(bool enable)
    {
        enableVision = enable;
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

        At(patrol, allert, TargetVisible());

        At(allert, search, NotTargetVisible());

        At(search, allert, TargetVisible());
        At(search, patrol, NotTargetVisible());

        stateMachineEx.SetState(idle);

        void At(IState _to, IState _from, Func<bool> _condition) => stateMachineEx.AddTransition(_to, _from, _condition);
        void AtAny(IState _state, Func<bool> _conditionstate) => stateMachineEx.AddAnyTransition(_state, _conditionstate);

        Func<bool> TargetVisible() => () => (GetTarget() != null);
        Func<bool> NotTargetVisible() => () => isEscaped;

        Func<bool> Enable() => () => OpenEye;
        Func<bool> Disable() => () => !OpenEye;
    }


    #region States

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


    #endregion

    #region IEnumerators

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
            RotateSearch(targetAngle, speedRotate, curAngle);
            counter += Time.deltaTime;
            yield return null;
        }
        yield return IEWaitPosition(waitTime * 0.5f, lastPos);
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
                GameMath.RotateToPosition(body, pos, speed, startAngle, targetAngle);
                pos = GetTarget().position;
            }
            else
            {
                SetLastPosition(pos);
                if (GameMath.CheckAngleToTarget(pos, body))
                    yield return IEWaitPosition(3f, pos);
            }       
            yield return null;
        }
    }
    IEnumerator IEWaitPosition(float waitTime, Vector3 pos)
    {
        float counter = 0;
        float speedMultiplier = 1.5f;
        float speed = SpeedRotate * speedMultiplier;
        while (counter < waitTime)
        {
            GameMath.RotateToPosition(body, pos, speed, startAngle, targetAngle);
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
            GameMath.FindVisibleTargets(body, visibleTargets, viewDistance, viewAngle);
        }
    }

    #endregion

    #region Methods

    public Transform GetTarget()
    {
        if (visibleTargets.Count != 0)
        {
            return visibleTargets[0];
        }
        return null;
    }
    void SetLastPosition(Vector3 position)
    {
        lastPos = position;
        lastPoint.position = position;
    }

    public void Find(bool enable)
    {
        if (enable) corFind = StartCoroutine(IEFindTargetsInRadius(timeDelay));
        else StopCoroutine(corFind);
    }
    public void Rotate(float targetAngle, float speedRotate, float middleAngle)
    {
        GameMath.RotateLookAround(body, speedRotate, middleAngle, targetAngle, ref isReverse);
    }
    public void RotateSearch(float targetAngle, float speedRotate, float middleAngle)
    {
        GameMath.RotateLookAround(body, speedRotate, middleAngle, targetAngle, this.startAngle, this.targetAngle, ref isReverse);
    }

    #endregion
}

