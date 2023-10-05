using System;
using System.Collections.Generic;
using System.Text;

namespace RaidMemberBot.Helpers.GreyMagic
{
    internal class AllocatedMemory : IDisposable
    {
        private readonly Dictionary<string, IntPtr> _allocated = new Dictionary<string, IntPtr>();
        private readonly ExternalProcessReader _memory;
        private uint _currentOffset;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AllocatedMemory" /> class.
        /// </summary>
        /// <param name="memory">The memory.</param>
        /// <param name="bytes">The bytes.</param>
        /// <remarks>Created 2012-02-15</remarks>
        internal AllocatedMemory(ExternalProcessReader memory, int bytes)
        {
            _memory = memory;
            Address = memory.AllocateMemory(bytes);
        }

        /// <summary>
        ///     Gets the <see cref="IntPtr" /> with the specified allocated name.
        /// </summary>
        /// <remarks>Created 2012-02-15</remarks>
        internal IntPtr this[string allocatedName]
        {
            get { return Address + (int)_allocated[allocatedName]; }
        }

        /// <summary>
        ///     Gets the address.
        /// </summary>
        /// <remarks>Created 2012-02-15</remarks>
        internal IntPtr Address { get; private set; }

        #region Implementation of IDisposable

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _memory.FreeMemory(Address);
            Address = IntPtr.Zero;
        }

        #endregion

        /// <summary>
        ///     Writes the specified offset in bytes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="offsetInBytes">The offset in bytes.</param>
        /// <param name="value">The value.</param>
        /// <remarks>Created 2012-02-15</remarks>
        internal void Write<T>(int offsetInBytes, T value) where T : struct
        {
            _memory.Write(Address + offsetInBytes, value);
        }

        /// <summary>
        ///     Writes the specified allocated name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="allocatedName">Name of the allocated.</param>
        /// <param name="value">The value.</param>
        /// <remarks>Created 2012-02-15</remarks>
        internal void Write<T>(string allocatedName, T value) where T : struct
        {
            if (!_allocated.ContainsKey(allocatedName))
                AllocateOfChunk(allocatedName, MarshalCache<T>.Size);

            _memory.Write(this[allocatedName], value);
        }

        /// <summary>
        ///     Writes the specified allocated name.
        /// </summary>
        /// <param name="allocatedName">Name of the allocated.</param>
        /// <param name="value">The value.</param>
        /// <remarks>Created 2012-09-05</remarks>
        internal void Write(string allocatedName, string value)
        {
            if (!_allocated.ContainsKey(allocatedName))
                AllocateOfChunk(allocatedName, value.Length + 1);
            _memory.WriteString(this[allocatedName], value, Encoding.ASCII);
        }

        /// <summary>
        ///     Writes the specified allocated name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="allocatedName">Name of the allocated.</param>
        /// <param name="offsetInBytes">The offset in bytes.</param>
        /// <param name="value">The value.</param>
        /// <remarks>Created 2012-02-15</remarks>
        internal void Write<T>(string allocatedName, int offsetInBytes, T value) where T : struct
        {
            _memory.Write(this[allocatedName] + offsetInBytes, value);
        }

        /// <summary>
        ///     Writes the string.
        /// </summary>
        /// <param name="allocatedName">Name of the allocated.</param>
        /// <param name="offsetInBytes">The offset in bytes.</param>
        /// <param name="value">The value.</param>
        /// <param name="encoding">The encoding.</param>
        /// <remarks>Created 2012-04-23</remarks>
        internal void WriteString(string allocatedName, int offsetInBytes, string value, Encoding encoding)
        {
            _memory.WriteString(this[allocatedName] + offsetInBytes, value, encoding);
        }

        /// <summary>
        ///     Writes the string.
        /// </summary>
        /// <param name="offsetInBytes">The offset in bytes.</param>
        /// <param name="value">The value.</param>
        /// <param name="encoding">The encoding.</param>
        /// <remarks>Created 2012-04-23</remarks>
        internal void WriteString(int offsetInBytes, string value, Encoding encoding)
        {
            _memory.WriteString(Address + offsetInBytes, value, encoding);
        }

