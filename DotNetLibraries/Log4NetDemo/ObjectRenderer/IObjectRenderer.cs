using System.IO;

namespace Log4NetDemo.ObjectRenderer
{
    /// <summary>
    /// 对象渲染器 接口
    /// </summary>
    /// <remarks>
    /// <para>某些特殊类型需要将特殊情况转换成 字符串，这个操作有这个接口完成</para>
    /// </remarks>
    public interface IObjectRenderer
    {
        /// <summary>
        /// 将一个对象转换成一个字符串
        /// </summary>
        /// <param name="rendererMap">用于查找的要转换对象使用的 IObjectRenderer 的映射地图，<seealso cref="RendererMap"/></param>
        /// <param name="obj">需要转换的对象</param>
        /// <param name="writer"></param>
        /// <remarks>
        /// <para><paramref name="rendererMap"/>非常有用，当待转换的对象<paramref name="obj"/>中含有特定类型的嵌套对象时，可以查找出用于转换字符串的特定的IObjectRenderer。
        /// 这个方法<see cref="RendererMap.FindAndRender(object, TextWriter)"/>就是在内部查找后，才调用查找得到的 IObjectRenderer 的这个方法的。</para>
        /// </remarks>
        void RenderObject(RendererMap rendererMap, object obj, TextWriter writer);
    }
}
