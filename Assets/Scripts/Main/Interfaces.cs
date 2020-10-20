using System;
using UnityEngine;

public class Interfaces 
{

}

public interface IMovable 
{
    float Speed { get; }

    void Move();    
}

public interface IPocket 
{
    int Keys { get; }
    float Scores { get; }

    bool UseKey();
    void AddKey();
    void Collect();
}

public interface IRotable
{
    float SpeedRotate { get; }
    void Rotate(float angle, float speed, float startAngle);
}
public interface IEye 
{
    bool OpenEye { get; }
    float Radius { get; }
    int Angle { get; }
    void Find(bool enable);

    void Alert(bool enable);
}
public interface IPatroling 
{
    void Patrol(bool enable);
    void Search(bool enable);
}
public interface IDamageable : IHeatlh
{
    void TakeDamage(int amount);
}
public interface IDamageDealer 
{
    void DealDamage(IDamageable damageable, int amount);
}

public interface IAttacking : IDamageDealer, IWeapon
{
    float DistanceAttack { get; }
}
public interface IWeapon
{
    float Damage { get; }
    void Attack(bool enable);
}

public interface IEquippable
{
    void Equip();
    void Unequip();
}

public interface ITakeable
{
    void Take(Collider2D collision);
}

public interface IHeatlh
{
    float Health { get; }
}

public interface IState
{
    Type TypeTick();
    void Tick();
    void OnEnter();
    void OnExit();
}
