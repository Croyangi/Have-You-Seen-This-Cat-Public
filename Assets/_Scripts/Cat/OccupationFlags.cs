[System.Flags] // This attribute allows you to treat the enum as a bitmask.
public enum OccupationFlags
{
    None = 0,          // 0000000, no flags set
    Body = 1 << 0,     // 0000001 (1), Body
    Eyes = 1 << 1,     // 0000010 (2), Eyes
    Head = 1 << 2,     // 0000100 (4), Head
    Legs = 1 << 3,     // 0001000 (8), Legs
    Pupils = 1 << 4,   // 0010000 (16), Pupils
    Tail = 1 << 5,     // 0100000 (32), Tail
    Whiskers = 1 << 6  // 1000000 (64), Whiskers
}