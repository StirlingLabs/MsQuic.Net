//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

namespace Microsoft.Quic
{
    public unsafe partial struct QUIC_API_TABLE
    {
        [NativeTypeName("QUIC_SET_CONTEXT_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, void> SetContext;

        [NativeTypeName("QUIC_GET_CONTEXT_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*> GetContext;

        [NativeTypeName("QUIC_SET_CALLBACK_HANDLER_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, void*, void> SetCallbackHandler;

        [NativeTypeName("QUIC_SET_PARAM_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, uint, uint, void*, int> SetParam;

        [NativeTypeName("QUIC_GET_PARAM_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, uint, uint*, void*, int> GetParam;

        [NativeTypeName("QUIC_REGISTRATION_OPEN_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_REGISTRATION_CONFIG*, QUIC_HANDLE**, int> RegistrationOpen;

        [NativeTypeName("QUIC_REGISTRATION_CLOSE_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void> RegistrationClose;

        [NativeTypeName("QUIC_REGISTRATION_SHUTDOWN_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_CONNECTION_SHUTDOWN_FLAGS, ulong, void> RegistrationShutdown;

        [NativeTypeName("QUIC_CONFIGURATION_OPEN_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_BUFFER*, uint, QUIC_SETTINGS*, uint, void*, QUIC_HANDLE**, int> ConfigurationOpen;

        [NativeTypeName("QUIC_CONFIGURATION_CLOSE_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void> ConfigurationClose;

        [NativeTypeName("QUIC_CONFIGURATION_LOAD_CREDENTIAL_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_CREDENTIAL_CONFIG*, int> ConfigurationLoadCredential;

        [NativeTypeName("QUIC_LISTENER_OPEN_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_LISTENER_EVENT*, int>, void*, QUIC_HANDLE**, int> ListenerOpen;

        [NativeTypeName("QUIC_LISTENER_CLOSE_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void> ListenerClose;

        [NativeTypeName("QUIC_LISTENER_START_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_BUFFER*, uint, sockaddr*, int> ListenerStart;

        [NativeTypeName("QUIC_LISTENER_STOP_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void> ListenerStop;

        [NativeTypeName("QUIC_CONNECTION_OPEN_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_CONNECTION_EVENT*, int>, void*, QUIC_HANDLE**, int> ConnectionOpen;

        [NativeTypeName("QUIC_CONNECTION_CLOSE_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void> ConnectionClose;

        [NativeTypeName("QUIC_CONNECTION_SHUTDOWN_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_CONNECTION_SHUTDOWN_FLAGS, ulong, void> ConnectionShutdown;

        [NativeTypeName("QUIC_CONNECTION_START_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_HANDLE*, ushort, sbyte*, ushort, int> ConnectionStart;

        [NativeTypeName("QUIC_CONNECTION_SET_CONFIGURATION_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_HANDLE*, int> ConnectionSetConfiguration;

        [NativeTypeName("QUIC_CONNECTION_SEND_RESUMPTION_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_SEND_RESUMPTION_FLAGS, ushort, byte*, int> ConnectionSendResumptionTicket;

        [NativeTypeName("QUIC_STREAM_OPEN_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_STREAM_OPEN_FLAGS, delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void*, QUIC_STREAM_EVENT*, int>, void*, QUIC_HANDLE**, int> StreamOpen;

        [NativeTypeName("QUIC_STREAM_CLOSE_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, void> StreamClose;

        [NativeTypeName("QUIC_STREAM_START_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_STREAM_START_FLAGS, int> StreamStart;

        [NativeTypeName("QUIC_STREAM_SHUTDOWN_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_STREAM_SHUTDOWN_FLAGS, ulong, int> StreamShutdown;

        [NativeTypeName("QUIC_STREAM_SEND_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_BUFFER*, uint, QUIC_SEND_FLAGS, void*, int> StreamSend;

        [NativeTypeName("QUIC_STREAM_RECEIVE_COMPLETE_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, ulong, void> StreamReceiveComplete;

        [NativeTypeName("QUIC_STREAM_RECEIVE_SET_ENABLED_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, byte, int> StreamReceiveSetEnabled;

        [NativeTypeName("QUIC_DATAGRAM_SEND_FN")]
        public delegate* unmanaged[Cdecl]<QUIC_HANDLE*, QUIC_BUFFER*, uint, QUIC_SEND_FLAGS, void*, int> DatagramSend;
    }
}
