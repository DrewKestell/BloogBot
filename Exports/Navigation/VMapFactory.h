// VMapFactory.h - Fixed header
#pragma once

#include <string>

namespace VMAP
{
    class VMapManager2;
    class IVMapManager;

    class VMapFactory
    {
    public:
        // Get or create the singleton VMapManager instance
        static IVMapManager* createOrGetVMapManager();

        // Clear and delete the VMapManager instance
        static void clear();

        // Initialize the VMAP system (does not load maps)
        static void initialize();

        // Get the path where VMAP files are located
        static std::string getVMapsPath();

        // Initialize a specific map (loads vmtree file if it exists)
        static void initializeMapForContinent(unsigned int mapId);

    private:
        // Private constructor to prevent instantiation
        VMapFactory() = delete;
        VMapFactory(const VMapFactory&) = delete;
        VMapFactory& operator=(const VMapFactory&) = delete;
    };
}