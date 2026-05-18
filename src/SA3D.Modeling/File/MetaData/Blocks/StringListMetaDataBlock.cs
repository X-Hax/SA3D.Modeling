using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using J113D.Json;
using SA3D.Common.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Meta data block containing a list of strings
	/// </summary>
	public abstract class StringListMetaDataBlock : MetaDataBlock
	{
		internal abstract class Base2JsonConverter<T> : ChildJsonObjectConverter<MetaDataBlockType, T, MetaDataBlock> where T : StringListMetaDataBlock, new()
		{
			private const string _values = nameof(Values);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<MetaDataBlockType, MetaDataBlock> ParentConverter => BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _values, new(PropertyTokenType.Array, null ) }
			});

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _values:
						return JsonSerializer.Deserialize<List<string>>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override T CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				List<string> stringValues = (List<string>?)values[_values]
					?? throw new InvalidDataException($"{typeof(T).Name} requires a \"{_values}\" property");

				return new()
				{
					Values = stringValues
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_values);
				JsonSerializer.Serialize(writer, value.Values, options);
			}
		}

		/// <summary>
		/// Metadata string  values
		/// </summary>
		public List<string> Values { get; set; } = [];

		/// <inheritdoc/>
		protected override void ReadContents(BinaryObjectReader reader)
		{
			long offset = reader.ReadOffsetValue();
			while(offset != uint.MaxValue)
			{
				Values.Add(reader.ReadStringAtOffsetOrEmpty(offset));
				offset = reader.ReadOffsetValue();
			}
		}

		/// <inheritdoc/>
		protected override void WriteContents(BinaryObjectWriter writer)
		{
			SeekToken start = writer.At();
			writer.Skip(Values.Count * sizeof(uint));
			writer.WriteUInt32(uint.MaxValue);

			uint[] offsets = new uint[Values.Count];
			for(int i = 0; i < offsets.Length; i++)
			{
				offsets[i] = (uint)writer.GetPositionOffset();
				writer.WriteString(StringBinaryFormat.NullTerminated, Values[i]);
				writer.Align(4);
			}

			using(writer.At())
			{
				start.Dispose();
				writer.WriteArray(offsets);
			}
		}
	}
}
