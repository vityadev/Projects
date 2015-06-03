using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace PWOOG
{
    class cMain
    {
        private static void Main(string[] args)
        {
            Console.Title = "PWOOG";

            Console.WriteLine("Соединяюсь с сервером...");
            PWClient pw = new PWClient("tausvodev", "ffffff");
            pw.Connect("192.168.1.34:29000");
            Console.WriteLine("Успешно.");

            Console.WriteLine("Вхожу на аккаунт...");
            if (pw.WaitLoginResult(100))
                Console.WriteLine("Успешно.");
            else
                throw new Exception("Ошибка при входе на аккаунт");

            Console.WriteLine("Получаю список персонажей...");
            if (pw.WaitCharsObtaining(100))
                Console.WriteLine("Получено {0} персонажей.", pw.Chars.Count);
            else
                throw new Exception("Ошибка при получении персонажей.");

             pw.SelectChar(pw.Chars[0]);
             Console.WriteLine("Выбран персонаж {0} для захода в мир", pw.Chars[0].Name);

             Console.WriteLine("Захожу в мир...");
             if (pw.WaitEntryInGame(100))
                 Console.WriteLine("Успешно!");
             else
                 throw new Exception("Ошибка при заходе в мир!");

             Console.WriteLine("Начинаю флудить...");
             Thread.Sleep(5000);

             while (true)
             {
                 //pw.SendLocalMessage(pw.SelectedChar.Id, "#reboot_server_2332658");
                 //Console.WriteLine("Сообщение послано!");
             }
        }
    }
}
