using Godot;

/// <summary>
/// Responsável pela movimentação do personagem: WASD relativo à câmera,
/// gravidade, pulo, giro suave do mesh visual, aceleração e desaceleração independentes.
/// </summary>
public partial class MovementComponent : Node
{
    [ExportGroup("Velocidade")]
    [Export(PropertyHint.Range, "0.1,20,0.1")]
    public float Speed { get; set; } = 6.0f;

    /// <summary>Taxa de aceleração ao iniciar o movimento.</summary>
    [Export(PropertyHint.Range, "1,200,0.5")]
    public float Acceleration { get; set; } = 30.0f;

    /// <summary>Taxa de frenagem ao soltar as teclas. Independente da aceleração.</summary>
    [Export(PropertyHint.Range, "1,200,0.5")]
    public float Deceleration { get; set; } = 45.0f;

    [ExportGroup("Pulo")]
    [Export(PropertyHint.Range, "0.1,15,0.1")]
    public float JumpVelocity { get; set; } = 6.0f;

    [ExportGroup("Rotação")]
    /// <summary>Velocidade com que o mesh visual gira para a direção de movimento.</summary>
    [Export(PropertyHint.Range, "1,30,0.5")]
    public float RotationSpeed { get; set; } = 12.0f;

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
        Vector2 input = Input.GetVector("MoveLeft", "MoveRight", "MoveForward", "MoveBackward");

        Vector3 direction = ComputeDirection(input);
        ApplyHorizontalVelocity(direction, delta);
        ApplyVerticalVelocity(delta);

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

    /// <summary>
    /// Aplica aceleração ao iniciar o movimento e desaceleração ao soltar as teclas.
    /// Usa <see cref="Acceleration"/> ao avançar e <see cref="Deceleration"/> ao frear.
    /// </summary>
    private void ApplyHorizontalVelocity(Vector3 direction, float delta)
    {
        Vector3 velocity = _body.Velocity;
        Vector2 current = new Vector2(velocity.X, velocity.Z);
        Vector2 target = new Vector2(direction.X, direction.Z) * Speed;

        float rate = direction.IsZeroApprox() ? Deceleration : Acceleration;
        Vector2 horizontal = current.MoveToward(target, rate * delta);

        velocity.X = horizontal.X;
        velocity.Z = horizontal.Y;
        _body.Velocity = velocity;
    }

    private void ApplyVerticalVelocity(float delta)
    {
        Vector3 velocity = _body.Velocity;

        if (!_body.IsOnFloor())
            velocity += _body.GetGravity() * delta;
        else if (Input.IsActionJustPressed("jump"))
            velocity.Y = JumpVelocity;
        else
            velocity.Y = 0.0f;

        _body.Velocity = velocity;
    }

    /// <summary>
    /// Gira suavemente o mesh do personagem na direção do deslocamento usando
    /// interpolação angular com decaimento exponencial independente de framerate.
    /// </summary>
    private void RotateVisual(Vector3 direction, float delta)
    {
        if (direction.IsZeroApprox()) return;

        Vector3 localDir = _body.GlobalBasis.Inverse() * direction;
        float targetYaw = Mathf.Atan2(-localDir.X, -localDir.Z);
        _visual.Rotation = new Vector3(0.0f,
            Mathf.LerpAngle(_visual.Rotation.Y, targetYaw,
                1.0f - Mathf.Exp(-RotationSpeed * delta)), 0.0f);
    }
}
