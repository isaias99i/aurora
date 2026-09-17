using Godot;

public partial class Player : CharacterBody3D
{
	[Export] public float Speed = 5.0f;
	[Export] public float JumpVelocity = 4.5f;
	[Export] public float MouseSensitivity = 0.01f;
	[Export] public float MinCameraLength = 3.0f;
	[Export] public float MaxCameraLength = 15.0f;

	private SpringArm3D springArm;

	public override void _Ready()
	{
		springArm = GetNode<SpringArm3D>("SpringArm3D");
	}

	public override void _Input(InputEvent @event)
	{
		// Rotação só com botão direito pressionado
		if (@event is InputEventMouseMotion motion && Input.IsMouseButtonPressed(MouseButton.Left))
		{
			RotateY(-motion.Relative.X * MouseSensitivity);
			springArm.RotateX(-motion.Relative.Y * MouseSensitivity);

			// Limitar inclinação vertical da câmera
			var rot = springArm.RotationDegrees;
			rot.X = Mathf.Clamp(rot.X, -45, 45);
			springArm.RotationDegrees = rot;
		}

		// Zoom com scroll do mouse
		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.WheelUp && mouseButton.Pressed)
				springArm.SpringLength = Mathf.Max(MinCameraLength, springArm.SpringLength - 1);

			if (mouseButton.ButtonIndex == MouseButton.WheelDown && mouseButton.Pressed)
				springArm.SpringLength = Mathf.Min(MaxCameraLength, springArm.SpringLength + 1);
		}
	}


	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Movimento WASD
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = 0;
			velocity.Z = 0;
		}

		// Gravidade
		if (!IsOnFloor())
			velocity.Y -= 9.8f * (float)delta;

		// Pulo
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
			velocity.Y = JumpVelocity;

		Velocity = velocity;
		MoveAndSlide();
	}
}
