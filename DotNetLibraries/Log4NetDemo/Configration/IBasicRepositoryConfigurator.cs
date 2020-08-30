namespace Log4NetDemo.Configration
{
    public interface IBasicRepositoryConfigurator
    {
        void Configure(Appender.IAppender appender);

        void Configure(params Appender.IAppender[] appenders);
    }
}
