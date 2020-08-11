namespace Log4NetDemo.Context
{
    public abstract class ContextPropertiesBase
    {
        public abstract object this[string key] { get; set; }
    }
}
