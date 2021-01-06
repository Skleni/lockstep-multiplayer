using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using MultiplayerTest.Packets;
using softaware.ViewPort.Commands;
using softaware.ViewPort.Core;

namespace MultiplayerTest
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private static readonly IReadOnlyList<string> playerColors = new[]
        {
            "#FF0000",
            "#00FF00",
            "#0000FF",
            "#FFD800"
        };

        private string endpoint = "127.0.0.1";
        private int port = 15000;

        private Server server;
        private Client client;        

        public MainViewModel(bool isServer)
        {
            IsServer = isServer;

            StartServerCommand = new Command(StartServer);
            StartGameCommand = new Command(StartGame);
            ConnectCommand = new Command(Connect);

            Players = new ObservableCollection<Player>();
        }

        public bool IsServer { get; }
        public bool IsClient => !IsServer;

        public string Endpoint
        {
            get => endpoint;
            set => SetProperty(ref endpoint, value);
        }

        public int Port
        {
            get => port;
            set => SetProperty(ref port, value);
        }

        public ObservableCollection<Player> Players { get; }

        public Command StartServerCommand { get; }
        public Command StartGameCommand { get; }
        public Command ConnectCommand { get; }

        public void SendMoveOrder(double x, double y)
        {
            if (IsServer)
            {
                server.SendOrder(x, y);
            }
            else
            {
                client.SendOrder(x, y);
            }
        }

        private void StartServer()
        {
            server = new Server();

            server.PlayerConnected += id =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    AddPlayer(id);
                });
            };

            server.OrderReceived += OrderReceived;

            server.Start(Port);
        }

        private void StartGame()
        {
            AddPlayer(Players.Count);
            server.StartGame();
            StartGameLoop();
        }

        private void Connect()
        {
            client = new Client();
            client.Connect();
            client.OrderReceived += OrderReceived;

            client.GameStarted += players =>
            {
                App.Current.Dispatcher.Invoke(() => 
                {
                    for (int i = 0; i < players; i++)
                    {
                        AddPlayer(i);
                    }
                });

                StartGameLoop();
            };
        }

        private void StartGameLoop()
        {
            new Thread(GameLoop)
            {
                IsBackground = true
            }.Start();
        }

        private void GameLoop()
        {
            const double Speed = 100;

            var frameDuration = TimeSpan.FromSeconds(1 / 60.0);
            
            while (true)
            {
                foreach (var player in Players)
                {
                    var toTarget = (x: player.TargetX - player.X, y: player.TargetY - player.Y);
                    var length = Math.Sqrt(toTarget.x * toTarget.x + toTarget.y * toTarget.y);
                    if (length > 1)
                    {
                        player.X += toTarget.x / length * Speed * frameDuration.TotalSeconds;
                        player.Y += toTarget.y / length * Speed * frameDuration.TotalSeconds;
                    }
                }                

                Thread.Sleep(frameDuration);
            }
        }

        private void AddPlayer(int id)
        {
            Players.Add(new Player()
            {
                Color = playerColors[id],
                X = id * 100,
                TargetX = id * 100
            });
        }

        private void OrderReceived(Order order)
        {
            var player = this.Players[order.Player];
            player.TargetX = order.TargetX;
            player.TargetY = order.TargetY;
        }
    }
}
