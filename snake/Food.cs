using System.Numerics;
using Raylib_cs;

namespace Ape.RaylibGames.Snake;

public struct Food
{
    public Vector2 Position;
    public Vector2 Size;
    public bool Active;
    public Color Color;
}
