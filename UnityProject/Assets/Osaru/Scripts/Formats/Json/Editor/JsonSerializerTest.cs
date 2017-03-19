﻿using NUnit.Framework;
using Osaru.Json;
using Osaru.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;


namespace OsaruTest.Json
{
    public static class TypeRegistoryExtensions
    {
        public static void TypeTest<T>(this Osaru.Serialization.TypeRegistory typeRegistory
            , T value, string expected)
        {
            var serializer = (SerializerBase<T>)typeRegistory.GetSerializer<T>();
            var serialized = serializer.SerializeToJson(value);
            Assert.AreEqual(expected, serialized);

            var deserializer = typeRegistory.GetDeserializer<T>();
            var deserialized = default(T);
            try
            {
                deserialized = Activator.CreateInstance<T>();
            }
            catch (Exception)
            {

            }
            var json = JsonParser.Parse(serialized);
            deserializer.Deserialize(json, ref deserialized);

            Assert.AreEqual(value, deserialized);
        }
    }


    public class SerializerTest
    {
        [Test]
        public void NumberTest()
        {
            var typeRegistory = new Osaru.Serialization.TypeRegistory();
            typeRegistory.TypeTest(1, "1");
        }

        [Test]
        public void ArrayTest()
        {
            var typeRegistory = new Osaru.Serialization.TypeRegistory();
            var array = new[] { 1, 2, 3 };
            typeRegistory.TypeTest(array, "[1,2,3]");
        }

        [Test]
        public void ListTest()
        {
            var typeRegistory = new Osaru.Serialization.TypeRegistory();
            var list = new List<int> { 1, 2, 3 };
            typeRegistory.TypeTest(list, "[1,2,3]");
        }

        [Test]
        public void Vector3Test()
        {
            var typeRegistory = new Osaru.Serialization.TypeRegistory();
            var v = new UnityEngine.Vector3(1, 2, 3);
            typeRegistory.TypeTest(v, "[1,2,3]");
        }

        [Serializable]
        struct Base64Struct
        {
            public byte[] Bytes;
            public List<Byte> ListBytes;

            public override bool Equals(System.Object obj)
            {
                // If parameter is null return false.
                if (obj == null)
                {
                    return false;
                }

                // If parameter cannot be cast to Point return false.
                var p = (Base64Struct)obj;

                // Return true if the fields match:
                return Equals(p);
            }

            public bool Equals(Base64Struct p)
            {
                // Return true if the fields match:
                return Bytes.SequenceEqual(p.Bytes)
                    && ListBytes.SequenceEqual(p.ListBytes)
                    ;
            }
        }

        [Test]
        public void Base64Test()
        {
            var typeRegistory = new Osaru.Serialization.TypeRegistory();
            var v = new Base64Struct
            {
                Bytes=new Byte[] { (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F', (byte)'G' },
                ListBytes = new List<Byte> { (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F', (byte)'G' },
            };
            typeRegistory.TypeTest(v, "{\"Bytes\":\"QUJDREVGRw==\",\"ListBytes\":\"QUJDREVGRw==\"}");
        }
    }
}
