using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameMath
{
    const float precision = 0.9999f;

    static LayerMask targetMask;
    static LayerMask obstacleMask;


    public static void Initialize(LayerMask obstacleMask, LayerMask targetMask)
    {
        GameMath.targetMask = targetMask;
        GameMath.obstacleMask = obstacleMask;
    }


    #region Rotate

    public static bool CheckAngle(Quaternion targetRotation, Quaternion objectRotation)
    {
        float oper = Mathf.Abs(Quaternion.Dot(objectRotation, targetRotation));
        return (oper > precision);
    }
    public static bool CheckAngleToTarget(Vector3 targetPosition, Transform objectTransform)
    {
        Vector2 dirToTarget = (targetPosition - objectTransform.position).normalized;
        float angle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
        Quaternion QtargetAngle = Quaternion.Euler(0f, 0f, angle);
        return !CheckAngle(QtargetAngle, objectTransform.localRotation);
    }

    public static void Rotate(Transform objectTransform, float speedRotate, float targetAngle)
    {
        Quaternion qTargetAngle = Quaternion.Euler(0f, 0f, targetAngle);
        Rotate(objectTransform, speedRotate, qTargetAngle);
    }
    public static void Rotate(Transform objectTransform, float speedRotate, Quaternion qTargetAngle)
    {
        objectTransform.rotation = Quaternion.RotateTowards(objectTransform.rotation, qTargetAngle, speedRotate * Time.deltaTime);
    }
    public static void RotateToPosition(Transform objectTransform, Vector3 targetPosition, float speedRotate)
    {
        Vector2 dirToTarget = (targetPosition - objectTransform.position).normalized;
        float angle = Mathf.Atan2(dirToTarget.x, dirToTarget.y) * Mathf.Rad2Deg;
        Rotate(objectTransform, speedRotate, -angle);
    }
    public static void RotateLookAround(Transform objectTransform, float speedRotate, float middleAngle, float targetAngle, ref bool isReverse)
    {
        targetAngle = (isReverse) ? -targetAngle : targetAngle;
        float angleRotate = middleAngle - targetAngle;
        Quaternion QtargetAngle = Quaternion.Euler(0f, 0f, angleRotate);
        if (GameMath.CheckAngle(QtargetAngle, objectTransform.localRotation)) isReverse = !isReverse;
        LocalRotate(objectTransform, speedRotate, QtargetAngle);
    }

    public static void LocalRotate(Transform objectTransform, float speedRotate, Quaternion qTargetAngle)
    {
        objectTransform.localRotation = Quaternion.RotateTowards(objectTransform.localRotation, qTargetAngle, speedRotate * Time.deltaTime);
    }

    #endregion

    #region Find
    public static List<Transform> FindVisibleTargets(Transform objectTransform, List<Transform> visibleTargets, float radius, float viewAngle)
    {
        visibleTargets.Clear();
        Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(objectTransform.position, radius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = CheckTarget(objectTransform, targetsInViewRadius[i].transform, viewAngle);
            if (target) visibleTargets.Add(target);
        }
        return visibleTargets;
    }

    static Transform CheckTarget(Transform objectTransform, Transform target, float viewAngle)
    {
        Vector2 dirToTarget = (target.position - objectTransform.position).normalized;
        Debug.DrawRay(objectTransform.position, dirToTarget, Color.red); // Debug
        float angle = Vector2.Angle(objectTransform.up, dirToTarget);
        if (angle < viewAngle)
        {
            float dstToTarget = Vector2.Distance(objectTransform.position, target.position);
            if (!Physics2D.Raycast(objectTransform.position, dirToTarget, dstToTarget, obstacleMask))
            {
                return target;
            }
        }
        return null;
    }
    #endregion
}
