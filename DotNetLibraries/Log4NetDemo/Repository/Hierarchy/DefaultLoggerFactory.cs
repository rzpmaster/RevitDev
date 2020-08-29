using Log4NetDemo.Core.Data;
using Log4NetDemo.Util;
using System;

namespace Log4NetDemo.Repository.Hierarchy
{
    class DefaultLoggerFactory : ILoggerFactory
    {
        internal DefaultLoggerFactory()
        {
        }

        #region Implementation of ILoggerFactory

        /// <summary>
        /// 创建一个 Logger 实例
        /// </summary>
        /// <param name="repository">将要属于哪个 ILoggerRepository</param>
        /// <param name="name">名字</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>如果<paramref name="name"/>是空，则会创建一个根节点 Logger</para>
        /// </remarks>
        public Logger CreateLogger(ILoggerRepository repository, string name)
        {
            if (name == null)
            {
                return new RootLogger(repository.LevelMap.LookupWithDefault(Level.Debug));
            }
            return new LoggerImpl(name);
        }

        #endregion

        internal sealed class LoggerImpl : Logger
        {
            internal LoggerImpl(string name) : base(name)
            {
            }
        }

        public class RootLogger : Logger
        {
            public RootLogger(Level level) : base("root")
            {
                this.Level = level;
            }

            override public Level EffectiveLevel
            {
                get
                {
                    return base.Level;
                }
            }

            override public Level Level
            {
                get { return base.Level; }
                set
                {
                    if (value == null)
                    {
                        LogLog.Error(declaringType, "You have tried to set a null level to root.", new LogException());
                    }
                    else
                    {
                        base.Level = value;
                    }
                }
            }

            private readonly static Type declaringType = typeof(RootLogger);
        }

    }
}
