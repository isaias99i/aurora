using Godot;

/// <summary>
/// Responsável pelo controle da câmera orbital em terceira pessoa: rotação por mouse,
/// zoom suave com scroll, limites de inclinação configuráveis e colisão automática
/// via SpringArm3D.
/// </summary>
public partial class CameraComponent : Node
{
    [ExportGroup("Câmera — Rotação")]
    /// <summary>
    /// Sensibilidade do mouse. Escala amigável de 0.01 a 1.0 — padrão recomendado: 0.2.
    /// Internamente convertida para rad/px via SensitivityScale.
    /// </summary>
    [Export(PropertyHint.Range, "0.01,1.0,0.01")]
    public float MouseSensitivity { get; set; } = 0.2f;

    /// <summary>Limite mínimo de inclinação vertical (graus, valor negativo = câmera baixa).</summary>
    [Export(PropertyHint.Range, "-89,-1,1")]
    public float VerticalAngleMin { get; set; } = -65.0f;

    /// <summary>Limite máximo de inclinação vertical (graus, câmera alta).</summary>
    [Export(PropertyHint.Range, "1,89,1")]
    public float VerticalAngleMax { get; set; } = 35.0f;

    [ExportGroup("Câmera — Zoom")]
    [Export(PropertyHint.Range, "1,20,0.5")]
    public float MinCameraDistance { get; set; } = 2.0f;

    [Export(PropertyHint.Range, "1,30,0.5")]
    public float MaxCameraDistance { get; set; } = 10.0f;

    [Export(PropertyHint.Range, "0.1,3,0.1")]
    public float ZoomStep { get; set; } = 0.5f;

    /// <summary>Velocidade de interpolação do zoom. Maior = zoom mais rápido.</summary>
    [Export(PropertyHint.Range, "1,30,1")]
    public float ZoomSmoothSpeed { get; set; } = 10.0f;

    // Converte a sensibilidade (0–1) para rad/px.
    // 0.2 * 0.015 = 0.003 rad/px — mesmo valor calibrado da versão anterior.
    private const float SensitivityScale = 0.015f;

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
    /// Processa eventos de input: release/capture do mouse, zoom (CameraZoomIn/Out)
    /// e rotação por movimentação do mouse. Chamado pelo PlayerController via _UnhandledInput.
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

        if (TryHandleZoom(@event)) return;

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

    /// <summary>
    /// Verifica e processa zoom via input actions CameraZoomIn / CameraZoomOut.
    /// Retorna true se o evento foi consumido.
    /// </summary>
    private bool TryHandleZoom(InputEvent @event)
    {
        if (@event.IsActionPressed("CameraZoomIn"))
        {
            _targetZoom = Mathf.Clamp(_targetZoom - ZoomStep, MinCameraDistance, MaxCameraDistance);
            GetViewport().SetInputAsHandled();
            return true;
        }

        if (@event.IsActionPressed("CameraZoomOut"))
        {
            _targetZoom = Mathf.Clamp(_targetZoom + ZoomStep, MinCameraDistance, MaxCameraDistance);
            GetViewport().SetInputAsHandled();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Aplica delta de mouse ao yaw/pitch e atualiza as rotações dos nós.
    /// Yaw: CameraPivot (horizontal) — Pitch: SpringArm3D (vertical, clampado).
    /// </summary>
    private void RotateCamera(Vector2 mouseDelta)
    {
        float sensitivity = MouseSensitivity * SensitivityScale;

        _yaw = Mathf.Wrap(_yaw - mouseDelta.X * sensitivity, -Mathf.Pi, Mathf.Pi);
        _pitch = Mathf.Clamp(
            _pitch - mouseDelta.Y * sensitivity,
            Mathf.DegToRad(VerticalAngleMin),
            Mathf.DegToRad(VerticalAngleMax));

        CameraPivot.Rotation = new Vector3(0.0f, _yaw, 0.0f);
        _springArm.Rotation = new Vector3(_pitch, 0.0f, 0.0f);
    }
}
