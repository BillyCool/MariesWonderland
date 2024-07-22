//#region Hooking

let libil2cpp;
let lastEnterLog = "", lastLeaveLog = "";
let lastEnterLogCount = 0, lastLeaveLogCount = 0;
const deduplicateLogs = true;

function awaitLibil2cpp(callback) {
  libil2cpp = Module.findBaseAddress('libil2cpp.so');

  if (libil2cpp) {
    console.log('libil2cpp.so loaded:', libil2cpp);
    callback();
  }
  else {
    setTimeout(() => awaitLibil2cpp(callback), 100);
  }
};

function onEnterLogWrapper(caller, log, debug = false) {
  var log = !log
    ? `-> ${new Date().toLocaleTimeString()} > ${caller}`
    : `-> ${new Date().toLocaleTimeString()} > ${caller}: ${log}`;

    console.log(log);

    if (debug) {
      console.log(Thread.backtrace(this.context, Backtracer.ACCURATE)
        .map(DebugSymbol.fromAddress).join('\n') + '\n');
    }
}

function onLeaveLogWrapper(caller, log, debug = false) {
  var log = !log
    ? `<- ${new Date().toLocaleTimeString()} > ${caller}`
    : `<- ${new Date().toLocaleTimeString()} > ${caller}: ${log}`;

    console.log(log);

    if (debug) {
      console.log(Thread.backtrace(this.context, Backtracer.ACCURATE)
        .map(DebugSymbol.fromAddress).join('\n') + '\n');
    }
}

function interceptLogWrapper(caller, log, debug = false) {
  var log = !log
    ? `<-> ${new Date().toLocaleTimeString()} > ${caller}`
    : `<-> ${new Date().toLocaleTimeString()} > ${caller}: ${log}`;

    console.log(log);

    if (debug) {
      console.log(Thread.backtrace(this.context, Backtracer.ACCURATE)
        .map(DebugSymbol.fromAddress).join('\n') + '\n');
    }
}

function callbackWrapper(callback, offset, debug = false) {
  const hexString = '0x' + offset.toString(16).toUpperCase();
  console.log(`Hooking ${callback.name}() at offset ${hexString}`);
  callback(offset, debug);
}

function genericEnterHook(name, offset, debug = false) {
  const func_ptr = libil2cpp.add(offset);
  Interceptor.attach(func_ptr, {
    onEnter(args) {
      if (lastEnterLog == name && deduplicateLogs) {
        lastEnterLogCount++;
      }
      else {
        if (lastEnterLogCount > 1) {
          onLeaveLogWrapper(lastEnterLog, `(x${lastEnterLogCount})`, debug);
        }
        lastEnterLogCount = 0;
        lastEnterLog = name;
        onEnterLogWrapper(name, null, debug);
      }
    },
  });
}

function genericLeaveHook(name, offset, debug = false) {
  const func_ptr = libil2cpp.add(offset);
  Interceptor.attach(func_ptr, {
    onLeave(result) {
      if (lastLeaveLog == name && deduplicateLogs) {
        lastLeaveLogCount++;
      }
      else {
        if (lastLeaveLogCount > 1) {
          onLeaveLogWrapper(lastLeaveLog, `(x${lastLeaveLogCount})`, debug);
        }
        lastLeaveLogCount = 0;
        lastLeaveLog = name;
        onLeaveLogWrapper(name, null, debug);
      }
    },
  });
}

function genericEnterLeaveHook(name, offset, debug = false) {
  const func_ptr = libil2cpp.add(offset);
  Interceptor.attach(func_ptr, {
    onEnter(args) {
      if (lastEnterLog == name && deduplicateLogs) {
        lastEnterLogCount++;
      }
      else {
        if (lastEnterLogCount > 1) {
          onLeaveLogWrapper(lastEnterLog, `(x${lastEnterLogCount})`, debug);
        }
        lastEnterLogCount = 0;
        lastEnterLog = name;
        onEnterLogWrapper(name, null, debug);
      }
    },
    onLeave(result) {
      if (lastLeaveLog == name && deduplicateLogs) {
        lastLeaveLogCount++;
      }
      else {
        if (lastLeaveLogCount > 1) {
          onLeaveLogWrapper(lastLeaveLog, `(x${lastLeaveLogCount})`, debug);
        }
        lastLeaveLogCount = 0;
        lastLeaveLog = name;
        onLeaveLogWrapper(name, null, debug);
      }
    },
  });
}

