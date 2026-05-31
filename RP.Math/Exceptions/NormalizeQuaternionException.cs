namespace RP.Math.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// An exception for use when a quaternion cannot be normalized (e.g. it has zero magnitude).
    /// </summary>
    [Serializable]
    public class NormalizeQuaternionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizeQuaternionException"/> class.
        /// </summary>
        public NormalizeQuaternionException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizeQuaternionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public NormalizeQuaternionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizeQuaternionException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public NormalizeQuaternionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizeQuaternionException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The serialization information.</param>
        /// <param name="context">The streaming context.</param>
#if NET8_0_OR_GREATER
        [Obsolete("Legacy formatter-based serialization is obsolete; retained for binary back-compatibility.")]
#pragma warning disable SYSLIB0051
#endif
        protected NormalizeQuaternionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#if NET8_0_OR_GREATER
#pragma warning restore SYSLIB0051
#endif
    }
}
