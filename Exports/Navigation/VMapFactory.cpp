// VMapFactory.cpp - Fixed version with proper initialization
#include "VMapFactory.h"
#include "VMapManager2.h"
#include <filesystem>
#include <string>
#include <iostream>

#ifdef _WIN32
#include <windows.h>
extern "C" IMAGE_DOS_HEADER __ImageBase;
#endif

namespace VMAP
{
    static VMapManager2* gVMapManager = nullptr;
    static bool gInitialized = false;

    IVMapManager* VMapFactory::createOrGetVMapManager()
    {
        if (!gVMapManager)
        {
            gVMapManager = new VMapManager2();

            std::string vmapsPath = getVMapsPath();
            gVMapManager->setBasePath(vmapsPath);
        }
        return gVMapManager;
    }

    void VMapFactory::clear()
    {
        if (gVMapManager)
        {
            delete gVMapManager;
            gVMapManager = nullptr;
            gInitialized = false;
        }
    }

    std::string VMapFactory::getVMapsPath()
    {
        std::string vmapsPath;
#ifdef _WIN32
        // Get the DLL/EXE path
        WCHAR dllPath[MAX_PATH] = { 0 };
        GetModuleFileNameW((HINSTANCE)&__ImageBase, dllPath, _countof(dllPath));

        // Convert to string and find directory
        std::wstring ws(dllPath);
        std::string pathAndFile(ws.begin(), ws.end());

        size_t lastSlash = pathAndFile.find_last_of("\\/");
        if (lastSlash != std::string::npos)
        {
            std::string dirPath = pathAndFile.substr(0, lastSlash + 1);
            vmapsPath = dirPath + "vmaps\\";

            // Check if the directory exists
            if (std::filesystem::exists(vmapsPath))
            {
                return vmapsPath;
            }

            // Try parent directory
            size_t parentSlash = pathAndFile.find_last_of("\\/", lastSlash - 1);
            if (parentSlash != std::string::npos)
            {
                dirPath = pathAndFile.substr(0, parentSlash + 1);
                vmapsPath = dirPath + "vmaps\\";
                if (std::filesystem::exists(vmapsPath))
                {
                    return vmapsPath;
                }
            }
        }
        return vmapsPath;
#endif
    }

    void VMapFactory::initialize()
    {
        if (gInitialized)
            return;

        VMapManager2* manager = static_cast<VMapManager2*>(createOrGetVMapManager());
        if (!manager)
        {
            return;
        }

        gInitialized = true;
    }

    void VMapFactory::initializeMapForContinent(unsigned int mapId)
    {
        if (!gInitialized)
        {
            initialize();
        }

        VMapManager2* manager = static_cast<VMapManager2*>(createOrGetVMapManager());
        if (!manager)
        {
            return;
        }

        // Check if the map file exists before trying to initialize
        std::string vmapsPath = getVMapsPath();
        char filename[256];
        snprintf(filename, sizeof(filename), "%03u.vmtree", mapId);
        std::string fullPath = vmapsPath + filename;

        if (!std::filesystem::exists(fullPath))
        {
            return;
        }

        if (!manager->isMapInitialized(mapId))
        {
            try
            {
                manager->initializeMap(mapId);
            }
            catch (const std::exception& e)
            {
                std::cerr << "[VMAP] Failed to initialize map " << mapId << ": " << e.what() << std::endl;
            }
        }
    }
}