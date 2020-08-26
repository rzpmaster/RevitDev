namespace Log4NetDemo.Repository
{
    public interface IBasicRepositoryConfigurator
    {
        void Configure(Appender.IAppender appender);

        void Configure(params Appender.IAppender[] appenders);
    }
}
