using System;
public class SearchState : IState
{
    IPatroling patroling;

    public SearchState(IPatroling patroling)
    {

        this.patroling = patroling;

    }

    public void OnEnter()
    {
        this.patroling.Search(true);
    }

    public void OnExit()
    {
        this.patroling.Search(false);
    }

    public void Tick()
    {

    }

    public Type TypeTick()
    {
        return null;
    }
}
