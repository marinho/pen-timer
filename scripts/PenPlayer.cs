using Godot;
using System;
using System.Linq;

public partial class PenPlayer : CharacterBody3D
{
	[Export]
	public float Speed = 5.0f;
	[Export]
	public float JumpVelocity = 4.5f;
	// [Export]
	// public Transform3D BoundaryPointA;
	// [Export]
	// public Transform3D BoundaryPointB;
	[Export]
	public Sprite3D InkSprite;
	// add a property to assign the parent node for ink sprites
	[Export]
	public NodePath InkParentPath;
	[Export]
	public float InkDropDistance = 0.1f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	// private constants for inputs
	const string moveLeftInput = "move_left";
	const string moveRightInput = "move_right";
	const string moveForwardInput = "move_forward";
	const string moveBackInput = "move_back";
	const string uiAcceptInput = "ui_accept";

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed(uiAcceptInput) && IsOnFloor())
			velocity.Y = JumpVelocity;

		// Get the input direction and handle the movement/deceleration.
		Vector2 inputDir = Input.GetVector(moveLeftInput, moveRightInput, moveForwardInput, moveBackInput);
		Vector2 skewedInputDir = new Vector2(inputDir.X, inputDir.Y);
		
		// TODO: fix this ugly code, either by using the direction of the camera, or by rotating the coordinates of the input
		if ( inputDir.X != 0 && inputDir.Y != 0) {
			if (inputDir.X < 0 && inputDir.Y < 0) {
				skewedInputDir.X = inputDir.X;
				skewedInputDir.Y = 0;
			} else if (inputDir.X > 0 && inputDir.Y < 0) {
				skewedInputDir.X = 0;
				skewedInputDir.Y = inputDir.Y;
			} else if (inputDir.X < 0 && inputDir.Y > 0) {
				skewedInputDir.X = 0;
				skewedInputDir.Y = inputDir.Y;
			} else if (inputDir.X > 0 && inputDir.Y > 0) {
				skewedInputDir.X = inputDir.X;
				skewedInputDir.Y = 0;
			}
		} else if (inputDir.X != 0) {
			skewedInputDir.Y = -inputDir.X;
		} else if (inputDir.Y != 0) {
			skewedInputDir.X = inputDir.Y;
		}

		Vector3 direction = (Transform.Basis * new Vector3(skewedInputDir.X, 0, skewedInputDir.Y)).Normalized();

		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		// Set the velocity.
		Velocity = velocity;
		MoveAndSlide();

		// drop ink if the player has moved and is on the floor
		if (IsOnFloor() && skewedInputDir != Vector2.Zero)
			DropInk();
	}

	void DropInk()
	{
		// get the number of children in InkParentPath that are positioned at the same X and Z as the player, with a radius of 0.5
		var numChildren = GetNode<Node3D>(InkParentPath).GetChildren().ToList()
			.Where(child => child is Sprite3D)
			.Select(child => (Sprite3D)child)
			.Where(child => child is Sprite3D && child.Transform.Origin.DistanceTo(Transform.Origin) < InkDropDistance)
			.Count();

		// if there are more than 0 children, return
		if (numChildren > 0)
			return;

		// get a clone of InkSprite and add it to the scene
		var ink = (Sprite3D)InkSprite.Duplicate();

		// set the position of the ink to have X and Z to the position of the player, but keep Y as it was predefined
		ink.Transform = new Transform3D(ink.Transform.Basis, new Vector3(Transform.Origin.X, 0, Transform.Origin.Z));

		// add the ink to InkParentPath
		GetNode<Node3D>(InkParentPath).AddChild(ink);

		// print the number of children in InkParentPath
		// GD.Print(GetNode<Node3D>(InkParentPath).GetChildren().Count());
	}


}
