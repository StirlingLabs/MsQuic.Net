//
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
//

namespace Microsoft.Quic
{
    public enum QUIC_SERVER_RESUMPTION_LEVEL
    {
        QUIC_SERVER_NO_RESUME,
        QUIC_SERVER_RESUME_ONLY,
        QUIC_SERVER_RESUME_AND_ZERORTT,
    }
}
