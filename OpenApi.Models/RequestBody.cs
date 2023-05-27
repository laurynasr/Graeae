﻿using System.Text.Json;
using System.Text.Json.Nodes;

namespace OpenApi.Models;

public class RequestBody
{
	private static readonly string[] KnownKeys =
	{
		"description",
		"content",
		"required"
	};

	public string? Description { get; set; }
	public Dictionary<string, MediaType> Content { get; set; }
	public bool? Required { get; set; }
	public ExtensionData? ExtensionData { get; set; }

	public static RequestBody FromNode(JsonNode? node, JsonSerializerOptions? options)
	{
		if (node is not JsonObject obj)
			throw new JsonException("Expected an object");

		if (obj.ContainsKey("$ref"))
		{
			var link = new RequestBodyRef(obj.ExpectUri("$ref", "reference"))
			{
				Description = obj.MaybeString("description", "reference"),
				Summary = obj.MaybeString("summary", "reference")
			};

			obj.ValidateReferenceKeys();

			return link;
		}
		else
		{
			var link = new RequestBody
			{
				Description = obj.ExpectString("description", "request body"),
				Content = obj.ExpectMap("content", "request body", x => MediaType.FromNode(x, options)),
				Required = obj.MaybeBool("required", "request body"),
				ExtensionData = ExtensionData.FromNode(obj)
			};

			obj.ValidateNoExtraKeys(KnownKeys, link.ExtensionData?.Keys);

			return link;
		}
	}
}

public class RequestBodyRef : RequestBody
{
	public Uri Ref { get; }
	public string? Summary { get; set; }
	public new string? Description { get; set; }

	public bool IsResolved { get; private set; }

	public RequestBodyRef(Uri reference)
	{
		Ref = reference ?? throw new ArgumentNullException(nameof(reference));
	}

	public void Resolve()
	{
		// resolve the $ref and set all of the props
		// remember to use base.*

		IsResolved = true;
	}
}