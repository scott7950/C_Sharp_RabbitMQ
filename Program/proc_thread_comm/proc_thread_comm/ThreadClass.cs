using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace proc_thread_comm
{

    class ThreadClass
    {
        private IConnection connection;

        private string q1Name = "QT2PChan";
        private string q2Name = "QP2TChan";

        private string HostName;

        private IModel channel1;

        private IModel channel2;
        private QueueingBasicConsumer consumer2;

        public ThreadClass(string HostName = "localhost", string q1Name = "QT2PChan", string q2Name = "QP2TChan")
        {
            this.HostName = HostName;
            this.q1Name = q1Name;
            this.q2Name = q2Name;
        }

        public void Config()
        {
            var factory = new ConnectionFactory() { HostName = this.HostName };
            connection = factory.CreateConnection();
            channel1 = connection.CreateModel();
            channel1.QueueDeclare(queue: q1Name, durable: false, exclusive: false, autoDelete: false, arguments: null);

            channel2 = connection.CreateModel();
            channel2.QueueDeclare(queue: q2Name, durable: false, exclusive: false, autoDelete: false, arguments: null);
            consumer2 = new QueueingBasicConsumer(channel2);
            channel2.BasicConsume(queue: q2Name, noAck: true, consumer: consumer2);
        }

        public void Start()
        {
            //Send M1
            SendMessage("M1");

            //Get M2
            RecvMessage("M2");

            //Send M3
            SendMessage("M3");

        }

        public void RecvMessage(string message)
        {

            Console.WriteLine(" [x] Awaiting " + message + " Message");
            string recvMessage = "";

            do
            {
                BasicDeliverEventArgs ea = (BasicDeliverEventArgs)consumer2.Queue.Dequeue();

                try
                {
                    recvMessage = Encoding.UTF8.GetString(ea.Body);
                    Console.WriteLine(" [.] Message " + recvMessage + " received by Thread");
                }
                catch (Exception e)
                {
                    Console.WriteLine(" [.] Error: " + e.Message);
                }

            } while (recvMessage != message);
        }

        public void SendMessage(string message)
        {
            Console.WriteLine(" [x] Sending " + message + " Message");

            var props = channel1.CreateBasicProperties();
            var messageBytes = Encoding.UTF8.GetBytes(message);
            channel1.BasicPublish(exchange: "", routingKey: q1Name, basicProperties: props, body: messageBytes);
        }

        public void Close()
        {
            connection.Close();
        }
    }
}
