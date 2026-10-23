# Marie's Wonderland

An open-source server implementation for mobile game NieR Reincarnation.

## Requirements

#### PC
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio 2026](https://visualstudio.microsoft.com/downloads) or [Rider](https://www.jetbrains.com/rider/)
- [Android Platform Tools](https://developer.android.com/tools/releases/platform-tools) (`adb`)

#### Phone
- An Android device or an [Android Studio](https://developer.android.com/studio) emulator (physical device not required)

## Setup overview

### 1. Run the server
Open `MariesWonderland.slnx` and run the project.

By default the server listens on every network address on two ports (see [Ports and certificates](#ports-and-certificates) to change them):
- port 80 (HTTP/1.1 and HTTP/2): asset serving
- port 443 (HTTP/2): gRPC

### 2. Expose the server
The game communicates over gRPC (HTTP/2). You do not need ngrok or an external tunnel if your emulator or device can reach your machine directly.

- If running on an emulator: configure the emulator to reach the host (Android Studio emulators usually can reach the host).
- If running from a remote device or across a network: open ports 80 and 443 on your firewall/NAT and ensure those ports are forwarded to the machine running the server so the game can reach `http(s)://<your-host>`.

Ensure any network path supports HTTP/2 for gRPC traffic on port 443.

## Configuration
Server settings live in `src/appsettings.development.json`:

```json
{
  "Server": {
    "Paths": {
      "AssetDatabase": "<path to extracted asset revisions>",
      "MasterDatabase": "<path to extracted master data>",
      "ResourcesBaseUrl": "http://<your-host>/aaaaaaaaaaaaaaaaaaaaaaaa"
    },
    "Data": {
      "LatestMasterDataVersion": "1234567890",
      "UserDataPath": "Data/UserData"
    }
  }
}
```

- The `ResourcesBaseUrl` value must be exactly 43 characters long.
- If you change the length of that segment, you may also need to update the server-side minimal API that serves the short path (the `/aaaaaaaa...` handler) so its expected length matches your new value.

### Ports and certificates
The listening addresses come from the standard ASP.NET Core `Kestrel:Endpoints` section in `src/appsettings.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": { "Url": "http://*:80", "Protocols": "Http1AndHttp2" },
      "Grpc": { "Url": "http://*:443", "Protocols": "Http2" }
    }
  }
}
```

Override them in `src/appsettings.Local.json` (git-ignored) to use other ports, bind one address, or run behind a reverse proxy. To serve gRPC over TLS, give its endpoint an `https://` URL and a certificate:

```json
"Grpc": {
  "Url": "https://*:443",
  "Protocols": "Http2",
  "Certificate": { "Path": "cert.pfx", "Password": "<password>" }
}
```

They can also be set from environment variables, e.g. `Kestrel__Endpoints__Grpc__Url=http://*:5001` (handy in Docker).

## Project structure
```
src/                  .NET 10 gRPC + HTTP server
  proto/              protobuf service definitions
  Services/           gRPC service implementations
  Data/               in-memory data stores (master + user)
  Models/             entity and type definitions
  Extensions/         DI, HTTP, and gRPC helpers
  Configuration/      strongly-typed options
  Http/               HTTP API handlers (asset serving, etc.)
  Interceptors/       gRPC interceptors (diff, logging, auth)
  Helpers/            shared game logic helpers
tests/                xUnit test project
  Infrastructure/     shared test base classes and fixtures
  Interceptors/       interceptor unit tests
```

## Disclaimer
See [DISCLAIMER.md](DISCLAIMER.md).

## Special Thanks
 - [onepiecefreak3](https://github.com/onepiecefreak3)
 - [Walter-Sparrow](https://github.com/Walter-Sparrow)
