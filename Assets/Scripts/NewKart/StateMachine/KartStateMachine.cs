using UnityEngine;

public class KartStateMachine
{
    public KartState currentState;
    public KartState previousState;

    public KartStateMachine(KartState state)
    {
        previousState = null;
        currentState = state;
        currentState.InitializeState();
    }

    public void ChangeState(KartState state)
    {
        previousState = currentState;
        currentState = state;
        currentState.InitializeState();
    }
}
