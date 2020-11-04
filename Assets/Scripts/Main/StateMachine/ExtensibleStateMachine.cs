using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtensibleStateMachine : MonoBehaviour
{
    class Transition
    {
        public IState To { get; }
        public Func<bool> Condition { get; }

        public Transition(IState to, Func<bool> condition)
        {
            this.To = to;
            this.Condition = condition;
        }
    }

    IState current;
    Dictionary<Type, List<Transition>> transitions = new Dictionary<Type, List<Transition>>();
    List<Transition> currentTransitions = new List<Transition>();
    List<Transition> anyTransitions = new List<Transition>();

    static List<Transition> emptyTransitions = new List<Transition>();

    public void Tick()
    {
        Transition transition = GetTransition();
        if (transition != null)
            SetState(transition.To);
        current?.Tick();
    }
    public void SetState(IState state)
    {
        if (state == current) return;

        current?.OnExit();
        current = state;

        if (Utils.Instance.DebugMode) 
        { 
            Debug.Log(current.ToString() + " - " + this.gameObject); 
        }

        transitions.TryGetValue(current.GetType(), out currentTransitions);
        if (currentTransitions == null)
            currentTransitions = emptyTransitions;

        current.OnEnter();

    }
    public void AddTransition ( IState from , IState to, Func<bool> predicate)
    {
        if(transitions.TryGetValue(from.GetType(),out var outTransitions) == false)
        {
            outTransitions = new List<Transition>();
            transitions[from.GetType()] = outTransitions;
        }
        outTransitions.Add(new Transition(to, predicate));
    }

    public void AddAnyTransition(IState state, Func<bool> predicate)
    {
        anyTransitions.Add(new Transition(state, predicate));
    }

    Transition GetTransition()
    {
        foreach (var trans in anyTransitions)
            if (trans.Condition())
                return trans;
        foreach (var trans in currentTransitions)
            if (trans.Condition())
                return trans;
        return null;
    }
}
