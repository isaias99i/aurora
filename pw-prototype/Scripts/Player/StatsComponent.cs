using Godot;

/// <summary>
/// Gerenciará os atributos RPG do personagem: Level, HP, MP e atributos base.
/// Estrutura inicial — sem lógica de gameplay ainda.
/// </summary>
public partial class StatsComponent : Node
{
    [ExportGroup("Progressão")]
    [Export] public int Level { get; set; } = 1;
    [Export] public int Experience { get; set; } = 0;
    [Export] public int ExperienceToNextLevel { get; set; } = 1000;

    [ExportGroup("Vida")]
    [Export] public int MaxHp { get; set; } = 100;
    [Export] public int CurrentHp { get; set; } = 100;

    [ExportGroup("Mana")]
    [Export] public int MaxMp { get; set; } = 50;
    [Export] public int CurrentMp { get; set; } = 50;

    [ExportGroup("Atributos")]
    [Export] public int Strength { get; set; } = 10;
    [Export] public int Defense { get; set; } = 10;
    [Export] public int Intelligence { get; set; } = 10;

    // Sinais preparados para os sistemas de combate e UI futuros.
    [Signal] public delegate void HealthChangedEventHandler(int currentHp, int maxHp);
    [Signal] public delegate void ManaChangedEventHandler(int currentMp, int maxMp);
    [Signal] public delegate void DiedEventHandler();
    [Signal] public delegate void LevelUpEventHandler(int newLevel);
}
