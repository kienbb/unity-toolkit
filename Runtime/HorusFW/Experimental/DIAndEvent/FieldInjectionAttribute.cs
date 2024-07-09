using System;

namespace HorusFW.DI
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class FieldInjectionAttribute : System.Attribute
    {
        public FieldInjectionAttribute()
        {
        }
    }
}
