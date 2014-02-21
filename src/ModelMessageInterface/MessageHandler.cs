﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using ZMQ;

namespace ModelMessageInterface
{
    public class MessageHandler
    {
        private static readonly IDictionary<string, Type> SupportedTypes = new Dictionary<string, Type>
        {
            {"bool", typeof (bool)},
            {"int32", typeof (int)},
            {"float32", typeof (float)},
            {"float64", typeof (double)}
        };

        public static Array ToArray(byte[] bytes, string valueType, int[] shape)
        {
            Array values = Array.CreateInstance(SupportedTypes[valueType], shape);

            Buffer.BlockCopy(bytes, 0, values, 0, bytes.Length);

            return values;
        }

        public static Message GetMessage(Socket socket)
        {
            string json = socket.Recv(Encoding.UTF8);

            Debug.WriteLine(json);

            JObject jsonObject = JObject.Parse(json);

            var name = jsonObject.Value<string>("name");
            int[] shape = jsonObject["shape"].Values<int>().ToArray();
            var dtype = jsonObject.Value<string>("dtype");
            byte[] bytes = socket.Recv();

            // special case, zero-rank array (single value)
            shape = shape.Length == 0 ? new[] { 1 } : shape;

            return new Message { Values = ToArray(bytes, dtype, shape), Name = name, Shape = shape };
        }

        public struct Message
        {
            public string Name;
            public int[] Shape;
            public Array Values;
        }
    }
}
