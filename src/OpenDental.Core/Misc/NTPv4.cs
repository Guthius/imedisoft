using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace OpenDentBusiness;

public class NTPv4
{
    private const long TicksPerSecond = TimeSpan.TicksPerSecond;
    private static readonly DateTime Epoch = new(1900, 1, 1);
        
    public double GetTime(string nistServerUrl)
    {
        var arrayReceivedPacket = new byte[48];
        var addresses = Dns.GetHostEntry(nistServerUrl).AddressList;
        var ipEndPoint = new IPEndPoint(addresses[0], 123);
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.ReceiveTimeout = 2000; //Two seconds.  Too short?
        socket.Connect(ipEndPoint);
        //Create packet for sending then send to NIST server.
        socket.Send(MakePacket());
        try
        {
            socket.Receive(arrayReceivedPacket);
        }
        catch
        {
            //Response not received before Timeout.
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            socket.Dispose();
            var messageTextFail = "NTPv4 time request from " + nistServerUrl + " timed out.";
            EventLog.WriteEntry("OpenDental", messageTextFail, EventLogEntryType.Information);
            return double.MaxValue;
        }

        //Convert the received NTP packet to a usable time stamp.
        var destination = DateTime.Now.ToUniversalTime();
        var originate = RawToDateTime(arrayReceivedPacket, 24);
        var receive = RawToDateTime(arrayReceivedPacket, 32);
        var transmit = RawToDateTime(arrayReceivedPacket, 40);
        var offset = (receive - originate - (destination - transmit)).TotalMilliseconds / 2; //Offset calculation based off Ntpv4 specification.
        //Close connection
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
        socket.Dispose();
        var messageText = "NTPv4 time request received from " + nistServerUrl + "."
                          + "\nOriginate:  " + originate.ToString("hh:mm:ss.fff tt")
                          + "\nReceive:  " + receive.ToString("hh:mm:ss.fff tt")
                          + "\nTransmit:  " + transmit.ToString("hh:mm:ss.fff tt")
                          + "\nDestination:  " + destination.ToString("hh:mm:ss.fff tt")
                          + "\nOffset:  " + offset + " milliseconds";
        EventLog.WriteEntry("OpenDental", messageText, EventLogEntryType.Information);
        return offset;
    }

    private static DateTime RawToDateTime(byte[] arraySource, int startIdx)
    {
        ulong seconds = 0;
        var arraySeconds = new byte[8];
        for (var i = 0; i <= 3; i++)
        {
            arraySeconds[3 - i] = arraySource[startIdx + i];
        }

        seconds = BitConverter.ToUInt64(arraySeconds, 0);
        ulong fractions = 0;
        var arrayFractions = new byte[8];
        for (var i = 4; i <= 7; i++)
        {
            arrayFractions[7 - i] = arraySource[startIdx + i];
        }

        fractions = BitConverter.ToUInt64(arrayFractions, 0);
        var ticks = seconds * TicksPerSecond + fractions * TicksPerSecond / 0x100000000L;
        return Epoch + TimeSpan.FromTicks((long) ticks);
    }

    private byte[] MakePacket()
    {
        var arrayPacket = new byte[48];
        //byte 0
        arrayPacket[0] = 0x1B; //Identifies us as a Client, and using Version NTPv4
        //byte 1-39 don't fill
        //byte 40-47 (Current system time)
        var ticks = (ulong) (DateTime.Now.ToUniversalTime() - Epoch).Ticks;
        var seconds = ticks / TicksPerSecond;
        var fractions = ticks % TicksPerSecond * 0x100000000L / TicksPerSecond;
        var arraySeconds = BitConverter.GetBytes(seconds);
        var arrayFractions = BitConverter.GetBytes(fractions);
        for (var i = 3; i >= 0; i--)
        {
            arrayPacket[40 + i] = arraySeconds[3 - i];
        }

        for (var i = 7; i >= 4; i--)
        {
            arrayPacket[40 + i] = arrayFractions[7 - i];
        }

        return arrayPacket;
    }
}