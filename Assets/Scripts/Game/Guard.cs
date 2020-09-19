using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIShodan
{
    namespace Enemies
    {
        public class Guard : MonoBehaviour, IDamageable, IDamageDealer, IMovable, IPatroling, IEye
        {
            public float Health => throw new System.NotImplementedException();

            public float Speed => throw new System.NotImplementedException();

            public float SpeedRotate => throw new System.NotImplementedException();

            public bool OpenEye => throw new System.NotImplementedException();

            public float Radius => throw new System.NotImplementedException();

            public int Angle => throw new System.NotImplementedException();

            public void Alert(bool enable)
            {
            
            }

            public void DealDamage(IDamageable damageable, int amount)
            {

            }


            public void Find(bool enable)
            {
                
            }

            public void Move()
            {

            }

            public void Patrol(bool enable)
            {

            }

            public void Search(bool enable)
            {
                throw new System.NotImplementedException();
            }

            public void TakeDamage(int amount)
            {

            }
        }
    }
}