using Godot;

/// <summary>
/// Gerenciará o sistema de combate: ataques, skills, dano e cooldowns.
/// Estrutura inicial — sem lógica de gameplay ainda.
/// </summary>
public partial class CombatComponent : Node
{
    [ExportGroup("Combate")]
    [Export(PropertyHint.Range, "0.5,10,0.5")]
    public float AttackRange { get; set; } = 2.0f;

    [Export(PropertyHint.Range, "0.1,10,0.1")]
    public float AttackCooldown { get; set; } = 1.0f;

    // Sinais preparados para feedback de UI e sistema de dano.
    [Signal] public delegate void AttackStartedEventHandler();
    [Signal] public delegate void AttackFinishedEventHandler();
    [Signal] public delegate void DamageDealtEventHandler(int amount, Node3D target);
    [Signal] public delegate void DamageReceivedEventHandler(int amount);

    /// <summary>Inicia um ataque básico. Implementação futura.</summary>
    public void Attack() { /* TODO */ }

    /// <summary>Usa uma skill pelo seu ID. Implementação futura.</summary>
    public void UseSkill(int skillId) { /* TODO */ }
}
