namespace Log4NetDemo.Context
{
    /// <summary>
    /// ContextProperties 属性字典的基类
    /// </summary>
    /// <remarks>
    /// <para>
    /// 定义了一个索引器
    /// </para>
    /// </remarks>
    public abstract class ContextPropertiesBase
    {
        public abstract object this[string key] { get; set; }
    }
}
