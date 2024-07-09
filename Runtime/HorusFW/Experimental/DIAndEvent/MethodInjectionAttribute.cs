using System;

namespace HorusFW.DI
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MethodInjectionAttribute : System.Attribute
    {
        public MethodInjectionAttribute()
        {
        }
    }
}
