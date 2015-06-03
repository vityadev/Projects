using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Timers;

namespace PWOOG
{
    partial class PWClient
    {
        private Socket socket;
        private SocketAsyncEventArgs socketRAEA;
        private SocketAsyncEventArgs socketSAEA;
        private ActionQueueAsync sendQueue;

        private Crypt crypt = null;
        private bool IsLoginCompleted = false;
        private bool IsCharsReceived = false;
        private bool IsInGame = false;

        public PWClient(string login, string password)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,ProtocolType.Tcp);
            socket.ReceiveBufferSize = ushort.MaxValue;
            socket.SendBufferSize = ushort.MaxValue;

            socketRAEA = new SocketAsyncEventArgs();
            socketRAEA.SetBuffer(new byte[ushort.MaxValue], 0, ushort.MaxValue);
            socketRAEA.Completed += new EventHandler<SocketAsyncEventArgs>(socket_Receive);

            socketSAEA = new SocketAsyncEventArgs();
            socketSAEA.Completed += new EventHandler<SocketAsyncEventArgs>(socket_Send);
            sendQueue = new ActionQueueAsync(false);

            p_Inicialization();
            Globals.Login = login;
            Globals.Password = password;
        }

        private class ActionQueueAsync
        {
            private Queue<Action> queue;
            private object wrap;
            private bool autoEnd;

            public ActionQueueAsync(bool autoEnd)
            {
                queue = new Queue<Action>();
                wrap = new object();
                this.autoEnd = autoEnd;
            }
            public void Start(Action task)
            {
                lock (wrap)
                {
                    queue.Enqueue(task);
                    if (queue.Count == 1)
                        StartFirst();
                }
            }
            private void StartFirst()
            {
                Action action = queue.Peek();
                if (autoEnd)
                    action += End;
                Task.Factory.StartNew(action);
            }
            public void End()
            {
                lock (wrap)
                {
                    queue.Dequeue();
                    if (queue.Count != 0)
                        StartFirst();
                }
            }
        }

        private void SendAsync(PacketContainer container)
        {
            Action action = () =>
            {
                byte[] buffer = container.InitContainer;
                //Console.WriteLine("S1:" + BitConverter.ToString(buffer));
                if (IsLoginCompleted)
                    crypt.Encrypt(ref buffer);
                socketSAEA.SetBuffer(buffer, 0, buffer.Length);
                socket.SendAsync(socketSAEA);
            };
            sendQueue.Start(action);
        }

        private void SendAsync(PacketBuilder packet)
        {
            Action action = () =>
            {
                byte[] buffer = packet.Init();
                //Console.WriteLine("S2:" + BitConverter.ToString(buffer));
                if (IsLoginCompleted)
                    crypt.Encrypt(ref buffer);
                socketSAEA.SetBuffer(buffer, 0, buffer.Length);
                socket.SendAsync(socketSAEA);
            };
            sendQueue.Start(action);
        }

        public void SendAsync(byte[] bytes)
        {
            Action action = () =>
            {
                //Console.WriteLine("S3:" + BitConverter.ToString(bytes));
                if (IsLoginCompleted)
                    crypt.Encrypt(ref bytes);
                socketSAEA.SetBuffer(bytes, 0, bytes.Length);
                socket.SendAsync(socketSAEA);
            };
            sendQueue.Start(action);
        }

        private void socket_Send(object obj, SocketAsyncEventArgs e)
        {
            sendQueue.End();
        }

        private void ReceiveAsync()
        {
            socket.ReceiveAsync(socketRAEA);
        }

        private void socket_Receive(object obj, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success || e.BytesTransferred == 0)
                return;

            byte[] buffer = Helper.GetArray(e.Buffer, 0, (uint)e.BytesTransferred);
            if (IsLoginCompleted)
                crypt.Decrypt(ref buffer);
            //Console.WriteLine("R:" + BitConverter.ToString(buffer));
            NetStream[] netStream1 = NetStream.FromBuff(buffer);
            foreach (var elm1 in netStream1)
                if (elm1.Header == 0x00)
                {
                    NetStream[] netStream2 = NetStream.FromContainer(elm1);
                    foreach (var elm2 in netStream2)
                        p(elm2);
                }
                else
                    p(elm1);
            ReceiveAsync();
        }

        public void Connect(string servAddr)
        {
            int port = 29000; IPAddress addr; IPEndPoint endPoint;
            string[] splited = servAddr.Split(new char[1] { ':' }, 2);

            try
            {
                addr = IPAddress.Parse(splited[0]);
            }
            catch
            {
                throw new ArgumentException("Ip-адрес сервера задан некорректно");
            }
            try
            {
                port = int.Parse(splited[1]);
            }
            catch
            {
                throw new ArgumentException("Порт сервера задан некорректно");
            }
            try
            {
                endPoint = new IPEndPoint(addr, port);
            }
            catch
            {
                throw new ArgumentException("Порт сервера задан некорректно, значение находится за границами допустимого");
            }
            try
            {
                socket.Connect(endPoint);
            }
            catch (SocketException ex)
            {
                throw new Exception(string.Format("Подключение к серверу не удалось, {0}", ex.Message));
            }
            ReceiveAsync();
        }

        public bool WaitLoginResult(int time = 10)
        {
            if (IsLoginCompleted)
                return true;
            for (int i = 0; i < time * 10; i++)
            {
                Thread.Sleep(100);
                if (IsLoginCompleted)
                    return true;
            }
            return false;
        }

        public bool WaitCharsObtaining(int time = 10)
        {
            if (IsCharsReceived)
                return true;
            for (int i = 0; i < time * 10; i++)
            {
                Thread.Sleep(100);
                if (IsCharsReceived)
                    return true;
            }
            return false;
        }

        public bool WaitEntryInGame(int time = 10)
        {
            if (IsInGame)
                return true;
            for (int i = 0; i < time * 10; i++)
            {
                Thread.Sleep(100);
                if (IsInGame)
                    return true;
            }
            return false;
        }
    }
}