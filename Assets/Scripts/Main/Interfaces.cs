using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interfaces 
{

}

public interface IMovable 
{
    float Speed { get; }

    void Move();    
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
public interface IWeapon
{
    void Attack();
}

public interface IEquippable
{
    void Equip();
    void Unequip();
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
