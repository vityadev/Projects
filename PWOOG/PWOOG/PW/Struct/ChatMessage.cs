using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PWOOG
{
    /// <summary>
    /// Тип сообщения для чата
    /// </summary>
    public enum ChatMessageType
    {
        /// <summary>
        /// Приватное сообщение (персонаж персонажу)
        /// </summary>
        Private,
        /// <summary>
        /// Локальное сообщение (вокруг персонажа)
        /// </summary>
        Local,
        /// <summary>
        /// Глобальное сообщение (мировое)
        /// </summary>
        World
    }

    /// <summary>
    /// Сообщение
    /// </summary>
    public struct ChatMessage
    {
        /// <summary>
        /// Тип сообщения
        /// </summary>
        public ChatMessageType Type;
        /// <summary>
        /// Ник получателя
        /// </summary>
        public string ReceiverNick;
        /// <summary>
        /// Ид получателя
        /// </summary>
        public uint ReceiverUID;
        /// <summary>
        /// Ник отправителя
        /// </summary>
        public string SenderNick;
        /// <summary>
        /// Ид отправителя
        /// </summary>
        public uint SenderUID;
        /// <summary>
        /// Сообщение
        /// </summary>
        public string Message;
    }
}
