﻿using System;


namespace ObjectStructure.Json.Deserializers
{
    public class EnumStringDeserializer<T> : DeserializerBase<T>
    {
        public override void Deserialize(JsonParser json, ref T outValue, TypeRegistory r)
        {
            outValue = (T)Enum.Parse(typeof(T), json.GetString());
        }
    }
}