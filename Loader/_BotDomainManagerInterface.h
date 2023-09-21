#pragma once

#include "stdafx.h"
#include <unknwn.h>

#define BOT_GUID "A15DDC0D-53EF-4776-8DA2-E87399C6654D"

struct __declspec(uuid(BOT_GUID)) _BotDomainManagerInterface;

struct _BotDomainManagerInterface : IUnknown
{
    virtual
        HRESULT __stdcall HelloWorld(LPWSTR name, LPWSTR* result) = 0;
};