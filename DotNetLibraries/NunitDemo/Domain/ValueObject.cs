using System;
using System.Collections.Generic;
using System.Linq;

namespace NunitDemo.Domain
{
    /// <summary>
    /// 模拟值类型，但是他们任然是引用类型，保留他们做为引用类型的操作符
    /// 如果他们所有的原子数据指向同一个实例（reference相同），则判定他们相同
    /// </summary>
    public abstract class ValueObject
    {
        public static bool EqualOperator(ValueObject left, ValueObject right)
        {
            // 0^1=1 1^0=1 只有一个成立才为真 0^0=0 1^1=0 
            if (Object.ReferenceEquals(left, null) ^ Object.ReferenceEquals(right, null))
                return false;

            // 走到这里说明 要么都是null 要么都是非空
            // || 任意一个成立则为真
            // 如果第一个成立 说明都是null 返回真没问题
            // 如果第一个不成立 说明都不是null,第二局不会报错，并且只有第二个成立才会返回真 妙啊~~ 
            return Object.ReferenceEquals(left, null) || left.Equals(right);
        }

        public static bool NotEqualOperator(ValueObject left, ValueObject right)
        {
            return !(EqualOperator(left, right));
        }

        protected abstract IEnumerable<object> GetAtomicValues();

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
                return false;

            ValueObject other = (ValueObject)obj;
            IEnumerator<object> thisValues = this.GetAtomicValues().GetEnumerator();
            IEnumerator<object> otherValues = other.GetAtomicValues().GetEnumerator();

            while (thisValues.MoveNext() && otherValues.MoveNext())
            {
                // 和上面的 EqualOperator 思想一样
                if (ReferenceEquals(thisValues.Current, null) ^ ReferenceEquals(otherValues.Current, null))
                {
                    return false;
                }
                if (thisValues.Current != null && !thisValues.Current.Equals(otherValues.Current))
                {
                    return false;
                }
            }

            // 如果两个枚举器后面都没有了，说明他们个数一样，返回真
            return !thisValues.MoveNext() && !otherValues.MoveNext();
        }

        public override int GetHashCode()
        {
            return GetAtomicValues()
             .Select(x => x != null ? x.GetHashCode() : 0)
             .Aggregate((x, y) => x ^ y);
        }

        /// <summary>
        /// 浅克隆
        /// </summary>
        /// <returns></returns>
        public ValueObject Copy()
        {
            return this.MemberwiseClone() as ValueObject;
        }

        //public static bool operator ==(ValueObject left, ValueObject right)
        //{
        //    // 0^1=1 1^0=1 只有一个成立才为真 0^0=0 1^1=0 
        //    if (Object.ReferenceEquals(left, null) ^ Object.ReferenceEquals(right, null))
        //        return false;

        //    // 走到这里说明 要么都是null 要么都是非空
        //    // || 任意一个成立则为真
        //    // 如果第一个成立 说明都是null 返回真没问题
        //    // 如果第一个不成立 说明都不是null,第二局不会报错，并且只有第二个成立才会返回真 妙啊~~ 
        //    return Object.ReferenceEquals(left, null) || left.Equals(right);
        //}

        //public static bool operator !=(ValueObject left, ValueObject right)
        //{
        //    return !(EqualOperator(left, right));
        //}
    }
}
