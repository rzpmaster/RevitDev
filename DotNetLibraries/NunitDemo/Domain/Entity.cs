using System;

namespace NunitDemo.Domain
{
    /// <summary>
    /// 只比较 Id，如果Id相同，则判定他们相同
    /// </summary>
    public abstract class Entity
    {
        int? _requestedHashCode;
        int _id;
        public virtual int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public override bool Equals(object obj)
        {
            //确定不为空，并且是Entity或其子类
            if (obj == null || !(obj is Entity))
                return false;

            //是否指向同一个引用？
            if (Object.ReferenceEquals(this, obj))
                return true;

            //如果没有指向同一个引用，他们的类型是否一样？
            if (this.GetType() != obj.GetType())
                return false;

            //如果类型一样，比较Id
            Entity item = (Entity)obj;
            return item.Id == this.Id;
        }

        public override int GetHashCode()
        {
            if (!_requestedHashCode.HasValue)
                // _requestedHashCode = null;
                _requestedHashCode = this.Id.GetHashCode() ^ 31; // XOR for random distribution (http://blogs.msdn.com/b/ericlippert/archive/2011/02/28/guidelines-and-rules-for-gethashcode.aspx)

            return _requestedHashCode.Value;
        }

        public static bool operator ==(Entity left, Entity right)
        {
            if (Object.Equals(left, null))
                return (Object.Equals(right, null)) ? true : false;
            else
                return left.Equals(right);
        }

        public static bool operator !=(Entity left, Entity right)
        {
            return !(left == right);
        }
    }
}
