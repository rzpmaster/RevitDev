using System;
using System.Diagnostics.CodeAnalysis;

namespace MessagingDemo.Common
{
    public class WeakFunc<T, TResult> : WeakFunc<TResult>, IExecuteWithObjectAndResult
    {
        private Func<T, TResult> _staticFunc;

        public override string MethodName
        {
            get
            {
                if (_staticFunc != null)
                {
                    return _staticFunc.Method.Name;
                }

                return Method.Name;
            }
        }

        public override bool IsAlive
        {
            get
            {
                if (_staticFunc == null
                    && Reference == null)
                {
                    return false;
                }

                if (_staticFunc != null)
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

        public WeakFunc(Func<T, TResult> func, bool keepTargetAlive = false)
            : this(func == null ? null : func.Target, func, keepTargetAlive)
        {
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1062:Validate arguments of public methods",
            MessageId = "1",
            Justification = "Method should fail with an exception if func is null.")]
        public WeakFunc(object target, Func<T, TResult> func, bool keepTargetAlive = false)
        {
            if (func.Method.IsStatic)
            {
                _staticFunc = func;

                if (target != null)
                {
                    // Keep a reference to the target to control the
                    // WeakAction's lifetime.
                    Reference = new WeakReference(target);
                }

                return;
            }

            Method = func.Method;
            FuncReference = new WeakReference(func.Target);

            LiveReference = keepTargetAlive ? func.Target : null;
            Reference = new WeakReference(target);

#if DEBUG
            if (FuncReference != null
                && FuncReference.Target != null
                && !keepTargetAlive)
            {
                var type = FuncReference.Target.GetType();

                if (type.Name.StartsWith("<>")
                    && type.Name.Contains("DisplayClass"))
                {
                    System.Diagnostics.Debug.WriteLine(
                        "You are attempting to register a lambda with a closure without using keepTargetAlive. Are you sure? Check http://galasoft.ch/s/mvvmweakaction for more info.");
                }
            }
#endif
        }

        public override TResult Execute()
        {
            return Execute(default(T));
        }

        public TResult Execute(T parameter)
        {
            if (_staticFunc != null)
            {
                return _staticFunc(parameter);
            }

            var funcTarget = FuncTarget;

            if (IsAlive)
            {
                if (Method != null
                    && (LiveReference != null
                        || FuncReference != null)
                    && funcTarget != null)
                {
                    return (TResult)Method.Invoke(
                        funcTarget,
                        new object[]
                        {
                            parameter
                        });
                }

            }

            return default(TResult);
        }

        public object ExecuteWithObject(object parameter)
        {
            var parameterCasted = (T)parameter;
            return Execute(parameterCasted);
        }

        public override void MarkForDeletion()
        {
            _staticFunc = null;
            base.MarkForDeletion();
        }
    }
}
