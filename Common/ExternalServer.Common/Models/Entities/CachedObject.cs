using System;
using ExternalServer.Common.Specifications.DataObjects;

namespace ExternalServer.Common.Models.Entities {
    public class CachedObject : ICachedObject {

        public object Object { get; private set; }

        public TimeSpan Lifetime {
            get {
                return DateTime.Now - CreationDate;
            }
        }

        private DateTime CreationDate;

        public CachedObject(object o) {
            CreationDate = DateTime.Now;
            Object = o;
        }
    }
}
