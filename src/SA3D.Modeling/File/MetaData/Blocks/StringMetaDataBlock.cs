using Amicitia.IO.Binary;
using J113D.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Meta data block containing a string
	/// </summary>
	public abstract class StringMetaDataBlock : MetaDataBlock
	{
		internal abstract class Base2JsonConverter<T> : ChildJsonObjectConverter<MetaDataBlockType, T, MetaDataBlock> where T : StringMetaDataBlock, new()
		{
			private const string _value = nameof(Value);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<MetaDataBlockType, MetaDataBlock> ParentConverter => BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _value, new(PropertyTokenType.String, null ) }
			});

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _value:
						return reader.GetString()!;
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override T CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				string stringValue = (string?)values[_value]
					?? throw new InvalidDataException($"{typeof(T).Name} requires a \"{_value}\" property");

				return new()
				{
					Value = stringValue
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
			{
				writer.WriteString(_value, value.Value);
			}
		}

		/// <summary>
		/// Metadata string value
		/// </summary>
		public string Value { get; set; } = string.Empty;

		/// <inheritdoc/>
		protected override void ReadContents(BinaryObjectReader reader)
		{
			Value = reader.ReadString(StringBinaryFormat.NullTerminated);
		}

		/// <inheritdoc/>
		protected override void WriteContents(BinaryObjectWriter writer)
		{
			writer.WriteString(StringBinaryFormat.NullTerminated, Value);
			writer.Align(4);
		}
	}
}
