﻿using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;
using Json.Schema;

namespace OpenApi.Models.SchemaDraft4;

[SchemaKeyword(Name)]
[SchemaSpecVersion(Draft4Support.Draft4Version)]
[SchemaSpecVersion(SpecVersion.Draft202012)]
[DependsOnAnnotationsFrom(typeof(MinimumKeyword))]
[JsonConverter(typeof(Draft4ExclusiveMaximumKeywordJsonConverter))]
public class Draft4ExclusiveMaximumKeyword : IJsonSchemaKeyword
{
	public const string Name = "exclusiveMaximum";

	private readonly ExclusiveMaximumKeyword? _numberSupport;

	/// <summary>
	/// The ID.
	/// </summary>
	public bool? BoolValue { get; }

	public decimal? NumberValue => _numberSupport?.Value;

	/// <summary>
	/// Creates a new <see cref="IdKeyword"/>.
	/// </summary>
	/// <param name="value">Whether the `minimum` value should be considered exclusive.</param>
	public Draft4ExclusiveMaximumKeyword(bool value)
	{
		BoolValue = value;
	}

	public Draft4ExclusiveMaximumKeyword(decimal value)
	{
		_numberSupport = new ExclusiveMaximumKeyword(value);
	}

	public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint, IReadOnlyList<KeywordConstraint> localConstraints, EvaluationContext context)
	{
		if (BoolValue.HasValue)
		{
			if (!BoolValue.Value) return KeywordConstraint.Skip;

			var maximumConstraint = localConstraints.SingleOrDefault(x => x.Keyword == MaximumKeyword.Name);
			if (maximumConstraint == null) return KeywordConstraint.Skip;

			var localSchema = schemaConstraint.GetLocalSchema(context.Options);

			var value = localSchema.GetMaximum()!.Value;
			return new KeywordConstraint(Name, (e, c) => Evaluator(e, c, value))
			{
				SiblingDependencies = new[] { maximumConstraint }
			};
		}

		return _numberSupport!.GetConstraint(schemaConstraint, localConstraints, context);
	}

	private void Evaluator(KeywordEvaluation evaluation, EvaluationContext context, decimal limit)
	{
		var schemaValueType = evaluation.LocalInstance.GetSchemaValueType();
		if (schemaValueType is not (SchemaValueType.Number or SchemaValueType.Integer))
		{
			evaluation.MarkAsSkipped();
			return;
		}

		var number = evaluation.LocalInstance!.AsValue().GetNumber();

		if (number >= limit)
			evaluation.Results.Fail(Name, ErrorMessages.GetExclusiveMaximum(context.Options.Culture), ("received", number), ("limit", BoolValue));
	}
}

internal class Draft4ExclusiveMaximumKeywordJsonConverter : JsonConverter<Draft4ExclusiveMaximumKeyword>
{
	public override Draft4ExclusiveMaximumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return reader.TokenType switch
		{
			JsonTokenType.True or JsonTokenType.False => new Draft4ExclusiveMaximumKeyword(reader.GetBoolean()),
			JsonTokenType.Number => new Draft4ExclusiveMaximumKeyword(reader.GetDecimal()),
			_ => throw new JsonException("Expected boolean or number")
		};
	}

	public override void Write(Utf8JsonWriter writer, Draft4ExclusiveMaximumKeyword value, JsonSerializerOptions options)
	{
		if (value.BoolValue.HasValue)
		{
			writer.WriteBoolean(Draft4ExclusiveMaximumKeyword.Name, value.BoolValue.Value);
		}
		else
		{
			writer.WriteNumber(Draft4ExclusiveMaximumKeyword.Name, value.NumberValue!.Value);
		}
	}
}