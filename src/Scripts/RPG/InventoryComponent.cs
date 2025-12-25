using Godot;
using System;
using System.Collections.Generic;

public partial class InventoryComponent : Node
{
    [Export] public int Capacity = 36;

    private readonly List<ItemStack> _items = new();
    public IReadOnlyList<ItemStack> Items => _items;

    public event Action? OnChanged;

    public bool Add(ItemDef def, int qty)
    {
        if (qty <= 0) return false;

        // stack into existing
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].Def == def && _items[i].Qty < def.MaxStack)
            {
                int canAdd = Math.Min(qty, def.MaxStack - _items[i].Qty);
                _items[i] = new ItemStack(def, _items[i].Qty + canAdd);
                qty -= canAdd;
                if (qty == 0) { OnChanged?.Invoke(); return true; }
            }
        }

        while (qty > 0)
        {
            if (_items.Count >= Capacity) { OnChanged?.Invoke(); return false; }
            int take = Math.Min(qty, def.MaxStack);
            _items.Add(new ItemStack(def, take));
            qty -= take;
        }

        OnChanged?.Invoke();
        return true;
    }

    public bool Remove(ItemDef def, int qty)
    {
        for (int i = _items.Count - 1; i >= 0 && qty > 0; i--)
        {
            if (_items[i].Def != def) continue;
            int take = Math.Min(qty, _items[i].Qty);
            int left = _items[i].Qty - take;
            qty -= take;

            if (left <= 0) _items.RemoveAt(i);
            else _items[i] = new ItemStack(def, left);
        }

        OnChanged?.Invoke();
        return qty == 0;
    }
}
