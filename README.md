![MsQuic.Net](https://raw.githubusercontent.com/StirlingLabs/MsQuic.Net/main/msquic-dotnet.jpg)

### Stirling Labs' .Net wrapper for [MsQuic](https://github.com/microsoft/msquic)

## 🚀 How to install

Most people should just use [the NuGet package](https://www.nuget.org/packages/StirlingLabs.MsQuic/).

```bash
> dotnet add PROJECT package StirlingLabs.MsQuic
> dotnet restore # to download and apply the native bindings for your platform
```

If know what you're doing and want to run our in-development GitHub CI builds, configure [GitHub Packages for Stirling Labs](https://github.com/StirlingLabs/Logging/blob/master/docs/GitHubPackages.md) and restore.


## 👀 What's included

MsQuic.Net solves our requirements for cross-platform datagrams and Quic access by wrapping [MsQuic](https://github.com/microsoft/msquic) and using portable certificates (i.e. [QuicTLS/OpenSSL](https://github.com/quictls/openssl)) and our [portable sockets](https://github.com/StirlingLabs/sockaddr.Net) implementation.

## 🐣 Lifecycle

- PRs that further the project's goals are encouraged! 
- The intent is to support the current [MsQuic](https://github.com/microsoft/msquic) release. 
- All native binaries are built by Microsoft and packaged into NuPkg by our CI.
- Release versions of MsQuic.Net follow our internal [numbering](https://github.com/StirlingLabs/Version.Net) system, while the native bindings naturally follow Microsoft's release numbering.

Developed by [Stirling Labs](https://stirlinglabs.github.io).
