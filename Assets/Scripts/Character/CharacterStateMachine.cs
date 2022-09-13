using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateMachine {
	public CharacterState CurrentState { get; private set; }

	public void Initialize(CharacterState startingState) {
		CurrentState = startingState;
		startingState.Enter();
	}

	public void ChangeState(CharacterState newState) {
		CurrentState.Exit();

		CurrentState = newState;
		newState.Enter();
	}
}
