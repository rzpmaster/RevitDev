using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingDemo
{
    public interface IMessenger
    {
        #region Register

        /// <summary>
        /// 注册消息类型为 TMessage 的收件人
        /// </summary>
        /// <typeparam name="TMessage">消息</typeparam>
        /// <param name="recipient">收件人</param>
        /// <param name="action">接收消息时的Action</param>
        /// <param name="keepTargetAlive"></param>
        void Register<TMessage>(object recipient, Action<TMessage> action, bool keepTargetAlive = false);

        /// <summary>
        /// 注册消息类型为 TMessage 的收件人
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="recipient">收件人</param>
        /// <param name="token">token</param>
        /// <param name="action"></param>
        /// <param name="keepTargetAlive"></param>
        void Register<TMessage>(object recipient, object token, Action<TMessage> action, bool keepTargetAlive = false);

        /// <summary>
        /// 注册消息类型为 TMessage 的收件人
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="recipient">收件人</param>
        /// <param name="token">token</param>
        /// <param name="receiveDerivedMessagesToo">是否将消息也发给收件人</param>
        /// <param name="action"></param>
        /// <param name="keepTargetAlive"></param>
        void Register<TMessage>(object recipient, object token, bool receiveDerivedMessagesToo, Action<TMessage> action, bool keepTargetAlive = false);

        /// <summary>
        /// 注册消息类型为 TMessage 的收件人
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="recipient">收件人</param>
        /// <param name="receiveDerivedMessagesToo">是否将消息也发给收件人<</param>
        /// <param name="action"></param>
        /// <param name="keepTargetAlive"></param>
        void Register<TMessage>(object recipient, bool receiveDerivedMessagesToo, Action<TMessage> action, bool keepTargetAlive = false);

        #endregion

        #region Send

        /// <summary>
        /// 向所有已注册的收件人发消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="message"></param>
        void Send<TMessage>(TMessage message);

        /// <summary>
        /// 向目标发消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="message"></param>
        [SuppressMessage(
            "Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "This syntax is more convenient than other alternatives.")]
        void Send<TMessage, TTarget>(TMessage message);

        /// <summary>
        /// 向具有指定token的收件人发送消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="message"></param>
        /// <param name="token"></param>
        void Send<TMessage>(TMessage message, object token);

        #endregion

        #region Unregister

        /// <summary>
        /// 注销收件人,他将收不到任何类型的消息
        /// </summary>
        /// <param name="recipient"></param>
        void Unregister(object recipient);

        /// <summary>
        /// 注销收件人的 TMessage 类型的消息,他将收不到该类型的消息,但是他任然会收到其他类型的消息(如果注册过)
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="recipient"></param>
        [SuppressMessage(
            "Microsoft.Design",
            "CA1004:GenericMethodsShouldProvideTypeParameter",
            Justification = "This syntax is more convenient than other alternatives.")]
        void Unregister<TMessage>(object recipient);

        /// <summary>
        /// 注销收件人 给定 Action 的 TMessage 类型的消息,其他类型的消息任然会接受到(如果注册过)
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="recipient"></param>
        /// <param name="action"></param>
        void Unregister<TMessage>(object recipient, Action<TMessage> action);

        /// <summary>
        /// 注销收件人 给定 token 给定 Action 的 TMessage 类型的消息,其他类型的消息任然会接受到(如果注册过)
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="recipient"></param>
        /// <param name="token"></param>
        /// <param name="action"></param>
        void Unregister<TMessage>(object recipient, object token, Action<TMessage> action);

        #endregion
    }
}
