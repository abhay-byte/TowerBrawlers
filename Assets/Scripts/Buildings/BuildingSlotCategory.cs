using System;

[Flags]
public enum BuildingSlotCategory
{
    None = 0,
    Building = 1 << 0,
    Tower = 1 << 1,
    Any = Building | Tower
}
