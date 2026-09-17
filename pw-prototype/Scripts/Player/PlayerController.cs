using Godot;

/// <summary>
/// Controlador principal do personagem. Inicializa e coordena os componentes,
/// sem conter nenhuma lógica de gameplay.
/// </summary>
public partial class PlayerController : CharacterBody3D
{
    // Referências públicas para que sistemas externos possam acessar componentes.
    public MovementComponent Movement { get; private set; }
    public CameraComponent Camera { get; private set; }
    public StatsComponent Stats { get; private set; }
    public CombatComponent Combat { get; private set; }
    public InventoryComponent Inventory { get; private set; }
    public InteractionComponent Interaction { get; private set; }

    public override void _Ready()
    {
        ResolveComponents();
        InitializeComponents();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        Camera.HandleInput(@event);
    }

    public override void _PhysicsProcess(double delta)
    {
        float step = (float)delta;
        Movement.Process(step);
        Camera.ProcessZoom(step);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusOut)
            Camera.ReleaseMouse();
    }

    public override void _ExitTree()
    {
        Camera.ReleaseMouse();
    }

    // -------------------------------------------------------------------------
    // Inicialização privada
    // -------------------------------------------------------------------------

    private void ResolveComponents()
    {
        Movement = GetNode<MovementComponent>("MovementComponent");
        Camera = GetNode<CameraComponent>("CameraComponent");
        Stats = GetNode<StatsComponent>("StatsComponent");
        Combat = GetNode<CombatComponent>("CombatComponent");
        Inventory = GetNode<InventoryComponent>("InventoryComponent");
        Interaction = GetNode<InteractionComponent>("InteractionComponent");
    }

    private void InitializeComponents()
    {
        var visual = GetNode<Node3D>("Visual");
        var cameraPivot = GetNode<Node3D>("CameraPivot");
        var springArm = GetNode<SpringArm3D>("CameraPivot/SpringArm3D");

        // A câmera é inicializada primeiro para que o Movement possa receber a referência.
        Camera.Initialize(cameraPivot, springArm, GetRid());
        Movement.Initialize(this, visual, Camera);
    }
}
