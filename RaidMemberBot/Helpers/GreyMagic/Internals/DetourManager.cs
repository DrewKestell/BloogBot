using RaidMemberBot.Mem;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RaidMemberBot.Helpers.GreyMagic.Internals
{
    /// <summary>
    ///     A manager class to handle function detours, and hooks.
    /// </summary>
    internal class DetourManager : Manager<Detour>
    {
        internal DetourManager(MemoryBase memory)
            : base(memory)
        {
        }

        /// <summary>
        ///     Creates a new Detour.
        /// </summary>
        /// <param name="target">
        ///     The original function to detour. (This delegate should already be registered via
        ///     Magic.RegisterDelegate)
        /// </param>
        /// <param name="newTarget">The new function to be called. (This delegate should NOT be registered!)</param>
        /// <param name="name">The name of the detour.</param>
        /// <returns>
        ///     A <see cref="Detour" /> object containing the required methods to apply, remove, and call the original
        ///     function.
        /// </returns>
        internal Detour Create(Delegate target, Delegate newTarget, string name)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (newTarget == null)
                throw new ArgumentNullException("newTarget");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            if (!Utilities.HasUFPAttribute(target))
                throw new MissingAttributeException(
                    "The target delegate does not have the proper UnmanagedFunctionPointer attribute!");
            if (!Utilities.HasUFPAttribute(newTarget))
                throw new MissingAttributeException(
                    "The new target delegate does not have the proper UnmanagedFunctionPointer attribute!");

            if (Applications.ContainsKey(name))
                throw new ArgumentException(string.Format("The {0} detour already exists!", name), "name");

            Detour d = new Detour(target, newTarget, name, Memory);
            Applications.Add(name, d);
            return d;
        }

        /// <summary>
        ///     Creates and applies new Detour.
        /// </summary>
        /// <param name="target">
        ///     The original function to detour. (This delegate should already be registered via
        ///     Magic.RegisterDelegate)
        /// </param>
        /// <param name="newTarget">The new function to be called. (This delegate should NOT be registered!)</param>
        /// <param name="name">The name of the detour.</param>
        /// <returns>
        ///     A <see cref="Detour" /> object containing the required methods to apply, remove, and call the original
        ///     function.
        /// </returns>
        internal Detour CreateAndApply(Delegate target, Delegate newTarget, string name)
        {
            Detour ret = Create(target, newTarget, name);
            if (ret != null)
                ret.Apply();
            return ret;
        }
    }

    /// <summary>
    ///     Contains methods, and information for a detour, or hook.
    /// </summary>
    internal class Detour : IMemoryOperation
    {
        private readonly IntPtr _hook;

        /// <summary>
        ///     This var is not used within the detour itself. It is only here
        ///     to keep a reference, to avoid the GC from collecting the delegate instance!
        /// </summary>
        private readonly Delegate _hookDelegate;

        private readonly MemoryBase _memory;

        private readonly List<byte> _new;
        private readonly List<byte> _orginal;
        private readonly IntPtr _target;
        private readonly Delegate _targetDelegate;

        internal Detour(Delegate target, Delegate hook, string name, MemoryBase memory)
        {
            _memory = memory;
            Name = name;
            _targetDelegate = target;
            _target = Marshal.GetFunctionPointerForDelegate(target);
            _hookDelegate = hook;
            _hook = Marshal.GetFunctionPointerForDelegate(hook);

            //Store the orginal bytes
            _orginal = new List<byte>();
            _orginal.AddRange(memory.ReadBytes(_target, 6));

            //Setup the detour bytes
            _new = new List<byte> { 0x68 };
            byte[] tmp = BitConverter.GetBytes(_hook.ToInt32());
            _new.AddRange(tmp);
            _new.Add(0xC3);

            Hack parHack = new Hack(_target, _orginal.ToArray(), _new.ToArray(), Name);
        }

        /// <summary>
        ///     Calls the original function, and returns a return value.
        /// </summary>
        /// <param name="args">
        ///     The arguments to pass. If it is a 'void' argument list,
        ///     you MUST pass 'null'.
        /// </param>
        /// <returns>An object containing the original functions return value.</returns>
        internal object CallOriginal(params object[] args)
        {
            Remove();
            object ret = _targetDelegate.DynamicInvoke(args);
            Apply();
            return ret;
        }

        #region IMemoryOperation Members

        /// <summary>
        ///     Returns true if this Detour is currently applied.
        /// </summary>
        public bool IsApplied { get; private set; }

        /// <summary>
        ///     Returns the name for this Detour.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Applies this Detour to memory. (Writes new bytes to memory)
        /// </summary>
        /// <returns></returns>
        public bool Apply()
        {
            if (_memory.WriteBytes(_target, _new.ToArray()) == _new.Count)
            {
                IsApplied = true;
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Removes this Detour from memory. (Reverts the bytes back to their originals.)
        /// </summary>
        /// <returns></returns>
        public bool Remove()
        {
            if (_memory.WriteBytes(_target, _orginal.ToArray()) == _orginal.Count)
            {
                IsApplied = false;
                return true;
            }
            return false;
        }


        public void Dispose()
        {
            if (IsApplied)
                Remove();
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
