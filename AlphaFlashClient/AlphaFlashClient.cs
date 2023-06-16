using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Microsoft.Win32;

namespace AlphaFlashCSharpClient
{
    class AlphaFlashClient
    {
        private Socket m_sockMNI = null;
        private bool loginFlag = false;

        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("AlphaFlashClient Usage: <server ip address> <port> <username> <password>");
                Environment.Exit(1);
            }

            string server = args[0];
            int port = int.Parse(args[1]);
            string username = args[2];
            string password = args[3];
            AlphaFlashClient client = new AlphaFlashClient(server, port, username, password);

            byte[] byteBufferRec = new byte[256];

            try
            {
                while (true)
                {
                    if (!client.loginFlag)
                    {
                        int bytesReceived = client.m_sockMNI.Receive(byteBufferRec, 0, byteBufferRec.Length, SocketFlags.None);
                        string response = Encoding.Default.GetString(byteBufferRec, 0, bytesReceived);
                        Console.WriteLine("Received {0} bytes from MNI: {1}", bytesReceived, response);
                        if (response.Contains("OK"))
                            client.loginFlag = true;
                        else
                            throw new Exception("Login failed:"+response);
                    }
                    else
                    {
                        Array.Clear(byteBufferRec, 0, byteBufferRec.Length);
                        int noOfBytesReceived = client.m_sockMNI.Receive(byteBufferRec, 0, byteBufferRec.Length, SocketFlags.None);
                        Console.WriteLine("No. of Bytes Received from MNI: {0}", noOfBytesReceived);
                        client.ReadMessageFromMNI(byteBufferRec, noOfBytesReceived);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Exception from AlphaFlashClient " + e.ToString());
            }
            finally
            {
                client.m_sockMNI.Close();
            }
        }

        public AlphaFlashClient(string server, int port, string username, string password)
	    {        
            ConnectToMNI(server, port, username, password);
	    }

        private void ConnectToMNI(string server, int port, string username, string password)
        {
            string MNIServerAddress = server;
            int MNIServerPort = port;
            
            m_sockMNI = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint serverEndPoint = new IPEndPoint(Dns.GetHostAddresses(server)[0], MNIServerPort);
            
            m_sockMNI.Connect(serverEndPoint);
            Console.WriteLine("Connected to MNI at {0}:{1} ...now sending authentication information", MNIServerAddress, MNIServerPort);

            string authString = "AUTH " + username + " " + password + "\n\n";
            byte[] byteBuffer = Encoding.Default.GetBytes(authString);
            m_sockMNI.Send(byteBuffer, 0, byteBuffer.Length, SocketFlags.None);
            Console.WriteLine("Sent {0} bytes to MNI...", byteBuffer.Length);
        }

        private void ReadMessageFromMNI(byte[] messageByteBuffer, int numOfBytes)
        {
            Array.Reverse(messageByteBuffer,0,2);
            ushort messageLength = BitConverter.ToUInt16(messageByteBuffer, 0);

            if (messageLength != numOfBytes) return;

            Array.Reverse(messageByteBuffer, 2, 4);
            int txmitId = BitConverter.ToInt32(messageByteBuffer, 2);

            Array.Reverse(messageByteBuffer, 8, 2);
            ushort categoryId = BitConverter.ToUInt16(messageByteBuffer, 8);

            int indicatorId = messageByteBuffer[6] << 24 | (messageByteBuffer[7] & 0xff) << 16 | ((categoryId >> 8) & 0xff) << 8 | (categoryId & 0xff);

            Console.WriteLine("category id: {0} version:{1} type:{2} txmitId:{3} indicatorId:{4}", categoryId, messageByteBuffer[7], messageByteBuffer[6], txmitId, indicatorId);

            int field_buffer_offset = Constants.HEADER_SIZE;	  
            do
            {		  
              int field_type = messageByteBuffer[field_buffer_offset];
              int field_id = messageByteBuffer[field_buffer_offset+1];
              int value_offset = field_buffer_offset+2;
              
              switch (field_type) 
              {
                case Constants.FLOAT_FIELD_TYPE:
                    {
                        Array.Reverse(messageByteBuffer, value_offset, Constants.FLOAT_FIELD_SIZE);
                        float field_value = BitConverter.ToSingle(messageByteBuffer, value_offset);

                        field_buffer_offset += Constants.FLOAT_INDICATOR_SIZE;
                        Console.WriteLine( "field type:" + field_type + " field id:"+ field_id + " field value:" + field_value );
                    }
                    break;
            		
                case Constants.SHORT_FIELD_TYPE:
                    {
                        short field_value = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(messageByteBuffer, value_offset));
                        field_buffer_offset += Constants.SHORT_INDICATOR_SIZE;
	                    Console.WriteLine( "field type:" + field_type + " field id:"+ field_id + " field value:" + field_value );
                    }
                    break;
            		
                case Constants.LONG_FIELD_TYPE:
                    {
                      long field_value = IPAddress.NetworkToHostOrder(BitConverter.ToInt64(messageByteBuffer, value_offset));
                      field_buffer_offset += Constants.LONG_INDICATOR_SIZE;
                      Console.WriteLine( "field type:" + field_type + " field id:"+ field_id + " field value:" + field_value );
                    }
                    break;
            		
                case Constants.DOUBLE_FIELD_TYPE:
                    {
                        Array.Reverse(messageByteBuffer, value_offset, Constants.DOUBLE_FIELD_SIZE);
                        double field_value = BitConverter.ToDouble(messageByteBuffer, value_offset);
                        field_buffer_offset += Constants.DOUBLE_INDICATOR_SIZE;
	                    Console.WriteLine( "field type:" + field_type + " field id:"+ field_id + " field value:" + field_value );
                    }
                    break;
            		
                case Constants.BOOL_FIELD_TYPE:
                    {
	                    bool field_value = BitConverter.ToBoolean(messageByteBuffer,value_offset);
                        field_buffer_offset += Constants.BOOL_INDICATOR_SIZE;
	                    Console.WriteLine( "field type:" + field_type + " field id:"+ field_id + " field value:" + field_value );
                    }
                    break;				
            		
                case Constants.YES_NO_NA_FIELD_TYPE:				
                case Constants.DIRECTIONAL_FIELD_TYPE:
                    {
	                    byte field_value = messageByteBuffer[value_offset];
                        field_buffer_offset += Constants.DIRECTIONAL_INDICATOR_SIZE;
	                    Console.WriteLine("field type:" + field_type + " field id:"+ field_id + " field value:" + field_value );
                    }
                    break;			

                case Constants.INT_FIELD_TYPE:
                    {
                        int field_value = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(messageByteBuffer,value_offset));
                        field_buffer_offset += Constants.INT_INDICATOR_SIZE;
                        Console.WriteLine("field type:" + field_type + " field id:" + field_id + " field value:" + field_value);
                    }
                    break;								
            						
                default:
                    {
	                    Console.WriteLine( "UNKNOWN DATA TYPE:"+field_type+" field id:"+field_id );
                        field_buffer_offset = messageLength;
                    }
                    break;
              } 		 
            }
            while (field_buffer_offset < (messageLength - Constants.CRC_SIZE));
	  
            Array.Reverse(messageByteBuffer, (messageLength - 4), 4);
            uint crcField = BitConverter.ToUInt32(messageByteBuffer, (messageLength - 4));

            Console.WriteLine("crc field: {0}", crcField);
            Console.WriteLine("--");
        }

    }
}
