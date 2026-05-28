using System;

namespace breaking_worse.Objects.Scene;

[Flags]
public enum Transition
{
    None = 0,
    Flip_H = 1 << 0,
    Flip_V = 1 << 1,
    Flip_D = 1 << 2,

    Rotate_90 = Flip_D | Flip_H,
    Rotate_180 = Flip_H | Flip_V,
    Rotate_270 = Flip_V | Flip_D,

    Rotate_90AndFlip_H = Flip_H | Flip_V | Flip_D,
}