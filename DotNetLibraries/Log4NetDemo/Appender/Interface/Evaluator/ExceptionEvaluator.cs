using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Log4NetDemo.Core.Data;

namespace Log4NetDemo.Appender.Interface.Evaluator
{
    public class ExceptionEvaluator : ITriggeringEventEvaluator
    {
        private Type m_type;

        private bool m_triggerOnSubclass;

        public ExceptionEvaluator()
        {
            // empty
        }

        public ExceptionEvaluator(Type exType, bool triggerOnSubClass)
        {
            if (exType == null)
            {
                throw new ArgumentNullException("exType");
            }

            m_type = exType;
            m_triggerOnSubclass = triggerOnSubClass;
        }

        public Type ExceptionType
        {
            get { return m_type; }
            set { m_type = value; }
        }

        public bool TriggerOnSubclass
        {
            get { return m_triggerOnSubclass; }
            set { m_triggerOnSubclass = value; }
        }

        public bool IsTriggeringEvent(LoggingEvent loggingEvent)
        {
            if (loggingEvent == null)
            {
                throw new ArgumentNullException("loggingEvent");
            }

            if (m_triggerOnSubclass && loggingEvent.ExceptionObject != null)
            {
                // check if loggingEvent.ExceptionObject is of type ExceptionType or subclass of ExceptionType
                Type exceptionObjectType = loggingEvent.ExceptionObject.GetType();
                return exceptionObjectType == m_type || exceptionObjectType.IsSubclassOf(m_type);
            }
            else if (!m_triggerOnSubclass && loggingEvent.ExceptionObject != null)
            {   // check if loggingEvent.ExceptionObject is of type ExceptionType
                return loggingEvent.ExceptionObject.GetType() == m_type;
            }
            else
            {   // loggingEvent.ExceptionObject is null
                return false;
            }
        }
    }
}
