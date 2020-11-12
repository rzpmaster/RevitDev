using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Threading;
using MessagingDemo.Common;

namespace MessagingDemo
{
    public class Messenger : IMessenger
    {
        private static readonly object CreationLock = new object();
        private readonly object _registerLock = new object();

        private static IMessenger _defaultInstance;

        private Dictionary<Type, List<WeakActionAndToken>> _recipientsOfSubclassesAction;
        private Dictionary<Type, List<WeakActionAndToken>> _recipientsStrictAction;

        public static IMessenger Default
        {
            get
            {
                if (_defaultInstance == null)
                {
                    lock (CreationLock)
                    {
                        if (_defaultInstance == null)
                        {
                            _defaultInstance = new Messenger();
                        }
                    }
                }

                return _defaultInstance;
            }
        }

        #region IMessenger Members

        public virtual void Register<TMessage>(object recipient, Action<TMessage> action, bool keepTargetAlive = false)
        {
            Register(recipient, null, false, action, keepTargetAlive);
        }

        public virtual void Register<TMessage>(object recipient, object token, Action<TMessage> action, bool keepTargetAlive = false)
        {
            Register(recipient, token, false, action, keepTargetAlive);
        }

        public virtual void Register<TMessage>(object recipient, object token, bool receiveDerivedMessagesToo, Action<TMessage> action, bool keepTargetAlive = false)
        {
            lock (_registerLock)
            {
                var messageType = typeof(TMessage);

                Dictionary<Type, List<WeakActionAndToken>> recipients;

                if (receiveDerivedMessagesToo)
                {
                    if (_recipientsOfSubclassesAction == null)
                    {
                        _recipientsOfSubclassesAction = new Dictionary<Type, List<WeakActionAndToken>>();
                    }

                    recipients = _recipientsOfSubclassesAction;
                }
                else
                {
                    if (_recipientsStrictAction == null)
                    {
                        _recipientsStrictAction = new Dictionary<Type, List<WeakActionAndToken>>();
                    }

                    recipients = _recipientsStrictAction;
                }

                lock (recipients)
                {
                    List<WeakActionAndToken> list;

                    if (!recipients.ContainsKey(messageType))
                    {
                        list = new List<WeakActionAndToken>();
                        recipients.Add(messageType, list);
                    }
                    else
                    {
                        list = recipients[messageType];
                    }

                    var weakAction = new WeakAction<TMessage>(recipient, action, keepTargetAlive);

                    var item = new WeakActionAndToken
                    {
                        Action = weakAction,
                        Token = token
                    };

                    list.Add(item);
                }
            }

            RequestCleanup();
        }

        public virtual void Register<TMessage>(object recipient, bool receiveDerivedMessagesToo, Action<TMessage> action, bool keepTargetAlive = false)
        {
            Register(recipient, null, receiveDerivedMessagesToo, action, keepTargetAlive);
        }


        public virtual void Send<TMessage>(TMessage message)
        {
            SendToTargetOrType(message, null, null);
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "This syntax is more convenient than other alternatives.")]
        public virtual void Send<TMessage, TTarget>(TMessage message)
        {
            SendToTargetOrType(message, typeof(TTarget), null);
        }

        public virtual void Send<TMessage>(TMessage message, object token)
        {
            SendToTargetOrType(message, null, token);
        }


