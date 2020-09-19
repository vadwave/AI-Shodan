using System;

public class AllertState : IState
{
    IEye eye;

    public AllertState(IEye eye)
    {
        this.eye = eye;
    }

    public void OnEnter()
    {
        eye.Alert(true);
    }

    public void OnExit()
    {
        eye.Alert(false);
    }

    public void Tick()
    {
        
    }

    public Type TypeTick()
    {
        return null;
    }
}
