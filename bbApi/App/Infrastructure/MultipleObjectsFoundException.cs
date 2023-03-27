namespace bbApi.App.Infrastructure
{
    public class MultipleObjectsFoundException : Exception
    {
        public MultipleObjectsFoundException() { }
        public MultipleObjectsFoundException(string message) : base(message) { }
        public MultipleObjectsFoundException(string message, Exception inner) : base(message, inner) { }
        protected MultipleObjectsFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
