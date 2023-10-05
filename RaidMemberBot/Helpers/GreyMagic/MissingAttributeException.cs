using System;
using System.Runtime.Serialization;

namespace RaidMemberBot.Helpers.GreyMagic
{
    /// <summary>
    ///     An exception that is thrown when a struct, class, or delegate is missing proper attributes.
    /// </summary>
    [Serializable]
    internal class MissingAttributeException : Exception
    {
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp

        /// <summary>
        /// </summary>
        internal MissingAttributeException()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        internal MissingAttributeException(string message) : base(message)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        internal MissingAttributeException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected MissingAttributeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
