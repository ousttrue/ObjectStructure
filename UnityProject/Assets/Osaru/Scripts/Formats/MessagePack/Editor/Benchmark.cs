﻿using NUnit.Framework;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Text;
using Osaru.Serialization.Serializers;
using Osaru;
using Osaru.Json;
using Osaru.MessagePack;
using Osaru.Serialization;
using UnityEngine;
using Osaru.Serialization.Deserializers;


namespace OsaruTest
{
#if !UNITY_EDITOR
    static class Console
    {
        public static void WriteLine(string fmt="", params object[] args)
        {

        }
    }
#endif

    /// <summary>
    /// from https://github.com/neuecc/ZeroFormatter/blob/master/sandbox/PerformanceComparison/Program.cs
    /// </summary>
    public class Benchmark
    {
        TypeRegistry m_r;

        [SetUp]
        public void Setup()
        {
            m_r = new TypeRegistry();
            m_r.AddSerialization(TypeSerialization.Create(
                Single3.Serialize
                , new Single3.Single3Deserializer()
                ));
        }

        public string HtmlPath
        {
            get
            {
#if UNITY_EDITOR
                return UnityEngine.Application.dataPath + "/Osaru/Tests/CSharpHtml.txt";
#else
                return "CSharpHtml.txt";
#endif
            }
        }

        const int Iteration = 10;
        static bool dryRun = true;

        [Serializable]
        public class Person : IEquatable<Person>
        {
            public virtual int Age { get; set; }
            public virtual string FirstName { get; set; }
            public virtual string LastName { get; set; }
            public virtual Sex Sex { get; set; }

            public override string ToString()
            {
                return String.Format("{{{0}, {1}, {2}, {3}}}"
                    , Age, FirstName, LastName, Sex);
            }

            public bool Equals(Person other)
            {
                if (other == null)
                {
                    int a = 0;
                }
                return Age == other.Age
                    && FirstName == other.FirstName
                    && LastName == other.LastName
                    && Sex == other.Sex;
            }
        }

        public enum Sex : sbyte
        {
            Unknown, Male, Female,
        }

        static void Validate(string label, Person original, IList<Person> originalList, Person copy, IList<Person> copyList)
        {
            if (!EqualityComparer<Person>.Default.Equals(original, copy)) Console.WriteLine(label + " Invalid Deserialize Small Object");
            if (!originalList.SequenceEqual(copyList)) Console.WriteLine(label + " Invalid Deserialize Large Array");
        }

        static void Validate2<T>(string label, T original, T copy)
        {
            if (!EqualityComparer<T>.Default.Equals(original, copy)) Console.WriteLine(label + " Invalid Deserialize");
        }

        static void Validate2<T>(string label, IList<T> original, IList<T> copy)
        {
            if (!original.SequenceEqual(copy)) Console.WriteLine(label + " Invalid Deserialize");
        }

        struct Measure : IDisposable
        {
            string label;
            Stopwatch s;

            public Measure(string label)
            {
                this.label = label;
                System.GC.Collect(2, GCCollectionMode.Forced/*, blocking: true*/);
                this.s = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                s.Stop();
                //if (!dryRun)
                {
                    //Console.WriteLine($"{ label,15}   {s.Elapsed.TotalMilliseconds} ms");
                }

                System.GC.Collect(2, GCCollectionMode.Forced/*, blocking: true*/);
            }
        }

        public struct Single3
        {
            public float x;

            public float y;

            public float z;

            public Single3(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public override bool Equals(object obj)
            {
                if (obj is Single3)
                {
                    return this.Equals((Single3)obj);
                }
                return false;
            }

            public bool NearlyEqual(float a, float b, float epsilon)
            {
                float absA = Math.Abs(a);
                float absB = Math.Abs(b);
                float diff = Math.Abs(a - b);

                if (a == b)
                { // shortcut, handles infinities
                    return true;
                }
                else if (a == 0 || b == 0 || diff < float.Epsilon)
                {
                    // a or b is zero or both are extremely close to it
                    // relative error is less meaningful here
                    return diff < epsilon;
                }
                else
                { // use relative error
                    return diff / (absA + absB) < epsilon;
                }
            }

            public bool Equals(Single3 p)
            {
                const float EPSILON = 1e-5f;
                if (!NearlyEqual(x, p.x, EPSILON)) return false;
                if (!NearlyEqual(y, p.y, EPSILON)) return false;
                if (!NearlyEqual(z, p.z, EPSILON)) return false;
                return true;
            }

            public override string ToString()
            {
                return string.Format("[{0}, {1}, {2}]", x, y, z);
            }

            [Serializer]
            public static void Serialize(Single3 t, IFormatter f)
            {
                f.BeginList(3);
                f.Value(t.x);
                f.Value(t.y);
                f.Value(t.z);
                f.EndList();
            }
            public class Single3Deserializer : IDeserializerBase<Single3>
            {
                IDeserializerBase<Single> m_d;
                public void Setup(TypeRegistry r)
                {
                    m_d=r.GetDeserializer<Single>();
                }
                public void Deserialize<PARSER>(PARSER parser, ref Single3 outValue) where PARSER : IParser<PARSER>
                {
                    var it = parser.ListItems.GetEnumerator();
                    it.MoveNext(); m_d.Deserialize(it.Current, ref outValue.x);
                    it.MoveNext(); m_d.Deserialize(it.Current, ref outValue.y);
                    it.MoveNext(); m_d.Deserialize(it.Current, ref outValue.z);
                }
            }
            [DeserializerFactory]
            public static IDeserializerFactory CreateDeserializer()
            {
                return LambdaDeserializerFactory.CreateFactory(
                    new Single3Deserializer());
            }
        }

