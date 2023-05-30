﻿using System.Text.Json;
using System.Text.Json.Nodes;

namespace OpenApi.Models;

public class OAuthFlowCollection
{
	private static readonly string[] KnownKeys =
	{
		"implicit",
		"password",
		"clientCredentials",
		"authorizationCode"
	};

	public OAuthFlow? Implicit { get; set; }
	public OAuthFlow? Password { get; set; }
	public OAuthFlow? ClientCredentials { get; set; }
	public OAuthFlow? AuthorizationCode { get; set; }
	public ExtensionData? ExtensionData { get; set; }

	public static OAuthFlowCollection FromNode(JsonNode? node)
	{
		if (node is not JsonObject obj)
			throw new JsonException("Expected an object");

		var flows = new OAuthFlowCollection
		{
			Implicit = obj.Maybe("implicit", OAuthFlow.FromNode),
			Password = obj.Maybe("password", OAuthFlow.FromNode),
			ClientCredentials = obj.Maybe("clientCredentials", OAuthFlow.FromNode),
			AuthorizationCode = obj.Maybe("authorizationCode", OAuthFlow.FromNode),
			ExtensionData = ExtensionData.FromNode(obj)
		};

		obj.ValidateNoExtraKeys(KnownKeys, flows.ExtensionData?.Keys);

		return flows;
	}

	public static JsonNode? ToNode(OAuthFlowCollection? flows)
	{
		if (flows == null) return null;

		var obj = new JsonObject();

		obj.MaybeAdd("implicit", OAuthFlow.ToNode(flows.Implicit));
		obj.MaybeAdd("password", OAuthFlow.ToNode(flows.Password));
		obj.MaybeAdd("clientCredentials", OAuthFlow.ToNode(flows.ClientCredentials));
		obj.MaybeAdd("authorizationCode", OAuthFlow.ToNode(flows.AuthorizationCode));
		obj.AddExtensions(flows.ExtensionData);

		return obj;
	}
}