using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace p7_client_frontendV2
{
    public class Client
    {
        IModel _channel;
        string _clientId = string.Empty;
        string _serverName;
        string _dataServerName;
        string _replyQueueName;
        string _correlationId;
        string _replyConsumerTag;
        Dictionary<string, EventHandler<BasicDeliverEventArgs>> _subscribers = new Dictionary<string, EventHandler<BasicDeliverEventArgs>>();

        public Client()
        {
            Init();       
        }

        void Init()
        {
            var factory = new ConnectionFactory() { UserName = "rabbit2", Password = "rabbit2", HostName = "rabbit2.rabbitmq" };
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
            var queueName = _channel.QueueDeclare("ClientRegisterQueue", exclusive: false);

            _channel.ExchangeDeclare(exchange: "server", type: "topic");

            _channel.QueueBind(queue: queueName,
                                     exchange: "server",
                                     routingKey: "clientRegister");
            _correlationId = Guid.NewGuid().ToString();

            _replyQueueName = _channel.QueueDeclare(autoDelete: true, exclusive: true).QueueName;

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var response = Encoding.UTF8.GetString(body);

                if (ea.BasicProperties.CorrelationId == _correlationId)
                {
                    ResponseDTO? responseDto = JsonSerializer.Deserialize<ResponseDTO>(response);
                    _clientId = responseDto?.ClientId ?? string.Empty;
                    _serverName = responseDto?.ServerName ?? string.Empty;
                    _dataServerName = responseDto?.DataServerName ?? string.Empty;
                    Console.WriteLine($"Received response: clientId = {_clientId}, server = {_serverName}, data server = {_dataServerName}");

                    var queueName = _channel.QueueDeclare("Client_" + _clientId, exclusive: false);

                    _channel.QueueBind(queue: queueName,
                                     exchange: "client",
                                     routingKey: _clientId);

                    Connect();
                }
                else
                {
                    Console.WriteLine($"Expected correlationId: {_correlationId} but received response with correlationId: {ea.BasicProperties.CorrelationId}");
                }
            };

            _replyConsumerTag = _channel.BasicConsume(
                consumer: consumer,
                queue: _replyQueueName,
                autoAck: true);
        }

        public void Register(string Username)
        {
            var props = _channel.CreateBasicProperties();

            props.CorrelationId = _correlationId;
            props.ReplyTo = _replyQueueName;

            Console.WriteLine($"Username: {Username}");
            var username = Username;

            var messageBytes = Encoding.UTF8.GetBytes(username);

            _channel.BasicPublish(exchange: "server", routingKey: "clientRegister", basicProperties: props, body: messageBytes);

            Console.WriteLine("Registration sent to server");
        }

        public void Connect()
        {
            var messageBytes = Encoding.UTF8.GetBytes(_clientId);
            _channel.BasicPublish(exchange: "server", routingKey: $"{_serverName}.clientConnect", body: messageBytes);
            Console.WriteLine($"Connected to server {_serverName}. You can now freely send messages!");
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += HandleMessage;

            _channel.BasicConsume(queue: "Client_" + _clientId,
                true,
                consumer: consumer,
                consumerTag: _clientId);

            _channel.BasicCancel(_replyConsumerTag);
        }


        public void HandleMessage(object? model, BasicDeliverEventArgs ea)
        {
            string msgType = Encoding.UTF8.GetString((byte[])ea.BasicProperties.Headers["type"]);
            if(_subscribers.ContainsKey(msgType))
            {
                EventHandler<BasicDeliverEventArgs> ev = _subscribers[msgType];
                ev.Invoke(model, ea);
            }
        }

        public void AddConsumer(string headerTag, EventHandler<BasicDeliverEventArgs> remoteProcedure)
        {
            if(_subscribers.ContainsKey(headerTag))
            {
                _subscribers.Remove(headerTag);
            }
            _subscribers.Add(headerTag, remoteProcedure);
        }


        public void SendMessage(string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "server", routingKey: $"{_serverName}.{_clientId}", body: messageBytes);
            Console.WriteLine("Message sent");
        }

        public void SendMessage(string message, string headerKey, string headerValue)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var props = _channel.CreateBasicProperties();
            props.Headers = new Dictionary<string, object>();
            props.Headers.Add(headerKey, headerValue);
            _channel.BasicPublish(exchange: "server", routingKey: $"{_serverName}.{_clientId}", props, body: messageBytes);
            Console.WriteLine("Message sent");
        }

        public string GetClientId()
        {
            return _clientId;
        }

    }
}
