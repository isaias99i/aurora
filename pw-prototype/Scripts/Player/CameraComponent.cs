using Godot;

/// <summary>
/// Responsável pelo controle da câmera orbital em terceira pessoa: rotação
/// por mouse, zoom suave com scroll e limites de inclinação vertical.
/// </summary>
public partial class CameraComponent : Node
{
    [ExportGroup("Câmera")]
    // Radianos por pixel — não multiplicar por delta.
    [Export(PropertyHint.Range, "0.0001,0.02,0.0001")]
    public float MouseSensitivity { get; set; } = 0.003f;

    [Export(PropertyHint.Range, "1,20,0.5")]
    public float MinCameraDistance { get; set; } = 2.0f;

    [Export(PropertyHint.Range, "1,30,0.5")]
    public float MaxCameraDistance { get; set; } = 12.0f;

    [Export(PropertyHint.Range, "0.1,2,0.1")]
    public float ZoomStep { get; set; } = 0.5f;

    private const float ZoomSmoothSpeed = 12.0f;

    /// <summary>
    /// Pivot horizontal da câmera. Exposto para que o MovementComponent
    /// obtenha a base de orientação relativa à câmera.
    /// </summary>
    public Node3D CameraPivot { get; private set; }

    private SpringArm3D _springArm;
    private float _yaw;
    private float _pitch;
    private float _targetZoom;

    /// <summary>
    /// Chamado pelo PlayerController durante _Ready para injetar dependências.
    /// </summary>
    public void Initialize(Node3D cameraPivot, SpringArm3D springArm, Rid excludeRid)
    {
        CameraPivot = cameraPivot;
        _springArm = springArm;
        _yaw = CameraPivot.Rotation.Y;
        _pitch = _springArm.Rotation.X;
        _targetZoom = _springArm.SpringLength;

        // Exclui o corpo do jogador para a câmera não colidir consigo mesma.
        _springArm.AddExcludedObject(excludeRid);
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    /// <summary>
    /// Processa eventos de input: release/capture do mouse, scroll de zoom
    /// e rotação por movimentação do mouse. Chamado pelo PlayerController
    /// a partir de _UnhandledInput.
    /// </summary>
    public void HandleInput(InputEvent @event)
    {
        if (@event.IsActionPressed("release_mouse"))
        {
            ReleaseMouse();
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

        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed
            && TryHandleZoom(mouseButton))
            return;

        if (@event is InputEventMouseMotion motion
            && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            RotateCamera(motion.ScreenRelative);
            GetViewport().SetInputAsHandled();
        }
    }

    /// <summary>
    /// Interpola suavemente o SpringArm em direção ao zoom alvo.
    /// Chamado pelo PlayerController a cada _PhysicsProcess.
    /// </summary>
    public void ProcessZoom(float delta)
    {
        if (Mathf.IsEqualApprox(_springArm.SpringLength, _targetZoom)) return;

        _springArm.SpringLength = Mathf.Lerp(
            _springArm.SpringLength, _targetZoom,
            1.0f - Mathf.Exp(-ZoomSmoothSpeed * delta));
    }

    /// <summary>Libera o cursor do mouse e suspende os controles.</summary>
    public void ReleaseMouse()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    // -------------------------------------------------------------------------
    // Métodos privados de câmera
    // -------------------------------------------------------------------------

    private bool TryHandleZoom(InputEventMouseButton mouseButton)
    {
        if (mouseButton.ButtonIndex == MouseButton.WheelUp)
        {
            _targetZoom = Mathf.Clamp(_targetZoom - ZoomStep, MinCameraDistance, MaxCameraDistance);
            GetViewport().SetInputAsHandled();
            return true;
        }

        if (mouseButton.ButtonIndex == MouseButton.WheelDown)
        {
            _targetZoom = Mathf.Clamp(_targetZoom + ZoomStep, MinCameraDistance, MaxCameraDistance);
            GetViewport().SetInputAsHandled();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Aplica delta de mouse ao yaw/pitch e atualiza as rotações dos nós.
    /// O yaw vive no CameraPivot (horizontal); o pitch vive no SpringArm3D (vertical),
    /// clampado entre −65° e 35°.
    /// </summary>
    private void RotateCamera(Vector2 mouseDelta)
    {
        _yaw = Mathf.Wrap(_yaw - mouseDelta.X * MouseSensitivity, -Mathf.Pi, Mathf.Pi);
        _pitch = Mathf.Clamp(
            _pitch - mouseDelta.Y * MouseSensitivity,
            Mathf.DegToRad(-65.0f),
            Mathf.DegToRad(35.0f));

        CameraPivot.Rotation = new Vector3(0.0f, _yaw, 0.0f);
        _springArm.Rotation = new Vector3(_pitch, 0.0f, 0.0f);
    }
}
