using System;

namespace Wave {
    [Serializable]
    public class Connection {
        public string name;

        public bool ConnectsTo(Connection other) {
            return name == other.name;
        }
    }
}