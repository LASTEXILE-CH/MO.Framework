﻿using MO.WpfTest.Game;
using ProtoMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MO.WpfTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<long, GamePlayer> _totalPlayer;
        private GamePlayer curPlayer;
        private MOClient client;
        private MOPoint oldPoint;
        private Timer timer;
        private int lockedNum;
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                _totalPlayer = new Dictionary<long, GamePlayer>();
                client = new MOClient(ReceivedCallback);
                client.Login();
                client.ConnectGate();
                curPlayer = new GamePlayer(client.UserId);
                root.Children.Add(curPlayer.Rect_Player);
                oldPoint = new MOPoint();
                timer = new Timer(TimerCallback, null, 50, 50);
            }
            catch (Exception)
            {

            }
        }

        private void TimerCallback(object sender)
        {
            if (Interlocked.CompareExchange(ref lockedNum, 1, 0) == 0)
            {
                var curPoint = curPlayer.GetPoint();
                if (curPoint.X != oldPoint.X || curPoint.Y != oldPoint.Y)
                {
                    client.UploadPoint(curPoint);
                    oldPoint = curPoint.Clone();
                }
                Interlocked.Exchange(ref lockedNum, 0);
            }
        }

        private void ReceivedCallback(MOMsg msg)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (msg.ActionId == 100000)
                {
                    client.JoinRoom(10000);
                }
                else if (msg.ActionId == 100001)
                {
                    S2C100001 rep = S2C100001.Parser.ParseFrom(msg.Content);

                    foreach (var item in rep.UserPoints)
                    {
                        if (item.UserId == curPlayer.UserId)
                            continue;

                        if (!_totalPlayer.ContainsKey(item.UserId))
                        {
                            var newPlayer = new GamePlayer(item.UserId);
                            root.Children.Add(newPlayer.Rect_Player);
                            _totalPlayer.Add(item.UserId, newPlayer);
                            newPlayer.SetPoint(item.Point);
                        }
                    }
                }
                else if (msg.ActionId == 100002)
                {
                    S2C100002 rep = S2C100002.Parser.ParseFrom(msg.Content);
                    if (rep.UserId == curPlayer.UserId)
                        return;

                    if (!_totalPlayer.ContainsKey(rep.UserId))
                    {
                        var newPlayer = new GamePlayer(rep.UserId);
                        root.Children.Add(newPlayer.Rect_Player);
                        _totalPlayer.Add(rep.UserId, newPlayer);
                    }
                }
                else if (msg.ActionId == 100004)
                {
                    S2C100004 rep = S2C100004.Parser.ParseFrom(msg.Content);
                    if (rep.UserId == curPlayer.UserId)
                        return;

                    GamePlayer gamePlayer;
                    if (_totalPlayer.TryGetValue(rep.UserId, out gamePlayer))
                    {
                        gamePlayer.SetPoint(rep.Point);
                    }
                }
                else if (msg.ActionId == 100006)
                {
                    S2C100006 rep = S2C100006.Parser.ParseFrom(msg.Content);
                    if (rep.UserId == curPlayer.UserId)
                        return;
                    GamePlayer player = null;
                    if(_totalPlayer.TryGetValue(rep.UserId,out player))
                    {
                        root.Children.Remove(player.Rect_Player);
                    }
                }
            });
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            var step = (long)(this.Width / 100);
            var point = curPlayer.GetPoint();
            switch (e.Key)
            {
                case Key.Up:
                    point.Y -= step;
                    break;
                case Key.Down:
                    point.Y += step;
                    break;
                case Key.Left:
                    point.X -= step;
                    break;
                case Key.Right:
                    point.X += step;
                    break;
            }
            curPlayer.SetPoint(point);
            //client.UploadPoint(curPlayer.GetPoint());
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            client.Exit();
        }
    }
}