![MsQuic.Net](https://raw.githubusercontent.com/StirlingLabs/MsQuic.Net/main/msquic-dotnet.jpg)

### Stirling Labs' .Net wrapper for [MsQuic](https://github.com/microsoft/msquic)

## 🚀 How to install

Most people should just use [the NuGet package](https://www.nuget.org/packages/StirlingLabs.MsQuic/).

If know what you're doing and want to run our internal GitHub builds, configure [GitHub Packages for Stirling Labs](https://github.com/StirlingLabs/Logging/blob/master/docs/GitHubPackages.md) then:

```bash
> dotnet add PROJECT package StirlingLabs.MsQuic
```

## 👀 What's included

MsQuic.Net solves our requirements for cross-platform datagrams and Quic access by wrapping [MsQuic](https://github.com/microsoft/msquic) and using portable certificates (i.e. [QuicTLS/OpenSSL](https://github.com/quictls/openssl)) and our [portable sockets](hhttps://github.com/StirlingLabs/sockaddr.Net) implementation.

## 🐣 Lifecycle

- PRs that further the project's goals are encouraged! 
- The intent is to support the current [MsQuic](https://github.com/microsoft/msquic) release.
- Release versions follow our internal [numbering](https://github.com/StirlingLabs/Version.Net) system.

Developed by [Stirling Labs](https://stirlinglabs.github.io).
