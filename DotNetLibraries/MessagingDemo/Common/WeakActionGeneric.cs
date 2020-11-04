using System;
using System.Diagnostics.CodeAnalysis;

namespace MessagingDemo.Common
{
    class WeakAction<T> : WeakAction, IExecuteWithObject
    {
        private Action<T> _staticAction;

        public WeakAction(Action<T> action, bool keepTargetAlive = false)
            : this(action == null ? null : action.Target, action, keepTargetAlive)
        {
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1062:Validate arguments of public methods",
            MessageId = "1",
            Justification = "Method should fail with an exception if action is null.")]
        public WeakAction(object target, Action<T> action, bool keepTargetAlive = false)
        {
            if (action.Method.IsStatic)
            {
                _staticAction = action;

                if (target != null)
                {
                    // Keep a reference to the target to control the
                    // WeakAction's lifetime.
                    Reference = new WeakReference(target);
                }

                return;
            }


            Method = action.Method;
            ActionReference = new WeakReference(action.Target);
            LiveReference = keepTargetAlive ? action.Target : null;
            Reference = new WeakReference(target);

#if DEBUG
            if (ActionReference != null
                && ActionReference.Target != null
                && !keepTargetAlive)
            {
                var type = ActionReference.Target.GetType();

                if (type.Name.StartsWith("<>")
                    && type.Name.Contains("DisplayClass"))
                {
                    System.Diagnostics.Debug.WriteLine(
                        "You are attempting to register a lambda with a closure without using keepTargetAlive. Are you sure? Check http://galasoft.ch/s/mvvmweakaction for more info.");
                }
            }
#endif
        }

        public override string MethodName
        {
            get
            {
                if (_staticAction != null)
                {
                    return _staticAction.Method.Name;
                }

                return Method.Name;
            }

        }

        public override bool IsAlive
        {
            get
            {
                if (_staticAction == null
                    && Reference == null)
                {
                    return false;
                }

                if (_staticAction != null)
                {
                    if (Reference != null)
                    {
                        return Reference.IsAlive;
                    }

                    return true;
                }

                return Reference.IsAlive;
            }
        }

        public override void Execute()
        {
            Execute(default(T));
        }

        public void Execute(T parameter)
        {
            if (_staticAction != null)
            {
                _staticAction(parameter);
                return;
            }

            var actionTarget = ActionTarget;

            if (IsAlive)
            {
                if (Method != null
                    && (LiveReference != null
                        || ActionReference != null)
                    && actionTarget != null)
                {
                    Method.Invoke(
                        actionTarget,
                        new object[]
                        {
                            parameter
                        });
                }
            }
        }

        #region IExecuteWithObject Members

        public void ExecuteWithObject(object parameter)
        {
            var parameterCasted = (T)parameter;
            Execute(parameterCasted);
        }

        public override void MarkForDeletion()
        {
            _staticAction = null;
            base.MarkForDeletion();
        }

        #endregion

    }
}
