﻿using NUnit.Framework;
using Osaru.Json;
using Osaru;
using Osaru.MessagePack;
using Osaru.Serialization;
using System;


namespace OsaruTest
{
    public class ConverterTest
    {
        static ArraySegment<Byte> ConvertToJson(ArraySegment<Byte> messagePack)
        {
            var json = new JsonFormatter();
            MessagePackParser.Parse(messagePack).Convert(json);
            return json.GetStore().Bytes;
        }

        static ArraySegment<Byte> ConvertToMessagePack(string json)
        {
            var msgPack = new MessagePackFormatter();
            JsonParser.Parse(json).Convert(msgPack);
            return msgPack.GetStore().Bytes;
        }

        static ArraySegment<Byte> FormatToMessagePack<T>(T value)
        {
            var typeRegistory = new TypeRegistory();
            var s=typeRegistory.GetSerializer<T>();
            return s.SerializeToMessagePack(value);
        }

        static string FormatToJson<T>(T value)
        {
            var typeRegistory = new TypeRegistory();
            var s = typeRegistory.GetSerializer<T>();
            return s.SerializeToJson(value);
        }

        static T Deserialize<T, PARSER>(PARSER parser, ref T t)
            where PARSER: IParser<PARSER>
        {
            var typeRegistory = new TypeRegistory();
            var d = typeRegistory.GetDeserializer<T>();
            d.Deserialize(parser, ref t);
            return t;
        }

        static void ConvertJsonToMessagePackTest<T>(T value)
        {
            // todo fixed int
            var converted = ConvertToMessagePack(FormatToJson(value));
            var c = MessagePackParser.Parse(converted);
            var cc = default(T);

            var formated = FormatToMessagePack(value);
            var f = MessagePackParser.Parse(formated);
            var ff = default(T);

            Assert.AreEqual(Deserialize(f, ref ff), Deserialize(c, ref cc));
        }

        static void ConvertMessagePackToJsonTest<T>(T value)
        {
            var converted = ConvertToJson(FormatToMessagePack(value));
            var c = JsonParser.Parse(converted);
            var cc = default(T);

            var formated = FormatToJson(value);
            var f = JsonParser.Parse(formated);
            var ff = default(T);

            Assert.AreEqual(Deserialize(f, ref ff), Deserialize(c, ref cc));
        }

        [Test]
        public void JsonToMessagePackTest()
        {
            ConvertJsonToMessagePackTest(1);
            ConvertJsonToMessagePackTest("abc");
            ConvertJsonToMessagePackTest(true);
            //ConvertJsonToMessagePackTest((object)null);
            ConvertJsonToMessagePackTest(new UnityEngine.Vector3(1, 2, 3));
        }

        [Test]
        public void MessagePackToJsonTest()
        {
            ConvertMessagePackToJsonTest(1);
            ConvertMessagePackToJsonTest("abc");
            ConvertMessagePackToJsonTest(true);
            //ConvertMessagePackToJsonTest((object)null);
            ConvertMessagePackToJsonTest(new UnityEngine.Vector3(1, 2, 3));
        }
    }
}