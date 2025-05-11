using System;

namespace SuperShop.Bll
{

    [Serializable]
    public class SuperShopBusinessException : Exception
    {
        public int MyProperty { get; set; }
        public SuperShopBusinessException() { }
        public SuperShopBusinessException(string message) : base(message) { }
        public SuperShopBusinessException(string message, Exception inner) : base(message, inner) { }
        protected SuperShopBusinessException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
