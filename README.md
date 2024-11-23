# Marie's Wonderland

An attempt at a private server implementation for mobile game NieR Reincarnation. The project is currently on hold due to a bug with Frida and newer versions of Android/Google Play that prevent running Frida Server correctly on mobile devices, more details [here](https://github.com/frida/frida/issues/2958).

## Game information
- Built in Unity 2019.4.29 with C# and IL2CPP
- Has a GRPC-based server
- Has a web-based API

## Requirements

#### PC
- [Android Platform Tools](https://developer.android.com/tools/releases/platform-tools)
- [Visual Studio 2022](https://visualstudio.microsoft.com/downloads)
- [Visual Studio Code](https://code.visualstudio.com/download) or similar text editor
- [ngrok](https://ngrok.com/download) or similar
    - HTTP/2 support is required
- [Frida tools](https://frida.re/docs/installation)

#### Phone
- Latest version of the game installed (v3.7.1)
- Rooted Android device
    - USB debugging enabled
    - [Frida server](https://frida.re/docs/android) installed and running

## How to run
- Connect your Android device to your PC and ensure it shows up when executing the following command: `adb devices`
- Run the GRPC server
    - Open the `MariesWonderland.sln` solution and run the project. The server runs on ports 7777 (HTTP) and 7776 (HTTPS)
- Run ngrok
    - Execute the following command: `ngrok http --app-protocol=http2 7777`
    - Take note of your public ngrok domain
- Hook and run the game
    - Open `frida/hooks.js` in VSCode and change the variable `SERVER_ADDRESS` to your ngrok domain
    - Execute the following command from the VSCode terminal (adjust the file path): `frida -Uf com.square_enix.android_googleplay.nierspww -l "path\to\hooks.js"`