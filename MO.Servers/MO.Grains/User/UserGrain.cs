﻿using MO.GrainInterfaces;
using MO.GrainInterfaces.Network;
using MO.GrainInterfaces.User;
using Orleans;
using Orleans.Streams;
using ProtoMessage;
using System;
using System.Threading.Tasks;

namespace MO.Grains.User
{
    public class UserGrain : Grain, IUser, IAsyncObserver<MOMsg>
    {
        private IClientboundPacketSink _sink;
        private StreamSubscriptionHandle<MOMsg> _globalHandler;
        private StreamSubscriptionHandle<MOMsg> _roomHandler;
        private int _roomId;

        public override async Task OnActivateAsync()
        {
            await base.OnActivateAsync();
        }

        public override async Task OnDeactivateAsync()
        {
            await UnbindClientPacketSink();
            //取消消息订阅
            await UnsubscribeGlobal();
            await UnsubscribeRoom();
            await base.OnDeactivateAsync();
        }

        #region 订阅消息

        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            return Task.CompletedTask;
        }

        public async Task OnNextAsync(MOMsg item, StreamSequenceToken token = null)
        {
            await Notify(item);
        }

        #endregion

        public Task BindClientPacketSink(IClientboundPacketSink sink)
        {
            if (_sink != null)
                _sink.Close();
            _sink = sink;
            return Task.CompletedTask;
        }

        public Task UnbindClientPacketSink()
        {
            _sink = null;
            return Task.CompletedTask;
        }

        public async Task Notify(MOMsg packet)
        {
            if (_sink != null)
                await _sink.SendPacket(packet);
        }

        public async Task SubscribeGlobal(Guid streamId)
        {
            if (_globalHandler == null)
            {
                var streamProvider = this.GetStreamProvider(StreamProviders.JobsProvider);
                var stream = streamProvider.GetStream<MOMsg>(streamId, StreamProviders.Namespaces.ChunkSender);
                _globalHandler = await stream.SubscribeAsync(OnNextAsync);
            }
        }

        public async Task UnsubscribeGlobal()
        {
            //取消全服消息订阅
            if (_globalHandler != null)
            {
                await _globalHandler.UnsubscribeAsync();
                _globalHandler = null;
            }
        }

        public async Task SubscribeRoom(Guid streamId)
        {
            if (_roomHandler == null)
            {
                var streamProvider = this.GetStreamProvider(StreamProviders.JobsProvider);
                var stream = streamProvider.GetStream<MOMsg>(streamId, StreamProviders.Namespaces.ChunkSender);
                _roomHandler = await stream.SubscribeAsync(OnNextAsync);
            }
        }

        public async Task UnsubscribeRoom()
        {
            if (_roomHandler != null)
            {
                await _roomHandler.UnsubscribeAsync();
                _roomHandler = null;
            }
        }

        public Task SetRoomId(int roomId)
        {
            _roomId = roomId;
            return Task.CompletedTask;
        }

        public Task<int> GetRoomId()
        {
            return Task.FromResult(_roomId);
        }
    }
}