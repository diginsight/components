#region using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
#endregion

namespace Common
{
    #region IMath
    public interface IMath<T>
    {
        T Add(T v1, T v2);
        T Multiply(T v1, T v2);
        T Subtract(T v1, T v2);
        T Divide(T v1, T v2);

        T MaxValue { get; }
        T MinValue { get; }
        T Abs(T a);

        T Parse(string s, IFormatProvider provider);
        T ToT(decimal d);
        bool TryParse(string s, NumberStyles style, IFormatProvider provider, out T result);
    }
    #endregion
}
namespace Common
{
    using T = System.Int32;
    #region MathInt32
    public class MathInt32 : IMath<T>
    {
        public T Add(T v1, T v2) { return v1 + v2; }
        public T Multiply(T v1, T v2) { return v1 * v2; }
        public T Subtract(T v1, T v2) { return v1 - v2; }
        public T Divide(T v1, T v2) { return v1 / v2; }

        public T MaxValue { get { return T.MaxValue; } }
        public T MinValue { get { return T.MinValue; } }
        public T Abs(T v) { return Math.Abs(v); }

        public T Parse(string s, IFormatProvider provider) { return T.Parse(s, provider); }
        public bool TryParse(string s, NumberStyles style, IFormatProvider provider, out T result) { return T.TryParse(s, style, provider, out result); }
        public T ToT(decimal d) { return Convert.ToInt32(d); }
    }
    #endregion
}
namespace Common
{
    using T = System.Int64;
    #region MathInt64
    public class MathInt64 : IMath<T>
    {
        public T Add(T v1, T v2) { return v1 + v2; }
        public T Multiply(T v1, T v2) { return v1 * v2; }
        public T Subtract(T v1, T v2) { return v1 - v2; }
        public T Divide(T v1, T v2) { return v1 / v2; }

        public T MaxValue { get { return T.MaxValue; } }
        public T MinValue { get { return T.MinValue; } }
        public T Abs(T v) { return Math.Abs(v); }

        public T Parse(string s, IFormatProvider provider) { return T.Parse(s, provider); }
        public bool TryParse(string s, NumberStyles style, IFormatProvider provider, out T result) { return T.TryParse(s, style, provider, out result); }
        public T ToT(decimal d) { return Convert.ToInt64(d); }
    }
    #endregion
}
namespace Common
{
    using T = System.Decimal;
    #region MathDecimal
    public class MathDecimal : IMath<T>
    {
        public T Add(T v1, T v2) { return v1 + v2; }
        public T Multiply(T v1, T v2) { return v1 * v2; }
        public T Subtract(T v1, T v2) { return v1 - v2; }
        public T Divide(T v1, T v2) { return v1 / v2; }

        public T MaxValue { get { return T.MaxValue; } }
        public T MinValue { get { return T.MinValue; } }
        public T Abs(T v) { return Math.Abs(v); }

        public T Parse(string s, IFormatProvider provider) { return T.Parse(s, provider); }
        public bool TryParse(string s, NumberStyles style, IFormatProvider provider, out T result) { return T.TryParse(s, style, provider, out result); }
        public T ToT(decimal d) { return Convert.ToDecimal(d); }

        public static decimal Floor(decimal value, int decimalPlaces)
        {
            decimal adjustment = Math.Round((decimal)Math.Pow(10, decimalPlaces));
            return Math.Floor(value * adjustment) / adjustment;
        }
        public static decimal Ceiling(decimal value, int decimalPlaces)
        {
            decimal adjustment = Math.Round((decimal)Math.Pow(10, decimalPlaces));
            return Math.Ceiling(value * adjustment) / adjustment;
        }
    }
    #endregion
}
namespace Common
{
    using T = System.Single;
    #region MathSingle
    public class MathSingle : IMath<T>
    {
        public T Add(T v1, T v2) { return v1 + v2; }
        public T Multiply(T v1, T v2) { return v1 * v2; }
        public T Subtract(T v1, T v2) { return v1 - v2; }
        public T Divide(T v1, T v2) { return v1 / v2; }

        public T MaxValue { get { return T.MaxValue; } }
        public T MinValue { get { return T.MinValue; } }
        public T Abs(T v) { return Math.Abs(v); }

        public T Parse(string s, IFormatProvider provider) { return T.Parse(s, provider); }
        public bool TryParse(string s, NumberStyles style, IFormatProvider provider, out T result) { return T.TryParse(s, style, provider, out result); }
        public T ToT(decimal d) { return Convert.ToSingle(d); }
    }
    #endregion
}
namespace Common
{
    using T = System.Double;
    #region MathDouble
    public class MathDouble : IMath<T>
    {
        public T Add(T v1, T v2) { return v1 + v2; }
        public T Multiply(T v1, T v2) { return v1 * v2; }
        public T Subtract(T v1, T v2) { return v1 - v2; }
        public T Divide(T v1, T v2) { return v1 / v2; }

