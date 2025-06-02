#pragma once

#include "stdafx.h"
#include "_BotDomainManagerInterface.h"
#include <metahost.h>

class BotHostControl : IHostControl
{
public:
    BotHostControl()
    {
        m_refCount = 0;
        m_defaultDomainManager = NULL;
    }

    virtual ~BotHostControl()
    {
        if (m_defaultDomainManager != NULL)
        {
            m_defaultDomainManager->Release();
        }
    }

    HRESULT __stdcall GetHostManager(REFIID id, void** ppHostManager)
    {
        *ppHostManager = NULL;
        return E_NOINTERFACE;
    }

    HRESULT __stdcall SetAppDomainManager(DWORD dwAppDomainID, IUnknown* pUnkAppDomainManager)
    {
        HRESULT hr = S_OK;
        hr = pUnkAppDomainManager->QueryInterface(__uuidof(_BotDomainManagerInterface), (PVOID*)&m_defaultDomainManager);
        return hr;
    }

    _BotDomainManagerInterface* GetBotDomainManagerInterface()
    {
        if (m_defaultDomainManager)
        {
            m_defaultDomainManager->AddRef();
        }
        return m_defaultDomainManager;
    }

    HRESULT __stdcall QueryInterface(const IID& iid, void** ppv)
    {
        if (!ppv) return E_POINTER;
        *ppv = this;
        AddRef();
        return S_OK;
    }

    ULONG __stdcall AddRef()
    {
        return InterlockedIncrement(&m_refCount);
    }

    ULONG __stdcall Release()
    {
        if (InterlockedDecrement(&m_refCount) == 0)
        {
            delete this;
            return 0;
        }
        return m_refCount;
    }

private:
    long m_refCount;
    _BotDomainManagerInterface* m_defaultDomainManager;
};