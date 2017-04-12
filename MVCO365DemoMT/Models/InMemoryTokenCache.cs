using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;

namespace MVCO365Demo.Models
{
    public class InMemoryTokenCache : TokenCache
    {
        private static Dictionary<string, UserTokenCache> TokenCacheState = new Dictionary<string, UserTokenCache>();

        public string User { get; set; }
        private UserTokenCache CacheEntry { get; set; }

        public InMemoryTokenCache(string user)
        {
            // associate the cache to the current user of the web app
            this.User = user;
            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
            this.BeforeWrite = BeforeWriteNotification;

            // look up the entry in the DB
            UserTokenCache cachedEntry;
            if (TokenCacheState.TryGetValue(user, out cachedEntry))
            {
                this.CacheEntry = cachedEntry;
            }

            // place the entry in memory
            var cachedBits = this.CacheEntry?.cacheBits;
            this.Deserialize(cachedBits);
        }
        
        // clean up the DB
        public override void Clear()
        {
            base.Clear();

            TokenCacheState.Remove(this.User);
        }

        // Notification raised before ADAL accesses the cache.
        // This is your chance to update the in-memory copy from the DB, if the in-memory version is stale
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            if (this.CacheEntry == null)
            {
                // first time access
                UserTokenCache cachedEntry;
                if (TokenCacheState.TryGetValue(this.User, out cachedEntry))
                {
                    this.CacheEntry = cachedEntry;
                    var cachedBits = this.CacheEntry?.cacheBits;
                    this.Deserialize(cachedBits);
                }
            }
            else
            {   // retrieve last write from the DB
                UserTokenCache cachedEntry;
                if (TokenCacheState.TryGetValue(this.User, out cachedEntry) && (cachedEntry.LastWrite > CacheEntry.LastWrite))
                {
                    // Our in-memory copy is older than the cached state, so we need to update
                    this.CacheEntry = cachedEntry;
                    var cachedBits = this.CacheEntry?.cacheBits;
                    this.Deserialize(cachedBits);
                }
            }
        }

        // Notification raised after ADAL accessed the cache.
        // If the HasStateChanged flag is set, ADAL changed the content of the cache
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if state changed
            if (this.HasStateChanged)
            {
                if (this.CacheEntry == null)
                {
                    this.CacheEntry = new UserTokenCache();
                }
                this.CacheEntry.webUserUniqueId = User;
                this.CacheEntry.cacheBits = this.Serialize();
                this.CacheEntry.LastWrite = DateTime.UtcNow;

                //// update the DB and the lastwrite
                TokenCacheState[this.User] = this.CacheEntry;
                this.CacheEntry.UserTokenCacheId = this.User.GetHashCode();

                this.HasStateChanged = false;
            }
        }

        void BeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        }
    }
}