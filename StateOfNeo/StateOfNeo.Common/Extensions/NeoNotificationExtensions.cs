using Neo;
using Neo.SmartContract;
using Neo.VM;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
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
                        var result = new BigInteger(rawValue);
                        SetPropertyValue(property.Name, instance, result);
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

        public static IEnumerable<string> ToStringList(this IEnumerable<StackItem> stackItems, int toSkip = 0)
        {
            return stackItems.Skip(toSkip)
                .Select(si => (si is Neo.VM.Types.Array) ?
                    string.Join(", ", (si as Neo.VM.Types.Array).ToStringList()) : si.GetByteArray().ToHexString());
        }

        private static void SetPropertyValue(string propertyName, object instance, object value) =>
            instance.GetType().GetProperty(propertyName).SetValue(instance, value);

        public static string GetNotificationType(this NotifyEventArgs args)
        {
            try
            {
                return (args.State as Neo.VM.Types.Array)[0].GetByteArray().ToHexString().HexStringToString();
            }
            catch (Exception e)
            {
                Log.Warning($@"NeoNotificationExtensions - {System.Reflection.MethodBase.GetCurrentMethod().Name}.State 
                    could not cast {args.GetType().Name} to Neo.VM.Types.Array. Return result is args.State as string.");
                return args.State.GetByteArray().ToHexString().HexStringToString();
            }
        }

        public static T GetNotification<T>(this NotifyEventArgs args, int toSkip = 1) =>
            (args.State as Neo.VM.Types.Array).Skip(toSkip).CreateObject<T>();
    }
}
