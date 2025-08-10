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

            // Set the base path but DON'T auto-load maps here
            std::string vmapsPath = getVMapsPath();
            gVMapManager->setBasePath(vmapsPath);

            std::cout << "[VMAP] VMapManager created with path: " << vmapsPath << std::endl;
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
            std::string vmapsPath = dirPath + "vmaps\\";

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
#else
        // For non-Windows, try various paths
        std::vector<std::string> paths = {
            "./vmaps/",
            "../vmaps/",
            "../../vmaps/",
            "/opt/vmaps/",
            "/usr/local/share/vmaps/"
        };

        for (const auto& path : paths)
        {
            if (std::filesystem::exists(path))
                return path;
        }
#endif
        // Default fallback - create directory if it doesn't exist
        std::string defaultPath = "vmaps/";
        if (!std::filesystem::exists(defaultPath))
        {
            std::cout << "[VMAP] Warning: vmaps directory not found at " << defaultPath << std::endl;
            std::cout << "[VMAP] Creating empty vmaps directory..." << std::endl;
            std::filesystem::create_directory(defaultPath);
        }
        return defaultPath;
    }

    void VMapFactory::initialize()
    {
        if (gInitialized)
            return;

        std::cout << "[VMAP] VMapFactory::initialize() called" << std::endl;

        // Just ensure the manager exists, don't load maps
        VMapManager2* manager = static_cast<VMapManager2*>(createOrGetVMapManager());
        if (!manager)
        {
            std::cerr << "[VMAP] Failed to create VMapManager!" << std::endl;
            return;
        }

        // Check if vmaps path exists
        std::string vmapsPath = getVMapsPath();
        if (!std::filesystem::exists(vmapsPath))
        {
            std::cerr << "[VMAP] Warning: VMAP path does not exist: " << vmapsPath << std::endl;
            std::cerr << "[VMAP] VMAP functionality will be limited without extracted map files" << std::endl;
        }
        else
        {
            std::cout << "[VMAP] VMAP path verified: " << vmapsPath << std::endl;

            // List available vmtree files for debugging
            int mapCount = 0;
            try
            {
                for (const auto& entry : std::filesystem::directory_iterator(vmapsPath))
                {
                    if (entry.path().extension() == ".vmtree")
                    {
                        mapCount++;
                    }
                }
                std::cout << "[VMAP] Found " << mapCount << " map files in " << vmapsPath << std::endl;
            }
            catch (const std::exception& e)
            {
                std::cerr << "[VMAP] Error scanning directory: " << e.what() << std::endl;
            }
        }

        gInitialized = true;
        std::cout << "[VMAP] VMapFactory initialized successfully" << std::endl;
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
            std::cerr << "[VMAP] Manager not available for map initialization" << std::endl;
            return;
        }

        // Check if the map file exists before trying to initialize
        std::string vmapsPath = getVMapsPath();
        char filename[256];
        snprintf(filename, sizeof(filename), "%03u.vmtree", mapId);
        std::string fullPath = vmapsPath + filename;

        if (!std::filesystem::exists(fullPath))
        {
            std::cout << "[VMAP] Map file not found: " << fullPath << " (this is normal if maps aren't extracted)" << std::endl;
            return;
        }

        if (!manager->isMapInitialized(mapId))
        {
            std::cout << "[VMAP] Initializing map " << mapId << std::endl;
            try
            {
                manager->initializeMap(mapId);
                std::cout << "[VMAP] Map " << mapId << " initialized successfully" << std::endl;
            }
            catch (const std::exception& e)
            {
                std::cerr << "[VMAP] Failed to initialize map " << mapId << ": " << e.what() << std::endl;
            }
        }
    }
}