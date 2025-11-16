using Microsoft.Maui.Graphics;

namespace Stok73.Models;

public sealed record HomeMenuItem(
    string Id,
    string Title,
    string Glyph,
    Color AccentColor,
    string Route
);
