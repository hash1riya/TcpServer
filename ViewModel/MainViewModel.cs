﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TcpServer.Model;

namespace TcpServer.ViewModel;
public partial class MainViewModel : ObservableObject
{
    public required Server Server;

    [ObservableProperty]
    public bool isRunning = false;
    [ObservableProperty]
    public bool isNotRunning = true;
    [ObservableProperty]
    public bool isConnected = false;
    [ObservableProperty]
    private string ip = "127.0.0.1";
    [ObservableProperty]
    private int port = 1024;
    [ObservableProperty]
    private string message = string.Empty;

    public ObservableCollection<Message> MessageHistory { get; set; } = [];

    public async void Run()
    {
        this.IsRunning = true;
        this.IsNotRunning = false;
        int bytesRead = 0;
        try
        {
            await this.Server.ClientConnect();
            while (this.Server.TcpClient.Connected)
            {
                if (!this.IsConnected)
                    this.MessageHistory.Add(new Message()
                    {
                        Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                        Content = $"{this.Server.TcpClient.Client.RemoteEndPoint} connected",
                        Sender = "SYSTEM:"
                    });
                this.IsConnected = true;
                bytesRead = await Server.Receive();
                if(!string.IsNullOrEmpty(System.Text.Encoding.UTF8.GetString(Server.Buffer, 0, bytesRead)))
                    this.MessageHistory.Add(new Message()
                    {
                        Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                        Content = System.Text.Encoding.UTF8.GetString(Server.Buffer, 0, bytesRead),
                        Sender = $"{this.Server.TcpClient.Client.RemoteEndPoint}:"
                    });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
        finally
        {
            if (IsRunning)
            {
                this.MessageHistory.Add(new Message()
                {
                    Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    Content = $"{this.Server.TcpClient.Client.RemoteEndPoint} disconnected",
                    Sender = "SYSTEM:"
                });
                this.Run();
            }
            this.IsConnected = false;
        }
    }


    [RelayCommand]
    private void Start()
    {
        this.Server = new Server(Ip, Port);
        this.MessageHistory.Add(new Message()
        {
            Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            Content = "Server was setted up. Waiting for client...",
            Sender = "SYSTEM:"
        });
        this.Run();
    }
    [RelayCommand]
    private void Stop()
    {
        this.IsRunning = false;
        this.IsNotRunning = true;
        this.IsConnected = false;
        this.Server.Stop();
        this.MessageHistory.Add(new Message()
        {
            Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            Content = "Server was shutted down!",
            Sender = "SYSTEM:"
        });
    }
    [RelayCommand]
    private void Send()
    {
        this.Server.Send(this.Message);
        this.MessageHistory.Add(new Message()
        {
            Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            Sender = "me:",
            Content = this.Message
        });
        if (this.Message.ToLower() == "bye")
            this.Stop();
        this.Message = string.Empty;
    }
}
