using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

public class CharacterGroundedState : CharacterState {
	public CharacterGroundedState(CharacterController characterController, CharacterStateMachine stateMachine) 
		: base(characterController, stateMachine) {
	}

	public override void Enter() {
		base.Enter();

		horizontalMoveAction.Enable();
		jumpAction.Enable();

		characterController.OnGroundMovementReset();
		characterController.SetMovementY(false);

		characterController.anim.SetAnimationParameter("IsInAir", false);
		characterController.anim.SetAnimationParameter("IsJumping", false);
	}

	public override void Exit() {
		base.Exit();
		jumpAction.Disable();
	}

	public override void HandleInput() {
		base.HandleInput();
		if (jumpAction.inProgress) {
			stateMachine.ChangeState(characterController.jumping);
		}
	}

	public override void LogicUpdate() {
		base.LogicUpdate();
		characterController.anim.SetAnimationParameter("HorizontalSpeed", Mathf.Abs(horizontalMoveAction.ReadValue<float>()));
	}

	public override void PhysicsUpdate() {
		base.PhysicsUpdate();

		characterController.Move(horizontalMoveAction.ReadValue<float>());

		bool grounded = characterController.GetIfGrounded();
		if (!grounded) {
			stateMachine.ChangeState(characterController.falling);
		}
	}
}