using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace ChatServer
{
    public class ServerObject
    {
        public string allusers;
        static TcpListener tcpListener; // сервер для прослушивания
        public List<ClientObject> clients = new List<ClientObject>(); // все подключения
        public List<String> clientsName = new List<string>();
        public List<String> usersId = new List<string>();
        protected internal void AddConnection(ClientObject clientObject, string Id)
        {
            
            clients.Add(clientObject);
            usersId.Add(Id);
            setUsersOnline();

        }
        protected internal void RemoveConnection(string id)
        {
            // получаем по id закрытое подключение
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);
            // и удаляем его из списка подключений
            if (client != null)
                clients.Remove(client);
            
            

        }
        // прослушивание входящих подключений
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 755);
                tcpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        // трансляция сообщения подключенным клиентам
        protected internal void BroadcastMessage(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                    clients[i].Stream.Write(data, 0, data.Length); //передача данных
               
            }
        }
        protected internal void BroadcastPrivateMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id == id) // если id клиента  равно id отправляющего
                {
                    clients[i].Stream.Write(data, 0, data.Length); //передача данных
                }
            }
        }
        // отключение всех клиентов
        protected internal void Disconnect()
        {
            tcpListener.Stop(); //остановка сервера

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); //отключение клиента
            }
            Environment.Exit(0); //завершение процесса
        }
        public void setUsersOnline()
        {
            allusers = "[changeUsersList]";
            string[] users_online = clientsName.ToArray();
            foreach (string i in users_online)
            {
                allusers += ";" + i;
            }
            BroadcastMessage(allusers);
        }
    }
}