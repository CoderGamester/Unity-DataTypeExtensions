using Newtonsoft.Json;
using UnityEngine;
using System;

namespace GameLovers.Serialization
{
    /// <summary>
    /// JSON converter for Unity's Color struct that handles serialization to/from:
    /// - Hex color strings (e.g. "#FF0000FF" for red)
    /// - RGBA object format (e.g. {"r":1,"g":0,"b":0,"a":1})
    /// </summary>
    /// <remarks>
    /// This converter enables proper serialization of Unity Color objects with Newtonsoft.Json.
    /// It's particularly useful for saving color data in JSON configuration files or network payloads.
    /// </remarks>
    public class ColorJsonConverter : JsonConverter<Color>
    {
        /// <summary>
        /// Reads JSON data and converts it to a Unity Color object
        /// </summary>
        /// <param name="reader">JSON reader providing the input data</param>
        /// <param name="objectType">Type of object to deserialize (should be Color)</param>
        /// <param name="existingValue">Existing value of the object being read</param>
        /// <param name="hasExistingValue">Whether there is an existing value</param>
        /// <param name="serializer">JSON serializer instance</param>
        /// <returns>Deserialized Color object</returns>
        /// <remarks>
        /// Supports both hex color strings and RGBA object formats:
        /// - "#RRGGBBAA" or "#RRGGBB" (missing alpha defaults to 1)
        /// - {"r":1,"g":0,"b":0,"a":1} (missing components default to 0, except alpha which defaults to 1)
        /// </remarks>
        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                string colorString = reader.Value.ToString();
                Color color;
                if (ColorUtility.TryParseHtmlString(colorString, out color))
                {
                    return color;
                }
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                float r = 0, g = 0, b = 0, a = 1;
                
                reader.Read();
                while (reader.TokenType != JsonToken.EndObject)
                {
                    string propertyName = reader.Value.ToString().ToLower();
                    reader.Read();
                    
                    switch (propertyName)
                    {
                        case "r": r = Convert.ToSingle(reader.Value); break;
                        case "g": g = Convert.ToSingle(reader.Value); break;
                        case "b": b = Convert.ToSingle(reader.Value); break;
                        case "a": a = Convert.ToSingle(reader.Value); break;
                    }
                    
                    reader.Read();
                }
                
                return new Color(r, g, b, a);
            }
            
            return Color.white;
        }

        /// <summary>
        /// Writes a Color object to JSON format
        /// </summary>
        /// <param name="writer">JSON writer for output</param>
        /// <param name="value">Color value to serialize</param>
        /// <param name="serializer">JSON serializer instance</param>
        /// <remarks>
        /// Always serializes to hex string format (e.g. "#FF0000FF" for red)
        /// This provides a compact and web-friendly representation
        /// </remarks>
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WriteValue("#" + ColorUtility.ToHtmlStringRGBA(value));
        }
    }
}