        public T MaxValue { get { return T.MaxValue; } }
        public T MinValue { get { return T.MinValue; } }
        public T Abs(T v) { return Math.Abs(v); }

        public T Parse(string s, IFormatProvider provider) { return T.Parse(s, provider); }
        public bool TryParse(string s, NumberStyles style, IFormatProvider provider, out T result) { return T.TryParse(s, style, provider, out result); }
        public T ToT(decimal d) { return Convert.ToDouble(d); }
    }
    #endregion
}
namespace Common
{
    #region Math
    public class Math<T> : IMath<T>
    {
        #region internal state
        public static IMath<T> Default { get; private set; }
        #endregion
        #region .ctor
        static Math()
        {
            if (typeof(T) == typeof(int))
            {
                Default = (IMath<T>)new MathInt32();
            }
            else if (typeof(T) == typeof(long))
            {
                Default = (IMath<T>)new MathInt64();
            }
            else if (typeof(T) == typeof(decimal))
            {
                Default = (IMath<T>)new MathDecimal();
            }
            else if (typeof(T) == typeof(float))
            {
                Default = (IMath<T>)new MathSingle();
            }
            else if (typeof(T) == typeof(double))
            {
                Default = (IMath<T>)new MathDouble();
            }
            else
            {
                throw new InvalidOperationException("Type not supported");
            }
        }
        #endregion

        public T Add(T v1, T v2) { return Default.Add(v1, v2); }
        public T Multiply(T v1, T v2) { return Default.Multiply(v1, v2); }
        public T Subtract(T v1, T v2) { return Default.Subtract(v1, v2); }
        public T Divide(T v1, T v2) { return Default.Divide(v1, v2); }
        public T MaxValue { get { return Default.MaxValue; } }
        public T MinValue { get { return Default.MinValue; } }
        public T Abs(T v) { return Default.Abs(v); }

        public T Parse(string s, IFormatProvider provider) { return Default.Parse(s, provider); }
        public bool TryParse(string s, NumberStyles style, IFormatProvider provider, out T result) { return Default.TryParse(s, style, provider, out result); }
        public T ToT(decimal d) { return Default.ToT(d); }

    }
    #endregion

    public class Number<T, C>
        where C : IMath<T>, new()
    {
        #region internal state
        private static C _calculator = new C();
        #endregion
        #region .ctor
        public Number() : this(default(T)) { }
        public Number(T v)
        {
            _value = (T)v;
        }
        #endregion

        #region Value
        private T _value;
        public T Value
        {
            get { return _value; }
            set { _value = value; }
        }
        #endregion

        #region Add
        public T Add(T v1, T v2)
        {
            return _calculator.Add(v1, v2);
        }
        #endregion
        #region Multiply
        public T Multiply(T v1, T v2)
        {
            return _calculator.Multiply(v1, v2);
        }
        #endregion
        #region Subtract
        public T Subtract(T v1, T v2)
        {
            return _calculator.Subtract(v1, v2);
        }
        #endregion
        #region Divide
        public T Divide(T v1, T v2)
        {
            return _calculator.Divide(v1, v2);
        }
        #endregion

        #region Parse
        public T Parse(string s, IFormatProvider provider) { return _calculator.Parse(s, provider); }
        #endregion
        #region TryParse
        public bool TryParse(string s, NumberStyles style, IFormatProvider provider, out T result) { return _calculator.TryParse(s, style, provider, out result); }
        #endregion
        #region Negate
        public static T Negate(T d) { return d; }
        #endregion
        #region MaxValue
        public T MaxValue { get { return _calculator.MaxValue; } }
        #endregion
        #region MinValue
        public T MinValue { get { return _calculator.MinValue; } }
        #endregion

        #region operator Number
        public static implicit operator Number<T, C>(T a)
        {
            return new Number<T, C>(a);
        }
        #endregion
        #region operator T
        public static implicit operator T(Number<T, C> a)
        {
            return a.Value;
        }
        #endregion

        #region operator +
        public static Number<T, C> operator +(Number<T, C> a, Number<T, C> b)
        {
            return _calculator.Add(a, b);
        }
        #endregion
        #region operator -
        public static Number<T, C> operator -(Number<T, C> a, Number<T, C> b)
        {
            return _calculator.Subtract(a, b);
        }
        #endregion
        #region operator *
        public static Number<T, C> operator *(Number<T, C> a, Number<T, C> b)
        {
            return _calculator.Multiply(a, b);
        }
        #endregion
        #region operator /
        public static Number<T, C> operator /(Number<T, C> a, Number<T, C> b)
        {
            return _calculator.Divide(a, b);
        }
        #endregion
    }
}
