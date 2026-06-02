using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Zenseless.OpenTK;

namespace Framework;

public static class Draw
{
	/// <summary>
	/// Draw a sprite with the given texture, bounds, and texture coordinates.
	/// </summary>
	/// <param name="texture"></param>
	/// <param name="bounds"></param>
	/// <param name="texCoord"></param>
	public static void Sprite(Texture2D texture, Box2 bounds, Box2 texCoord)
	{
		//Use the texture of the sprite for drawing
		GL.BindTexture(TextureTarget.Texture2D, texture.Handle);
		Rectangle(bounds, texCoord);
	}


	/// <summary>
	/// Draws a textured rectangle using the provided texture coordinates.
	/// </summary>
	/// <param name="rectangle">The geometrical coordinates of the rectangle.</param>
	/// <param name="texCoords">The texture coordinates of the rectangle.</param>
	public static void Rectangle(Box2 rectangle, Box2 texCoords)
	{
		GL.Begin(PrimitiveType.Quads);
		GL.TexCoord2(texCoords.Min);
		GL.Vertex2(rectangle.Min);
		GL.TexCoord2(texCoords.Max.X, texCoords.Min.Y);
		GL.Vertex2(rectangle.Max.X, rectangle.Min.Y);
		GL.TexCoord2(texCoords.Max);
		GL.Vertex2(rectangle.Max);
		GL.TexCoord2(texCoords.Min.X, texCoords.Max.Y);
		GL.Vertex2(rectangle.Min.X, rectangle.Max.Y);
		GL.End();
	}

	//public static Vector2[] CreateCoordinates(this IReadOnlyList<Box2> boxes)
	//{
	//	Vector2[] points = new Vector2[boxes.Count * 4];
	//	for (int i = 0; i < boxes.Count; ++i)
	//	{
	//		var box = boxes[i];
	//		var min = box.Min;
	//		var max = box.Max;
	//		var id = i * 4;
	//		points[id] = min;
	//		points[id + 1] = new Vector2(max.X, min.Y);
	//		points[id + 2] = box.Max;
	//		points[id + 3] = new Vector2(min.X, max.Y);
	//	}
	//	return points;
	//}

	//public static void SpriteBatch(Texture2D texture, Vector2[] coords, Vector2[] texCoord)
	//{
	//	//Use the texture of the sprite for drawing
	//	GL.BindTexture(TextureTarget.Texture2D, texture.Handle);
	//	int attrTex = 1;
	//	GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 0, coords);
	//	GL.VertexAttribPointer(attrTex, 2, VertexAttribPointerType.Float, false, 0, texCoord);
	//	GL.EnableVertexAttribArray(0);
	//	GL.EnableVertexAttribArray(attrTex);
	//	GL.DrawArrays(PrimitiveType.Quads, 0, coords.Length); // draw with vertex array data
	//	GL.DisableVertexAttribArray(attrTex);
	//	GL.DisableVertexAttribArray(0);
	//}
}