        public abstract class BenchSerializerBase
        {
            public abstract T Serialize<T>(T t);

            protected T Serialize<Parser, T>(TypeRegistry r
                , IFormatter f
                , Func<ArraySegment<Byte>, Parser> parser, T original)
                where Parser : IParser<Parser>
            {
                T copy = default(T);
                var packed = default(ArraySegment<Byte>);

                // Note:We should check MessagePackSerializer.Get<T>() on every iteration
                // But currenly MsgPack-Cli has bug of get serializer
                // https://github.com/msgpack/msgpack-cli/issues/191
                // so, get serializer at first.
                // and If enum serialization options to ByUnderlyingValue, gets more fast but we check default option only.

                var serializer = (SerializerBase<T>)r.GetSerializer<T>();
                var deserializer = r.GetDeserializer<T>();

                using (new Measure("Serialize"))
                {
                    for (int i = 0; i < Iteration; i++)
                    {
                        f.Clear();
                        serializer.Serialize(original, f);
                        packed = f.GetStore().Bytes;
                    }
                }

                using (new Measure("Deserialize"))
                {
                    for (int i = 0; i < Iteration; i++)
                    {
                        //copy = Osaru.MessagePack.Deserializer.Deserialize<T>(bytes);
                        deserializer.Deserialize(parser(packed), ref copy);
                    }
                }

                Assert.AreEqual(original, copy);

                using (new Measure("ReSerialize"))
                {
                    for (int i = 0; i < Iteration; i++)
                    {
                        f.Clear();
                        serializer.Serialize(copy, f);
                        packed = f.GetStore().Bytes;
                    }
                }

                if (!dryRun)
                {
                    //Console.WriteLine(string.Format("{0,15}   {1}", "Binary Size", ToHumanReadableSize(bytes.Length)));
                }

                return copy;
            }
        }

        class MessagePackSerializer : BenchSerializerBase
        {
            TypeRegistry m_r;

            public MessagePackSerializer(TypeRegistry r)
            {
                m_r = r;
            }

            public override T Serialize<T>(T value)
            {
                var formatter = new MessagePackFormatter();
                return Serialize(m_r
                    , formatter
                    , MessagePackParser.Parse
                    , value);
            }
        }

        class JsonSerializer : BenchSerializerBase
        {
            TypeRegistry m_r;

            public JsonSerializer(TypeRegistry r)
            {
                m_r = r;
            }

            public override T Serialize<T>(T value)
            {
                var formatter = new JsonFormatter();
                return Serialize(m_r
                    , formatter
                    , x => JsonParser.Parse(x)
                    , value);
            }
        }

        [Test]
        public void MessagePackBenchmarkTest()
        {
            BenchMarkTest(new MessagePackSerializer(m_r));
        }

        [Test]
        public void JsonBenchmarkTest()
        {
            BenchMarkTest(new JsonSerializer(m_r));
        }

        void BenchMarkTest(BenchSerializerBase s)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var p = new Person
            {
                Age = 99999,
                FirstName = "Windows",
                LastName = "Server",
                Sex = Sex.Male,
            };
            IList<Person> l = Enumerable.Range(1000, 1000).Select(x => new Person { Age = x, FirstName = "Windows", LastName = "Server", Sex = Sex.Female }).ToArray();

            var integer = 1;
            var v3 = new Single3 { x = 12345.12345f, y = 3994.35226f, z = 325125.52426f };
            IList<Single3> v3List = Enumerable.Range(1, 100).Select(_ => new Single3 { x = 12345.12345f, y = 3994.35226f, z = 325125.52426f }).ToArray();
            var largeString = File.ReadAllText(HtmlPath);

            Console.WriteLine("Warming-up"); //Console.WriteLine();
            s.Serialize(p);
            s.Serialize(l);
            s.Serialize(integer); s.Serialize(v3); s.Serialize(largeString); s.Serialize(v3List);

            dryRun = false;

            //Console.WriteLine();
            Console.WriteLine("Small Object(int,string,string,enum) {0} Iteration", Iteration); //Console.WriteLine();

            var c = s.Serialize(p); //Console.WriteLine();

            Console.WriteLine("Large Array(SmallObject[1000]) {0} Iteration", Iteration); //Console.WriteLine();

            var C = s.Serialize(l); //Console.WriteLine();

            Validate("Osaru.MessagePack", p, l, c, C);

            //Console.WriteLine();
            Console.WriteLine("Additional Benchmarks"); //Console.WriteLine();

            Console.WriteLine("Int32(1) {0} Iteration", Iteration); //Console.WriteLine();

            var W2 = s.Serialize(integer); //Console.WriteLine();

            Console.WriteLine("Vector3(float, float, float) {0} Iteration", Iteration); //Console.WriteLine();

            var X2 = s.Serialize(v3); //Console.WriteLine();

            Console.WriteLine("HtmlString({0}bytes) {1} Iteration"
                , Encoding.UTF8.GetByteCount(largeString)
                , Iteration); //Console.WriteLine();

            var Y2 = s.Serialize(largeString); //Console.WriteLine();

            Console.WriteLine("Vector3[100] {0} Iteration", Iteration); //Console.WriteLine();

            var Z2 = s.Serialize(v3List); //Console.WriteLine();

            Validate2("MsgPack-Cli", W2, integer);
            Validate2("MsgPack-Cli", X2, v3);
            Validate2("MsgPack-Cli", Y2, largeString);
            Validate2("MsgPack-Cli", Z2, v3List);

            //ストップウォッチを止める
            sw.Stop();

            //結果を表示する
            UnityEngine.Debug.LogFormat("[{1}]{0}", sw.Elapsed, s.GetType().Name);
        }
    }
}
