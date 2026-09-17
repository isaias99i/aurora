using Godot;

public partial class Player : CharacterBody3D
{
	[ExportGroup("Movimento")]
	[Export(PropertyHint.Range, "0.1,20,0.1")]
	public float Speed { get; set; } = 6.0f;

	[Export(PropertyHint.Range, "1,100,0.5")]
	public float Acceleration { get; set; } = 30.0f;

	[Export(PropertyHint.Range, "0.1,15,0.1")]
	public float JumpVelocity { get; set; } = 6.0f;

	[ExportGroup("Camera")]
	// Radianos por pixel; não multiplicar o deslocamento do mouse por delta.
	[Export(PropertyHint.Range, "0.0001,0.02,0.0001")]
	public float MouseSensitivity { get; set; } = 0.003f;

	[Export(PropertyHint.Range, "1,20,0.5")]
	public float MinCameraDistance { get; set; } = 2.0f;

	[Export(PropertyHint.Range, "1,30,0.5")]
	public float MaxCameraDistance { get; set; } = 12.0f;

	[Export(PropertyHint.Range, "0.1,2,0.1")]
	public float ZoomStep { get; set; } = 0.5f;

	private const float VisualTurnSpeed = 12.0f;
	private const float ZoomSmoothSpeed = 12.0f;
	private Node3D _visual;
	private Node3D _cameraPivot;
	private SpringArm3D _springArm;
	private float _cameraYaw;
	private float _cameraPitch;
	private float _targetZoom;

	public override void _Ready()
	{
		_visual = GetNode<Node3D>("Visual");
		_cameraPivot = GetNode<Node3D>("CameraPivot");
		_springArm = GetNode<SpringArm3D>("CameraPivot/SpringArm3D");
		_cameraYaw = _cameraPivot.Rotation.Y;
		_cameraPitch = _springArm.Rotation.X;
		_targetZoom = _springArm.SpringLength;
		_springArm.AddExcludedObject(GetRid());
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("release_mouse"))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
			GetViewport().SetInputAsHandled();
			return;
		}

		if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true }
			&& Input.MouseMode != Input.MouseModeEnum.Captured)
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
			GetViewport().SetInputAsHandled();
			return;
		}

		if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
		{
			if (mouseButton.ButtonIndex == MouseButton.WheelUp)
			{
				_targetZoom = Mathf.Clamp(_targetZoom - ZoomStep, MinCameraDistance, MaxCameraDistance);
				GetViewport().SetInputAsHandled();
				return;
			}

			if (mouseButton.ButtonIndex == MouseButton.WheelDown)
			{
				_targetZoom = Mathf.Clamp(_targetZoom + ZoomStep, MinCameraDistance, MaxCameraDistance);
				GetViewport().SetInputAsHandled();
				return;
			}
		}

		if (@event is InputEventMouseMotion motion
			&& Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			_cameraYaw = Mathf.Wrap(_cameraYaw - motion.ScreenRelative.X * MouseSensitivity,
				-Mathf.Pi, Mathf.Pi);
			_cameraPitch = Mathf.Clamp(_cameraPitch - motion.ScreenRelative.Y * MouseSensitivity,
				Mathf.DegToRad(-65.0f), Mathf.DegToRad(35.0f));
			_cameraPivot.Rotation = new Vector3(0.0f, _cameraYaw, 0.0f);
			_springArm.Rotation = new Vector3(_cameraPitch, 0.0f, 0.0f);
			GetViewport().SetInputAsHandled();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		float step = (float)delta;
		bool controlsActive = Input.MouseMode == Input.MouseModeEnum.Captured;
		Vector2 input = controlsActive
			? Input.GetVector("move_left", "move_right", "move_forward", "move_back")
			: Vector2.Zero;

		// O pivô contém apenas o giro horizontal da câmera: olhar para cima/baixo
		// não altera a velocidade, e diagonais não são mais rápidas.
		Vector3 direction = _cameraPivot.GlobalBasis * new Vector3(input.X, 0.0f, input.Y);
		direction.Y = 0.0f;
		direction = direction.Normalized() * input.Length();

		Vector3 velocity = Velocity;
		Vector2 horizontal = new Vector2(velocity.X, velocity.Z).MoveToward(
			new Vector2(direction.X, direction.Z) * Speed, Acceleration * step);
		velocity.X = horizontal.X;
		velocity.Z = horizontal.Y;

		if (!IsOnFloor())
			velocity += GetGravity() * step;
		else if (controlsActive && Input.IsActionJustPressed("jump"))
			velocity.Y = JumpVelocity;
		else
			velocity.Y = 0.0f;

		Velocity = velocity;
		MoveAndSlide();

		if (!direction.IsZeroApprox())
		{
			Vector3 localDirection = GlobalBasis.Inverse() * direction;
			float targetYaw = Mathf.Atan2(-localDirection.X, -localDirection.Z);
			_visual.Rotation = new Vector3(0.0f,
				Mathf.LerpAngle(_visual.Rotation.Y, targetYaw,
					1.0f - Mathf.Exp(-VisualTurnSpeed * step)), 0.0f);
		}

		if (!Mathf.IsEqualApprox(_springArm.SpringLength, _targetZoom))
		{
			_springArm.SpringLength = Mathf.Lerp(_springArm.SpringLength, _targetZoom,
				1.0f - Mathf.Exp(-ZoomSmoothSpeed * step));
		}
	}

	public override void _Notification(int what)
	{
		if (what == NotificationApplicationFocusOut)
			Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	public override void _ExitTree()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}
}
