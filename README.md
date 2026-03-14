# Marie's Wonderland

An attempt at a private server implementation for the mobile game NieR Reincarnation.

## Game information
- Built in Unity 2019.4.29 with C# and IL2CPP
- Has a gRPC-based API server
- Has a supplementary web-based HTTP API

**Current status:** We can get past the initial loading screens and reach the in-game state, but the game is not fully playable, yet.

## Requirements

#### PC
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Visual Studio 2026](https://visualstudio.microsoft.com/downloads) or [Rider](https://www.jetbrains.com/rider/)
- [Android Platform Tools](https://developer.android.com/tools/releases/platform-tools) (`adb`)

#### Phone
- An Android device or an [Android Studio](https://developer.android.com/studio) emulator (physical device not required)

#### Patching (Google Colab)
- A Google account to run the patcher notebook

## Setup overview

### 1. Run the server
Open `MariesWonderland.sln` and run the project, or from the `src/` directory

The server listens on the standard HTTP and HTTPS ports on localhost:
- `http://localhost` (port 80) - used for HTTP asset serving
- `https://localhost` (port 443) - used for gRPC (HTTP/2)

### 2. Expose the server
The game communicates over gRPC (HTTP/2). You do not need ngrok or an external tunnel if your emulator or device can reach your machine directly.

- If running on an emulator: configure the emulator to reach the host (Android Studio emulators usually can reach the host).
- If running from a remote device or across a network: open ports 80 and 443 on your firewall/NAT and ensure those ports are forwarded to the machine running the server so the game can reach `http(s)://<your-host>`.

Ensure any network path supports HTTP/2 for gRPC traffic on port 443.

### 3. Patch the APK
The `scripts/patcher.ipynb` notebook runs entirely in Google Colab - no local toolchain needed.

1. Open [Google Colab](https://colab.research.google.com) and upload `scripts/patcher.ipynb`
2. Fill in the configuration at the top of the code cell:
   - `protocol` - `http` for plain tunnels, `https` for TLS-terminated endpoints
   - `server_host` - your hostname (without protocol), e.g. `192.168.1.1`
   - `server_port` - leave empty unless using a non-standard port
3. Run the single code cell
4. Wait for it to complete and the patched APK will be automatically downloaded

**What the patcher does:**
- Rewrites server URLs and hostnames in `global-metadata.dat` (IL2CPP string literals)
- Applies ARM64 binary patches to `libil2cpp.so`: SSL bypass, encryption passthrough, plain Octo asset list
- When using `http`: patches `AndroidManifest.xml` and `network_security_config.xml` to allow cleartext traffic

> **Note:** Replacement strings must not be longer than the originals. Use short hostnames and omit the port if you run into length issues.

### 4. Install and run
Install `NieRReincarnation-patched.apk` in your phone or emulator. Launch the game. It will connect to your local server.

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
      "LatestMasterDataVersion": "20240404193219",
      "UserDataPath": "Data/UserData",
      "MasterDataPath": "Data/MasterData"
    }
  }
}
```

- The `ResourcesBaseUrl` value must be exactly 43 characters long.
- If you change the length of that segment, you may also need to update the server-side minimal API that serves the short path (the `/aaaaaaaa...` handler) so its expected length matches your new value.

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
scripts/
  patcher.ipynb       Google Colab APK patcher notebook
  hooks.js            legacy Frida hooks (for reference only)
```

## Disclaimer
See [DISCLAIMER.md](DISCLAIMER.md).

## Special Thanks
 - [onepiecefreak3](https://github.com/onepiecefreak3)
 - [Walter-Sparrow](https://github.com/Walter-Sparrow)