function writeString(addr, text) {
  addr.add(0x10).writeInt(text.length);
  addr.add(0x14).writeUtf16String(text);
}

//#endregion

// #region Private Server

function DarkOctoSetupper_GetE(offset) {
  const func_ptr = libil2cpp.add(offset);
  Interceptor.attach(func_ptr, {
    onLeave(result) {
      writeString(result, `https://${SERVER_ADDRESS}/`)
    }
  });
}

function Config_Api_MakeMasterDataUrl(offset) {
  const func_ptr = libil2cpp.add(offset);
  Interceptor.attach(func_ptr, {
    onLeave(result) {
      writeString(result, `https://${SERVER_ADDRESS}/`)
      onLeaveLogWrapper(DarkOctoSetupper_GetE.name, result.add(0x14).readUtf16String());
    }
  });
}

function ChannelProvider_Setup(offset) {
  const func_ptr = libil2cpp.add(offset);
  Interceptor.attach(func_ptr, {
    onEnter(args) {
      writeString(args[0], `${SERVER_ADDRESS}:443`);
      onEnterLogWrapper(ChannelProvider_Setup.name, args[0].add(0x14).readUtf16String());
    }
  });
}

function OctoAPI_CreateUrl(offset) {
  const func_ptr = libil2cpp.add(offset);
  Interceptor.attach(func_ptr, {
    onLeave(result) {
      onLeaveLogWrapper(OctoAPI_CreateUrl.name, result.add(0x14).readUtf16String());
    }
  });
}

function HandleNet_Encrypt(offset) {
  const func_ptr = libil2cpp.add(offset);
  Interceptor.attach(func_ptr, {
    onEnter(args) {
      this.input = args[1];
    },
    onLeave(result) {
      result.replace(this.input);
    }
  });
}

function HandleNet_Decrypt(offset) {
  var func_ptr = libil2cpp.add(offset);
  var func = new NativeFunction(libil2cpp.add(0x279410C), 'pointer', ['pointer']);
  Interceptor.replace(func_ptr, new NativeCallback((_arg1) => {
    return func(_arg1); // Can't return _arg1 for some reason
  }, 'pointer', ['pointer']));
}

function LocalizeText_GetWordOrDefault(offset) {
  let matchDic = {
    '© SQUARE ENIX Developed by Applibot, Inc.': '© Marie\'s Wonderland',
    'Ver. {0}': 'Ver. 0.0.1',
    'Basic play for this game is free, but a portion of items are available for purchase.': 'Escaping to another branch to dodge EoS',
    'To underage players:': 'To all prayers:'
  };
  let containsDic = {
    'consult your parent or guardian': 'You are now entering Marie\'s Wonderland'
  };

  const func_ptr = libil2cpp.add(offset);
  Interceptor.attach(func_ptr, {
    onLeave(result) {
      var text = result.add(0x14).readUtf16String();
      //onLeaveLogWrapper(LocalizeText_GetWordOrDefault.name, text);

      if (text in matchDic) {
        writeString(result, matchDic[text]);
      }
      else {
        for (let key in containsDic) {
          if (text.includes(key)) {
            writeString(result, containsDic[key]);
            break;
          }
        }
      }
    }
  });
}

//#endregion

const SERVER_ADDRESS = 'humbly-tops-calf.ngrok-free.app';

awaitLibil2cpp(() => {
  callbackWrapper(LocalizeText_GetWordOrDefault, 0x2ACE4D8); // Text replacements
  callbackWrapper(DarkOctoSetupper_GetE, 0x3638FC0);
  callbackWrapper(Config_Api_MakeMasterDataUrl, 0x2E5B114); // Master db URL
  callbackWrapper(ChannelProvider_Setup, 0x35C3C9C); // GRPC server URL
  callbackWrapper(OctoAPI_CreateUrl, 0x4C2723C);
  callbackWrapper(HandleNet_Encrypt, 0x279410C); // Bypass GRPC encryption
  callbackWrapper(HandleNet_Decrypt, 0x279420C); // Bypass GRPC decryption
});

// frida -Uf com.square_enix.android_googleplay.nierspww -l "path\to\hooks.js"