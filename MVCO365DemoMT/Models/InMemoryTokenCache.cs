using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MVCO365Demo.Models
{
    public class InMemoryTokenCache : TokenCache
    {
        private static Dictionary<string, InMemoryTokenCache> TokenCacheState = new Dictionary<string, InMemoryTokenCache>();

        public string User { get; private set; }

        public static InMemoryTokenCache TokenCacheForUser(string user)
        {
            user = "shared-token-cache";

            InMemoryTokenCache instance;
            if (!TokenCacheState.TryGetValue(user, out instance))
            {
                instance = new InMemoryTokenCache(user);
                TokenCacheState[user] = instance;
            }
            return instance;
        }

        private InMemoryTokenCache(string user)
        {
            Debug.WriteLine($"Creating a new TokenCache object for {user}.");
            // associate the cache to the current user of the web app
            this.User = user;
            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
            this.BeforeWrite = BeforeWriteNotification;

            this.Deserialize(null);
        }
        
        // Notification raised before ADAL accesses the cache.
        // This is your chance to update the in-memory copy from the DB, if the in-memory version is stale
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Debug.WriteLine($"BeforeAccessNotification for {args.Resource} as {User}, unique-id: {args.UniqueId}.");
        }

        // Notification raised after ADAL accessed the cache.
        // If the HasStateChanged flag is set, ADAL changed the content of the cache
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            Debug.WriteLine($"AfterAccessNotification for {args.Resource} as {User}, unique-id: {args.UniqueId}.");
        }

        void BeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
            Debug.WriteLine($"BeforeWriteNotitification for {User}, resource: {args.Resource}.");
        }
    }
}