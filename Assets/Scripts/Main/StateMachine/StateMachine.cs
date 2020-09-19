using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    private Dictionary<Type, IState> availablesStates;
    public event Action<IState> OnStateChanged;
    private IState current;

    public void SetStates(Dictionary<Type, IState> states)
    {
        availablesStates = states;
    }

    private void Update()
    {
        if (current == null)
        {
            current = availablesStates.Values.First();
        }
        Type nextState = current?.TypeTick();
        if (nextState != null && nextState != current?.GetType())
            SwitchToNewState(nextState);
    }

    private void SwitchToNewState(Type nextState)
    {
        current = availablesStates[nextState];
        OnStateChanged?.Invoke(current);
    }
}
