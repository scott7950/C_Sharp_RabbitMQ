using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ProcessClass
{
    class ProcessClass
    {
        private string HostName;

        private string q1Name = "QT2PChan";
        private string q2Name = "QP2TChan";

        private IConnection connection;

        private IModel channel1;
        private QueueingBasicConsumer consumer1;

        private IModel channel2;

        public ProcessClass(string HostName = "localhost", string q1Name = "QT2PChan", string q2Name = "QP2TChan")
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
            consumer1 = new QueueingBasicConsumer(channel1);
            channel1.BasicConsume(queue: q1Name, noAck: true, consumer: consumer1);

            channel2 = connection.CreateModel();
            channel2.QueueDeclare(queue: q2Name, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        public void Start()
        {

            //Get M1
            RecvMessage("M1");

            //Send M2
            SendMessage("M2");

            //Get M3
            RecvMessage("M3");

        }

        public void RecvMessage(string message)
        {

            Console.WriteLine(" [x] Awaiting " + message + " Message");

            string recvMessage = "";

            do
            {
                BasicDeliverEventArgs ea = (BasicDeliverEventArgs)consumer1.Queue.Dequeue();

                try
                {
                    recvMessage = Encoding.UTF8.GetString(ea.Body);
                    Console.WriteLine(" [.] Message " + recvMessage + " received by Process");
                }
                catch (Exception e)
                {
                    Console.WriteLine(" [.] Error: " + e.Message);
                }

            } while (recvMessage != message);
        }

        private void SendMessage(string message)
        {
            Console.WriteLine(" [x] Sending " + message + " Message");

            var messageBytes = Encoding.UTF8.GetBytes(message);
            var props = channel2.CreateBasicProperties();
            channel2.BasicPublish(exchange: "", routingKey: q2Name, basicProperties: props, body: messageBytes);
        }

        public void Close()
        {
            connection.Close();
        }
    }
}
