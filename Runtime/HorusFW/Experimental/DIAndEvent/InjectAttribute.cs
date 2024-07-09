using System;

namespace HorusFW.DI
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class InjectAttribute : System.Attribute
    {
        public string SourceName { get; private set; }
        public Type SourceType { get; private set; }
        public bool AcceptSubclass { get; private set; }
        /// <summary>
        /// dùng cho inject class instance singleton
        /// </summary>
        public InjectAttribute() { }

        public InjectAttribute(Type sourceType, string sourceName, bool acceptSubclass = true)
        {
            SourceName = sourceName;
            SourceType = sourceType;
            AcceptSubclass = acceptSubclass;
        }
    }
}
