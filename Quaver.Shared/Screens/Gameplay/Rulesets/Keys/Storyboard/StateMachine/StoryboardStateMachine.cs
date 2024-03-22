using System;
using System.Collections.Generic;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.StateMachine;

public class StoryboardStateMachine
{
    private readonly List<StoryboardStateMachineState> _states = new();

    public const int IdleStateId = -1;
    public int CurrentStateId { get; set; } = IdleStateId;
    
    public void ChangeState(int id)
    {
        if (CurrentStateId != IdleStateId)
        {
            _states[CurrentStateId].OnDisable();
        }

        if (id == IdleStateId)
        {
            CurrentStateId = IdleStateId;
            return;
        }

        if (id >= _states.Count || id < 0) throw new ArgumentOutOfRangeException(nameof(id), $"No state has the id {id}");
        var state = _states[id];
        CurrentStateId = id;
        state.OnEnable();
    }

    public void ExitToGlobal()
    {
        ChangeState(IdleStateId);
    }

    public bool RegisterState(StoryboardStateMachineState state)
    {
        state.Id = _states.Count;
        _states.Add(state);
        state.OnInitialize();
        state.OnEnable();
        return true;
    }

    public void Update()
    {
        if (CurrentStateId == IdleStateId) return;
        var state = _states[CurrentStateId];
        var nextStateId = state.Update();
        if (nextStateId == CurrentStateId) return;
        ChangeState(nextStateId);
    }
}