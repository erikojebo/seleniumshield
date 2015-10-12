using System;
using System.Linq;

namespace SeleniumShield.Metadata
{
    public static class CustomAttributeExtensions
    {
         public static T GetCustomAttribute<T>(this Type type, bool inherit = true)
            where T : Attribute
         {
             return (T)type.GetCustomAttributes(typeof(T), inherit).SingleOrDefault();
         }
    }
}