﻿using System;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raven.Json.Utilities;

namespace Raven.Json.Linq
{
    public abstract class RavenJToken
    {
        /// <summary>
        /// Gets the node type for this <see cref="RavenJToken"/>.
        /// </summary>
        /// <value>The type.</value>
        public abstract JTokenType Type { get; }

        /// <summary>
        /// Clones this object
        /// </summary>
        /// <returns>A cloned RavenJToken</returns>
        public abstract RavenJToken CloneToken();

		/// <summary>
		/// Gets the <see cref="JToken"/> with the specified key.
		/// </summary>
		/// <value>The <see cref="JToken"/> with the specified key.</value>
		public virtual RavenJToken this[object key]
		{
			get { throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType())); }
			set { throw new InvalidOperationException("Cannot set child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType())); }
		}

        internal static RavenJToken FromObjectInternal(object o, JsonSerializer jsonSerializer)
        {
            ValidationUtils.ArgumentNotNull(o, "o");
            ValidationUtils.ArgumentNotNull(jsonSerializer, "jsonSerializer");

            RavenJToken token;
            using (var jsonWriter = new RavenJTokenWriter())
            {
                jsonSerializer.Serialize(jsonWriter, o);
                token = jsonWriter.Token;
            }

            return token;
        }

        /// <summary>
        /// Creates a <see cref="RavenJToken"/> from an object.
        /// </summary>
        /// <param name="o">The object that will be used to create <see cref="RavenJToken"/>.</param>
        /// <returns>A <see cref="RavenJToken"/> with the value of the specified object</returns>
        public static RavenJToken FromObject(object o)
        {
            return FromObjectInternal(o, new JsonSerializer());
        }

        /// <summary>
        /// Creates a <see cref="RavenJToken"/> from an object using the specified <see cref="JsonSerializer"/>.
        /// </summary>
        /// <param name="o">The object that will be used to create <see cref="RavenJToken"/>.</param>
        /// <param name="jsonSerializer">The <see cref="JsonSerializer"/> that will be used when reading the object.</param>
        /// <returns>A <see cref="RavenJToken"/> with the value of the specified object</returns>
        public static RavenJToken FromObject(object o, JsonSerializer jsonSerializer)
        {
            return FromObjectInternal(o, jsonSerializer);
        }

        /// <summary>
        /// Returns the indented JSON for this token.
        /// </summary>
        /// <returns>
        /// The indented JSON for this token.
        /// </returns>
        public override string ToString()
        {
            return ToString(Formatting.Indented);
        }

        /// <summary>
        /// Returns the JSON for this token using the given formatting and converters.
        /// </summary>
        /// <param name="formatting">Indicates how the output is formatted.</param>
        /// <param name="converters">A collection of <see cref="JsonConverter"/> which will be used when writing the token.</param>
        /// <returns>The JSON for this token using the given formatting and converters.</returns>
        public string ToString(Formatting formatting, params JsonConverter[] converters)
        {
            using (var sw = new StringWriter(CultureInfo.InvariantCulture))
            {
                var jw = new JsonTextWriter(sw);
                jw.Formatting = formatting;

                WriteTo(jw, converters);

                return sw.ToString();
            }
        }

        /// <summary>
        /// Writes this token to a <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="writer">A <see cref="JsonWriter"/> into which this method will write.</param>
        /// <param name="converters">A collection of <see cref="JsonConverter"/> which will be used when writing the token.</param>
        public abstract void WriteTo(JsonWriter writer, params JsonConverter[] converters);

		/// <summary>
		/// Creates a <see cref="RavenJToken"/> from a <see cref="JsonReader"/>.
		/// </summary>
		/// <param name="reader">An <see cref="JsonReader"/> positioned at the token to read into this <see cref="JToken"/>.</param>
		/// <returns>
		/// An <see cref="RavenJToken"/> that contains the token and its descendant tokens
		/// that were read from the reader. The runtime type of the token is determined
		/// by the token type of the first token encountered in the reader.
		/// </returns>
		public static RavenJToken ReadFrom(JsonReader reader)
		{
			ValidationUtils.ArgumentNotNull(reader, "reader");

			if (reader.TokenType == JsonToken.None)
			{
				if (!reader.Read())
					throw new Exception("Error reading JToken from JsonReader.");
			}

			switch (reader.TokenType)
			{
				case JsonToken.StartObject:
					return RavenJObject.Load(reader);
				case JsonToken.StartArray:
					return RavenJArray.Load(reader);
				case JsonToken.String:
				case JsonToken.Integer:
				case JsonToken.Float:
				case JsonToken.Date:
				case JsonToken.Boolean:
				case JsonToken.Bytes:
				case JsonToken.Null:
				case JsonToken.Undefined:
					return new RavenJValue(reader.Value);
			}

			// TODO: loading constructor and parameters?
			throw new Exception("Error reading JToken from JsonReader. Unexpected token: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}

		/// <summary>
		/// Load a <see cref="RavenJToken"/> from a string that contains JSON.
		/// </summary>
		/// <param name="json">A <see cref="String"/> that contains JSON.</param>
		/// <returns>A <see cref="RavenJToken"/> populated from the string that contains JSON.</returns>
		public static RavenJToken Parse(string json)
		{
			JsonReader jsonReader = new JsonTextReader(new StringReader(json));

			return Load(jsonReader);
		}

		/// <summary>
		/// Creates a <see cref="JToken"/> from a <see cref="JsonReader"/>.
		/// </summary>
		/// <param name="reader">An <see cref="JsonReader"/> positioned at the token to read into this <see cref="JToken"/>.</param>
		/// <returns>
		/// An <see cref="JToken"/> that contains the token and its descendant tokens
		/// that were read from the reader. The runtime type of the token is determined
		/// by the token type of the first token encountered in the reader.
		/// </returns>
		public static RavenJToken Load(JsonReader reader)
		{
			return ReadFrom(reader);
		}

		/// <summary>
		/// Gets the <see cref="RavenJToken"/> with the specified key converted to the specified type.
		/// </summary>
		/// <typeparam name="T">The type to convert the token to.</typeparam>
		/// <param name="key">The token key.</param>
		/// <returns>The converted token value.</returns>
		public virtual T Value<T>(object key)
		{
			return this[key].Convert<RavenJToken, T>();
		}

		/// <summary>
		/// Compares the values of two tokens, including the values of all descendant tokens.
		/// </summary>
		/// <param name="t1">The first <see cref="JToken"/> to compare.</param>
		/// <param name="t2">The second <see cref="JToken"/> to compare.</param>
		/// <returns>true if the tokens are equal; otherwise false.</returns>
		public static bool DeepEquals(RavenJToken t1, RavenJToken t2)
		{
			return (t1 == t2 || (t1 != null && t2 != null && t1.DeepEquals(t2)));
		}

		internal abstract bool DeepEquals(RavenJToken node);
		internal abstract int GetDeepHashCode();
    }
}
