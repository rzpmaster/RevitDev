namespace Log4NetDemo.Core.Interface
{
    /// <summary>
    /// 
    /// </summary>
    public interface IOptionHandler
    {
        /// <summary>
        /// 激活 Options
        /// </summary>
        /// <remarks>
        /// <para>
        /// 如果实现了此接口，必须在设置完属性之后调用此方法
        /// </para>
        /// </remarks>
        void ActivateOptions();
    }
}
