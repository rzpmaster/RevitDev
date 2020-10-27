namespace Log4NetDemo.Core.Interface
{
    /// <summary>
    /// 提供一个方法,用于 延时激活 配置的信息
    /// </summary>
    public interface IOptionHandler
    {
        /// <summary>
        /// 激活 Options
        /// </summary>
        /// <remarks>
        /// <para>允许对象延迟激活其配置选项,知道设置了 all Options</para>
        /// <para>对于需要配置多个选项的对象来说,这个时非常必要的,因为在没有设置完成之后激活配置是不安全的</para>
        /// </remarks>
        void ActivateOptions();
    }
}
