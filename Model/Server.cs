using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;

namespace TcpServer.Model;
public class Server
{
    public TcpListener tcpListener;
    public TcpClient TcpClient;
    NetworkStream? stream;
    public byte[] Buffer;

    public Server(string ip, int port)
    {
        this.tcpListener = new TcpListener(IPAddress.Parse(ip), port);
        this.TcpClient = new();
        this.Buffer = new byte[1024];
        this.tcpListener.Start();
    }

    public async Task ClientConnect() => this.TcpClient = await this.tcpListener.AcceptTcpClientAsync();
    public void Stop()
    {
        this.tcpListener.Stop();
        this.TcpClient.Client.Close();
    }
    public async Task<int> Receive()
    {
        try
        {
            this.stream = this.TcpClient.GetStream();
            return await this.stream.ReadAsync(this.Buffer);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return 0;
        }
    }
    public async void Send(string message)
    {
        if(this.stream != null)
            await this.stream.WriteAsync(Encoding.UTF8.GetBytes(message));
    }
}
