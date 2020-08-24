using Log4NetDemo.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Log4NetDemo.ObjectRenderer
{
    /// <summary>
    /// 对象 对象渲染器 的 映射字典
    /// key:Type value:IObjectRenderer
    /// </summary>
    /// <remarks>
    /// <para>维护一个对象和这个对象类型的 IObjectRenderer 的映射字典</para>
    /// </remarks>
    public class RendererMap
    {
        private System.Collections.Hashtable m_map;
        private System.Collections.Hashtable m_cache = new System.Collections.Hashtable();

        private static IObjectRenderer s_defaultRenderer = new DefaultRenderer();

        public RendererMap()
        {
            m_map = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
        }

        /// <summary>
        /// 将对象转换为字符串（）
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public string FindAndRender(object obj)
        {
            // Optimisation for strings
            string strData = obj as String;
            if (strData != null)
            {
                return strData;
            }

            StringWriter stringWriter = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);
            FindAndRender(obj, stringWriter);
            return stringWriter.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="writer"></param>
        public void FindAndRender(object obj, TextWriter writer)
        {
            if (obj == null)
            {
                writer.Write(SystemInfo.NullText);
            }
            else
            {
                // Optimisation for strings
                string str = obj as string;
                if (str != null)
                {
                    writer.Write(str);
                }
                else
                {
                    // Lookup the renderer for the specific type
                    try
                    {
                        Get(obj.GetType()).RenderObject(this, obj, writer);
                    }
                    catch (Exception ex)
                    {
                        // Exception rendering the object
                        LogLog.Error(declaringType, "Exception while rendering object of type [" + obj.GetType().FullName + "]", ex);

                        // return default message
                        string objectTypeName = "";
                        if (obj != null && obj.GetType() != null)
                        {
                            objectTypeName = obj.GetType().FullName;
                        }

                        writer.Write("<log4net.Error>Exception rendering object type [" + objectTypeName + "]");
                        if (ex != null)
                        {
                            string exceptionText = null;

                            try
                            {
                                exceptionText = ex.ToString();
                            }
                            catch
                            {
                                // Ignore exception
                            }

                            writer.Write("<stackTrace>" + exceptionText + "</stackTrace>");
                        }
                        writer.Write("</log4net.Error>");
                    }
                }
            }
        }

        /// <summary>
        /// 根据类型找到合适的 IObjectRenderer
        /// 现在缓存中找，缓存中没找到就在哈希表中找，还没找到就返回默认值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IObjectRenderer Get(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            IObjectRenderer result = null;

            // Check cache
            result = (IObjectRenderer)m_cache[type];

            if (result == null)
            {
                for (Type cur = type; cur != null; cur = cur.BaseType)
                {
                    // Search the type's interfaces
                    result = SearchTypeAndInterfaces(cur);
                    if (result != null)
                    {
                        break;
                    }
                }

                // if not set then use the default renderer
                if (result == null)
                {
                    result = s_defaultRenderer;
                }

                // Add to cache
                m_cache[type] = result;
            }

            return result;
        }

        public IObjectRenderer DefaultRenderer
        {
            get { return s_defaultRenderer; }
        }

        /// <summary>
        /// Clear the custom renderers defined by using <see cref="Put"/>.
        /// </summary>
        public void Clear()
        {
            m_map.Clear();
            m_cache.Clear();
        }

        /// <summary>
        /// Register an <see cref="IObjectRenderer"/> for <paramref name="typeToRender"/>
        /// 并且存入内部的哈希表，下次这个类型 转换字符串时，就会使用这个 IObjectRenderer。
        /// </summary>
        /// <param name="typeToRender"></param>
        /// <param name="renderer"></param>
        /// <remarks>
        /// 注册一个自定义的对象渲染器 IObjectRenderer 
        /// This renderer will be returned from a call to <see cref="Get(Type)"/>specifying the same <paramref name="typeToRender"/> as an argument.
        /// /remarks>
        public void Put(Type typeToRender, IObjectRenderer renderer)
        {
            m_cache.Clear();

            if (typeToRender == null)
            {
                throw new ArgumentNullException("typeToRender");
            }
            if (renderer == null)
            {
                throw new ArgumentNullException("renderer");
            }

            m_map[typeToRender] = renderer;
        }


        /// <summary>
        /// 递归搜索类型是否存储着特定的转换 IObjectRenderer
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private IObjectRenderer SearchTypeAndInterfaces(Type type)
        {
            IObjectRenderer r = (IObjectRenderer)m_map[type];
            if (r != null)
            {
                return r;
            }
            else
            {
                foreach (Type t in type.GetInterfaces())
                {
                    r = SearchTypeAndInterfaces(t);
                    if (r != null)
                    {
                        return r;
                    }
                }
            }
            return null;
        }

        private readonly static Type declaringType = typeof(RendererMap);
    }
}
