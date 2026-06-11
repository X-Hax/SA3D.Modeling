using J113D.Json;
using SA3D.Common;
using SA3D.Common.Lookup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.AnimationData
{
	/// <summary>
	/// List of keyframes with a label
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[JsonConverter(typeof(KeyframeArrayJsonConverterFactory))]
	public class KeyframeArray<T> : SortedDictionary<uint, T>, ILabel
	{
		private const string _labelPrefix = "keyframes_";

		/// <inheritdoc/>
		public string LabelPrefix => _labelPrefix;

		/// <inheritdoc/>
		public string Label { get; set; }

		/// <summary>
		/// Creates a new empty keyframe array with a custom label
		/// </summary>
		/// <param name="label">The label</param>
		public KeyframeArray(string label)
		{
			Label = label;
		}

		/// <summary>
		/// Creates a new empty keyframe array with a generated label
		/// </summary>
		public KeyframeArray() : this(_labelPrefix.GenerateIdentifier()) { }

		/// <summary>
		/// Creates a new Keyframe array with a custom label and a set of values
		/// </summary>
		/// <param name="label">Label to use</param>
		/// <param name="values">Values to use</param>
		public KeyframeArray(string label, IDictionary<uint, T> values) : base(values)
		{
			Label = label;
		}

		/// <summary>
		/// Creates a new Keyframe array with a generated label and a set of values
		/// </summary>
		/// <param name="values">Values to use</param>
		public KeyframeArray(IDictionary<uint, T> values) : this(_labelPrefix.GenerateIdentifier(), values) { }
	}

	internal class KeyframeArrayJsonConverterFactory : JsonConverterFactory
	{
		/// <inheritdoc/>
		public override bool CanConvert(Type typeToConvert)
		{
			return typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(KeyframeArray<>);
		}

		/// <inheritdoc/>
		public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		{
			Type elementType = typeToConvert.GetGenericArguments()[0];

			JsonConverter converter = (JsonConverter)Activator.CreateInstance(
				typeof(KeyframeArrayJsonConverter<>).MakeGenericType([elementType]),
				BindingFlags.Instance | BindingFlags.Public,
				binder: null,
				args: null,
				culture: null)!;

			return converter;
		}

		private class KeyframeArrayJsonConverter<T> : SimpleJsonObjectConverter<KeyframeArray<T>>
		{
			private const string _label = nameof(LabeledArray<>.Label);
			private const string _keyframes = "Keyframes";

			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _label, new(PropertyTokenType.String, "")},
				{ _keyframes, new(PropertyTokenType.Object, null) }
			});

			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				return propertyName switch
				{
					_label => reader.GetString(),
					_keyframes => JsonSerializer.Deserialize<SortedDictionary<uint, T>>(ref reader, options),
					_ => throw new InvalidPropertyException(),
				};
			}

			protected override KeyframeArray<T> Create(ReadOnlyDictionary<string, object?> values)
			{
				string label = (string)values[_label]!;
				SortedDictionary<uint, T> keyframes = (SortedDictionary<uint, T>?)values[_keyframes] ?? throw new InvalidDataException("Keyframe array is missing required property \"Keyframes\"!");

				return new(label, keyframes);
			}

			protected override void WriteValues(Utf8JsonWriter writer, KeyframeArray<T> value, JsonSerializerOptions options)
			{
				writer.WriteString(_label, value.Label);

				writer.WritePropertyName(_keyframes);
				JsonSerializer.Serialize(writer, (IDictionary<uint, T>)value, options);
			}
		}
	}
}
