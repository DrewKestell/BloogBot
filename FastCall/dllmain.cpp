//// dllmain.cpp : Definiert den Einstiegspunkt für die DLL-Anwendung.
#include "stdafx.h"
//#include <windows.h>

//using namespace std;

//#include <stdio.h>

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:

		break;

	case DLL_PROCESS_DETACH:

		break;

	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
		break;
	}

	return TRUE;
}

extern "C"
{
	typedef struct XYZXYZ { float X1; float Y1; float Z1; float X2; float Y2; float Z2; };
	typedef struct Intersection { float X; float Y; float Z; float R; };

	void __declspec(dllexport) __stdcall _DoString(char* code, unsigned int parPtr)
	{
		typedef void __fastcall func(char* code, char* zero);
		func* f = (func*)parPtr;
		f(code, "Zzuk");
		return;

	}

	unsigned int __declspec(dllexport) __stdcall _MultiplyTransformWithFacingMatrix(unsigned int returnMatrix, unsigned int facingMatrix, unsigned int posMatrix, unsigned int funcPtr)
	{
		typedef unsigned int __fastcall func(unsigned int returnMatrix, unsigned int facingMatrix, unsigned int posMatrix);
		func* f = (func*)funcPtr;
		return f(returnMatrix, facingMatrix, posMatrix);
	}


	int __declspec(dllexport) __stdcall _LuaPushString(unsigned int parLuaState, char* parString, unsigned int parPtr)
	{
		typedef int __fastcall func(unsigned int LuaStatePtr, char* parString);
		func* f = (func*)parPtr;
		return f(parLuaState, parString);
	}

	int __declspec(dllexport) __stdcall _LuaIsString(unsigned int LuaStatePtr, int number, unsigned int parPtr)
	{
		typedef int __fastcall func(unsigned int LuaStatePtr, int number);
		func* f = (func*)parPtr;
		return f(LuaStatePtr, number);
	}

	int __declspec(dllexport) __stdcall _LuaIsNumber(unsigned int LuaStatePtr, int number, unsigned int parPtr)
	{
		typedef int __fastcall func(unsigned int LuaStatePtr, int number);
		func* f = (func*)parPtr;
		return f(LuaStatePtr, number);
	}

	void __declspec(dllexport) __stdcall _RegFunc(char* funcName, unsigned int funcPtr, unsigned int parPtr)
	{
		typedef void __fastcall func(char* funcName, unsigned int funcPtr);
		func* f = (func*)parPtr;
		f(funcName, funcPtr);
	}

	void __declspec(dllexport) __stdcall _UnregFunc(char* funcName, unsigned int funcPtr, unsigned int parPtr)
	{
		typedef void __fastcall func(char* funcName, unsigned int funcPtr);
		func* f = (func*)parPtr;
		f(funcName, funcPtr);
	}

	double __declspec(dllexport) __stdcall _LuaToNumber(unsigned int LuaStatePtr, int number, unsigned int parPtr)
	{
		typedef double __fastcall func(unsigned int LuaStatePtr, int number);
		func* f = (func*)parPtr;
		return f(LuaStatePtr, number);
	}

	unsigned int __declspec(dllexport) __stdcall _LuaToString(unsigned int LuaStatePtr, int number, unsigned int parPtr)
	{
		typedef unsigned int __fastcall func(unsigned int LuaStatePtr, int number);
		func* f = (func*)parPtr;
		return f(LuaStatePtr, number);
	}

	unsigned int __declspec(dllexport) __stdcall _GetText(char* varName, unsigned int parPtr)
	{
		typedef unsigned int __fastcall func(char* varName, unsigned int nonSense, int zero);
		func* f = (func*)parPtr;
		return f(varName, 0xFFFFFFFF, 0);
	}

	void __declspec(dllexport) __stdcall _EnumVisibleObjects(unsigned int callback, int filter, unsigned int parPtr)
	{
		typedef void __fastcall func(unsigned int callback, int filter);
		func* f = (func*)parPtr;
		f(callback, filter);
	}

	BYTE __declspec(dllexport) __stdcall _Intersect(XYZXYZ* points, float* distance, Intersection* intersection, unsigned int flags, unsigned int parPtr)
	{
		typedef BYTE __fastcall func(struct XYZXYZ* addrPoints, float* addrDistance, struct Intersection* addrIntersection, unsigned int flags);
		func* f = (func*)parPtr;
		return f(points, distance, intersection, flags);
	}

	unsigned int __declspec(dllexport) __stdcall _CastSpell(int SpellId, unsigned int parPtr)
	{
		typedef unsigned int __fastcall func(int SpellId, int zero, int zero2, int zero3);
		func* f = (func*)parPtr;
		return f(SpellId, 0, 0, 0);
	}

	void __declspec(dllexport) __stdcall _UseItem(unsigned int itemPtr, unsigned int useItemPtr, unsigned int parPtr)
	{
		typedef void __fastcall func(unsigned int itemPtr, unsigned int useItemPtr);
		func* f = (func*)parPtr;
		f(itemPtr, useItemPtr);
	}

	void __declspec(dllexport) __stdcall _SellItem(unsigned int parCount, unsigned long long parVendorGuid, unsigned long long parItemGuid, unsigned int parPtr)
	{
		typedef void __fastcall func(unsigned int itemCount, unsigned int _zero, unsigned long long vendorGuid, unsigned long long itemGuid);
		func* f = (func*)parPtr;
		f(parCount, 0, parVendorGuid, parItemGuid);
	}

	void __declspec(dllexport) __stdcall _BuyVendorItem(int parItemIndex, int parQuantity, unsigned long long parVendorGuid, unsigned int parPtr)
	{
		typedef void __fastcall func(unsigned int itemIndex, unsigned int Quantity, unsigned long long vendorGuid, int _one);
		func* f = (func*)parPtr;
		f(parItemIndex, parQuantity, parVendorGuid, 5);
	}

	void __declspec(dllexport) __stdcall _LootSlot(int parSlot, unsigned int parPtr)
	{
		typedef void __fastcall func(unsigned int parSlot, int unk0);
		func* f = (func*)parPtr;
		f(parSlot, 0);
	}
}
