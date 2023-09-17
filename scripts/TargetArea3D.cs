using Godot;
using System;

public partial class TargetArea3D : Area3D
{
	[Export]
	public string PenPlayerName = "Pen Player";
	[Export]
	public AudioStream HitSound;
	[Export]
	public AudioStreamPlayer3D hitSoundPlayer;

	public void _OnBodyEntered(Node3D body)
	{
		if (body.Name != PenPlayerName)
			return;

		// play the hit sound
		hitSoundPlayer.Stream = HitSound;
		hitSoundPlayer.Play();

		// print the name of the body that entered
		GD.Print(body.Name);
	}
}
