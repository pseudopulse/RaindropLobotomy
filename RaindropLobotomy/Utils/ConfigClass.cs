using System;
using System.Threading.Tasks;

namespace RaindropLobotomy {
    public abstract class ConfigClass {
        public abstract string Section { get; }
        public abstract void Initialize();
        public T Option<T>(string key, string desc, T def) {
            return Main.config.Bind<T>(Section, key, def, desc).Value;
        }

        public ConfigClass() {
            Initialize();
        }
    }
}