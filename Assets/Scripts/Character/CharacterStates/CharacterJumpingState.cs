using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterJumpingState : CharacterState {
	private bool grounded;

	public CharacterJumpingState(CharacterController characterController, CharacterStateMachine stateMachine) 
		: base(characterController, stateMachine) {
	}

	public override void Enter() {
		base.Enter();

		jumpAction.Disable();

		characterController.Jump();

		characterController.anim.SetAnimationParameter("IsInAir", true);
		characterController.anim.SetAnimationParameter("IsJumping", true);

		grounded = false;
	}

	public override void LogicUpdate() {
		base.LogicUpdate();
	}

	public override void PhysicsUpdate() {
		base.PhysicsUpdate();
		characterController.Move(horizontalMoveAction.ReadValue<float>());

		grounded = characterController.GetIfGrounded(ignorePlatforms: true);
		if (grounded) {
			stateMachine.ChangeState(characterController.grounded);
		}
		bool isFalling = characterController.GetIfIsFalling();
		if (isFalling) {
			stateMachine.ChangeState(characterController.falling);
		}
	}
}
