using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using WebSocketSharp;

using SharpWebSocket = WebSocketSharp.WebSocket;

namespace MikeSchweitzer.WebSocket.Internal
{
    internal class DotNetWebSocket : IWebSocket
    {
        #region Private Fields
        private readonly Uri _uri;
        private readonly string[] _subprotocols;
        private readonly bool _disableSslValidation;
        private readonly int _maxReceiveBytes;
        
        private SharpWebSocket _socket;

        private readonly Queue<WebSocketMessage> _incomingMessages = new Queue<WebSocketMessage>();
        #endregion

        #region IWebSocket Events
        public event OpenedHandler Opened;
        public event MessageReceivedHandler MessageReceived;
        public event ErrorHandler Error;
        public event ClosedHandler Closed;
        #endregion

        #region IWebSocket Properties
        public WebSocketState State
        {
            get
            {
                switch (_socket?.ReadyState)
                {
                    case WebSocketSharp.WebSocketState.Connecting:
                        return WebSocketState.Connecting;

                    case WebSocketSharp.WebSocketState.Open:
                        return WebSocketState.Open;

                    case WebSocketSharp.WebSocketState.Closing:
                        return WebSocketState.Closing;

                    case WebSocketSharp.WebSocketState.Closed:
                        return WebSocketState.Closed;

                    default:
                        return WebSocketState.Closed;
                }
            }
        }
        #endregion

        #region Ctor/Dtor
        public DotNetWebSocket(
            Uri uri,
            IEnumerable<string> subprotocols,
            Dictionary<string, string> headers = null,
            bool disableSslValidation = false,
            int maxReceiveBytes = 4096)
        {
            _uri = uri;
            _subprotocols = subprotocols?.ToArray();
            _disableSslValidation = disableSslValidation;
            _maxReceiveBytes = maxReceiveBytes;
        }
        #endregion

        #region IWebSocket Methods
        public void ProcessIncomingMessages()
        {
            if (_incomingMessages.Count == 0)
                return;
            
            var messages = _incomingMessages.ToArray();
            _incomingMessages.Clear();
            
            foreach (var message in messages)
                MessageReceived?.Invoke(message);
        }

        public void AddOutgoingMessage(WebSocketMessage message)
        {
            if (message.Type == WebSocketDataType.Binary)
                _socket.SendAsync(message.Bytes, null);
            else
                _socket.SendAsync(message.String, null);
        }

        public Task ConnectAsync()
        {
            _socket = new SharpWebSocket(_uri.AbsoluteUri, _subprotocols);

            if (_socket.IsSecure)
                _socket.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
            
            if (_disableSslValidation)
                _socket.SslConfiguration.ServerCertificateValidationCallback = (_, _, _, _) => true;

            _socket.OnOpen += OnOpen;
            _socket.OnMessage += OnMessage;
            _socket.OnError += OnError;
            _socket.OnClose += OnClose;

            _socket.Connect();
            return Task.CompletedTask;
        }
        
        public Task CloseAsync()
        {
            switch (State)
            {
                case WebSocketState.Closed:
                case WebSocketState.Closing:
                    return Task.CompletedTask;
            }

            _socket.CloseAsync(CloseStatusCode.Normal, "");
            
            return Task.CompletedTask;
        }

        public void Cancel()
        {
            switch (State)
            {
                case WebSocketState.Closed:
                case WebSocketState.Closing:
                    return;
            }

            _socket.CloseAsync(CloseStatusCode.Normal, "");
        }
        #endregion

        #region Internal Event Helpers
        private void OnOpen(object sender, EventArgs e)
        {
            Opened?.Invoke();
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            if (e.RawData.Length > _maxReceiveBytes)
                return;
            
            var message = e.IsBinary
                ? new WebSocketMessage(e.RawData)
                : new WebSocketMessage(e.Data);
            
            _incomingMessages.Enqueue(message);
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            Error?.Invoke(e.Message);
        }

        private void OnClose(object sender, CloseEventArgs e)
        {
            var closeCode = WebSocketHelpers.ConvertCloseCode(e.Code);
            Closed?.Invoke(closeCode);
            _socket = null;
        }
        #endregion
    }
}