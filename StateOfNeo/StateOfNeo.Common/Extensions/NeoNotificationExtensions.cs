using Neo;
using Neo.SmartContract;
using Neo.VM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Numerics;
using System.Text;

namespace StateOfNeo.Common.Extensions
{
    public static class NeoNotificationExtensions
    {
        public static T CreateObject<T>(this IEnumerable<StackItem> stackItems)
        {
            var instance = Activator.CreateInstance<T>();
            var properties = instance.GetType().GetProperties()
                .Where(x => !x.GetCustomAttributes(typeof(NotMappedAttribute), true).Any())
                .ToArray();

            if (stackItems.Count() == properties.Count())
            {
                for (int i = 0; i < stackItems.Count(); i++)
                {
                    var property = properties[i];
                    var rawValue = stackItems.ElementAt(i).GetByteArray();
                    if (property.PropertyType == typeof(BigInteger))
                    {
                        var valueAsString = rawValue.ToHexString().HexStringToString();
                        if (BigInteger.TryParse(valueAsString, out BigInteger defaultInt))
                        {
                            SetPropertyValue(property.Name, instance, BigInteger.Parse(valueAsString));
                        }
                    }
                    else if (property.PropertyType == typeof(byte[]))
                    {
                        SetPropertyValue(property.Name, instance, rawValue);
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        var valueAsString = rawValue.ToHexString().HexStringToString();
                        SetPropertyValue(property.Name, instance, valueAsString);
                    }
                }
            }

            return instance;
        }

        public static IEnumerable<string> ToStringList(this IEnumerable<StackItem> stackItems) =>
            stackItems.Skip(1).Select(si => si.GetByteArray().ToHexString().HexStringToString());

        private static void SetPropertyValue(string propertyName, object instance, object value) =>
            instance.GetType().GetProperty(propertyName).SetValue(instance, value);

        public static string GetNotificationType(this NotifyEventArgs args) =>
            (args.State as Neo.VM.Types.Array)[0].GetByteArray().ToHexString().HexStringToString();

        public static T GetNotification<T>(this NotifyEventArgs args) =>
            (args.State as Neo.VM.Types.Array).Skip(1).CreateObject<T>();
    }
}
