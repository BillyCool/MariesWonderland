//#region Hooking

let libil2cpp;
let lastEnterLog = "", lastLeaveLog = "";
let lastEnterLogCount = 0, lastLeaveLogCount = 0;
const deduplicateLogs = true;

function awaitLibil2cpp(callback) {
  try {
    libil2cpp = Process.getModuleByName('libil2cpp.so').base;
    console.log('libil2cpp.so loaded:', libil2cpp);
    callback();
  } catch (error) {
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

function readString(addr) {
  return addr.add(0x14).readUtf16String();
}

//#endregion

// #region Private Server

function DarkOctoSetupper_CreateSetting(offset) {
  const func_ptr = libil2cpp.add(offset);
  Interceptor.attach(func_ptr, {
    onLeave(result) {
        const urlPtr = result.add(0x10).readPointer();
        const appId = result.add(0x18).readInt();
        const clientSecretPtr = result.add(0x20).readPointer();
        const aesKeyPtr = result.add(0x28).readPointer();
        const aPtr = result.add(0x30).readPointer();
        const cachingType = result.add(0x38).readInt();
        const version = result.add(0x3C).readInt();
        const maxParallelDownload = result.add(0x48).readInt();
        const maxParallelLoad = result.add(0x4C).readInt();
        const maximumAvailableDiskSpace = result.add(0x50).readS64();
        const expirationDelay = result.add(0x58).readInt();
        const allowDeleted = result.add(0x5C).readU8();
        const enableAssetDatabase = result.add(0x5D).readU8();

        onLeaveLogWrapper(DarkOctoSetupper_CreateSetting.name, `Url=${readString(urlPtr)}`);
        onLeaveLogWrapper(DarkOctoSetupper_CreateSetting.name, `AppId=${appId}`);
        onLeaveLogWrapper(DarkOctoSetupper_CreateSetting.name, `ClientSecretKey=${readString(clientSecretPtr)}`);
        onLeaveLogWrapper(DarkOctoSetupper_CreateSetting.name, `AesKey=${readString(aesKeyPtr)}`);
        onLeaveLogWrapper(DarkOctoSetupper_CreateSetting.name, `A=${readString(aPtr)}`);
        onLeaveLogWrapper(DarkOctoSetupper_CreateSetting.name, `CachingType=${cachingType}`);
        onLeaveLogWrapper(DarkOctoSetupper_CreateSetting.name, `Version=${version}`);
        onLeaveLogWrapper(DarkOctoSetupper_CreateSetting.name, `MaxParallelDownload=${maxParallelDownload}`);
        onLeaveLogWrapper(DarkOctoSetupper_CreateSetting.name, `MaxParallelLoad=${maxParallelLoad}`);
        onLeaveLogWrapper(DarkOctoSetupper_CreateSetting.name, `MaximumAvailableDiskSpace=${maximumAvailableDiskSpace}`);
        onLeaveLogWrapper(DarkOctoSetupper_CreateSetting.name, `ExpirationDelay=${expirationDelay}`);
        onLeaveLogWrapper(DarkOctoSetupper_CreateSetting.name, `AllowDeleted=${allowDeleted}`);
        onLeaveLogWrapper(DarkOctoSetupper_CreateSetting.name, `EnableAssetDatabase=${enableAssetDatabase}`);
    }
  });
}

function DarkOctoSetupper_GetE(offset) {
  const func_ptr = libil2cpp.add(offset);
  Interceptor.attach(func_ptr, {
    onLeave(result) {
      writeString(result, `https://${SERVER_ADDRESS}/`);
      onLeaveLogWrapper(DarkOctoSetupper_GetE.name, readString(result));
    }
  });
}

function Config_Api_MakeWebViewUrl(offset) {
  const func_ptr = libil2cpp.add(offset);
  Interceptor.attach(func_ptr, {
    onEnter(args) {
      this.basePath = readString(args[0]);
      this.path = readString(args[1]);
    },
    onLeave(result) {
        const getLanguagePathFunc = new NativeFunction(libil2cpp.add(0x2E5B5C4), 'pointer', []);
        const languagePath = readString(getLanguagePathFunc());
        writeString(result, `https://${SERVER_ADDRESS}${this.basePath}${languagePath}${this.path}`);
        onLeaveLogWrapper(Config_Api_MakeWebViewUrl.name, readString(result));
    }
  });
}

function Config_Api_MakeMasterDataUrl(offset) {
  const func_ptr = libil2cpp.add(offset);
  Interceptor.attach(func_ptr, {
    onEnter(args) {
      this.masterVersion = readString(args[0]);
    },
    onLeave(result) {
      writeString(result, `https://${SERVER_ADDRESS}/assets/release/${this.masterVersion}/database.bin.e`)
      onLeaveLogWrapper(Config_Api_MakeMasterDataUrl.name, readString(result));
    }
  });
}

function ChannelProvider_Setup(offset) {
  const func_ptr = libil2cpp.add(offset);
  Interceptor.attach(func_ptr, {
    onEnter(args) {
      writeString(args[0], `${SERVER_ADDRESS}:443`);
    }
  });
}

function OctoAPI_CreateUrl(offset) {
  const func_ptr = libil2cpp.add(offset);
  Interceptor.attach(func_ptr, {
    onLeave(result) {
      onLeaveLogWrapper(OctoAPI_CreateUrl.name, readString(result));
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
  try {
    Interceptor.replace(func_ptr, new NativeCallback(function (thisArg, receivedMessage) {
      return receivedMessage;
    }, 'pointer', ['pointer', 'pointer']));
  }
  catch (e) {
    console.log('HandleNet_Decrypt replace error:', e);
  }
}

function OctoAPI_DecryptAes(offset) {
  const func_ptr = libil2cpp.add(offset);
  try {
    Interceptor.replace(func_ptr, new NativeCallback(function (thisArg, bytes) {
      return bytes;
    }, 'pointer', ['pointer', 'pointer']));
  }
  catch (e) {
    console.log('OctoAPI_DecryptAes replace error:', e);
  }
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
      var text = readString(result);
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

function DarkClient_InvokeAsync(offset) {
  const func_ptr = libil2cpp.add(offset);
  Interceptor.attach(func_ptr, {
    onEnter(args) {
      this.path = readString(args[1]);
      onEnterLogWrapper(DarkClient_InvokeAsync.name, `path=${this.path}`);
    }
  });
}

function DataManager_SetUrls(offset) {
  const func_ptr = libil2cpp.add(offset);
  Interceptor.attach(func_ptr, {
    onEnter(args) {
      try {
        const db = args[0];
        if (db.isNull()) {
          onEnterLogWrapper(DataManager_SetUrls.name, 'db NULL');
          return;
        }

        const revision = db.add(0x10).readInt();
        const assetBundleListPtr = db.add(0x18).readPointer();
        const tagnamePtr = db.add(0x20).readPointer();
        const resourceListPtr = db.add(0x28).readPointer();
        const urlFormatPtr = db.add(0x30).readPointer();

        const safeListCount = (ptr) => {
          try {
            if (ptr.isNull()) return 0;
            return ptr.add(0x10).readInt();
          } catch (e) { return 'err'; }
        };

        const urlFormat = (urlFormatPtr.isNull()) ? null : readString(urlFormatPtr);
        const assetBundleCount = safeListCount(assetBundleListPtr);
        const tagnameCount = safeListCount(tagnamePtr);
        const resourceCount = safeListCount(resourceListPtr);

        onEnterLogWrapper(DataManager_SetUrls.name, `revision=${revision}, urlFormat=${urlFormat}`);
        onEnterLogWrapper(DataManager_SetUrls.name, `assetBundleList=${assetBundleCount}, tagname=${tagnameCount}, resourceList=${resourceCount}`);
      }
      catch (e) {
        console.log('DataManager_SetUrls onEnter error:', e);
      }
    }
  });
}

function DataManager_ApplyToDatabase(offset) {
  const func_ptr = libil2cpp.add(offset);
  Interceptor.attach(func_ptr, {
    onEnter(args) {
      this.instance = args[0];
    },
    onLeave(result) {
      try {
        const inst = this.instance;
        if (!inst || inst.isNull()) {
          onLeaveLogWrapper(DataManager_ApplyToDatabase.name, 'Applied to database (instance NULL)');
          return;
        }
        const revision = inst.add(0x3C).readInt();
        const urlFormatPtr = inst.add(0x40).readPointer();
        let urlFormat = null;
        try {
          if (!urlFormatPtr.isNull()) urlFormat = readString(urlFormatPtr);
        } catch (e) { urlFormat = '<err reading>'; }
        onLeaveLogWrapper(DataManager_ApplyToDatabase.name, `Applied to database, Revision=${revision}, urlFormat=${urlFormat}`);
      }
      catch (e) {
        console.log('DataManager_ApplyToDatabase onLeave error:', e);
      }
    }
  });
}

//#endregion

const SERVER_ADDRESS = 'humbly-tops-calf.ngrok-free.app';

awaitLibil2cpp(() => {
  callbackWrapper(LocalizeText_GetWordOrDefault, 0x2ACE4D8); // Text replacements
  //callbackWrapper(DarkOctoSetupper_CreateSetting, 0x3639410);
  callbackWrapper(DarkOctoSetupper_GetE, 0x3638FC0);
  callbackWrapper(Config_Api_MakeWebViewUrl, 0x2E5B00C); // WebView URL
  callbackWrapper(Config_Api_MakeMasterDataUrl, 0x2E5B114); // Master db URL
  callbackWrapper(ChannelProvider_Setup, 0x35C3C9C); // GRPC server URL
  callbackWrapper(OctoAPI_CreateUrl, 0x4C2723C);
  callbackWrapper(HandleNet_Encrypt, 0x279410C); // Bypass GRPC encryption
  callbackWrapper(HandleNet_Decrypt, 0x279420C); // Bypass GRPC decryption
  callbackWrapper(DarkClient_InvokeAsync, 0x38AC274); // GRPC requests logging
  callbackWrapper(OctoAPI_DecryptAes, 0x4C27410);
  //callbackWrapper(DataManager_SetUrls, 0x3DA0170);
  //callbackWrapper(DataManager_ApplyToDatabase, 0x3D9F5EC);
});

function StackTrace(offset) {
  const func_ptr = libil2cpp.add(offset);
  Interceptor.attach(func_ptr, {
    onEnter(args) {
      // Log stack trace
      console.log(Thread.backtrace(this.context, Backtracer.ACCURATE)
        .map(DebugSymbol.fromAddress).join('\n') + '\n');
    }
  });
}

// frida -Uf com.square_enix.android_googleplay.nierspww -l "path\to\hooks.js"