        public virtual void Unregister(object recipient)
        {
            UnregisterFromLists(recipient, _recipientsOfSubclassesAction);
            UnregisterFromLists(recipient, _recipientsStrictAction);
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "This syntax is more convenient than other alternatives.")]
        public virtual void Unregister<TMessage>(object recipient)
        {
            Unregister<TMessage>(recipient, null, null);
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "This syntax is more convenient than other alternatives.")]
        public virtual void Unregister<TMessage>(object recipient, object token)
        {
            Unregister<TMessage>(recipient, token, null);
        }

        public virtual void Unregister<TMessage>(object recipient, Action<TMessage> action)
        {
            Unregister(recipient, null, action);
        }

        public virtual void Unregister<TMessage>(object recipient, object token, Action<TMessage> action)
        {
            UnregisterFromLists(recipient, token, action, _recipientsStrictAction);
            UnregisterFromLists(recipient, token, action, _recipientsOfSubclassesAction);
            RequestCleanup();
        }

        #endregion

        public static void OverrideDefault(IMessenger newMessenger)
        {
            _defaultInstance = newMessenger;
        }

        public static void Reset()
        {
            _defaultInstance = null;
        }

        [SuppressMessage(
            "Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "Non static access is needed.")]
        public void ResetAll()
        {
            Reset();
        }


        #region Cleanup

        /// <summary>
        /// 请求清理
        /// 扫描并清理 收件人action 列表
        /// 由于 收件人action 保存的是弱引用,所以即使有人引用也可以被清理
        /// 在清理期间将会删除所有已经被回收的收件人
        /// 此操作可能需要时间,一般在空闲的时候调用, Messenger 应该使用他,但是不是强求的
        /// </summary>
        public void RequestCleanup()
        {
            if (!_isCleanupRegistered)
            {
                Action cleanupAction = Cleanup;

                Dispatcher.CurrentDispatcher.BeginInvoke(
                    cleanupAction,
                    DispatcherPriority.ApplicationIdle,
                    null);

                _isCleanupRegistered = true;
            }
        }

        /// <summary>
        /// 清除扫描收件人列表，并恢复至最初状态
        /// </summary>
        public void Cleanup()
        {
            CleanupList(_recipientsOfSubclassesAction);
            CleanupList(_recipientsStrictAction);
            _isCleanupRegistered = false;
        }

        private bool _isCleanupRegistered;

        private void CleanupList(IDictionary<Type, List<WeakActionAndToken>> lists)
        {
            if (lists == null)
            {
                return;
            }

            lock (lists)
            {
                var listsToRemove = new List<Type>();
                foreach (var list in lists)
                {
                    var recipientsToRemove = list.Value
                        .Where(item => item.Action == null || !item.Action.IsAlive)
                        .ToList();

                    foreach (var recipient in recipientsToRemove)
                    {
                        list.Value.Remove(recipient);
                    }

                    if (list.Value.Count == 0)
                    {
                        listsToRemove.Add(list.Key);
                    }
                }

                foreach (var key in listsToRemove)
                {
                    lists.Remove(key);
                }
            }
        }

        #endregion


        #region private Members

        private void SendToTargetOrType<TMessage>(TMessage message, Type messageTargetType, object token)
        {
            var messageType = typeof(TMessage);

            if (_recipientsOfSubclassesAction != null)
            {
                // Clone to protect from people registering in a "receive message" method
                // Correction Messaging BL0008.002
                var listClone =
                    _recipientsOfSubclassesAction.Keys.Take(_recipientsOfSubclassesAction.Count()).ToList();

                foreach (var type in listClone)
                {
                    List<WeakActionAndToken> list = null;

                    if (messageType == type
                        || messageType.IsSubclassOf(type)
                        || type.IsAssignableFrom(messageType))
                    {
                        lock (_recipientsOfSubclassesAction)
                        {
                            list = _recipientsOfSubclassesAction[type].Take(_recipientsOfSubclassesAction[type].Count()).ToList();
                        }
                    }

                    SendToList(message, list, messageTargetType, token);
                }
            }

            if (_recipientsStrictAction != null)
            {
                List<WeakActionAndToken> list = null;

                lock (_recipientsStrictAction)
                {
                    if (_recipientsStrictAction.ContainsKey(messageType))
                    {
                        list = _recipientsStrictAction[messageType]
                            .Take(_recipientsStrictAction[messageType].Count())
                            .ToList();
                    }
                }

                if (list != null)
                {
                    SendToList(message, list, messageTargetType, token);
                }
            }

            RequestCleanup();
        }

        private void SendToList<TMessage>(TMessage message, IEnumerable<WeakActionAndToken> weakActionsAndTokens, Type messageTargetType, object token)
        {
            if (weakActionsAndTokens != null)
            {
                // Clone to protect from people registering in a "receive message" method
                // Correction Messaging BL0004.007
                var list = weakActionsAndTokens.ToList();
                var listClone = list.Take(list.Count()).ToList();

                foreach (var item in listClone)
                {
                    var executeAction = item.Action as IExecuteWithObject;

                    if (executeAction != null
                        && item.Action.IsAlive
                        && item.Action.Target != null
                        && (messageTargetType == null
                            || item.Action.Target.GetType() == messageTargetType
                            || messageTargetType.IsAssignableFrom(item.Action.Target.GetType()))
                        && ((item.Token == null && token == null)
                            || item.Token != null && item.Token.Equals(token)))
                    {
                        executeAction.ExecuteWithObject(message);
                    }
                }
            }
        }


        private void UnregisterFromLists(object recipient, Dictionary<Type, List<WeakActionAndToken>> lists)
        {
            if (recipient == null
                || lists == null
                || lists.Count == 0)
            {
                return;
            }

            lock (lists)
            {
                foreach (var messageType in lists.Keys)
                {
                    foreach (var item in lists[messageType])
                    {
                        var weakAction = (IExecuteWithObject)item.Action;

                        if (weakAction != null
                            && recipient == weakAction.Target)
                        {
                            weakAction.MarkForDeletion();
                        }
                    }
                }
            }
        }

        private void UnregisterFromLists<TMessage>(object recipient, object token, Action<TMessage> action, Dictionary<Type, List<WeakActionAndToken>> lists)
        {
            var messageType = typeof(TMessage);

            if (recipient == null
                || lists == null
                || lists.Count == 0
                || !lists.ContainsKey(messageType))
            {
                return;
            }

            lock (lists)
            {
                foreach (var item in lists[messageType])
                {
                    var weakActionCasted = item.Action as WeakAction<TMessage>;

                    if (weakActionCasted != null
                        && recipient == weakActionCasted.Target
                        && (action == null
                            || action.Method.Name == weakActionCasted.MethodName)
                        && (token == null
                            || token.Equals(item.Token)))
                    {
                        item.Action.MarkForDeletion();
                    }
                }
            }
        }

        #endregion

        #region Nested type: WeakActionAndToken

        private struct WeakActionAndToken
        {
            public WeakAction Action;

            public object Token;
        }

        #endregion
    }
}
