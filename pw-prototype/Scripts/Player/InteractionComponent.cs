using Godot;

/// <summary>
/// Gerenciará interações com o mundo: NPCs, baús e objetos interativos.
/// Estrutura inicial — sem lógica de gameplay ainda.
/// </summary>
public partial class InteractionComponent : Node
{
    [ExportGroup("Interação")]
    [Export(PropertyHint.Range, "0.5,10,0.5")]
    public float InteractionRange { get; set; } = 2.5f;

    // Sinal preparado para sistemas de diálogo e quests futuros.
    [Signal] public delegate void InteractionStartedEventHandler(Node3D target);
    [Signal] public delegate void InteractionEndedEventHandler();

    /// <summary>
    /// Tenta interagir com o objeto interativo mais próximo dentro do alcance.
    /// Implementação futura.
    /// </summary>
    public void TryInteract() { /* TODO */ }
}
