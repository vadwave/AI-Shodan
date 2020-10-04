using Unity.Mathematics;
using Unity.MLAgents;
using UnityEngine;

public class Player : Agent, IDamageable, IDamageDealer, IMovable, IRotable, IEye
{
    [SerializeField] float health = 100f;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotateSpeed = 3f;
    [SerializeField] Rigidbody2D rigidbody;
    [SerializeField] Transform body;
    public float Health => health;

    public float Speed => moveSpeed;

    public float SpeedRotate => rotateSpeed;

    public bool OpenEye => throw new System.NotImplementedException();

    public float Radius => throw new System.NotImplementedException();

    public int Angle => throw new System.NotImplementedException();

    public void DealDamage(IDamageable damageable, int amount)
    {
        throw new System.NotImplementedException();
    }

    public void Find(bool enable)
    {
        
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
    }

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

    #region Old
    void InputOldControl(ref float[] actionsOut)
    {
        if (Input.GetKey(KeyCode.W))
        {
            actionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            actionsOut[0] = 2;
        }
        //right
        if (Input.GetKey(KeyCode.A))
        {
            actionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            actionsOut[1] = 2;
        }
        //rotate
        if (Input.GetKey(KeyCode.E))
        {
            actionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            actionsOut[2] = 2;
        }
        Vector3 mousePos = Utils.Instance.GetPosMousePosition();
        Vector2 direction = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        actionsOut[3] = angle;
    }
    void GetOldDirection(float[] vectorAction, out Vector3 dir, out Vector3 rotateDir, out float angleRotate)
    {
        dir = Vector3.zero;
        rotateDir = Vector3.zero;

        float forwardAxis = vectorAction[0];
        float rightAxis = vectorAction[1];
        float rotateAxis = vectorAction[2];
        angleRotate = vectorAction[3];

        switch (forwardAxis)
        {
            case 1:
                dir += transform.up * moveSpeed;
                break;
            case 2:
                dir += transform.up * -moveSpeed;
                break;
        }

        switch (rightAxis)
        {
            case 1:
                dir += transform.right * -moveSpeed;
                break;
            case 2:
                dir += transform.right * moveSpeed;
                break;
        }

        switch (rotateAxis)
        {
            case 1:
                rotateDir = transform.forward * -1f;
                break;
            case 2:
                rotateDir = transform.forward * 1f;
                break;
        }
    }
    #endregion

    #endregion



    void Start()
    {
        
    }
    void Update()
    {
        
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
}
