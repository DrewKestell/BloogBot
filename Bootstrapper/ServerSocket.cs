using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Bootstrapper
{
    public class SocketServer : IDisposable
    {
        private readonly int _port;
        private readonly IPAddress _ipAddress;
        private readonly Subject<InstanceUpdate> _instanceUpdateSubject;
        private Socket _listener;
        private Task _backgroundTask;
        private bool _listen;

        public SocketServer(int port, IPAddress ipAddress)
        {
            this._port = port;
            this._ipAddress = ipAddress;
            this._instanceUpdateSubject = new Subject<InstanceUpdate>();
        }

        public IObservable<InstanceUpdate> InstanceUpdateObservable => this._instanceUpdateSubject;

        public void Start()
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _listener.Bind(new IPEndPoint(_ipAddress, _port));
            _listener.Listen(10);
            _listen = true;
            _backgroundTask = Task.Run(StartAsync);
        }

        private async Task StartAsync()
        {
            while (_listen)
            {
                Socket clientSocket = _listener.Accept();
                ThreadPool.QueueUserWorkItem(_ => HandleClient(clientSocket));
            }
        }

        private void HandleClient(Socket clientSocket)
        {
            byte[] buffer = new byte[1024];

            while (_listen)
            {
                int receivedDataLength = clientSocket.Receive(buffer);
                if (receivedDataLength == 0)
                {
                    break;
                }

                string json = System.Text.Encoding.UTF8.GetString(buffer, 0, receivedDataLength);
                if (json.Count(x => x == '}') > 1)
                {
                    json = json.Substring(json.LastIndexOf('{'), json.Length - json.LastIndexOf('{'));
                }
                InstanceUpdate instanceUpdate = Newtonsoft.Json.JsonConvert.DeserializeObject<InstanceUpdate>(json);
                _instanceUpdateSubject.OnNext(instanceUpdate);
                
                Array.Clear(buffer, 0, buffer.Length);
                buffer[0] = 0x01;
                clientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
            }

            clientSocket.Close();
        }

        public void Stop()
        {
            _listen = false;
            _backgroundTask.Wait(TimeSpan.FromSeconds(5));
            _listener?.Close();
            _listener = null;
        }

        public void Dispose()
        {
            Stop();
            _instanceUpdateSubject?.Dispose();
        }
    }

}
