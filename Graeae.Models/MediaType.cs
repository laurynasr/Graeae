﻿using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Schema;

namespace Graeae.Models;

/// <summary>
/// Models a media type object.
/// </summary>
[JsonConverter(typeof(MediaTypeJsonConverter))]
public class MediaType : IRefTargetContainer
{
	private static readonly string[] KnownKeys =
	{
		"schema",
		"example",
		"examples",
		"encoding"
	};

	/// <summary>
	/// Gets or sets a schema for the meta type.
	/// </summary>
	public JsonSchema? Schema { get; set; }
	/// <summary>
	/// Gets or sets an example.
	/// </summary>
	public JsonNode? Example { get; set; }
	/// <summary>
	/// Gets or sets a collection of examples.
	/// </summary>
	public Dictionary<string, Example>? Examples { get; set; }
	/// <summary>
	/// Gets or sets a collection of encodings.
	/// </summary>
	public Dictionary<string, Encoding>? Encoding { get; set; }
	/// <summary>
	/// Gets or set extension data.
	/// </summary>
	public ExtensionData? ExtensionData { get; set; }

	internal static MediaType FromNode(JsonNode? node, JsonSerializerOptions? options)
	{
		if (node is not JsonObject obj)
			throw new JsonException("Expected an object");

		var mediaType = new MediaType
		{
			Schema = obj.MaybeDeserialize<JsonSchema>("schema", options),
			Example = obj.TryGetPropertyValue("example", out var v) ? v : null,
			Examples = obj.MaybeMap("examples", Models.Example.FromNode),
			Encoding = obj.MaybeMap("encoding", x => Models.Encoding.FromNode(x, options)),
			ExtensionData = ExtensionData.FromNode(obj)
		};

		obj.ValidateNoExtraKeys(KnownKeys, mediaType.ExtensionData?.Keys);

		return mediaType;
	}

	internal static JsonNode? ToNode(MediaType? mediaType, JsonSerializerOptions? options)
	{
		if (mediaType == null) return null;

		var obj = new JsonObject();

		obj.MaybeSerialize("schema", mediaType.Schema, options);
		obj.MaybeAdd("example", mediaType.Example?.DeepClone());
		obj.MaybeAddMap("examples", mediaType.Examples, Models.Example.ToNode);
		obj.MaybeAddMap("encoding", mediaType.Encoding, x => Models.Encoding.ToNode(x, options));
		obj.AddExtensions(mediaType.ExtensionData);

		return obj;
	}

	object? IRefTargetContainer.Resolve(ReadOnlySpan<string> keys)
	{
		if (keys.Length == 0) return this;

		int keysConsumed = 1;
		IRefTargetContainer? target = null;
		switch (keys[0])
		{
			case "schema":
				if (Schema == null) return null;
				if (keys.Length == 1) return Schema;
				// TODO: consider some other kind of value being buried in a schema
				throw new NotImplementedException();
			case "example":
				return Example?.GetFromNode(keys.Slice(1));
			case "examples":
				if (keys.Length == 1) return null;
				keysConsumed++;
				target = Examples?.GetFromMap(keys[1]);
				break;
			case "encoding":
				if (keys.Length == 1) return null;
				keysConsumed++;
				target = Encoding?.GetFromMap(keys[1]);
				break;
		}

		return target != null
			? target.Resolve(keys.Slice(keysConsumed))
			: ExtensionData?.Resolve(keys);
	}

	internal IEnumerable<JsonSchema> FindSchemas()
	{
		if (Schema != null)
			yield return Schema;

		var theRest = GeneralHelpers.Collect(Encoding?.Values.SelectMany(x => x.FindSchemas()));

		foreach (var schema in theRest)
		{
			yield return schema;
		}
	}

	internal IEnumerable<IComponentRef> FindRefs()
	{
		return GeneralHelpers.Collect(
			Examples?.Values.SelectMany(x => x.FindRefs()),
			Encoding?.Values.SelectMany(x => x.FindRefs())
		);
	}
}

internal class MediaTypeJsonConverter : JsonConverter<MediaType>
{
	public override MediaType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var obj = JsonSerializer.Deserialize<JsonObject>(ref reader, options) ??
		          throw new JsonException("Expected an object");

		return MediaType.FromNode(obj, options);
	}

	public override void Write(Utf8JsonWriter writer, MediaType value, JsonSerializerOptions options)
	{
		var json = MediaType.ToNode(value, options);

		JsonSerializer.Serialize(writer, json, options);
	}
}
