using System;

namespace HorusFW.DI
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ClassInjectionAttribute : System.Attribute
    {
        public ClassInjectionAttribute()
        {
        }
    }
}
