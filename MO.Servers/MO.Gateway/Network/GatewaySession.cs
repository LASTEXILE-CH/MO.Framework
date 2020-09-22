﻿using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MO.Algorithm.Actions;
using MO.Algorithm.Actions.Enum;
using MO.Algorithm.Redis;
using MO.Common.Config;
using MO.Common.Security;
using MO.GrainInterfaces.Network;
using Newtonsoft.Json;
using Orleans;
using ProtoMessage;
using SshNet.Security.Cryptography;
using System;
using System.Threading.Tasks;

namespace MO.Gateway.Network
{
    public class GatewaySession
    {
        private readonly IClusterClient _client;
        private readonly ILogger _logger;
        private readonly Guid _sessionId;
        private readonly OutcomingPacketObserver _outcomingPacketObserver;
        private readonly IConfiguration _configuration;

        private IClientboundPacketObserver _clientboundPacketObserverRef;
        private IChannelHandlerContext _context;
        private IPacketRouter _router;

        public GatewaySession(IClusterClient client, ILoggerFactory loggerFactory,
            IConfiguration configuration, IChannelHandlerContext context)
        {
            _client = client;
            _logger = loggerFactory.CreateLogger<GatewaySession>();
            _configuration = configuration;
            _context = context;

            _sessionId = Guid.NewGuid();
            _outcomingPacketObserver = new OutcomingPacketObserver(this);
        }

        public async Task Startup()
        {
            _clientboundPacketObserverRef = await _client.CreateObjectReference<IClientboundPacketObserver>(_outcomingPacketObserver);
            await _client.GetGrain<IClientboundPacketSink>(_sessionId).Subscribe(_clientboundPacketObserverRef);
            _router = _client.GetGrain<IPacketRouter>(_sessionId);
        }

        public async Task Disconnect()
        {
            await _router.Disconnect();
        }

        public async Task DispatchIncomingPacket(MOMsg packet)
        {
            try
            {
                //md5签名验证
                //var key = _configuration.GetValue<string>("MD5Key");
                //var data = packet.ToByteString();
                //if (CryptoHelper.MD5_Encrypt($"{data}{key}").ToLower() != packet.Sign.ToLower())
                //{
                //    return;
                //}

                //token验证
                if (TokenRedis.Client.Get<string>(packet.UserId.ToString()) != packet.Token)
                {
                    await DispatchOutcomingPacket(packet.ParseResult(ErrorType.Hidden, "Token验证失败"));
                    return;
                }

                //刷新token时间
                await TokenRedis.Client.ExpireAsync(packet.UserId.ToString(), GameConstants.TOKENEXPIRE);
                await _client.GetGrain<IClientboundPacketSink>(_sessionId).Subscribe(_clientboundPacketObserverRef);
                await _router.SendPacket(packet);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"DispatchIncomingPacket异常:\n" +
                    $"{ex.Message}\n" +
                    $"{ex.StackTrace}\n" +
                    $"{JsonConvert.SerializeObject(packet)}");
            }
        }

        public async Task DispatchOutcomingPacket(MOMsg packet)
        {
            try
            {
                if (_context == null)
                    return;

                var bytes = packet.ToByteArray();
                IByteBuffer buffer = Unpooled.WrappedBuffer(bytes);
                await _context.WriteAndFlushAsync(buffer);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"DispatchOutcomingPacket异常:\n" +
                    $"{ex.Message}\n" +
                    $"{ex.StackTrace}\n" +
                    $"{JsonConvert.SerializeObject(packet)}");
            }
        }

        public async Task OnClosed()
        {
            if (_context == null)
                return;
            await _context.CloseAsync();
        }

        class OutcomingPacketObserver : IClientboundPacketObserver
        {
            private readonly GatewaySession session;

            public OutcomingPacketObserver(GatewaySession session)
            {
                this.session = session;
            }

            public async void OnClosed()
            {
                await session.OnClosed();
            }

            public async void ReceivePacket(MOMsg packet)
            {
                await session.DispatchOutcomingPacket(packet);
            }
        }
    }
}