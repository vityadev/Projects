using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PWOOG
{
    partial class PWClient
    {
        public List<PlayChar> Chars = new List<PlayChar>();
        public PlayChar SelectedChar { get; private set; }

        public void SelectChar(PlayChar selectedChar)
        {
            if (!Chars.Contains(selectedChar))
                throw new ArgumentException("Данного персонажа не существует");
            SelectedChar = selectedChar;
            p_SelectRole(SelectedChar.Id);
        }

        public void SendPublicMessage(uint senderId, string senderNick,string str)
        {
            p_PublicMessage(new ChatMessage { SenderUID = senderId, SenderNick = senderNick, Message = str });
        }

        public void SendLocalMessage(uint senderId, string str)
        {
            p_LocalMessage(new ChatMessage { SenderUID = senderId, Message = str });
        }

        public void SendPrivateMessage(uint senderId, string senderNick, uint receiverId, string receiverNick, string str)
        {
            p_PrivateMessage(new ChatMessage { SenderUID = senderId, SenderNick = senderNick, ReceiverNick = receiverNick , ReceiverUID = receiverId, Message = str });
        }
    }
}
