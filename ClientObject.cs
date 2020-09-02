using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Text;

namespace ChatServer
{
    public class ClientObject
    {
        public string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        string userName;
        TcpClient client;
        ServerObject server; // объект сервера

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this, this.Id);
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();
                // получаем имя пользователя
                string message = GetMessage();
                userName = message;

                CheckName(userName);

                server.clientsName.Add(userName);
                
                message = userName + " присоеденился";
                string clientsCount = Convert.ToString(server.clients.Count);
                
                server.BroadcastMessage($"Подключенных пользователей: {clientsCount}");
                // посылаем сообщение о входе в чат всем подключенным пользователям
                server.BroadcastMessage(message);

                Console.WriteLine(message);

                

                // в бесконечном цикле получаем сообщения от клиента
                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        message = String.Format("{0}: {1}", userName, message);
                        Console.WriteLine(message);
                        server.BroadcastMessage(message);
                    }
                    catch
                    {
                        message = String.Format("{0}: покинул чат", userName);
                        Console.WriteLine(message);
                        server.BroadcastMessage(message);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {

                // в случае выхода из цикла закрываем ресурсы
                //удаление из списка онлайна
                int index = server.usersId.IndexOf(this.Id);
                server.clientsName.RemoveAt(index);
                server.RemoveConnection(this.Id);
                string clientsCount = Convert.ToString(server.clients.Count);
                server.BroadcastMessage($"Подключенных пользователей: {clientsCount}");
                Close();
                
            }
        }

        // чтение входящего сообщения и преобразование в строку
        private string GetMessage()
        {
            byte[] data = new byte[64]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }

        // закрытие подключения
        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
        public void CheckName(string str)
        {
            if (server.clientsName.Contains(str)){
                server.BroadcastPrivateMessage("\nИмя занято! \nПерезайдите, сменив имя!\n",this.Id);
                client.Client.Close();
                server.RemoveConnection(this.Id);
                Close();
                
            }
        }
       
    }
}