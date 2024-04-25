using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

[MoonSharpUserData]
public class ModChartStateMachine
{
    private readonly List<StateMachineState> _states = new();

    public const int IdleStateId = -1;
    public int CurrentStateId { get; private set; } = IdleStateId;
    
    public void To(int id)
    {
        if (CurrentStateId != IdleStateId)
        {
            _states[CurrentStateId].OnLeave();
        }

        if (id == IdleStateId)
        {
            CurrentStateId = IdleStateId;
            return;
        }

        if (id >= _states.Count || id < 0) throw new ArgumentOutOfRangeException(nameof(id), $"No state has the id {id}");
        var state = _states[id];
        CurrentStateId = id;
        state.OnEnter();
    }

    public void Idle()
    {
        To(IdleStateId);
    }


    [MoonSharpHidden]
    public void Update()
    {
        // if (CurrentStateId == IdleStateId) return;
        // var state = _states[CurrentStateId];
        // var nextStateId = state.OnUpdateSelf();
        // if (nextStateId == CurrentStateId) return;
        // To(nextStateId);
    }
}