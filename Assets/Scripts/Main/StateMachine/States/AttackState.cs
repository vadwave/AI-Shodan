using System;

public class AttackState : IState
{
    IAttacking attacking;

    public AttackState(IAttacking attacking)
    {
        this.attacking = attacking;
    }

    public void OnEnter()
    {
        this.attacking.Attack(true);
    }

    public void OnExit()
    {
        this.attacking.Attack(false);
    }

    public void Tick()
    {
        
    }

    public Type TypeTick()
    {
        return null;
    }
}
