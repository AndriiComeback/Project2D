using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class CharacterState {
	protected CharacterController characterController;
	protected CharacterStateMachine stateMachine;

	public InputAction horizontalMoveAction;
	public InputAction jumpAction;
	public InputAction shootAction;

	protected CharacterState(CharacterController characterController, CharacterStateMachine stateMachine) {
		this.characterController = characterController;
		this.stateMachine = stateMachine;

		characterController.playerControls.Enable();
		horizontalMoveAction = characterController.playerControls.CharacterControls.HorizontalMove;
		jumpAction = characterController.playerControls.CharacterControls.Jump;
		shootAction = characterController.playerControls.CharacterControls.Shoot;
	}

	public virtual void Enter() {

	}

	public virtual void HandleInput() {

	}

	public virtual void LogicUpdate() {
	}

	public virtual void PhysicsUpdate() {

	}

	public virtual void Exit() {

	}
}
