using System.Net;
using System.Net.Sockets;

namespace AlphaFlashCSharpMcastClient
{
    class AlphaFlashMcastClient
    {
        private Socket m_sockMNI;
        private IPEndPoint receiveEndpoint;

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("AlphaFlashMcastClient Usage: <mcast address><port>");
                Environment.Exit(1);
            }

            string address = args[0];
            int port = int.Parse(args[1]);
            AlphaFlashMcastClient mcastClient = new AlphaFlashMcastClient(address, port);

            byte[] byteBufferRec = new byte[1024];

            try
            {
                EndPoint mniReceivePoint = mcastClient.receiveEndpoint;

                while (true)
                {
                    Array.Clear(byteBufferRec, 0, byteBufferRec.Length);

                    // Receive the multicast packet
                    int noOfBytesReceived = mcastClient.m_sockMNI.ReceiveFrom(byteBufferRec, 0, byteBufferRec.Length, SocketFlags.None, ref mniReceivePoint);

                    Console.WriteLine("No. of Bytes Received from MNI: {0}", noOfBytesReceived);
                    mcastClient.ReadMessageFromMNI(byteBufferRec, noOfBytesReceived);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Exception from AlphaFlashMcastClient " + e.ToString());
            }
            finally
            {
                mcastClient.m_sockMNI.Close();
            }
        }

        public AlphaFlashMcastClient(string address, int port)
	    {
            IPAddress ipAddress = IPAddress.Parse(address);
            ConnectToMNI(ipAddress, port);
	    }

        private void ConnectToMNI(IPAddress ipAddress, int port)
        {
            // Create the Socket
            m_sockMNI = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // Set the reuse address option
            m_sockMNI.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

            // Create an IPEndPoint and bind to it
            IPEndPoint ipEndpoint = new IPEndPoint(IPAddress.Any, port);
            m_sockMNI.Bind(ipEndpoint);

            // Add membership in the multicast group
            m_sockMNI.SetSocketOption(SocketOptionLevel.IP,SocketOptionName.AddMembership,
                        new MulticastOption(ipAddress, IPAddress.Any));

            // Create the EndPoint class
            receiveEndpoint = new IPEndPoint(IPAddress.Any, 0);
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
