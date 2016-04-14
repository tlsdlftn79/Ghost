﻿using Server.Common.IO;
using Server.Common.IO.Packet;
using Server.Common.Utilities;
using System;
using System.Net.Sockets;

namespace Server.Common.Net
{
    public abstract class Session : IDisposable
    {
        public const int ReceiveSize = 262144;

        private readonly Socket m_socket;

        private byte[] m_recvBuffer;
        private byte[] m_buffer;
        private int m_offset;

        private bool m_disposed;

        //private MapleIV m_siv;
        //private MapleIV m_riv;

        private object m_sendSync;

        public string Title { get; set; }

        public bool Disposed
        {
            get
            {
                return m_disposed;
            }
        }

        public Session(Socket socket)
        {
            m_socket = socket;
            m_socket.NoDelay = true;

            Title = socket.RemoteEndPoint.ToString().Split(':')[0];

            m_recvBuffer = new byte[ReceiveSize];
            m_buffer = new byte[ReceiveSize];
            m_offset = 0;

            m_disposed = false;

            m_sendSync = new object();

            //m_siv = new MapleIV((uint)Randomizer.Next());
            //m_riv = new MapleIV((uint)Randomizer.Next());

            Register();

            Receive();
        }

        protected abstract void Register();
        protected abstract void Unregister();
        protected abstract void Dispatch(InPacket inPacket);


        private void Receive()
        {
            if (m_disposed)
                return;

            SocketError errorCode = SocketError.Success;

            m_socket.BeginReceive(m_recvBuffer, 0, m_recvBuffer.Length, SocketFlags.None, out errorCode, EndReceive, null);

            if (errorCode != SocketError.Success)
                Dispose();
        }
        private void EndReceive(IAsyncResult ar)
        {
            if (!m_disposed)
            {
                SocketError errorCode = SocketError.Success;

                int length = m_socket.EndReceive(ar, out errorCode);

                if (errorCode != SocketError.Success || length == 0)
                {
                    Dispose();
                }
                else
                {
                    Append(length);
                    ManipulateBuffer();
                    Receive();
                }
            }
        }

        private void Append(int length)
        {
            if (m_buffer.Length - m_offset < length)
            {
                int newSize = m_buffer.Length * 2;

                while (newSize < m_offset + length)
                    newSize *= 2;

                Array.Resize<byte>(ref m_buffer, newSize);
            }

            Buffer.BlockCopy(m_recvBuffer, 0, m_buffer, m_offset, length);

            m_offset += length;
        }
        private void ManipulateBuffer()
        {
            while (m_offset >= 4 && m_disposed == false)
            {
                int size = 0;//MapleAes.GetLength(m_buffer);

                if (m_offset < size + 4)
                {
                    break;
                }

                var packetBuffer = new byte[size];
                Buffer.BlockCopy(m_buffer, 4, packetBuffer, 0, size);

                //MapleAes.Transform(packetBuffer, m_riv);

                m_offset -= size + 4;

                if (m_offset > 0)
                {
                    Buffer.BlockCopy(m_buffer, size + 4, m_buffer, 0, m_offset);
                }

                this.Dispatch(new InPacket(packetBuffer));
            }

        }

        public void Send(OutPacket outPacket)
        {
            if (m_disposed)
                return;

            lock (m_sendSync)
            {
                if (m_disposed)
                    return;

                byte[] packet = outPacket.Content;
                byte[] final = new byte[packet.Length + 4];

                //MapleAes.GetHeader(final, m_siv, Constants.Version.Major);
                //MapleAes.Transform(packet, m_siv);

                Buffer.BlockCopy(packet, 0, final, 4, packet.Length);

                SendRaw(final);
            }
        }
        private bool SendRaw(byte[] final)
        {
            int offset = 0;

            while (offset < final.Length)
            {
                SocketError errorCode = SocketError.Success;
                int sent = m_socket.Send(final, offset, final.Length - offset, SocketFlags.None, out errorCode);

                if (sent == 0 || errorCode != SocketError.Success)
                {
                    Dispose();
                    return false;
                }

                offset += sent;
            }

            return true;
        }

        public void Dispose()
        {
            if (!m_disposed)
            {
                m_disposed = true;

                try
                {
                    m_socket.Shutdown(SocketShutdown.Both);
                    m_socket.Disconnect(false);
                    m_socket.Dispose();
                }
                catch { }

                m_buffer = null;
                m_offset = 0;

                //m_siv = null;
                //m_riv = null;

                Unregister();
            }
        }
    }
}
