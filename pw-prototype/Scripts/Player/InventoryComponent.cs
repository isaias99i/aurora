using Godot;
using System.Collections.Generic;

/// <summary>
/// Gerenciará o inventário do personagem: slots, itens e equipamentos.
/// Estrutura inicial — sem lógica de gameplay ainda.
/// </summary>
public partial class InventoryComponent : Node
{
    [ExportGroup("Inventário")]
    [Export(PropertyHint.Range, "1,100,1")]
    public int MaxSlots { get; set; } = 30;

    // Lista de slots. O tipo Item será definido futuramente.
    private readonly List<object> _slots = new();

    // Sinais preparados para atualização de UI de inventário.
    [Signal] public delegate void ItemAddedEventHandler(int slotIndex);
    [Signal] public delegate void ItemRemovedEventHandler(int slotIndex);
    [Signal] public delegate void EquipmentChangedEventHandler(string slot);

    /// <summary>Adiciona um item ao inventário. Implementação futura.</summary>
    public void AddItem(object item) { /* TODO */ }

    /// <summary>Remove o item do slot indicado. Implementação futura.</summary>
    public void RemoveItem(int slotIndex) { /* TODO */ }

    /// <summary>Equipa o item do slot indicado. Implementação futura.</summary>
    public void EquipItem(int slotIndex) { /* TODO */ }
}
