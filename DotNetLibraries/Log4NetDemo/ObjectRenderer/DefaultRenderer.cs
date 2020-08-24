using Log4NetDemo.Util;
using System;
using System.Collections;
using System.IO;

namespace Log4NetDemo.ObjectRenderer
{
    public sealed class DefaultRenderer : IObjectRenderer
    {
        public DefaultRenderer()
        {
        }

        #region Implementation of IObjectRenderer

        public void RenderObject(RendererMap rendererMap, object obj, TextWriter writer)
        {
            if (rendererMap == null)
            {
                throw new ArgumentNullException("rendererMap");
            }

            if (obj == null)
            {
                writer.Write(SystemInfo.NullText);
                return;
            }

            Array objArray = obj as Array;
            if (objArray != null)
            {
                RenderArray(rendererMap, objArray, writer);
                return;
            }

            // Test if we are dealing with some form of collection object
            IEnumerable objEnumerable = obj as IEnumerable;
            if (objEnumerable != null)
            {
                // Get a collection interface if we can as its .Count property may be more
                // performant than getting the IEnumerator object and trying to advance it.
                ICollection objCollection = obj as ICollection;
                if (objCollection != null && objCollection.Count == 0)
                {
                    writer.Write("{}");
                    return;
                }

                // This is a special check to allow us to get the enumerator from the IDictionary
                // interface as this guarantees us DictionaryEntry objects. Note that in .NET 2.0
                // the generic IDictionary<> interface enumerates KeyValuePair objects rather than
                // DictionaryEntry ones. However the implementation of the plain IDictionary 
                // interface on the generic Dictionary<> still returns DictionaryEntry objects.
                IDictionary objDictionary = obj as IDictionary;
                if (objDictionary != null)
                {
                    RenderEnumerator(rendererMap, objDictionary.GetEnumerator(), writer);
                    return;
                }

                RenderEnumerator(rendererMap, objEnumerable.GetEnumerator(), writer);
                return;
            }

            IEnumerator objEnumerator = obj as IEnumerator;
            if (objEnumerator != null)
            {
                RenderEnumerator(rendererMap, objEnumerator, writer);
                return;
            }

            if (obj is DictionaryEntry)
            {
                RenderDictionaryEntry(rendererMap, (DictionaryEntry)obj, writer);
                return;
            }

            string str = obj.ToString();
            writer.Write((str == null) ? SystemInfo.NullText : str);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rendererMap"></param>
        /// <param name="array"></param>
        /// <param name="writer"></param>
        /// <remarks>
		/// <para>
		/// For a one dimensional array this is the
		///	array type name, an open brace, followed by a comma
		///	separated list of the elements (using the appropriate
		///	renderer), followed by a close brace. For example:
		///	<c>int[] {1, 2, 3}</c>.
		///	</para>
		///	<para>
		///	If the array is not one dimensional the 
		///	<c>Array.ToString()</c> is returned.
		///	</para>
		/// </remarks>
        private void RenderArray(RendererMap rendererMap, Array array, TextWriter writer)
        {
            if (array.Rank != 1)
            {
                writer.Write(array.ToString());
            }
            else
            {
                writer.Write(array.GetType().Name + " {");
                int len = array.Length;

                if (len > 0)
                {
                    rendererMap.FindAndRender(array.GetValue(0), writer);
                    for (int i = 1; i < len; i++)
                    {
                        writer.Write(", ");
                        rendererMap.FindAndRender(array.GetValue(i), writer);
                    }
                }
                writer.Write("}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rendererMap"></param>
        /// <param name="enumerator"></param>
        /// <param name="writer"></param>
        /// <remarks>
		/// <para>
		/// Rendered as an open brace, followed by a comma
		///	separated list of the elements (using the appropriate
		///	renderer), followed by a close brace. For example:
		///	<c>{a, b, c}</c>.
		///	</para>
		/// </remarks>
        private void RenderEnumerator(RendererMap rendererMap, IEnumerator enumerator, TextWriter writer)
        {
            writer.Write("{");

            if (enumerator != null && enumerator.MoveNext())
            {
                rendererMap.FindAndRender(enumerator.Current, writer);

                while (enumerator.MoveNext())
                {
                    writer.Write(", ");
                    rendererMap.FindAndRender(enumerator.Current, writer);
                }
            }

            writer.Write("}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rendererMap"></param>
        /// <param name="entry"></param>
        /// <param name="writer"></param>
        /// <remarks>
		/// <para>
		/// Render the key, an equals sign ('='), and the value (using the appropriate
		///	renderer). For example: <c>key=value</c>.
		///	</para>
		/// </remarks>
        private void RenderDictionaryEntry(RendererMap rendererMap, DictionaryEntry entry, TextWriter writer)
        {
            rendererMap.FindAndRender(entry.Key, writer);
            writer.Write("=");
            rendererMap.FindAndRender(entry.Value, writer);
        }
    }
}
