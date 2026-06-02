using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Text;

namespace Framework;

public static class SpriteSheetTools
{
	/// <summary>
	/// Calculates the texture coordinates for a sprite inside a sprite sheet.
	/// </summary>
	/// <param name="spriteId">The sprite number. Starts with 0 in the upper left corner and increase in western reading direction up to #sprites - 1.</param>
	/// <param name="columns">Number of sprites per row.</param>
	/// <param name="rows">Number of sprites per column.</param>
	/// <returns>Texture2D coordinates for a single sprite</returns>
	public static Box2 CalcTexCoords(uint spriteId, uint columns, uint rows)
	{
		uint row = spriteId / columns;
		uint col = spriteId % columns;

		float x = col / (float)columns;
		float y = 1f - ((row + 1f) / rows);
		float width = 1f / columns;
		float height = 1f / rows;

		return new Box2(x, y, x + width, y + height);
	}

	/// <summary>
	/// Converts a string to a sequence of sprite ids.
	/// </summary>
	/// <param name="text"></param>
	/// <param name="firstCharacter">ASCII code of the first character in the bitmap font.</param>
	/// <returns>An <see cref="IEnumerable{uint}"/></returns>
	public static IEnumerable<uint> StringToSpriteIds(string text, uint firstCharacter)
	{
		byte[] asciiBytes = Encoding.ASCII.GetBytes(text);
		foreach (var asciiCharacter in asciiBytes)
		{
			yield return asciiCharacter - firstCharacter;
		}
	}
}
