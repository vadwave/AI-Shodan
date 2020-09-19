using System;

public class PatrolState : IState
{
    IPatroling patroling;

    public PatrolState(IPatroling patroling)
    {

       this.patroling = patroling;
       
    }

    public void OnEnter()
    {
        this.patroling.Patrol(true);
    }

    public void OnExit()
    {
        this.patroling.Patrol(false);
    }

    public void Tick()
    {

    }

    public Type TypeTick()
    {
        return null;
    }

}
