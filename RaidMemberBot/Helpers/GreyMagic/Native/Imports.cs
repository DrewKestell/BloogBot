using System;
using System.Runtime.InteropServices;
using System.Security;

namespace RaidMemberBot.Helpers.GreyMagic.Native
{
    internal static class Imports
    {
        /// <summary>
        ///     Frees the loaded dynamic-link library (DLL) module and, if necessary, decrements its reference count.
        ///     When the reference count reaches zero, the module is unloaded from the address space of the calling process and the
        ///     handle is no longer valid.
        /// </summary>
        /// <param name="handle">
        ///     A handle to the loaded library module.
        ///     The LoadLibrary, LoadLibraryEx, GetModuleHandle, or GetModuleHandleEx function returns this handle.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is nonzero.
        ///     If the function fails, the return value is zero
        /// </returns>
        /// <remarks>Created 2012-01-17 13:00 by Nesox.</remarks>
        [DllImport("kernel32.dll")]
        [SuppressUnmanagedCodeSecurity]
        internal static extern bool FreeLibrary(IntPtr handle);

        /// <summary>
        ///     Opens an existing local process object.
        /// </summary>
        /// <param name="dwDesiredAccess">
        ///     The access to the process object. This access right is checked against the security descriptor for the process.
        ///     This parameter can be one or more of the process access rights.
        ///     If the caller has enabled the SeDebugPrivilege privilege, the requested access is granted regardless of the
        ///     contents of the security descriptor.
        /// </param>
        /// <param name="bInheritHandle">
        ///     if set to <c>true</c> processes created by this process will inherit the handle.
        ///     Otherwise, the processes do not inherit this handle.
        /// </param>
        /// <param name="dwProcessId">The identifier of the local process to be opened. </param>
        /// <returns>If the function succeeds, the return value is an open handle to the specified process.</returns>
        /// <remarks>Created 2012-01-17 13:00 by Nesox.</remarks>
        [DllImport("kernel32.dll")]
        internal static extern SafeMemoryHandle OpenProcess(ProcessAccessFlags dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        /// <summary>
        ///     Loads the specified module into the address space of the calling process.
        ///     The specified module may cause other modules to be loaded.
        /// </summary>
        /// <param name="libraryName">
        ///     The name of the module.
        ///     This can be either a library module (a .dll file) or an executable module (an .exe file).
        ///     The name specified is the file name of the module and is not related to the name stored in the library module
        ///     itself, as specified by the LIBRARY keyword in the module-definition (.def) file.
        ///     If the string specifies a full path, the function searches only that path for the module.
        ///     If the string specifies a relative path or a module name without a path, the function uses a standard search
        ///     strategy to find the module;
        ///     If the function cannot find the module, the function fails. When specifying a path, be sure to use backslashes (\),
        ///     not forward slashes (/).
        ///     If the string specifies a module name without a path and the file name extension is omitted, the function appends
        ///     the default library extension .dll to the module name.
        ///     To prevent the function from appending .dll to the module name, include a trailing point character (.) in the
        ///     module name string.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is a handle to the module.
        ///     If the function fails, the return value is 0
        /// </returns>
        /// <remarks>Created 2012-01-17 13:00 by Nesox.</remarks>
        [DllImport("kernel32")]
        [SuppressUnmanagedCodeSecurity]
        internal static extern IntPtr LoadLibrary(string libraryName);

        /// <summary>
        ///     Opens an existing thread object.
        /// </summary>
        /// <param name="dwDesiredAccess">
        ///     The access to the thread object.
        ///     This access right is checked against the security descriptor for the thread. This parameter can be one or more of
        ///     the thread access rights.
        ///     If the caller has enabled the SeDebugPrivilege privilege, the requested access is granted regardless of the
        ///     contents of the security descriptor.
        /// </param>
        /// <param name="bInheritHandle">
        ///     if set to <c>true</c> processes created by this process will inherit the handle.
        ///     Otherwise, the processes do not inherit this handle.
        /// </param>
        /// <param name="dwThreadId">The identifier of the thread to be opened.</param>
        /// <returns>
        ///     If the function succeeds, the return value is an open handle to the specified thread.
        ///     If the function fails, the return value is <c>IntPtr.Zero</c>
        /// </returns>
        /// <remarks>Created 2012-02-15</remarks>
        [DllImport("kernel32", EntryPoint = "OpenThread", SetLastError = true)]
        internal static extern IntPtr OpenThread(uint dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        /// <summary>
        ///     Reserves or commits a region of memory within the virtual address space of a specified process.
        ///     The function initializes the memory it allocates to zero, unless MEM_RESET is used.
        /// </summary>
        /// <param name="hProcess">
        ///     The handle to a process. The function allocates memory within the virtual address space of this process.
        ///     The handle must have the PROCESS_VM_OPERATION access right.
        /// </param>
        /// <param name="dwAddress">
        ///     The pointer that specifies a desired starting address for the region of pages that you want to allocate.
        ///     If you are reserving memory, the function rounds this address down to the nearest multiple of the allocation
        ///     granularity.
        ///     If you are committing memory that is already reserved, the function rounds this address down to the nearest page
        ///     boundary. To determine the size of a page and the allocation granularity on the host computer, use the
        ///     GetSystemInfo function.
        ///     If lpAddress is <c>IntPtr.Zero</c>, the function determines where to allocate the region.
        /// </param>
        /// <param name="nSize">
        ///     The size of the region of memory to allocate, in bytes.
        ///     If lpAddress is <c>IntPtr.Zero</c>, the function rounds dwSize up to the next page boundary.
        ///     If lpAddress is not <c>IntPtr.Zero</c>, the function allocates all pages that contain one or more bytes in the
        ///     range from lpAddress to lpAddress+dwSize. This means, for example, that a 2-byte range that straddles a page
        ///     boundary causes the function to allocate both pages.
        /// </param>
        /// <param name="dwAllocationType">The type of memory allocation. </param>
        /// <param name="dwProtect">
        ///     The memory protection for the region of pages to be allocated.
        ///     If the pages are being committed, you can specify any one of the memory protection constants.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is the base address of the allocated region of pages.
        ///     If the function fails, the return value is <c>IntPtr.Zero</c>
        /// </returns>
        /// <remarks>Created 2012-02-15</remarks>
        [DllImport("kernel32", EntryPoint = "VirtualAllocEx")]
        internal static extern IntPtr VirtualAllocEx(SafeMemoryHandle hProcess, uint dwAddress, int nSize,
            MemoryAllocationType dwAllocationType, MemoryProtectionType dwProtect);

        /// <summary>
        ///     Releases, decommits, or releases and decommits a region of memory within the virtual address space of a specified
        ///     process.
        /// </summary>
        /// <param name="hProcess">
        ///     A handle to a process. The function frees memory within the virtual address space of the process.
        ///     The handle must have the PROCESS_VM_OPERATION access right.
        /// </param>
        /// <param name="dwAddress">
        ///     A pointer to the starting address of the region of memory to be freed.
        ///     If the dwFreeType parameter is MEM_RELEASE, lpAddress must be the base address returned by the VirtualAllocEx
        ///     function when the region is reserved.
        /// </param>
        /// <param name="nSize">
        ///     The size of the region of memory to free, in bytes.
        ///     If the dwFreeType parameter is MEM_RELEASE, dwSize must be 0 (zero). The function frees the entire region that is
        ///     reserved in the initial allocation call to VirtualAllocEx.
        ///     If dwFreeType is MEM_DECOMMIT, the function decommits all memory pages that contain one or more bytes in the range
        ///     from the lpAddress parameter to (lpAddress+dwSize). This means, for example, that a 2-byte region of memory that
        ///     straddles a page boundary causes both pages to be decommitted. If lpAddress is the base address returned by
        ///     VirtualAllocEx and dwSize is 0 (zero), the function decommits the entire region that is allocated by
        ///     VirtualAllocEx. After that, the entire region is in the reserved state.
        /// </param>
        /// <param name="dwFreeType">The type of free operation.</param>
        /// <returns>
        ///     If the function succeeds, the return value is a nonzero value.
        ///     If the function fails, the return value is 0 (zero). To get extended error information, call GetLastError.
        /// </returns>
        /// <remarks>
        ///     Each page of memory in a process virtual address space has a Page State. The VirtualFreeEx function can decommit a
        ///     range of pages that are in different states, some committed and some uncommitted. This means that you can decommit
        ///     a range of pages without first determining the current commitment state of each page. Decommitting a page releases
        ///     its physical storage, either in memory or in the paging file on disk.
        ///     If a page is decommitted but not released, its state changes to reserved. Subsequently, you can call VirtualAllocEx
        ///     to commit it, or VirtualFreeEx to release it. Attempting to read from or write to a reserved page results in an
        ///     access violation exception.
        ///     The VirtualFreeEx function can release a range of pages that are in different states, some reserved and some
        ///     committed. This means that you can release a range of pages without first determining the current commitment state
        ///     of each page. The entire range of pages originally reserved by VirtualAllocEx must be released at the same time.
        ///     If a page is released, its state changes to free, and it is available for subsequent allocation operations. After
        ///     memory is released or decommitted, you can never refer to the memory again. Any information that may have been in
        ///     that memory is gone forever. Attempts to read from or write to a free page results in an access violation
        ///     exception. If you need to keep information, do not decommit or free memory that contains the information.
        ///     The VirtualFreeEx function can be used on an AWE region of memory and it invalidates any physical page mappings in
        ///     the region when freeing the address space. However, the physical pages are not deleted, and the application can use
        ///     them. The application must explicitly call FreeUserPhysicalPages to free the physical pages. When the process is
        ///     terminated, all resources are automatically cleaned up.
        ///     Created 2012-02-15.
        /// </remarks>
        [DllImport("kernel32", EntryPoint = "VirtualFreeEx")]
        internal static extern bool VirtualFreeEx(SafeMemoryHandle hProcess, IntPtr dwAddress, int nSize,
            MemoryFreeType dwFreeType);
    }

    // ReSharper disable InconsistentNaming

    [Flags]
    internal enum ProcessAccessFlags
    {
        /// <summary>
        ///     Required to delete the object.
        /// </summary>
        DELETE = 0x00010000,

        /// <summary>
        ///     Required to read information in the security descriptor for the object, not including the information in the SACL.
        ///     To read or write the SACL, you must request the ACCESS_SYSTEM_SECURITY access right.
        ///     For more information, see SACL Access Right.
        /// </summary>
        READ_CONTROL = 0x00020000,

        /// <summary>
        ///     The right to use the object for synchronization.
        ///     This enables a thread to wait until the object is in the signaled state.
        /// </summary>
        SYNCHRONIZE = 0x00100000,

        /// <summary>
        ///     Required to modify the DACL in the security descriptor for the object.
        /// </summary>
        WRITE_DAC = 0x00040000,

        /// <summary>
        ///     Required to change the owner in the security descriptor for the object.
        /// </summary>
        WRITE_OWNER = 0x00080000,

        /// <summary>
        ///     All possible access rights for a process object.
        /// </summary>
        PROCESS_ALL_ACCESS = 0x001F0FFF,

        /// <summary>
        ///     Required to create a process.
        /// </summary>
        PROCESS_CREATE_PROCESS = 0x0080,

        /// <summary>
        ///     Required to create a thread.
        /// </summary>
        PROCESS_CREATE_THREAD = 0x0002,

        /// <summary>
        ///     Required to create a process.
        /// </summary>
        PROCESS_DUP_HANDLE = 0x0040,

        /// <summary>
        ///     Required to retrieve certain information about a process, such as its token, exit code, and priority class
        /// </summary>
        PROCESS_QUERY_INFORMATION = 0x0400,

        /// <summary>
        ///     Required to retrieve certain information about a process (see QueryFullProcessImageName).
        ///     A handle that has the PROCESS_QUERY_INFORMATION access right is automatically granted
        ///     PROCESS_QUERY_LIMITED_INFORMATION.
        /// </summary>
        PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,

        /// <summary>
        ///     Required to set certain information about a process, such as its priority class
        /// </summary>
        PROCESS_SET_INFORMATION = 0x0200,

        /// <summary>
        ///     Required to set memory limits using SetProcessWorkingSetSize.
        /// </summary>
        PROCESS_SET_QUOTA = 0x0100,

        /// <summary>
        ///     Required to suspend or resume a process.
        /// </summary>
        PROCESS_SUSPEND_RESUME = 0x0800,

        /// <summary>
        ///     Required to terminate a process using TerminateProcess.
        /// </summary>
        PROCESS_TERMINATE = 0x0001,

        /// <summary>
        ///     Required to perform an operation on the address space of a process (see VirtualProtectEx and WriteProcessMemory).
        /// </summary>
        PROCESS_VM_OPERATION = 0x0008,

        /// <summary>
        ///     Required to read memory in a process using ReadProcessMemory.
        /// </summary>
        PROCESS_VM_READ = 0x0010,

        /// <summary>
        ///     Required to write to memory in a process using WriteProcessMemory.
        /// </summary>
        PROCESS_VM_WRITE = 0x0020
    }

    internal enum Protection
    {
        /// <summary>
        ///     Disables all access to the committed region of pages.
        ///     An attempt to read from, write to, or execute the committed region results in an access violation.
        ///     This flag is not supported by the CreateFileMapping function.
        /// </summary>
        PAGE_NOACCESS = 0x01,

        /// <summary>
        ///     Enables read-only access to the committed region of pages.
        ///     An attempt to write to the committed region results in an access violation.
        ///     If Data Execution Prevention is enabled, an attempt to execute code in the committed region results in an access
        ///     violation.
        /// </summary>
        PAGE_READONLY = 0x02,

        /// <summary>
        ///     Enables read-only or read/write access to the committed region of pages.
        ///     If Data Execution Prevention is enabled, attempting to execute code in the committed region results in an access
        ///     violation.
        /// </summary>
        PAGE_READWRITE = 0x04,

        /// <summary>
        ///     Enables read-only or copy-on-write access to a mapped view of a file mapping object.
        ///     An attempt to write to a committed copy-on-write page results in a private copy of the page being made for the
        ///     process.
        ///     The private page is marked as PAGE_READWRITE, and the change is written to the new page.
        ///     If Data Execution Prevention is enabled, attempting to execute code in the committed region results in an access
        ///     violation.
        ///     This flag is not supported by the VirtualAlloc or VirtualAllocEx functions.
        /// </summary>
        PAGE_WRITECOPY = 0x08,

        /// <summary>
        ///     Enables execute access to the committed region of pages.
        ///     An attempt to read from or write to the committed region results in an access violation.
        ///     This flag is not supported by the CreateFileMapping function.
        /// </summary>
        PAGE_EXECUTE = 0x10,

        /// <summary>
        ///     Enables execute or read-only access to the committed region of pages.
        ///     An attempt to write to the committed region results in an access violation.
        /// </summary>
        PAGE_EXECUTE_READ = 0x20,

        /// <summary>
        ///     Enables execute, read-only, or read/write access to the committed region of pages.
        /// </summary>
        PAGE_EXECUTE_READWRITE = 0x40,

        /// <summary>
        ///     Enables execute, read-only, or copy-on-write access to a mapped view of a file mapping object.
        ///     An attempt to write to a committed copy-on-write page results in a private copy of the page being made for the
        ///     process.
        ///     The private page is marked as PAGE_EXECUTE_READWRITE, and the change is written to the new page.
        ///     This flag is not supported by the VirtualAlloc or VirtualAllocEx functions.
        /// </summary>
        PAGE_EXECUTE_WRITECOPY = 0x80,

        /// <summary>
        ///     Pages in the region become guard pages. Any attempt to access a guard page causes the system to raise a
        ///     STATUS_GUARD_PAGE_VIOLATION exception and turn off the guard page status.
        ///     Guard pages thus act as a one-time access alarm. For more information, see Creating Guard Pages.
        ///     When an access attempt leads the system to turn off guard page status, the underlying page protection takes over.
        ///     If a guard page exception occurs during a system service, the service typically returns a failure status indicator.
        ///     This value cannot be used with PAGE_NOACCESS.
        ///     This flag is not supported by the CreateFileMapping function.
        /// </summary>
        PAGE_GUARD = 0x100,

        /// <summary>
        ///     Sets all pages to be non-cachable. Applications should not use this attribute except when explicitly required for a
        ///     device.
        ///     Using the interlocked functions with memory that is mapped with SEC_NOCACHE can result in an
        ///     EXCEPTION_ILLEGAL_INSTRUCTION exception.
        ///     The PAGE_NOCACHE flag cannot be used with the PAGE_GUARD, PAGE_NOACCESS, or PAGE_WRITECOMBINE flags.
        ///     The PAGE_NOCACHE flag can be used only when allocating private memory with the VirtualAlloc, VirtualAllocEx, or
        ///     VirtualAllocExNuma functions.
        ///     To enable non-cached memory access for shared memory, specify the SEC_NOCACHE flag when calling the
        ///     CreateFileMapping function.
        /// </summary>
        PAGE_NOCACHE = 0x200,

        /// <summary>
        ///     Sets all pages to be write-combined.
        ///     Applications should not use this attribute except when explicitly required for a device.
        ///     Using the interlocked functions with memory that is mapped as write-combined can result in an
        ///     EXCEPTION_ILLEGAL_INSTRUCTION exception.
        ///     The PAGE_WRITECOMBINE flag cannot be specified with the PAGE_NOACCESS, PAGE_GUARD, and PAGE_NOCACHE flags.
        ///     The PAGE_WRITECOMBINE flag can be used only when allocating private memory with the VirtualAlloc, VirtualAllocEx,
        ///     or VirtualAllocExNuma functions.
        ///     To enable write-combined memory access for shared memory, specify the SEC_WRITECOMBINE flag when calling the
        ///     CreateFileMapping function.
        /// </summary>
        PAGE_WRITECOMBINE = 0x400
    }

    [Flags]
    internal enum LoadLibraryFlags : uint
    {
        /// <summary>
        ///     If this value is used, and the executable module is a DLL, the system does not call DllMain for process and thread
        ///     initialization and termination.
        ///     Also, the system does not load additional executable modules that are referenced by the specified module.
        ///     Note  Do not use this value; it is provided only for backward compatibility.
        ///     If you are planning to access only data or resources in the DLL, use LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE or
        ///     LOAD_LIBRARY_AS_IMAGE_RESOURCE or both.
        ///     Otherwise, load the library as a DLL or executable module using the LoadLibrary function.
        /// </summary>
        DONT_RESOLVE_DLL_REFERENCES = 0x00000001,

        /// <summary>
        ///     If this value is used, the system maps the file into the calling process's virtual address space as if it were a
        ///     data file.
        ///     Nothing is done to execute or prepare to execute the mapped file. Therefore, you cannot call functions like
        ///     GetModuleFileName, GetModuleHandle or GetProcAddress with this DLL.
        ///     Using this value causes writes to read-only memory to raise an access violation. Use this flag when you want to
        ///     load a DLL only to extract messages or resources from it.
        ///     This value can be used with LOAD_LIBRARY_AS_IMAGE_RESOURCE.
        /// </summary>
        LOAD_LIBRARY_AS_DATAFILE = 0x00000002,

        /// <summary>
        ///     If this value is used and lpFileName specifies an absolute path, the system uses the alternate file search strategy
        ///     discussed in the Remarks section to find associated executable modules that the specified module causes to be
        ///     loaded.
        ///     If this value is used and lpFileName specifies a relative path, the behavior is undefined.
        ///     If this value is not used, or if lpFileName does not specify a path, the system uses the standard search strategy
        ///     discussed in the Remarks section to find associated executable modules that the specified module causes to be
        ///     loaded.
        ///     This value cannot be combined with any LOAD_LIBRARY_SEARCH flag.
        /// </summary>
        LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008,

        /// <summary>
        ///     If this value is used, the system does not check AppLocker rules or apply Software Restriction Policies for the
        ///     DLL.
        ///     This action applies only to the DLL being loaded and not to its dependencies.
        ///     This value is recommended for use in setup programs that must run extracted DLLs during installation.
        /// </summary>
        LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,

        /// <summary>
        ///     If this value is used, the system maps the file into the process's virtual address space as an image file.
        ///     However, the loader does not load the static imports or perform the other usual initialization steps.
        ///     Use this flag when you want to load a DLL only to extract messages or resources from it.
        ///     Unless the application depends on the image layout, this value should be used with either
        ///     LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE or LOAD_LIBRARY_AS_DATAFILE.
        /// </summary>
        LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020,

        /// <summary>
        ///     Similar to LOAD_LIBRARY_AS_DATAFILE, except that the DLL file on the disk is opened for exclusive write access.
        ///     Therefore, other processes cannot open the DLL file for write access while it is in use. However, the DLL can still
        ///     be opened by other processes.
        ///     This value can be used with LOAD_LIBRARY_AS_IMAGE_RESOURCE.
        /// </summary>
        LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,

        /// <summary>
        ///     If this value is used, the directory that contains the DLL is temporarily added to the beginning of the list of
        ///     directories that are searched for the DLL's dependencies. Directories in the standard search path are not searched.
        ///     The lpFileName parameter must specify a fully qualified path. This value cannot be combined with
        ///     LOAD_WITH_ALTERED_SEARCH_PATH.
        ///     For example, if Lib2.dll is a dependency of C:\Dir1\Lib1.dll, loading Lib1.dll with this value causes the system to
        ///     search for Lib2.dll only in C:\Dir1. To search for Lib2.dll in C:\Dir1 and all of the directories in the DLL search
        ///     path, combine this value with LOAD_LIBRARY_DEFAULT_DIRS.
        /// </summary>
        LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x00000100,

        /// <summary>
        ///     If this value is used, the application's installation directory is searched for the DLL and its dependencies.
        ///     Directories in the standard search path are not searched. This value cannot be combined with
        ///     LOAD_WITH_ALTERED_SEARCH_PATH.
        /// </summary>
        LOAD_LIBRARY_SEARCH_APPLICATION_DIR = 0x00000200,

        /// <summary>
        ///     If this value is used, directories added using the AddDllDirectory or the SetDllDirectory function are searched for
        ///     the DLL and its dependencies.
        ///     If more than one directory has been added, the order in which the directories are searched is unspecified.
        ///     Directories in the standard search path are not searched.
        ///     This value cannot be combined with LOAD_WITH_ALTERED_SEARCH_PATH.
        /// </summary>
        LOAD_LIBRARY_SEARCH_USER_DIRS = 0x00000400,

        /// <summary>
        ///     If this value is used, %windows%\system32 is searched for the DLL and its dependencies.
        ///     Directories in the standard search path are not searched.
        ///     This value cannot be combined with LOAD_WITH_ALTERED_SEARCH_PATH.
        /// </summary>
        LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800,

        /// <summary>
        ///     This value is a combination of LOAD_LIBRARY_SEARCH_APPLICATION_DIR, LOAD_LIBRARY_SEARCH_SYSTEM32, and
        ///     LOAD_LIBRARY_SEARCH_USER_DIRS.
        ///     Directories in the standard search path are not searched. This value cannot be combined with
        ///     LOAD_WITH_ALTERED_SEARCH_PATH.
        ///     This value represents the recommended maximum number of directories an application should include in its DLL search
        ///     path.
        /// </summary>
        LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000
    }

    /// <summary>
    ///     Values that determine how memory is allocated.
    /// </summary>
    internal enum MemoryAllocationType
    {
        /// <summary>
        ///     Allocates physical storage in memory or in the paging file on disk for the specified reserved memory pages. The
        ///     function initializes the memory to zero.
        ///     To reserve and commit pages in one step, call VirtualAllocEx with MEM_COMMIT | MEM_RESERVE.
        ///     The function fails if you attempt to commit a page that has not been reserved. The resulting error code is
        ///     ERROR_INVALID_ADDRESS.
        ///     An attempt to commit a page that is already committed does not cause the function to fail. This means that you can
        ///     commit pages without first determining the current commitment state of each page.
        /// </summary>
        MEM_COMMIT = 0x1000,

        /// <summary>
        ///     Reserves a range of the process's virtual address space without allocating any actual physical storage in memory or
        ///     in the paging file on disk.
        ///     You commit reserved pages by calling VirtualAllocEx again with MEM_COMMIT. To reserve and commit pages in one step,
        ///     call VirtualAllocEx with MEM_COMMIT | MEM_RESERVE.
        ///     Other memory allocation functions, such as malloc and LocalAlloc, cannot use reserved memory until it has been
        ///     released.
        /// </summary>
        MEM_RESERVE = 0x2000,

        /// <summary>
        ///     Indicates that data in the memory range specified by lpAddress and dwSize is no longer of interest. The pages
        ///     should not be read from or written to the paging file. However, the memory block will be used again later, so it
        ///     should not be decommitted. This value cannot be used with any other value.
        ///     Using this value does not guarantee that the range operated on with MEM_RESET will contain zeros. If you want the
        ///     range to contain zeros, decommit the memory and then recommit it.
        ///     When you use MEM_RESET, the VirtualAllocEx function ignores the value of fProtect. However, you must still set
        ///     fProtect to a valid protection value, such as PAGE_NOACCESS.
        ///     VirtualAllocEx returns an error if you use MEM_RESET and the range of memory is mapped to a file. A shared view is
        ///     only acceptable if it is mapped to a paging file.
        /// </summary>
        MEM_RESET = 0x80000,

        /// <summary>
        ///     Reserves an address range that can be used to map Address Windowing Extensions (AWE) pages.
        ///     This value must be used with MEM_RESERVE and no other values.
        /// </summary>
        MEM_PHYSICAL = 0x400000,

        /// <summary>
        ///     Allocates memory at the highest possible address. This can be slower than regular allocations, especially when
        ///     there are many allocations.
        /// </summary>
        MEM_TOP_DOWN = 0x100000,

        /// <summary>
        ///     Allocates memory using large page support.
        ///     The size and alignment must be a multiple of the large-page minimum. To obtain this value, use the
        ///     GetLargePageMinimum function.
        /// </summary>
        MEM_LARGE_PAGES = 0x20000000
    }

    /// <summary>
    ///     Values that determine how a block of memory is protected.
    /// </summary>
    [Flags]
    internal enum MemoryProtectionType
    {
        /// <summary>
        ///     Enables execute access to the committed region of pages. An attempt to read from or write to the committed region
        ///     results in an access violation.
        ///     This flag is not supported by the CreateFileMapping function.
        /// </summary>
        PAGE_EXECUTE = 0x10,

        /// <summary>
        ///     Enables execute or read-only access to the committed region of pages. An attempt to write to the committed region
        ///     results in an access violation.
        /// </summary>
        PAGE_EXECUTE_READ = 0x20,

        /// <summary>
        ///     Enables execute, read-only, or read/write access to the committed region of pages.
        /// </summary>
        PAGE_EXECUTE_READWRITE = 0x40,

        /// <summary>
        ///     Enables execute, read-only, or copy-on-write access to a mapped view of a file mapping object. An attempt to write
        ///     to a committed copy-on-write page results in a private copy of the page being made for the process. The private
        ///     page is marked as PAGE_EXECUTE_READWRITE, and the change is written to the new page.
        ///     This flag is not supported by the VirtualAlloc or VirtualAllocEx functions.
        /// </summary>
        PAGE_EXECUTE_WRITECOPY = 0x80,

        /// <summary>
        ///     Disables all access to the committed region of pages. An attempt to read from, write to, or execute the committed
        ///     region results in an access violation.
        /// </summary>
        PAGE_NOACCESS = 0x01,

        /// <summary>
        ///     Enables read-only access to the committed region of pages. An attempt to write to the committed region results in
        ///     an access violation. If Data Execution Prevention is enabled, an attempt to execute code in the committed region
        ///     results in an access violation.
        /// </summary>
        PAGE_READONLY = 0x02,

        /// <summary>
        ///     Enables read-only or read/write access to the committed region of pages. If Data Execution Prevention is enabled,
        ///     attempting to execute code in the committed region results in an access violation.
        /// </summary>
        PAGE_READWRITE = 0x04,

        /// <summary>
        ///     Enables read-only or copy-on-write access to a mapped view of a file mapping object. An attempt to write to a
        ///     committed copy-on-write page results in a private copy of the page being made for the process. The private page is
        ///     marked as PAGE_READWRITE, and the change is written to the new page. If Data Execution Prevention is enabled,
        ///     attempting to execute code in the committed region results in an access violation.
        ///     This flag is not supported by the VirtualAlloc or VirtualAllocEx functions.
        /// </summary>
        PAGE_WRITECOPY = 0x08,

        /// <summary>
        ///     Pages in the region become guard pages. Any attempt to access a guard page causes the system to raise a
        ///     STATUS_GUARD_PAGE_VIOLATION exception and turn off the guard page status. Guard pages thus act as a one-time access
        ///     alarm. For more information, see Creating Guard Pages.
        ///     When an access attempt leads the system to turn off guard page status, the underlying page protection takes over.
        ///     If a guard page exception occurs during a system service, the service typically returns a failure status indicator.
        ///     This value cannot be used with PAGE_NOACCESS.
        ///     This flag is not supported by the CreateFileMapping function.
        /// </summary>
        PAGE_GUARD = 0x100,

        /// <summary>
        ///     Sets all pages to be non-cachable. Applications should not use this attribute except when explicitly required for a
        ///     device. Using the interlocked functions with memory that is mapped with SEC_NOCACHE can result in an
        ///     EXCEPTION_ILLEGAL_INSTRUCTION exception.
        ///     The PAGE_NOCACHE flag cannot be used with the PAGE_GUARD, PAGE_NOACCESS, or PAGE_WRITECOMBINE flags.
        ///     The PAGE_NOCACHE flag can be used only when allocating private memory with the VirtualAlloc, VirtualAllocEx, or
        ///     VirtualAllocExNuma functions. To enable non-cached memory access for shared memory, specify the SEC_NOCACHE flag
        ///     when calling the CreateFileMapping function.
        /// </summary>
        PAGE_NOCACHE = 0x200,

        /// <summary>
        ///     Sets all pages to be write-combined.
        ///     Applications should not use this attribute except when explicitly required for a device. Using the interlocked
        ///     functions with memory that is mapped as write-combined can result in an EXCEPTION_ILLEGAL_INSTRUCTION exception.
        ///     The PAGE_WRITECOMBINE flag cannot be specified with the PAGE_NOACCESS, PAGE_GUARD, and PAGE_NOCACHE flags.
        ///     The PAGE_WRITECOMBINE flag can be used only when allocating private memory with the VirtualAlloc, VirtualAllocEx,
        ///     or VirtualAllocExNuma functions. To enable write-combined memory access for shared memory, specify the
        ///     SEC_WRITECOMBINE flag when calling the CreateFileMapping function.
        /// </summary>
        PAGE_WRITECOMBINE = 0x400
    }

    /// <summary>
    ///     Values that determine how a block of memory is freed.
    /// </summary>
    [Flags]
    internal enum MemoryFreeType
    {
        /// <summary>
        ///     Decommits the specified region of committed pages. After the operation, the pages are in the reserved state.
        ///     The function does not fail if you attempt to decommit an uncommitted page. This means that you can decommit a range
        ///     of pages without first determining the current commitment state.
        ///     Do not use this value with MEM_RELEASE.
        /// </summary>
        MEM_DECOMMIT = 0x4000,

        /// <summary>
        ///     Releases the specified region of pages. After this operation, the pages are in the free state.
        ///     If you specify this value, dwSize must be 0 (zero), and lpAddress must point to the base address returned by the
        ///     VirtualAlloc function when the region is reserved. The function fails if either of these conditions is not met.
        ///     If any pages in the region are committed currently, the function first decommits, and then releases them.
        ///     The function does not fail if you attempt to release pages that are in different states, some reserved and some
        ///     committed. This means that you can release a range of pages without first determining the current commitment state.
        ///     Do not use this value with MEM_DECOMMIT.
        /// </summary>
        MEM_RELEASE = 0x8000
    }
}
