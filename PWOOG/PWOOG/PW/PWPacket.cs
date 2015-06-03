using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Reflection;

namespace PWOOG
{
    partial class PWClient
    {
        private class PacketAttribute : Attribute
        {
            public ushort Header;
            public bool IsContainer;

            private PacketAttribute() { }

            public PacketAttribute(ushort header, bool isContainer)
            {
                this.Header = header;
                this.IsContainer = isContainer;
            }
        }

        private delegate void PacketHandler(NetStream packet);

        private Dictionary<PacketAttribute, PacketHandler> pAttribute_pHandler;

        private void p(NetStream packet)
        {
            foreach (var elm in pAttribute_pHandler)
            {
                if (elm.Key.Header == packet.Header && elm.Key.IsContainer == packet.IsContainer)
                {
                    try { elm.Value(packet); }
                    catch { }
                    return;
                }
            }
        }

        private void p_Inicialization()
        {
            pAttribute_pHandler = new Dictionary<PacketAttribute, PacketHandler>(ushort.MaxValue);

            var phType = typeof(PacketHandler);
            var paType = typeof(PacketAttribute);

            var funcArr = typeof(PWClient).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var func in funcArr)
            {
                var atrbArr = func.GetCustomAttributes(paType, false);
                if (atrbArr.Length == 1)
                {
                    var atrb = (PacketAttribute)atrbArr[0];
                    pAttribute_pHandler.Add(atrb, (PacketHandler)Delegate.CreateDelegate(phType, this, func));
                }
            }
        }

        private Timer KeepAliveTimer;

        private void StartKeepAliveTimer()
        {
            IsCharsReceived = true;
            KeepAliveTimer = new Timer();
            KeepAliveTimer.Interval = 15000 + new Random().Next(-2000, +2000);
            KeepAliveTimer.Elapsed += new ElapsedEventHandler((object obj, ElapsedEventArgs e) => { p_C2SKeepAlive(); });
            KeepAliveTimer.Start();
        }

        private void p_LogginAnnounce(string login, byte[] hash)
        {
            PacketBuilder packet = new PacketBuilder(2, false);
            packet.AddASCII(login);
            packet.AddHex(hash);
            SendAsync(packet);
        }

        private void p_CMKey(bool force, byte[] dechash)
        {
            PacketBuilder packet = new PacketBuilder(3, false);
            packet.AddHex(dechash);
            packet.Add(Convert.ToByte(force));
            SendAsync(packet);
        }

        private void p_RoleList(uint accountId, int slot)
        {
            PacketBuilder packet = new PacketBuilder(0x52, false);
            packet.Add(accountId);
            packet.Add(0);
            packet.Add(slot);
            SendAsync(packet);
        }

        private void p_SelectRole(uint roleId)
        {
            PacketBuilder packet = new PacketBuilder(0x46, false);
            packet.Add(roleId);
            SendAsync(packet);
        }

        private void p_C2SKeepAlive()
        {
            PacketBuilder packet = new PacketBuilder(0x5A, false);
            packet.Add((byte)90);
            SendAsync(packet);
        }

        private void p_EnterWorld(uint roleId)
        {
            PacketBuilder packet = new PacketBuilder(0x48, false);
            packet.Add(roleId);
            packet.Add(HexParser.GetBytes("000000000000000000000000"));
            SendAsync(packet);
            IsInGame = true;
        }

        private void p_PublicMessage(ChatMessage msg)
        {
            PacketBuilder packet = new PacketBuilder(0x4F, false);
            packet.Add((ushort)491);
            packet.Add(msg.SenderUID);
            packet.Add((ushort)30549);
            packet.Add((ushort)27985);
            packet.AddUnicode(msg.Message);
            packet.Add((byte)0);
            SendAsync(packet);
        }

        private void p_LocalMessage(ChatMessage msg)
        {
            PacketBuilder packet = new PacketBuilder(0x4F, false);
            packet.Add((ushort)0);
            packet.Add(msg.SenderUID);
            packet.Add((ushort)0);
            packet.Add((ushort)0);
            packet.AddUnicode(msg.Message);
            packet.Add((byte)0);
            SendAsync(packet);
        }

        private void p_PrivateMessage(ChatMessage msg)
        {
            PacketBuilder packet = new PacketBuilder(0x60, false);
            packet.Add((ushort)85);
            packet.AddUnicode(msg.SenderNick);
            packet.Add(msg.SenderUID);
            packet.AddUnicode(msg.ReceiverNick);
            packet.Add(msg.ReceiverUID);
            packet.AddUnicode(msg.Message);
            packet.Add((byte)0);
            packet.Add(msg.ReceiverUID);
            SendAsync(packet);
        }

