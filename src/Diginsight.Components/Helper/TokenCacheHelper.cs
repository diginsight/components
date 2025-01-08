#region using
//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using System;
using System.IdentityModel;
using System.IO;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
#endregion

#nullable enable    

namespace Diginsight.Components;

static class TokenCacheHelper
{
    public static Type T = typeof(TokenCacheHelper);
    //private ILogger<TokenCacheHelper> logger;

    /// <summary>Path to the token cache</summary>
    //public static readonly string CacheFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location + ".msalcache.bin3";
    public static readonly string CacheFilePath; // = System.Reflection.Assembly.GetExecutingAssembly().Location + ".msalcache.bin3";
    private static readonly object FileLock = new object();

    static TokenCacheHelper()
    {
        //using var sec = TraceLogger.BeginMethodScope(T);

        var tempPath = Path.GetTempPath(); // sec.LogDebug(new { tempPath });
        var executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
        CacheFilePath = $"{tempPath}iam\\{executingAssembly.GetName().Name}.msalcache.bin3";

        //sec.LogDebug(new { CacheFilePath });
    }

    public static void BeforeAccessNotification(TokenCacheNotificationArgs args)
    {
        //using var scope = TraceLogger.BeginMethodScope(T, new { args });

        lock (FileLock)
        {
            try
            {
                var cacheFileExists = File.Exists(CacheFilePath); // logger.LogDebug($"File.Exists({CacheFilePath}); returned {cacheFileExists}");
                var unprotectData = default(byte[]);

                if (cacheFileExists)
                {
                    unprotectData = ProtectedData.Unprotect(File.ReadAllBytes(CacheFilePath), null, DataProtectionScope.CurrentUser);
                    //logger.LogDebug($"ProtectedData.Unprotect(File.ReadAllBytes({CacheFilePath}), null, {DataProtectionScope.CurrentUser}); returned unprotectData");
                }

                args.TokenCache.DeserializeMsalV3(unprotectData); // logger.LogDebug($"args.TokenCache.DeserializeMsalV3(unprotectData);");
            }
            catch (Exception _)
            {
                //logger.LogException(ex);
            }
        }

    }

    public static Task BeforeAccessNotificationAsync(TokenCacheNotificationArgs args)
    {
        //using var scope = TraceLogger.BeginMethodScope(T, new { args });

        return Task.CompletedTask;
    }

    public static void AfterAccessNotification(TokenCacheNotificationArgs args)
    {
        //using var scope = TraceLogger.BeginMethodScope(T, new { args });

        // if the access operation resulted in a cache update
        if (args.HasStateChanged)
        {
            lock (FileLock)
            {
                try
                {
                    var path = Path.GetDirectoryName(CacheFilePath); // logger.LogDebug($"Path.GetDirectoryName({CacheFilePath}); returned {path}");
                    if (path != null) { Directory.CreateDirectory(path); } // logger.LogDebug($"Directory.CreateDirectory({path});");
                    File.WriteAllBytes(CacheFilePath, ProtectedData.Protect(args.TokenCache.SerializeMsalV3(), null, DataProtectionScope.CurrentUser));
                    //logger.LogDebug($"File.WriteAllBytes({CacheFilePath}, ProtectedData.Protect(args.TokenCache.SerializeMsalV3(), null, {DataProtectionScope.CurrentUser}));");
                }
                catch (Exception _)
                {
                    //logger.LogException(ex);
                }
            }
        }
    }
    public static Task AfterAccessNotificationAsync(TokenCacheNotificationArgs args)
    {
        //using var scope = TraceLogger.BeginMethodScope(T, new { args });

        return Task.CompletedTask;
    }

    public static void BeforeWriteNotification(TokenCacheNotificationArgs args)
    {
        //using var scope = TraceLogger.BeginMethodScope(T, new { args });
    }
    public static Task BeforeWriteNotificationAsync(TokenCacheNotificationArgs args)
    {
        //using var scope = TraceLogger.BeginMethodScope(T, new { args });

        return Task.CompletedTask;
    }

    internal static void EnableSerialization(ITokenCache tokenCache)
    {
        //using (var scope = TraceLogger.BeginMethodScope(T, new { tokenCache }))
        //{
        tokenCache.SetBeforeAccess(BeforeAccessNotification);
        tokenCache.SetAfterAccess(AfterAccessNotification);
        tokenCache.SetBeforeWrite(BeforeWriteNotification);

        tokenCache.SetBeforeAccessAsync(BeforeAccessNotificationAsync);
        tokenCache.SetAfterAccessAsync(AfterAccessNotificationAsync);
        tokenCache.SetBeforeWriteAsync(BeforeWriteNotificationAsync);
        //}
    }
}
