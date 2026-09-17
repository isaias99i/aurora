using Godot;

/// <summary>
/// Responsável pela movimentação do personagem: WASD relativo à câmera,
/// gravidade, pulo e giro suave do mesh visual.
/// </summary>
public partial class MovementComponent : Node
{
    [ExportGroup("Movimento")]
    [Export(PropertyHint.Range, "0.1,20,0.1")]
    public float Speed { get; set; } = 6.0f;

    [Export(PropertyHint.Range, "1,100,0.5")]
    public float Acceleration { get; set; } = 30.0f;

    [Export(PropertyHint.Range, "0.1,15,0.1")]
    public float JumpVelocity { get; set; } = 6.0f;

    private const float VisualTurnSpeed = 12.0f;

    private CharacterBody3D _body;
    private Node3D _visual;
    private CameraComponent _camera;

    /// <summary>
    /// Chamado pelo PlayerController durante _Ready para injetar dependências.
    /// </summary>
    public void Initialize(CharacterBody3D body, Node3D visual, CameraComponent camera)
    {
        _body = body;
        _visual = visual;
        _camera = camera;
    }

    /// <summary>
    /// Executa o ciclo completo de movimentação. Chamado pelo PlayerController
    /// a cada _PhysicsProcess.
    /// </summary>
    public void Process(float delta)
    {
        bool controlsActive = Input.MouseMode == Input.MouseModeEnum.Captured;
        Vector2 input = controlsActive
            ? Input.GetVector("move_left", "move_right", "move_forward", "move_back")
            : Vector2.Zero;

        Vector3 direction = ComputeDirection(input);
        ApplyHorizontalVelocity(direction, delta);
        ApplyVerticalVelocity(controlsActive, delta);

        _body.MoveAndSlide();
        RotateVisual(direction, delta);
    }

    // -------------------------------------------------------------------------
    // Métodos privados de movimento
    // -------------------------------------------------------------------------

    /// <summary>
    /// Projeta o input no plano horizontal da câmera. Olhar para cima/baixo
    /// não afeta a velocidade e diagonais não geram velocidade extra.
    /// </summary>
    private Vector3 ComputeDirection(Vector2 input)
    {
        Vector3 dir = _camera.CameraPivot.GlobalBasis * new Vector3(input.X, 0.0f, input.Y);
        dir.Y = 0.0f;
        return dir.Normalized() * input.Length();
    }

    private void ApplyHorizontalVelocity(Vector3 direction, float delta)
    {
        Vector3 velocity = _body.Velocity;
        Vector2 horizontal = new Vector2(velocity.X, velocity.Z).MoveToward(
            new Vector2(direction.X, direction.Z) * Speed, Acceleration * delta);
        velocity.X = horizontal.X;
        velocity.Z = horizontal.Y;
        _body.Velocity = velocity;
    }

    private void ApplyVerticalVelocity(bool controlsActive, float delta)
    {
        Vector3 velocity = _body.Velocity;

        if (!_body.IsOnFloor())
            velocity += _body.GetGravity() * delta;
        else if (controlsActive && Input.IsActionJustPressed("jump"))
            velocity.Y = JumpVelocity;
        else
            velocity.Y = 0.0f;

        _body.Velocity = velocity;
    }

    /// <summary>
    /// Gira suavemente o mesh do personagem na direção do deslocamento.
    /// </summary>
    private void RotateVisual(Vector3 direction, float delta)
    {
        if (direction.IsZeroApprox()) return;

        Vector3 localDir = _body.GlobalBasis.Inverse() * direction;
        float targetYaw = Mathf.Atan2(-localDir.X, -localDir.Z);
        _visual.Rotation = new Vector3(0.0f,
            Mathf.LerpAngle(_visual.Rotation.Y, targetYaw,
                1.0f - Mathf.Exp(-VisualTurnSpeed * delta)), 0.0f);
    }
}
