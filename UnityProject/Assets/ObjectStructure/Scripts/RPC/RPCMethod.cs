﻿using ObjectStructure.Serialization;
using ObjectStructure.Serialization.Deserializers;
using ObjectStructure.Serialization.Serializers;
using System;


namespace ObjectStructure.RPC
{
    public interface IRPCContext<T>
        where T: IParser<T>
    {
        RPCRequest<T> Request { get; }
        //RPCResponse<T> Response { get; }
        void Success();
        void Success<R>(R result, SerializerBase<R> s);
        void Error(Exception ex);
    }

    public interface IRPCMethod
    {
        void Call<T>(IRPCContext<T> f)
            where T : IParser<T>;
    }

    // Action<A0>
    public class RPCAction<A0> : IRPCMethod
    {
        IDeserializerBase<A0> m_d0;
        public delegate void Method(A0 a0);
        Method m_method;
        public RPCAction(TypeRegistory r, Method method)
        {
            m_method = method;
            m_d0 = r.GetDeserializer<A0>();
        }

        public void Call<T>(IRPCContext<T> f)
            where T : IParser<T>
        {
            try
            {
                var a0 = default(A0);
                m_d0.Deserialize(f.Request.Params[0], ref a0);

                m_method(a0);
                f.Success();
            }
            catch (Exception ex)
            {
                f.Error(ex);
            }
        }
    }

    // Action<A0, A1>
    public class RPCAction<A0, A1> : IRPCMethod
    {
        IDeserializerBase<A0> m_d0;
        IDeserializerBase<A1> m_d1;
        public delegate void Method(A0 a0, A1 a1);
        Method m_method;
        public RPCAction(TypeRegistory r, Method method)
        {
            m_method = method;
            m_d0 = r.GetDeserializer<A0>();
            m_d1 = r.GetDeserializer<A1>();
        }

        public void Call<T>(IRPCContext<T> f)
            where T : IParser<T>
        {
            try
            {
                var a0 = default(A0);
                m_d0.Deserialize(f.Request.Params[0], ref a0);

                var a1 = default(A1);
                m_d1.Deserialize(f.Request.Params[1], ref a1);

                m_method(a0, a1);
                f.Success();
            }
            catch (Exception ex)
            {
                f.Error(ex);
            }
        }
    }

    // Action<A0, A1, A2>
    public class RPCAction<A0, A1, A2> : IRPCMethod
    {
        IDeserializerBase<A0> m_d0;
        IDeserializerBase<A1> m_d1;
        IDeserializerBase<A2> m_d2;
        public delegate void Method(A0 a0, A1 a1, A2 a2);
        Method m_method;
        public RPCAction(TypeRegistory r, Method method)
        {
            m_method = method;
            m_d0 = r.GetDeserializer<A0>();
            m_d1 = r.GetDeserializer<A1>();
            m_d2 = r.GetDeserializer<A2>();
        }

        public void Call<T>(IRPCContext<T> f)
            where T : IParser<T>
        {
            try
            {
                var a0 = default(A0);
                m_d0.Deserialize(f.Request.Params[0], ref a0);

                var a1 = default(A1);
                m_d1.Deserialize(f.Request.Params[1], ref a1);

                var a2 = default(A2);
                m_d2.Deserialize(f.Request.Params[2], ref a2);

                m_method(a0, a1, a2);
                f.Success();
            }
            catch (Exception ex)
            {
                f.Error(ex);
            }
        }
    }

    // Func<R>
    public class RPCFunc<R> : IRPCMethod
    {
        public delegate R Method();
        Method m_method;
        SerializerBase<R> m_s;
        public RPCFunc(TypeRegistory r, Method method)
        {
            m_method = method;
            m_s=r.GetSerializer<R>();
        }

        public void Call<T>(IRPCContext<T> f)
            where T : IParser<T>
        {
            try
            {
                f.Success(m_method(), m_s);
            }
            catch(Exception ex)
            {
                f.Error(ex);
            }
        }
    }

    // Func<A0, R>
    public class RPCFunc<A0, R> : IRPCMethod
    {
        IDeserializerBase<A0> m_d0;
        public delegate R Method(A0 a0);
        Method m_method;
        SerializerBase<R> m_s;
        public RPCFunc(TypeRegistory r, Method method)
        {
            m_method = method;
            m_d0 = r.GetDeserializer<A0>();
            m_s = r.GetSerializer<R>();
        }

        public void Call<T>(IRPCContext<T> f)
            where T : IParser<T>
        {
            try
            {
                var a0 = default(A0);
                m_d0.Deserialize(f.Request.Params[0], ref a0);

                f.Success(m_method(a0), m_s);
            }
            catch (Exception ex)
            {
                f.Error(ex);
            }
        }
    }

    // Func<A0, A1, R>
    public class RPCFunc<A0, A1, R> : IRPCMethod
    {
        IDeserializerBase<A0> m_d0;
        IDeserializerBase<A1> m_d1;
        public delegate R Method(A0 a0, A1 a1);
        Method m_method;
        SerializerBase<R> m_s;
        public RPCFunc(TypeRegistory r, Method method)
        {
            m_method = method;
            m_d0 = r.GetDeserializer<A0>();
            m_d1 = r.GetDeserializer<A1>();
            m_s = r.GetSerializer<R>();
        }

        public void Call<T>(IRPCContext<T> f)
            where T : IParser<T>
        {
            try
            {
                var a0 = default(A0);
                m_d0.Deserialize(f.Request.Params[0], ref a0);

                var a1 = default(A1);
                m_d1.Deserialize(f.Request.Params[1], ref a1);

                f.Success(m_method(a0, a1), m_s);
            }
            catch (Exception ex)
            {
                f.Error(ex);
            }
        }
    }
}
