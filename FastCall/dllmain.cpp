#include "stdafx.h"

BOOL APIENTRY DllMain(HMODULE hModule,
    DWORD  ul_reason_for_call,
    LPVOID lpReserved
)
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }

    return TRUE;
}

extern "C"
{
    struct XYZXYZ { float X1; float Y1; float Z1; float X2; float Y2; float Z2; };
    struct Intersection { float X; float Y; float Z; float R; };
    struct XYZ { float X; float Y; float Z; };

    void __declspec(dllexport) __stdcall EnumerateVisibleObjects(unsigned int callback, int filter, unsigned int ptr)
    {
        typedef void __fastcall func(unsigned int callback, int filter);
        func* function = (func*)ptr;
        function(callback, filter);
    }

    void __declspec(dllexport) __stdcall LuaCall(char* code, unsigned int ptr)
    {
        typedef void __fastcall func(char* code, const char* unused);
        func* f = (func*)ptr;
        f(code, "Unused");
        return;
    }

    void __declspec(dllexport) __stdcall LootSlot(int slot, unsigned int ptr)
    {
        typedef void __fastcall func(unsigned int slot, int unused);
        func* f = (func*)ptr;
        f(slot, 0);
    }

    unsigned int __declspec(dllexport) __stdcall GetText(char* varName, unsigned int parPtr)
    {
        typedef unsigned int __fastcall func(char* varName, unsigned int nonSense, int zero);
        func* f = (func*)parPtr;
        return f(varName, 0xFFFFFFFF, 0);
    }

    BYTE __declspec(dllexport) __stdcall Intersect(XYZXYZ* points, float* distance, Intersection* intersection, unsigned int flags, unsigned int ptr)
    {
        typedef BYTE __fastcall func(struct XYZXYZ* addrPoints, float* addrDistance, struct Intersection* addrIntersection, unsigned int flags);
        func* f = (func*)ptr;
        return f(points, distance, intersection, flags);
    }

    bool __declspec(dllexport) __stdcall Intersect2(XYZ* p1, XYZ* p2, XYZ* intersection, float* distance, unsigned int flags, unsigned int ptr)
    {
        typedef bool __fastcall func(XYZ* p1, XYZ* p2, int ignore, XYZ* intersection, float* distance, unsigned int flags);
        func* f = (func*)ptr;
        return f(p1, p2, 0, intersection, distance, flags);
    }

    void __declspec(dllexport) __stdcall SellItemByGuid(unsigned int parCount, unsigned long long parVendorGuid, unsigned long long parItemGuid, unsigned int parPtr)
    {
        typedef void __fastcall func(unsigned int itemCount, unsigned int _zero, unsigned long long vendorGuid, unsigned long long itemGuid);
        func* f = (func*)parPtr;
        f(parCount, 0, parVendorGuid, parItemGuid);
    }

    void __declspec(dllexport) __stdcall BuyVendorItem(int parItemIndex, int parQuantity, unsigned long long parVendorGuid, unsigned int parPtr)
    {
        typedef void __fastcall func(unsigned int itemIndex, unsigned int Quantity, unsigned long long vendorGuid, int _one);
        func* f = (func*)parPtr;
        f(parItemIndex, parQuantity, parVendorGuid, 5);
    }

    unsigned int __declspec(dllexport) __stdcall GetObjectPtr(int parTypemask, unsigned long long parObjectGuid, int parLine, char* parFile, unsigned int parPtr)
    {
        typedef unsigned int __fastcall func(int typemask, unsigned long long objectGuid, int line, char* file);
        func* f = (func*)parPtr;
        return f(parTypemask, parObjectGuid, parLine, parFile);
    }
}