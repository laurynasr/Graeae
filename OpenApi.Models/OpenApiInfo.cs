﻿using System.Text.Json;
using System.Text.Json.Nodes;

namespace OpenApi.Models;

public class OpenApiInfo
{
	private static readonly string[] KnownKeys =
	{
		"title",
		"summary",
		"description",
		"termsOfService",
		"contact",
		"license",
		"version"
	};

	public string Title { get; set; }
	public string? Summary { get; set; }
	public string? Description { get; set; }
	public string? TermsOfService { get; set; }
	public ContactInfo? Contact { get; set; }
	public LicenseInfo? License { get; set; }
	public string Version { get; set; }
	public ExtensionData? ExtensionData { get; set; }

	public static OpenApiInfo FromNode(JsonNode? node)
	{
		if (node is not JsonObject obj)
			throw new JsonException("Expected an object");

		var info = new OpenApiInfo
		{
			Title = obj.ExpectString("title", "open api info"),
			Summary = obj.MaybeString("summary", "open api info"),
			Description = obj.MaybeString("description", "open api info"),
			TermsOfService = obj.MaybeString("termsOfService", "open api info"),
			Contact = obj.Maybe("contact", ContactInfo.FromNode),
			License = obj.Maybe("license", LicenseInfo.FromNode),
			Version = obj.ExpectString("version", "open api info"),
			ExtensionData = ExtensionData.FromNode(obj)
		};

		obj.ValidateNoExtraKeys(KnownKeys, info.ExtensionData?.Keys);

		return info;
	}

	public static JsonNode? ToNode(OpenApiInfo? info)
	{
		if (info == null) return null;

		var obj = new JsonObject
		{
			["title"] = info.Title,
			["version"] = info.Version
		};

		obj.MaybeAdd("summary", info.Summary);
		obj.MaybeAdd("description", info.Summary);
		obj.MaybeAdd("termsOfService", info.Summary);
		obj.MaybeAdd("contact", ContactInfo.ToNode(info.Contact));
		obj.MaybeAdd("license", LicenseInfo.ToNode(info.License));
		obj.AddExtensions(info.ExtensionData);

		return obj;
	}
}