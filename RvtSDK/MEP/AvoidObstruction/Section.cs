using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvoidObstruction
{
    /// <summary>
    /// 有障碍物的断面
    /// 
    ///  ——   |     ————
    ///      \  |   /
    ///       \    /
    ///        ——
    ///        
    /// </summary>
    class Section
    {
        XYZ m_dir;
        double m_startFactor;
        double m_endFactor;

        /// <summary>
        /// 障碍物 
        /// (可以有多个,当多个障碍物相距较近时,无法生成多个弯头,可以放在一个断面处理)
        /// </summary>
        List<ReferenceWithContext> m_refs;
        /// <summary>
        /// 三条管线构造成一个 U 型避让障碍物,如上图
        /// </summary>
        List<Pipe> m_pipes;

        private Section(XYZ dir)
        {
            m_dir = dir;
            m_startFactor = 0;
            m_endFactor = 0;
            m_refs = new List<ReferenceWithContext>();
            m_pipes = new List<Pipe>();
        }

        public XYZ PipeCenterLineDirection
        {
            get { return m_dir; }
        }

        public List<Pipe> Pipes
        {
            get { return m_pipes; }
        }

        public XYZ Start
        {
            get
            {
                return m_refs[0].GetReference().GlobalPoint + m_dir * m_startFactor;
            }
        }

        public XYZ End
        {
            get
            {
                return m_refs[m_refs.Count - 1].GetReference().GlobalPoint + m_dir * m_endFactor;
            }
        }

        public List<ReferenceWithContext> Refs
        {
            get { return m_refs; }
        }

        /// <summary>
        /// 设置翻管点距离碰撞点的距离
        /// </summary>
        /// <param name="index">0 => start, 1 => end</param>
        public void Inflate(int index, double value)
        {
            if (index == 0)
            {
                m_startFactor -= value;
            }
            else if (index == 1)
            {
                m_endFactor += value;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Index should be 0 or 1.");
            }
        }

        /// <summary>
        /// 构造断面
        /// 两个 ReferenceWithContext 一进一出 正好对应一个 障碍物
        /// </summary>
        /// <param name="allrefs">与管线碰撞的所有障碍物</param>
        /// <param name="dir">管线方向</param>
        /// <returns></returns>
        public static List<Section> BuildSections(List<ReferenceWithContext> allrefs, XYZ dir)
        {
            List<ReferenceWithContext> buildStack = new List<ReferenceWithContext>();
            List<Section> sections = new List<Section>();
            Section current = null;
            foreach (ReferenceWithContext geoRef in allrefs)
            {
                if (buildStack.Count == 0)
                {
                    current = new Section(dir);
                    sections.Add(current);
                }

                current.Refs.Add(geoRef);

                //这里为什么要用一个栈呢？
                //因为之前找和当前管道碰撞的 ReferenceWithContext 的时候，找的是 face
                //两个 face 对应一个元素，所以当一个 ReferenceWithContext 进去后再出来，正好对应这一个障碍物
                ReferenceWithContext tmp = Find(buildStack, geoRef);
                if (tmp != null)
                {
                    buildStack.Remove(tmp);
                }
                else
                    buildStack.Add(geoRef);
            }

            return sections;
        }

        /// <summary>
        /// 判断障碍物是否已经在集合中,返回找到的值
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        private static ReferenceWithContext Find(List<ReferenceWithContext> arr, ReferenceWithContext entry)
        {
            foreach (ReferenceWithContext tmp in arr)
            {
                if (tmp.GetReference().ElementId == entry.GetReference().ElementId)
                {
                    return tmp;
                }
            }
            return null;
        }
    }
}
