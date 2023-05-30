﻿using System.Text.Json;
using System.Text.Json.Nodes;

namespace OpenApi.Models;

public class ServerVariable
{
	private static readonly string[] KnownKeys =
	{
		"enum",
		"default",
		"description"
	};

	public IEnumerable<string>? Enum { get; set; }
	public string Default { get; set; }
	public string? Description { get; set; }
	public ExtensionData? ExtensionData { get; set; }

	public static ServerVariable FromNode(JsonNode? node)
	{
		if (node is not JsonObject obj)
			throw new JsonException("Expected an object");

		var vars = new ServerVariable
		{
			Enum = obj.MaybeArray("enum", x => x is JsonValue v && v.TryGetValue(out string? s) ? s : throw new JsonException("`enum` values must be strings")),
			Default = obj.ExpectString("default", "server variable"),
			Description = obj.MaybeString("description", "server variable"),
			ExtensionData = ExtensionData.FromNode(obj)
		};

		obj.ValidateNoExtraKeys(KnownKeys, vars.ExtensionData?.Keys);

		return vars;
	}

	public static JsonNode? ToNode(ServerVariable? variable, JsonSerializerOptions? options)
	{
		if (variable == null) return null;

		var obj = new JsonObject
		{
			["default"] = variable.Default
		};

		obj.MaybeAddArray("enum", variable.Enum, x => x);
		obj.MaybeAdd("description", variable.Description);
		obj.AddExtensions(variable.ExtensionData);

		return obj;
	}
}