
using System;
using System.Reflection;
using Konscious.Security.Cryptography;

namespace JellyfishTool.Services {

    public class IdentityService {

        public static string GetMachineName() => Environment.MachineName;

        public static string GetUserName() => Environment.UserName;

        public static string HashApiKey(string apiKey) {

            string[] parts = apiKey.Split("$");
            string salt = parts[0];
            string key = parts[1];

            byte[] rawSalt = Convert.FromBase64String(salt);
            byte[] rawKey = Convert.FromBase64String(key);

            Argon2 argon2 = new Argon2id(rawKey)
            {
                DegreeOfParallelism = 1,
                Iterations = 1, //Called 'time' in go for some reason?
                MemorySize = 64 * 1024,
                Salt = rawSalt
            };

            byte[] hash = argon2.GetBytes(47);
            string transmittableSalt = Convert.ToHexString(rawSalt);
            string transmittableHash = Convert.ToHexString(hash);

            return $"{transmittableSalt}${transmittableHash}";
        }

        public static string GetProductName() {
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            string name = assembly.GetName().Name;

            return name;
        }

        public static string GetProductVersion() {
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            string version = assembly.GetName().Version.ToString();

            return version;
        }
    }
}