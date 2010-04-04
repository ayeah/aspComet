﻿// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;

using AspComet.Eventing;

using Machine.Specifications;

using Rhino.Mocks;

namespace AspComet.Specifications.MessageHandlers
{
    public static class Constants
    {
        public const string MessageHandlingSubject = "Message handling";
    }

    public class EventHubMonitor
    {
        private readonly Dictionary<Type, IEvent> events = new Dictionary<Type, IEvent>();

        public void StartMonitoring<T>() where T : IEvent
        {
            EventHub.Subscribe<T>(ev => events[typeof(T)] = ev);
        }

        public void StartMonitoring<T1, T2>() 
            where T1 : IEvent 
            where T2 : IEvent
        {
            StartMonitoring<T1>();
            StartMonitoring<T2>();
        }

        public T RaisedEvent<T>() where T : IEvent
        {
            Type type = typeof(T);
            return events.ContainsKey(type) ? (T) events[type] : default(T);
        }
    }

    [Behaviors]
    public class ItHasHandledAMessage : MessageHandlerScenario<IMessageHandler>
    {
        It should_return_a_message_with_the_same_id_as_the_request_message = () =>
            result.Message.id.ShouldEqual(request.id);

        It should_return_a_message_with_the_same_channel_as_the_request_message = () =>
            result.Message.channel.ShouldEqual(request.channel);

        It should_return_a_message_with_the_same_clientId_as_the_request_message = () =>
            result.Message.clientId.ShouldEqual(request.clientId);
    }

    public class MessageHandlerScenario<TMessageHandler> : AutoStubbingScenario<TMessageHandler>
        where TMessageHandler : class, IMessageHandler
    {
        protected static Message request;
        protected static MessageHandlerResult result;
        protected static EventHubMonitor eventHubMonitor;
        protected static IClient client;

        Establish context = () =>
        {
            EventHub.Reset();
            eventHubMonitor = new EventHubMonitor();

            request = MessageBuilder.BuildRandomRequest();

            client = MockRepository.GenerateStub<IClient>();
            client.Stub(x => x.ID).Return(request.clientId);
        };
    }

    public static class MessageBuilder
    {
        private static readonly Random Random = new Random();

        public static Message BuildRandomRequest()
        {
            return new Message
            {
                channel = RandomString(),
                clientId = RandomString(),
                connectionType = RandomString(),
                error = RandomString(),
                id = RandomString(),
                minimumVersion = RandomString(),
                subscription = RandomString()
            };
        }

        private static string RandomString()
        {
            return Random.Next().ToString();
        }
    }
}