        /// <summary>
        ///     Writes the bytes.
        /// </summary>
        /// <param name="offsetInBytes">The offset in bytes.</param>
        /// <param name="bytes">The bytes.</param>
        /// <remarks>Created 2012-02-15</remarks>
        internal void WriteBytes(int offsetInBytes, byte[] bytes)
        {
            _memory.WriteBytes(Address + offsetInBytes, bytes);
        }

        /// <summary>
        ///     Writes the bytes.
        /// </summary>
        /// <param name="allocatedName">Name of the allocated.</param>
        /// <param name="bytes">The bytes.</param>
        /// <remarks>Created 2012-02-15</remarks>
        internal void WriteBytes(string allocatedName, byte[] bytes)
        {
            if (!_allocated.ContainsKey(allocatedName))
                AllocateOfChunk(allocatedName, bytes.Length);

            _memory.WriteBytes(this[allocatedName], bytes);
        }

        /// <summary>
        ///     Reads the specified offset in bytes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="offsetInBytes">The offset in bytes.</param>
        /// <returns></returns>
        /// <remarks>Created 2012-02-15</remarks>
        internal T Read<T>(int offsetInBytes) where T : struct
        {
            return _memory.Read<T>(Address + offsetInBytes);
        }

        /// <summary>
        ///     Reads the specified allocated name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="allocatedName">Name of the allocated.</param>
        /// <returns></returns>
        /// <remarks>Created 2012-02-15</remarks>
        internal T Read<T>(string allocatedName) where T : struct
        {
            return _memory.Read<T>(this[allocatedName]);
        }

        /// <summary>
        ///     Reads the specified allocated name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="allocatedName">Name of the allocated.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        /// <remarks>Created 2012-02-15</remarks>
        internal T Read<T>(string allocatedName, int offset) where T : struct
        {
            return _memory.Read<T>(this[allocatedName] + offset);
        }

        /// <summary>
        ///     Gets the address.
        /// </summary>
        /// <param name="offsetInBytes">The offset in bytes.</param>
        /// <returns></returns>
        /// <remarks>Created 2012-02-15</remarks>
        internal uint GetAddress(int offsetInBytes)
        {
            return (uint)(Address + offsetInBytes);
        }

        /// <summary>
        ///     Allocates the of chunk.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="allocatedName">Name of the allocated.</param>
        /// <remarks>Created 2012-02-15</remarks>
        internal void AllocateOfChunk<T>(string allocatedName) where T : struct
        {
            AllocateOfChunk(allocatedName, MarshalCache<T>.Size);
        }

        /// <summary>
        ///     Allocates the of chunk.
        /// </summary>
        /// <param name="allocatedName">Name of the allocated.</param>
        /// <param name="bytes">The bytes.</param>
        /// <remarks>Created 2012-02-15</remarks>
        internal void AllocateOfChunk(string allocatedName, int bytes)
        {
            _allocated.Add(allocatedName, (IntPtr)_currentOffset);
            _currentOffset += (uint)bytes;
            AlignTo(ref _currentOffset, 4);
        }

        /// <summary>
        ///     Gets the allocated chunk.
        /// </summary>
        /// <param name="allocatedName">Name of the allocated.</param>
        /// <returns></returns>
        /// <remarks>Created 2012-02-15</remarks>
        internal IntPtr GetAllocatedChunk(string allocatedName)
        {
            return Address + (int)_allocated[allocatedName];
        }

        /// <summary>
        ///     Aligns to.
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="align">The align.</param>
        /// <remarks>Created 2012-02-15</remarks>
        internal static void AlignTo(ref uint address, uint align)
        {
            var rest = address % align;
            if (rest != 0)
                address += align - rest;
        }

        /// <summary>
        ///     Calculates the total size.
        /// </summary>
        /// <param name="sizes">The sizes.</param>
        /// <returns></returns>
        /// <remarks>Created 2012-02-15</remarks>
        internal static uint CalculateTotalSize(params uint[] sizes)
        {
            uint totalSize = 0;
            for (var i = 0; i < sizes.Length; i++)
            {
                totalSize += sizes[i];
                AlignTo(ref totalSize, 4);
            }

            return totalSize;
        }
    }
}
