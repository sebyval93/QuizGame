using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.ObjectModel;

namespace GameServer
{
    public sealed class ServerManager
    {

        private static readonly ServerManager instance = new ServerManager();

        private static int _port = 4400;
        private Socket _serverSocket;
        public static readonly List<Socket> _clientSockets = new List<Socket>();
        private static List<User> _users = new List<User>();
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        private static UserManager userManager = UserManager.Instance;
        private static ChatRoomManager chatRoomManager;
        private static QuestionManager questionManager;
        private int maxMessageSize;

        public static ServerManager Instance
        {
            get
            {
                return instance;
            }
        }

        public ReadOnlyCollection<Socket> getAllClients()
        {
            return _clientSockets.AsReadOnly();
        }

        private class StateObject
        {
            // Client  socket.
            public Socket workSocket = null;
            // Size of receive buffer.
            public const int bufferSize = 2048;
            // Receive buffer.
            public byte[] buffer = new byte[bufferSize];
            public byte[] msgBuffer;
            public byte[] prefixBuffer = new byte[prefixSizeBytes];
            // Received data string.
            public StringBuilder sb = new StringBuilder();
            public int messageLenBytes;
            public int numBytesRead;
            public const int prefixSizeBytes = 4;
            // Whether to expect to read a prefix or not
            public bool awaitingPrefix = true;
        }

        public void ReadSettings()
        {
            if (JSONReader.ReadFile())
            {
                maxMessageSize = JSONReader.GetMaxMessageSize();
                SetupServer();
            }
            else
            {
                Console.WriteLine("Error! \nServer not setup, please run the administration tool to generate settings file!");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
        
        public void SetupServer()
        {
            chatRoomManager = ChatRoomManager.Instance;
            questionManager = QuestionManager.Instance;

            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            DBManager conn = DBManager.Instance;

            try
            {
                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, _port));
                _serverSocket.Listen(100);

                chatRoomManager.CreateLobby();

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    _serverSocket.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        _serverSocket);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket current = listener.EndAccept(ar);

            _clientSockets.Add(current);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = current;

            // new connection, start expecting a prefix
            current.BeginReceive(state.buffer, 0, StateObject.prefixSizeBytes, 0,
                new AsyncCallback(ReceiveCallback), state);

            Console.WriteLine("Client connected and registered. Ready for requests.");
        }

        public void ReceiveCallback(IAsyncResult ar)
        {

            StateObject state = (StateObject)ar.AsyncState;
            Socket current = state.workSocket;
            int bytesRead;
            //InetMessage inetMsg;

            try
            {
                bytesRead = current.EndReceive(ar);
            }
            catch (SocketException)
            {
                Console.WriteLine("Client forcefully disconnected");
                current.Close();

                User user = _users.Find(x => x.GetSocket().Equals(current));

                if (user == null)
                {
                    Console.WriteLine("Error: User not found, this should not happen unless the user was not authenticated!");
                    return;
                }

                ChatRoom room = chatRoomManager.GetRoomOfUser(user);

                if (room == null)
                {
                    Console.WriteLine("Error: Existing authenticated user not in ANY room!");
                    return;
                }

                if (!chatRoomManager.RemoveUserFromRoom(room.GetName(), user))
                {
                    Console.WriteLine("Error: User could not be removed from room!");
                    return;
                }

                _clientSockets.Remove(current);

                return;
            }

            // read succeeded

            if (bytesRead > 0)
            {
                // save last bytes read number to serve as an index.
                int prevBytesRead = state.numBytesRead;

                if (state.awaitingPrefix)
                {
                    // attempt to read a prefix.
                    state.numBytesRead += bytesRead;

                    if (state.numBytesRead > StateObject.prefixSizeBytes)
                    {
                        Console.WriteLine("Error: read too many bytes for prefix. Read total {0} bytes, but expected only {1}", state.numBytesRead, StateObject.prefixSizeBytes);
                        return;
                    }

                    Buffer.BlockCopy(state.buffer, 0, state.prefixBuffer, prevBytesRead, bytesRead);

                    if (state.numBytesRead == StateObject.prefixSizeBytes)
                    {
                        state.messageLenBytes = BitConverter.ToInt32(state.prefixBuffer, 0);
                        state.awaitingPrefix = false;
                        state.numBytesRead = 0;

                        state.msgBuffer = new byte[state.messageLenBytes];

                        if (state.messageLenBytes <= StateObject.bufferSize)
                        {
                            current.BeginReceive(state.buffer, 0, state.messageLenBytes, 0,
                                new AsyncCallback(ReceiveCallback), state);
                            return;
                        }
                        else
                        {
                            current.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0,
                                new AsyncCallback(ReceiveCallback), state);
                            return;
                        }
                    }

                    // get more prefix data.
                    current.BeginReceive(state.buffer, 0, StateObject.prefixSizeBytes - state.numBytesRead, 0,
                        new AsyncCallback(ReceiveCallback), state);
                    return;

                }
                else
                {
                    // we have a prefix, deal with the message.
                    state.numBytesRead += bytesRead;
                    if (state.numBytesRead > state.messageLenBytes)
                    {
                        Console.WriteLine("Error: Read more bytes than the message itself is supposed to have. Read total {0} bytes, but expected only {1}", state.numBytesRead, state.messageLenBytes);
                        return;
                    }

                    // read in the data
                    Buffer.BlockCopy(state.buffer, 0, state.msgBuffer, prevBytesRead, bytesRead);

                    if (state.numBytesRead == state.messageLenBytes)
                    {
                        // we are done getting the message.
                        // serialize it, and process it.

                        state.numBytesRead = 0;
                        state.awaitingPrefix = true;
                        state.messageLenBytes = 0;

                        InetMessage inetMsg = SerializationUtils.DeserializeInetMessage(state.msgBuffer);
                        Message.ProcessMessage(inetMsg, current);
                    }
                    else if (state.numBytesRead < state.messageLenBytes)
                    {
                        // ask for more

                        if ((state.messageLenBytes - state.numBytesRead) > StateObject.bufferSize)
                        {
                            current.BeginReceive(state.buffer, 0, StateObject.bufferSize, 0,
                                new AsyncCallback(ReceiveCallback), state);
                            return;
                        }
                        else
                        {
                            current.BeginReceive(state.buffer, 0, state.messageLenBytes - state.numBytesRead, 0,
                                new AsyncCallback(ReceiveCallback), state);
                            return;
                        }
                    }

                }
            }

            if (current.Connected)
                current.BeginReceive(state.buffer, 0, StateObject.prefixSizeBytes - state.numBytesRead, 0,
                    new AsyncCallback(ReceiveCallback), state);
            else
                return;
        }

        public void Send(Socket current, byte[] data)
        {
            // Begin sending the data to the remote device.
            if (current.Connected)
                current.BeginSend(data, 0, data.Length, 0,
                    new AsyncCallback(SendCallback), current);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket current = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = current.EndSend(ar);

            }
            catch
            { }
        }
        /// <summary>
        /// Close all connected client (we do not need to shutdown the server socket as its connections
        /// are already closed with the clients)
        /// </summary>
        private void CloseAllSockets()
        {
            foreach (Socket socket in _clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            _serverSocket.Close();
        }

    }
}
