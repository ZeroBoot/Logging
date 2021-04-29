using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace WebApp
{
    internal class OrderLogScope : IReadOnlyList<KeyValuePair<string, object>>
    {
        private readonly int _orderId;
        private readonly int _userId;
        public OrderLogScope(int orderId, int userId)
        {
            _orderId = orderId;
            _userId = userId;
        }

        public KeyValuePair<string, object> this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return new KeyValuePair<string, object>("OrderId", _orderId);
                }
                else if (index == 1)
                {
                    return new KeyValuePair<string, object>("UserId", _userId);
                }
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public int Count => 2;

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            for (int i = 0; i < Count; ++i)
            {
                yield return this[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => string.Format(CultureInfo.InvariantCulture, "OrderId:{0} UserId:{1}", _orderId, _userId);
    }
}
