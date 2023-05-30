﻿using System.Text.Json;
using System.Text.Json.Nodes;

namespace OpenApi.Models;

public class OAuthFlow
{
	private static readonly string[] KnownKeys =
	{
		"authorizationUrl",
		"tokenUrl",
		"refreshUrl",
		"scopes"
	};

	public Uri AuthorizationUrl { get; set; }
	public Uri TokenUrl { get; set; }
	public Uri? RefreshUrl { get; set; }
	public Dictionary<string, string> Scopes { get; set; }
	public ExtensionData? ExtensionData { get; set; }

	public static OAuthFlow FromNode(JsonNode? node)
	{
		if (node is not JsonObject obj)
			throw new JsonException("Expected an object");

		var flow = new OAuthFlow
		{
			AuthorizationUrl = obj.ExpectUri("authorizationUrl", "oauth flow"),
			TokenUrl = obj.ExpectUri("tokenUrl", "oauth flow"),
			RefreshUrl = obj.MaybeUri("refreshUrl", "oauth flow"),
			Scopes = obj.ExpectMap("scopes", "oauth flow", x => x is JsonValue v && v.TryGetValue(out string? s) ? s : throw new JsonException("scopes must be strings")),
			ExtensionData = ExtensionData.FromNode(obj)
		};

		obj.ValidateNoExtraKeys(KnownKeys, flow.ExtensionData?.Keys);

		return flow;
	}

	public static JsonNode? ToNode(OAuthFlow? flow)
	{
		if (flow == null) return null;

		var obj = new JsonObject
		{
			["authorizationUrl"] = flow.AuthorizationUrl.ToString(),
			["tokenUrl"] = flow.TokenUrl.ToString()
		};

		var scopes = new JsonObject();
		foreach (var (key, value) in flow.Scopes)
		{
			scopes.Add(key, value);
		}
		obj.Add("scopes", scopes);

		obj.MaybeAdd("refreshUrl", flow.RefreshUrl?.ToString());
		obj.AddExtensions(flow.ExtensionData);

		return obj;
	}
}