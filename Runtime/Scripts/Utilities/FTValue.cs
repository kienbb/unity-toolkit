using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace FreeTimeGames
{
    [TypeConverter(typeof(AbiValueTypeConverter))]
    [System.Serializable]
    public struct FTValue
    {
        public string Value;

        public FTValue(string v)
        {
            Value = v;
        }

        public FTValue(float v)
        {
            Value = v.ToString("0.000");
        }
        public FTValue(double v)
        {
            Value = v.ToString("0.000");
        }

        public FTValue(int v)
        {
            Value = v.ToString();
        }

        public FTValue(Vector3 v)
        {
            Value = $"{v.x};{v.y};{v.z}";
        }

        public Vector3 ToVector3()
        {
            List<float> l = Value.ToListFloat();
            return new Vector3(l[0], l[1], l[2]);
        }

        public FTValue(List<float> v)
        {
            Value = v.ToStringLine();
        }
        public FTValue(List<int> v)
        {
            Value = v.ToStringLine();
        }
        public FTValue(List<string> v)
        {
            Value = v.ToStringLine();
        }

        public static implicit operator FTValue(string v) => new FTValue(v);
        public static implicit operator string(FTValue g) => g.Value;

        public static implicit operator FTValue(double v) => new FTValue(v);
        public static implicit operator double(FTValue g) => double.Parse(g.Value);

        public static implicit operator FTValue(float v) => new FTValue(v);
        public static implicit operator float(FTValue g) => float.Parse(g.Value);

        public static implicit operator FTValue(int v) => new FTValue(v);
        public static implicit operator int(FTValue g) => int.Parse(g.Value);

        public static implicit operator FTValue(Vector3 v) => new FTValue(v);
        public static implicit operator Vector3(FTValue g) => g.ToVector3();

        public static implicit operator FTValue(List<float> v) => new FTValue(v);
        public static implicit operator List<float>(FTValue g) => g.Value.ToListFloat();

        public static implicit operator FTValue(List<int> v) => new FTValue(v);
        public static implicit operator List<int>(FTValue g) => g.Value.ToListInt();

        public static implicit operator FTValue(List<string> v) => new FTValue(v);
        public static implicit operator List<string>(FTValue g) => g.Value.ToListString();
        public override string ToString()
        {
            return Value;
        }
    }

    public class AbiValueTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string)
                || sourceType == typeof(float)
                || sourceType == typeof(int)
                || base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string)
                || destinationType == typeof(float)
                || destinationType == typeof(int)
                || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            var casted = value as string;
            return casted != null
                ? new FTValue(casted)
                : base.ConvertFrom(context, culture, value);
        }
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            var casted = (FTValue)value;
            return destinationType == typeof(string)
                ? casted
                : base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
