using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace McbeevCommerceBot
{
    public class BaseState : Dictionary<string, object>
    {
        public BaseState(IDictionary<string, object> source)
        {
            if (source != null)
            {
                source.ToList().ForEach(x => this.Add(x.Key, x.Value));
            }
        }

        protected T GetProperty<T>([CallerMemberName]string propName = null)
        {
            if (this.TryGetValue(propName, out object value))
            {
                return (T)value;
            }
            return default(T);
        }

        protected void SetProperty(object value, [CallerMemberName]string propName = null)
        {
            this[propName] = value;
        }
    }

    public class UserState : BaseState
    {
        public UserState() : base(null) { }

        public UserState(IDictionary<string, object> source) : base(source) { }

    }
}
