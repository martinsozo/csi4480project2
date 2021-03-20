﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCoreCryptographyCore.Concrete
{
    /// <summary>
    /// A simple implementation of key value store that is not meant
    /// to be used in production but in developer machine, this class
    /// uses a directory to store a master key used to encrypt/decript by
    /// that master key is stored in clear form.
    /// </summary>
    public class DevelopKeyValueStore : IKeyVaultStore
    {
        private readonly EncryptionKey _key;

        public DevelopKeyValueStore(string keyFolder)
        {
            if (!Directory.Exists(keyFolder))
            {
                Directory.CreateDirectory(keyFolder);
            }
            var keyName = Path.Combine(keyFolder, "developerKeyValueStore.key");
            if (!File.Exists(keyName))
            {
                using var key = new EncryptionKey();
                File.WriteAllBytes(keyName, key.Serialize());
            }

            _key = new EncryptionKey(File.ReadAllBytes(keyName));
        }

        public async Task<EncryptionKey> DecriptAsync(byte[] encryptedKey)
        {
            using var sourceMs = new MemoryStream(encryptedKey);
            using var destinationMs = new MemoryStream();
            await StaticEncryptor.DecryptAsync(sourceMs, destinationMs, _key).ConfigureAwait(false);
            return new EncryptionKey(destinationMs.ToArray());
        }

        public async Task<byte[]> EncryptAsync(EncryptionKey key)
        {
            using var destinationMs = new MemoryStream();
            using var sourceMs = new MemoryStream(key.Serialize());
            await StaticEncryptor.EncryptAsync(sourceMs , destinationMs, _key).ConfigureAwait(false);
            return destinationMs.ToArray();
        }
    }
}