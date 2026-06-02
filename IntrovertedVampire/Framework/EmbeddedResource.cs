using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Zenseless.OpenTK;

namespace Framework;

/// <summary>
/// Singleton class that handles embedded resource loading <seealso cref="https://go.microsoft.com/fwlink/?LinkId=204554"/>
/// </summary>
public static class EmbeddedResource
{
	/// <summary>
	/// Enumerates all embedded resources.
	/// </summary>
	/// <returns>A list of resource names</returns>
	public static IEnumerable<string> EnumerateResources() => _resourceNames;

	/// <summary>
	/// Load a texture out of the given embedded resource.
	/// </summary>
	/// <param name="name">The name of the resource that contains an image.</param>
	/// <returns>A Texture2D.</returns>
	public static Texture2D LoadTexture(string name)
	{
		using var stream = LoadResource(name);
		return stream.LoadTexture();
	}

	/// <summary>
	/// Open a stream from the given embedded resource name.
	/// </summary>
	/// <param name="name">Resource name resolved via shortest match</param>
	/// <returns>A stream</returns>
	public static Stream LoadResource(string name)
	{
		var stream = _assembly.GetManifestResourceStream(ShortestMatchName(name));
		if (stream is null)
		{
			var names = string.Join('\n', _resourceNames);
			throw new ArgumentException($"Could not find resource '{name}' in resources\n'{names}'");
		}
		return stream;
	}

	private static readonly Assembly _assembly = Assembly.GetEntryAssembly() ?? throw new ApplicationException("No entry assembly. Are you calling the code from an unmanaged source?");
	private static readonly HashSet<string> _resourceNames = [.. _assembly.GetManifestResourceNames()];

	/// <summary>
	/// Find the shortest resource name match for a given name.
	/// </summary>
	/// <param name="name">The name to search matches for.</param>
	/// <returns></returns>
	private static string ShortestMatchName(string name)
	{
		var matches = _resourceNames.Where(fullName => fullName.Contains(name, StringComparison.InvariantCultureIgnoreCase));
		if (!matches.Any()) throw new ArgumentException($"No matching resource for '{name}'");
		var shortest = matches.Aggregate((s, best) => s.Length < best.Length ? s : best);
		return shortest;
	}
}
