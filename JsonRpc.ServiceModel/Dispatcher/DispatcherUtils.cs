﻿using JsonRpc.ServiceModel.Channels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace JsonRpc.ServiceModel.Dispatcher
{
    class DispatcherUtils
    {
        public const string MessageIdKey = "MessageId";

        public static byte[] SerializeBody(object content, Encoding encoding)
        {
            var serializer = new JsonSerializer();
            using (var memStream = new MemoryStream()) {
                using (var writer = new JsonTextWriter(new StreamWriter(memStream, encoding))) {
                    writer.Formatting = Newtonsoft.Json.Formatting.None;

                    serializer.Serialize(writer, content);
                    writer.Flush();

                    return memStream.ToArray();
                }
            }
        }

        public static Message CreateMessage(MessageVersion messageVersion, string action, byte[] rawBody, Encoding encoding)
        {
            Message message = Message.CreateMessage(messageVersion,
                action, new RawBodyWriter(rawBody));

            message.Properties.Add(WebBodyFormatMessageProperty.Name,
                new WebBodyFormatMessageProperty(WebContentFormat.Raw));

            var respProp = new HttpResponseMessageProperty();
            respProp.Headers[HttpResponseHeader.ContentType] =
                String.Format("application/json; charset={0}", encoding.WebName);
            message.Properties.Add(HttpResponseMessageProperty.Name, respProp);

            return message;
        }

        public static byte[] DeserializeBody(Message message)
        {
            using (XmlDictionaryReader bodyReader = message.GetReaderAtBodyContents()) {
                bodyReader.ReadStartElement("Binary");
                return bodyReader.ReadContentAsBase64();
            }
        }
    }
}
