namespace Log4NetDemo.Appender.AppenderAttach
{
    /// <summary>
    /// 用于 附加、删除、检索 Appender 的接口(用于实现 Logger 和 Appender 的一对多关系)
    /// </summary>
    public interface IAppenderAttachable
    {
        AppenderCollection Appenders { get; }

        void AddAppender(IAppender appender);

        IAppender GetAppender(string name);

        void RemoveAllAppenders();

        IAppender RemoveAppender(IAppender appender);

        IAppender RemoveAppender(string name);
    }
}