        [Packet(0x01, false)]
        private void p_ServerInfo(NetStream stream)
        {
            Globals.Key = stream.ReadHex();
            byte authType = stream.ReadByte();
            string serverVersion = BitConverter.ToString(stream.ReadBytes(4));
            Globals.Hash = GetHash(Globals.Login, Globals.Password, Globals.Key);
            p_LogginAnnounce(Globals.Login, Globals.Hash);
        }

        [Packet(0x03, false)]
        private void p_SMKey(NetStream stream)
        {
            Globals.EncHash = stream.ReadHex();
            byte force = stream.ReadByte();
            IsLoginCompleted = true;
            Globals.DecHash = GetDecodeHash();
            Globals.RC4Client = GetRC4Key(Globals.EncHash, Globals.Login, Globals.Hash);
            Globals.RC4Server = GetRC4Key(Globals.DecHash, Globals.Login, Globals.Hash);
            crypt = new Crypt(Globals.RC4Client, Globals.RC4Server);
            p_CMKey(true, Globals.DecHash);
        }

        [Packet(0x04, false)]
        private void p_OnlineAnnounce(NetStream stream)
        {
            Globals.AccountId = stream.ReadUInt32();
            uint unk = stream.ReadUInt32();
            uint unk1 = stream.ReadUInt32();
            byte unkLen = stream.ReadByte();
            uint unk2 = stream.ReadUInt32();
            uint unk3 = stream.ReadUInt32();
            p_RoleList(Globals.AccountId, -1);
        }

        [Packet(0x53, false)]
        private void p_RoleList_Re(NetStream packet)
        {
            uint unk1 = packet.ReadUInt32();
            uint nexSlotId = packet.ReadUInt32();
            uint accountId = packet.ReadUInt32();
            uint unk2 = packet.ReadUInt32();
            byte isChar = packet.ReadByte();
            if (isChar != 0x01)
            {
                StartKeepAliveTimer();
                return;
            }
            uint charId = packet.ReadUInt32();
            byte gender = packet.ReadByte();
            byte race = packet.ReadByte();
            byte occupation = packet.ReadByte();
            uint level = packet.ReadUInt32();
            uint unk3 = packet.ReadUInt32();
            string name = packet.ReadUnicode();
            Chars.Add(new PlayChar { Id = charId, Gender = gender, Race = race, Occupation = occupation, Level = level, Name = name });
            p_RoleList(Globals.AccountId, (int)nexSlotId);
        }

        [Packet(0x47, false)]
        private void p_SelectRole_Re(NetStream packet)
        {
            byte[] Data = packet.ReadBytes(5);
            p_EnterWorld(SelectedChar.Id);
        }

        [Packet(0x5A, true)]
        private void p_S2CKeepAlive(NetStream packet)
        {
            byte unk = packet.ReadByte();
        }

        [Packet(0x55, true)]
        private void p_ReadSkill(NetStream packet)
        {
            packet.ReadUInt32();
          //  p_LocalMessage(new ChatMessage { SenderUID = SelectedChar.Id, Message = "Заюзан скилл персом с ID: " + packet.ReadUInt32() + " skillID: " + packet.ReadUInt32() });
        }

        [Packet(0x85, false)]
        private void p_ReadPublicChat(NetStream packet)
        {
            packet.ReadUInt16();
            packet.ReadUInt32();
           // p_LocalMessage(new ChatMessage { SenderUID = SelectedChar.Id, Message = "Мировой чат: ник - " + packet.ReadUnicode() + " сообщение - " + packet.ReadUnicode() });
        }

        [Packet(0x50, false)]
        private void p_ReadLocalChat(NetStream packet)
        {
            packet.ReadUInt16();
            packet.ReadUInt32();
          //  p_LocalMessage(new ChatMessage { SenderUID = SelectedChar.Id, Message = "Лол чат: сообщение - " + packet.ReadUnicode() });
        }

        [Packet(0x60, false)]
        private void p_ReadPrivateChat(NetStream packet)
        {
            packet.ReadUInt32();
            Console.WriteLine(packet.ReadUnicode());
           // p_LocalMessage(new ChatMessage { SenderUID = SelectedChar.Id, Message = "Пм чат: я не хочу его читать!" });
        }
    }
}
