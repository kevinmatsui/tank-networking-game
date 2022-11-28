/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// ProjectName: NetworkController.csproj
// FileName: Networking.cs
// FileType: Visual C# source file
// Professor: Daniel Kopta
// Author: Kevin Matsui, Ee Jae Ahn
// Date: 11/12/2021
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkUtil
{

    public static class Networking
    {
        /// <summary>
        /// This is a private helper method to handle the error when using async operations 
        /// </summary>
        /// <param name="state">connection state</param>
        /// <param name="errorMessage">specific error message</param>
        private static void ErrorHandle(SocketState state, string errorMessage)
        {
            // set the socket state's ErrorOccurred flag to true
            state.ErrorOccurred = true;
            // set specific error message for socket state
            state.ErrorMessage = errorMessage;
            // inform user that data is ready by calling delegate
            state.OnNetworkAction(state);
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        // Server-Side Code
        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Starts a TcpListener on the specified port and starts an event-loop to accept new clients.
        /// The event-loop is started with BeginAcceptSocket and uses AcceptNewClient as the callback.
        /// AcceptNewClient will continue the event-loop.
        /// </summary>
        /// <param name="toCall">The method to call when a new connection is made</param>
        /// <param name="port">The the port to listen on</param>
        public static TcpListener StartServer(Action<SocketState> toCall, int port)
        {
            // instantiation for listening incoming connection with current client's IPAddress and port/recipient
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            try
            {
                // start the listener
                listener.Start();
                // begin accepting a client with object that contains connection information
                listener.BeginAcceptSocket(AcceptNewClient, new Tuple<TcpListener, Action<SocketState>>(listener, toCall));
            }
            catch (Exception e)  // Handles the exception using a new socketstate
            {
                // creates new socketstate with given toCall, and a null socket
                SocketState state = new SocketState(toCall, null);
                // calls error handle helper method with the state, and the error message
                ErrorHandle(state, e.Message);
            }
            // returns the TCPListener created
            return listener;
        }

        /// <summary>
        /// To be used as the callback for accepting a new client that was initiated by StartServer, and 
        /// continues an event-loop to accept additional clients.
        ///
        /// Uses EndAcceptSocket to finalize the connection and create a new SocketState. The SocketState's
        /// OnNetworkAction should be set to the delegate that was passed to StartServer.
        /// Then invokes the OnNetworkAction delegate with the new SocketState so the user can take action. 
        /// 
        /// If anything goes wrong during the connection process (such as the server being stopped externally), 
        /// the OnNetworkAction delegate should be invoked with a new SocketState with its ErrorOccurred flag set to true 
        /// and an appropriate message placed in its ErrorMessage field. The event-loop should not continue if
        /// an error occurs.
        ///
        /// If an error does not occur, after invoking OnNetworkAction with the new SocketState, an event-loop to accept 
        /// new clients should be continued by calling BeginAcceptSocket again with this method as the callback.
        /// </summary>
        /// <param name="ar">The object asynchronously passed via BeginAcceptSocket. It must contain a tuple with 
        /// 1) a delegate so the user can take action (a SocketState Action), and 2) the TcpListener</param>
        private static void AcceptNewClient(IAsyncResult ar)
        {
            // object to get the connection information
            Tuple<TcpListener, Action<SocketState>> conInfo = (Tuple<TcpListener, Action<SocketState>>)ar.AsyncState;

            // creates a null socket to be initialized within the try block
            Socket newClient = null;

            try
            {
                // complete accpeting the client and store the client's data to new socket
                newClient = conInfo.Item1.EndAcceptSocket(ar);
            }
            catch (Exception e)  // catches any exception that occurs in EndAcceptSocket
            {
                // creates new state to handle the error
                SocketState state1 = new SocketState(conInfo.Item2, newClient);
                // calls private helper method to handle the error using the state and the exception message
                ErrorHandle(state1, e.Message);
                // returns from the method
                return;
            }

            // store the data for the current state of the Socket
            SocketState state = new SocketState(conInfo.Item2, newClient);

            // store the user's connection method that uses GetData 
            state.OnNetworkAction(state);

            try
            {
                // re calls the BeginAcceptSocket to continue the event loop for connections
                conInfo.Item1.BeginAcceptSocket(AcceptNewClient, new Tuple<TcpListener, Action<SocketState>>(conInfo.Item1, conInfo.Item2));
            }
            catch (Exception e)  // handles any errors in BeginAcceptSocket
            {
                // calls private helper method that handles exception using the state and exception message
                ErrorHandle(state, e.Message);
            }
        }

        /// <summary>
        /// Stops the given TcpListener.
        /// </summary>
        public static void StopServer(TcpListener listener)
        {
            // stops the TcpListener that was passed in 
            listener.Stop();
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        // Client-Side Code
        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Begins the asynchronous process of connecting to a server via BeginConnect, 
        /// and using ConnectedCallback as the method to finalize the connection once it's made.
        /// 
        /// If anything goes wrong during the connection process, toCall should be invoked 
        /// with a new SocketState with its ErrorOccurred flag set to true and an appropriate message 
        /// placed in its ErrorMessage field. Depending on when the error occurs, this should happen either
        /// in this method or in ConnectedCallback.
        ///
        /// This connection process should timeout and produce an error (as discussed above) 
        /// if a connection can't be established within 3 seconds of starting BeginConnect.
        /// 
        /// </summary>
        /// <param name="toCall">The action to take once the connection is open or an error occurs</param>
        /// <param name="hostName">The server to connect to</param>
        /// <param name="port">The port on which the server is listening</param>
        public static void ConnectToServer(Action<SocketState> toCall, string hostName, int port)
        {
            // Establish the remote endpoint for the socket.
            IPHostEntry ipHostInfo;
            IPAddress ipAddress = IPAddress.None;

            // Determine if the server address is a URL or an IP
            try
            {
                // gets the IPHostEntry with the given hostname
                ipHostInfo = Dns.GetHostEntry(hostName);
                bool foundIPV4 = false;
                // loops through the IPAddresses in the IPHostEntry address list
                foreach (IPAddress addr in ipHostInfo.AddressList)
                    // if the IPAddress is not IPV6, then breaks from the loop, sets ipAddress to the address, and sets foundIPV4 to true
                    if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                    {
                        foundIPV4 = true;
                        ipAddress = addr;
                        break;
                    }
                // Didn't find any IPV4 addresses
                if (!foundIPV4)
                {
                    // creates a new state to indicate the error
                    SocketState state1 = new SocketState(toCall, null);
                    // calls the private helper method to handle errors using the given state
                    ErrorHandle(state1, "IPV4 address not found!");
                    // returns from the method
                    return;
                }
            }
            catch (Exception)
            {
                // see if host name is a valid ipaddress
                try
                {
                    ipAddress = IPAddress.Parse(hostName);
                }
                catch (Exception e) // handles exceptions thrown by parsing the hostname to an ipaddress
                {
                    // creates new socketstate to display the error
                    SocketState state2 = new SocketState(toCall, null);
                    // calls the private helper method to handle errors using the given state
                    ErrorHandle(state2, e.Message);
                    // returns from the method 
                    return;
                }
            }

            // Create a TCP/IP socket.
            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Initializes a socketstate with the given socket and toCall
            SocketState state = new SocketState(toCall, socket);
            IAsyncResult result = null;
            try
            {
                // sets the IAsyncResult to the result of calling BeginConnect on the socket, using given 
                // ipaddress, port, and state, uses ConnectedCallback as callback method
                result = socket.BeginConnect(ipAddress, port, ConnectedCallback, state);
            }
            catch (Exception e)  // handles exceptions that occur in BeginConnect
            {
                // calls the private helper method to handle errors using the given state
                ErrorHandle(state, e.Message);
                return;
            }

            // success is false if it takes longer than 3 seconds for connection to be made
            bool success = result.AsyncWaitHandle.WaitOne(3000, true);
            if (!success)  // if the connection times out
            {
                // closes the socket associated with the given state, and returns
                state.TheSocket.Close();
                return;
            }

            // This disables Nagle's algorithm (google if curious!)
            // Nagle's algorithm can cause problems for a latency-sensitive 
            // game like ours will be 
            socket.NoDelay = true;
        }

        /// <summary>
        /// To be used as the callback for finalizing a connection process that was initiated by ConnectToServer.
        ///
        /// Uses EndConnect to finalize the connection.
        /// 
        /// As stated in the ConnectToServer documentation, if an error occurs during the connection process,
        /// either this method or ConnectToServer should indicate the error appropriately.
        /// 
        /// If a connection is successfully established, invokes the toCall Action that was provided to ConnectToServer (above)
        /// with a new SocketState representing the new connection.
        /// 
        /// </summary>
        /// <param name="ar">The object asynchronously passed via BeginConnect</param>
        private static void ConnectedCallback(IAsyncResult ar)
        {
            // get the socket's state by async operation
            SocketState state = (SocketState)ar.AsyncState;
            // try catch block to handle the error
            try
            {
                // complete the connection
                state.TheSocket.EndConnect(ar);
                // inform user that data is ready by calling delegate
                state.OnNetworkAction(state);
            }
            catch (Exception e)
            {
                // private method to handle the error
                ErrorHandle(state, e.Message);
            }
        }


        /////////////////////////////////////////////////////////////////////////////////////////
        // Server and Client Common Code
        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Begins the asynchronous process of receiving data via BeginReceive, using ReceiveCallback 
        /// as the callback to finalize the receive and store data once it has arrived.
        /// The object passed to ReceiveCallback via the AsyncResult should be the SocketState.
        /// 
        /// If anything goes wrong during the receive process, the SocketState's ErrorOccurred flag should 
        /// be set to true, and an appropriate message placed in ErrorMessage, then the SocketState's
        /// OnNetworkAction should be invoked. Depending on when the error occurs, this should happen either
        /// in this method or in ReceiveCallback.
        /// </summary>
        /// <param name="state">The SocketState to begin receiving</param>
        public static void GetData(SocketState state)
        {
            // try catch block to handle error when receiving data
            try
            {
                // start receiving data from a connected Socket and invoke callback method to complete the receive
                state.TheSocket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, ReceiveCallback, state);
            }
            catch (Exception e)
            {
                // private method to handle the error
                ErrorHandle(state, e.Message);
            }

        }

        /// <summary>
        /// To be used as the callback for finalizing a receive operation that was initiated by GetData.
        /// 
        /// Uses EndReceive to finalize the receive.
        ///
        /// As stated in the GetData documentation, if an error occurs during the receive process,
        /// either this method or GetData should indicate the error appropriately.
        /// 
        /// If data is successfully received:
        ///  (1) Read the characters as UTF8 and put them in the SocketState's unprocessed data buffer (its string builder).
        ///      This must be done in a thread-safe manner with respect to the SocketState methods that access or modify its 
        ///      string builder.
        ///  (2) Call the saved delegate (OnNetworkAction) allowing the user to deal with this data.
        /// </summary>
        /// <param name="ar"> 
        /// This contains the SocketState that is stored with the callback when the initial BeginReceive is called.
        /// </param>
        private static void ReceiveCallback(IAsyncResult ar)
        {
            // get the socket's state by async operation
            SocketState state = (SocketState)ar.AsyncState;
            int numBytes = 0; // initialization for message bytes

            // try catch block to handle error 
            try
            {
                // store the integer message bytes by completing the receive
                numBytes = state.TheSocket.EndReceive(ar);
            }
            catch (Exception e)
            {
                // private method to handle the error
                ErrorHandle(state, e.Message);
                return; // end the method
            }

            // if the remote host shuts down the socket connection
            if (numBytes == 0)
            {
                // private method to handle the error
                ErrorHandle(state, "Socket was disconnected!");
                return; // end the method
            }

            // get encoded message string from the integer bytes
            string encoded = Encoding.UTF8.GetString(state.buffer, 0, numBytes);
            // append the encoded message string to the state's data StringBuilder
            lock(state.data)
            {
                state.data.Append(encoded);
            }
            // call the saved delegate for having complete message from multiple receive operation
            state.OnNetworkAction(state);
        }

        /// <summary>
        /// Begin the asynchronous process of sending data via BeginSend, using SendCallback to finalize the send process.
        /// 
        /// If the socket is closed, does not attempt to send.
        /// 
        /// If a send fails for any reason, this method ensures that the Socket is closed before returning.
        /// </summary>
        /// <param name="socket">The socket on which to send the data</param>
        /// <param name="data">The string to send</param>
        /// <returns>True if the send process was started, false if an error occurs or the socket is already closed</returns>
        public static bool Send(Socket socket, string data)
        {
            // if the socket is closed
            if (!socket.Connected)
            {
                return false;
            }

            // store the message bytes from the string data
            byte[] messageBytes = Encoding.UTF8.GetBytes(data);

            // try catch block to handle the error
            try
            {
                // begin sending the message and invoke call back method to complete the send
                socket.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, SendCallback, socket);
                return true;
            }
            catch // if error was found
            {
                // close the socket
                socket.Close();
                return false;
            }
        }

        /// <summary>
        /// To be used as the callback for finalizing a send operation that was initiated by Send.
        ///
        /// Uses EndSend to finalize the send.
        /// 
        /// This method must not throw, even if an error occurred during the Send operation.
        /// </summary>
        /// <param name="ar">
        /// This is the Socket (not SocketState) that is stored with the callback when
        /// the initial BeginSend is called.
        /// </param>
        private static void SendCallback(IAsyncResult ar)
        {
            // get the connected client by async operation
            Socket client = (Socket)ar.AsyncState;

            // try catch block to handle the error
            try
            {
                // complete the send for the client
                client.EndSend(ar);
            }
            catch
            {
                // close the socket
                client.Close();
            }
        }


        /// <summary>
        /// Begin the asynchronous process of sending data via BeginSend, using SendAndCloseCallback to finalize the send process.
        /// This variant closes the socket in the callback once complete. This is useful for HTTP servers.
        /// 
        /// If the socket is closed, does not attempt to send.
        /// 
        /// If a send fails for any reason, this method ensures that the Socket is closed before returning.
        /// </summary>
        /// <param name="socket">The socket on which to send the data</param>
        /// <param name="data">The string to send</param>
        /// <returns>True if the send process was started, false if an error occurs or the socket is already closed</returns>
        public static bool SendAndClose(Socket socket, string data)
        {
            // if the socket is closed
            if (!socket.Connected)
            {
                return false;
            }

            // store the message bytes from the string data
            byte[] messageBytes = Encoding.UTF8.GetBytes(data);

            // try catch block to handle the error
            try
            {
                // begin sending the message and invoke call back method to complete the send
                socket.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, SendAndCloseCallback, socket);
                return true;
            }
            catch // if error was found
            {
                // close the socket
                socket.Close();
                return false;
            }
        }

        /// <summary>
        /// To be used as the callback for finalizing a send operation that was initiated by SendAndClose.
        ///
        /// Uses EndSend to finalize the send, then closes the socket.
        /// 
        /// This method must not throw, even if an error occurred during the Send operation.
        /// 
        /// This method ensures that the socket is closed before returning.
        /// </summary>
        /// <param name="ar">
        /// This is the Socket (not SocketState) that is stored with the callback when
        /// the initial BeginSend is called.
        /// </param>
        private static void SendAndCloseCallback(IAsyncResult ar)
        {
            // get the connected client by async operation
            Socket client = (Socket)ar.AsyncState;

            // try catch block to handle the error
            try
            {
                // complete the send for the client
                client.EndSend(ar);
            }
            catch
            {
            }

            // close the socket
            client.Close();
        }

    }
}