using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFallingState : CharacterState {
	private bool grounded;

	public CharacterFallingState(CharacterController characterController, CharacterStateMachine stateMachine)
		: base(characterController, stateMachine) {
	}

	public override void Enter() {
		base.Enter();

		jumpAction.Disable();

		characterController.SetGravityScale(true);
		characterController.anim.SetAnimationParameter("IsInAir", true);
		characterController.anim.SetAnimationParameter("IsJumping", false);

		grounded = false;
	}

	public override void LogicUpdate() {
		base.LogicUpdate();
	}

	public override void PhysicsUpdate() {
		base.PhysicsUpdate();
		characterController.Move(horizontalMoveAction.ReadValue<float>());

		grounded = characterController.GetIfGrounded();
		if (grounded) {
			stateMachine.ChangeState(characterController.grounded);
		}
	}
}
