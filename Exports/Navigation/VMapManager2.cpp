/**
 * MaNGOS is a full featured server for World of Warcraft, supporting
 * the following clients: 1.12.x, 2.4.3, 3.3.5a, 4.3.4a and 5.4.8
 *
 * Copyright (C) 2005-2025 MaNGOS <https://www.getmangos.eu>
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * World of Warcraft, and all World of Warcraft or Warcraft art, images,
 * and lore are copyrighted by Blizzard Entertainment, Inc.
 */

#include <iomanip>
#include <string>
#include <sstream>
#include <filesystem>
#include <iostream> // Optional, for debugging output
#include <fstream>

#include "VMapManager2.h"
#include "Vec3Ray.h"
#include "MapTree.h"
#include "GroupModel.h"
#include "ModelInstance.h"
#include "VMapDefinitions.h"
#include "GameObjectModel.h"

#define LOG_DEBUG(msg) std::cout << "[DEBUG] " << msg << std::endl
#define LOG_WARN(msg)  std::cerr << "[WARN] " << msg << std::endl
#define LOG_ERROR(msg) std::cerr << "[ERROR] " << msg << std::endl

namespace fs = std::filesystem;

static std::string getModelKey(const std::string& rawFilename) {
	// Remove all extensions (e.g., ".wmo.vmo" -> "name", ".m2.vmo" -> "name")
	fs::path p(rawFilename);
	std::string name = p.stem().string();
	if (fs::path(name).extension() == ".wmo" || fs::path(name).extension() == ".m2")
		name = fs::path(name).stem().string();
	return name;
}

namespace VMAP
{
	bool DefaultIsVMAPDisabledForPtr(unsigned int entry, uint8_t flags)
	{
		return false; // ← Always allow VMAP logic
	}
	/**
	 * @brief Constructor for VMapManager2.
	 */
	VMapManager2::VMapManager2() : IsVMAPDisabledForPtr(nullptr), iLoadedModelFiles(), iInstanceMapTrees()
	{
	}

	VMapManager2::VMapManager2(bool(*IsVMAPDisabledForPtr)(unsigned int, uint8_t))
		: IsVMAPDisabledForPtr(IsVMAPDisabledForPtr), iLoadedModelFiles(), iInstanceMapTrees()
	{
		assert(IsVMAPDisabledForPtr && "IsVMAPDisabledForPtr must not be null");
	}

	/**
	 * @brief Destructor for VMapManager2.
	 * Cleans up all loaded map trees and model files.
	 */
	VMapManager2::~VMapManager2(void)
	{
		for (InstanceTreeMap::iterator i = iInstanceMapTrees.begin(); i != iInstanceMapTrees.end(); ++i)
		{
			delete i->second;
		}
		for (ModelFileMap::iterator i = iLoadedModelFiles.begin(); i != iLoadedModelFiles.end(); ++i)
		{
			delete i->second.getModel();
		}
	}
	
	VMapManager2* VMapManager2::Instance()
	{
		if (!_instance)
			_instance = new VMapManager2(DefaultIsVMAPDisabledForPtr);
		return _instance;
	}

	VMapManager2* VMapManager2::_instance = nullptr;

	enum VMOType { VMO_WMO, VMO_M2, VMO_Unknown };

	VMOType detectVMOType(const std::string& filepath) {
		FILE* f = fopen(filepath.c_str(), "rb");
		if (!f) { return VMO_Unknown; }
		char magic[8] = { 0 };
		if (fread(magic, 1, 8, f) != 8) { fclose(f); return VMO_Unknown; }
		if (strncmp(magic, "RAWVMAP", 8) != 0) { fclose(f); return VMO_Unknown; }
		uint32_t vertexCount = 0, groupCount = 0;
		if (fread(&vertexCount, sizeof(uint32_t), 1, f) != 1 ||
			fread(&groupCount, sizeof(uint32_t), 1, f) != 1) {
			std::cerr << "[VMAP][Detect] Could not read counts in: " << filepath << std::endl;
			fclose(f); return VMO_Unknown;
		}

		fclose(f);
		if (groupCount == 1) return VMO_M2;
		if (groupCount > 1) return VMO_WMO;
		return VMO_Unknown;
	}

	void VMapManager2::Initialize()
	{
		const std::string basePath = "./vmaps";

		// --- Step 1: Static Map Tiles (unchanged) ---
		for (const auto& entry : fs::directory_iterator(basePath)) {
			const fs::path& path = entry.path();
			std::cout << "[VMAP][Static] Checking file: " << path.string() << std::endl;
			if (!fs::is_regular_file(path))
				continue;
			if (path.extension() == ".vmtree") {
				std::string filename = path.stem().string();
				try {
					unsigned int mapId = std::stoul(filename);
					for (int tileX = 0; tileX < 64; ++tileX) {
						for (int tileY = 0; tileY < 64; ++tileY) {
							if (StaticMapTree::CanLoadMap(basePath, mapId, tileX, tileY)) {
								loadMap(basePath.c_str(), mapId, tileX, tileY);
							}
						}
					}
				}
				catch (const std::exception& e) {
					std::cerr << "[VMAP][Static][Exception] Skipping invalid .vmtree file: " << filename << " (" << e.what() << ")" << std::endl;
				}
			}
		}

		// --- Step 2: Only load RAWVMAP model files as exported by this toolchain ---
		for (const auto& entry : fs::directory_iterator(basePath)) {
			const fs::path& path = entry.path();
			if (!fs::is_regular_file(path))
				continue;

			// Only consider .vmo, .wmo.vmo, .m2.vmo extensions (the actual file extensions written by vmangos exporters)
			std::string ext = path.extension().string();
			if (ext != ".vmo" && ext != ".wmo.vmo" && ext != ".m2.vmo")
				continue;

			// Check for RAWVMAP magic
			std::ifstream file(path, std::ios::binary);
			char magic[8] = { 0 };
			if (!file.read(magic, 8) || std::strncmp(magic, "RAWVMAP", 8) != 0)
				continue; // Skip non-RAWVMAP files

			// Use the filename as the key, without double extension (so: foo.m2, bar.wmo, etc)
			std::string modelName = path.stem().string();

			WorldModel* model = acquireUniversalModelInstance(basePath, modelName, 0);
			if (!model) {
				std::cerr << "[VMAP][Dynamic][Error] Failed to parse RAWVMAP model: " << path.string() << std::endl;
				continue;
			}

			GameObjectModel obj;
			obj.name = modelName;
			obj.iModel = model;
			obj.iModelBound = model->bound;
			obj.iPos = Vec3(0.0f, 0.0f, 0.0f);
			obj.iQuat = Quat::identity();
			obj.iScale = 1.0f;
			obj.iInvScale = 1.0f;

			// Compute transformed bounds for dynamic tree (if needed)
			const Vec3 min = obj.iModelBound.min;
			const Vec3 max = obj.iModelBound.max;
			Vec3 corners[8] = {
				Vec3(min.x, min.y, min.z), Vec3(max.x, min.y, min.z),
				Vec3(min.x, max.y, min.z), Vec3(max.x, max.y, min.z),
				Vec3(min.x, min.y, max.z), Vec3(max.x, min.y, max.z),
				Vec3(min.x, max.y, max.z), Vec3(max.x, max.y, max.z)
			};
			Vec3 initialCorner = obj.iQuat * corners[0] + obj.iPos;
			AABox transformed;
			transformed.set(initialCorner, initialCorner);
			for (int i = 1; i < 8; ++i) {
				Vec3 corner = obj.iQuat * corners[i] + obj.iPos;
				transformed.merge(corner);
			}
			obj.iBound = transformed;

			_dynamicTree.insert(obj);
		}

		_dynamicTree.balance();
	}

	void VMapManager2::loadSubmodelsRecursively(WorldModel* model, unsigned int flags) {
		// All subgroups already loaded; nothing to do unless you add doodad/external refs
	}

	void VMapManager2::DestroyInstance()
	{
		delete _instance;
		_instance = nullptr;
	}

	/**
	 * @brief Converts a position from the game world to the internal representation.
	 *
	 * @param x The x-coordinate in the game world.
	 * @param y The y-coordinate in the game world.
	 * @param z The z-coordinate in the game world.
	 * @return Vec3 The converted position.
	 */
	Vec3 VMapManager2::convertPositionToInternalRep(float x, float y, float z) const
	{
		Vec3 pos;
		const float mid = 0.5f * 64.0f * 533.33333333f;
		pos.x = mid - x;
		pos.y = mid - y;
		pos.z = z;

		return pos;
	}

	/**
	 * @brief Generates the map file name based on the map ID.
	 *
	 * @param pMapId The map ID.
	 * @return std::string The generated map file name.
	 */
	std::string VMapManager2::getMapFileName(unsigned int pMapId)
	{
		std::stringstream fname;
		fname.width(3);
		fname << std::setfill('0') << pMapId << std::string(MAP_FILENAME_EXTENSION2);
		return fname.str();
	}

	/**
	 * @brief Loads a map tile.
	 *
	 * @param pBasePath The base path to the map files.
	 * @param pMapId The map ID.
	 * @param x The x-coordinate of the tile.
	 * @param y The y-coordinate of the tile.
	 * @return VMAPLoadResult The result of the load operation.
	 */
	VMAPLoadResult VMapManager2::loadMap(const char* pBasePath, unsigned int pMapId, int x, int y)
	{
		VMAPLoadResult result = VMAP_LOAD_RESULT_IGNORED;

		if (isMapLoadingEnabled())
		{
			bool loaded = _loadMap(pMapId, pBasePath, x, y);
			if (loaded)
			{
				result = VMAP_LOAD_RESULT_OK;
			}
			else
			{
				result = VMAP_LOAD_RESULT_ERROR;
			}
		}

		return result;
	}

	/**
	 * @brief Internal method to load a map tile.
	 *
	 * @param pMapId The map ID.
	 * @param basePath The base path to the map files.
	 * @param tileX The x-coordinate of the tile.
	 * @param tileY The y-coordinate of the tile.
	 * @return bool True if the tile was loaded successfully, false otherwise.
	 */
	bool VMapManager2::_loadMap(unsigned int pMapId, const std::string& basePath, unsigned int tileX, unsigned int tileY)
	{
		auto instanceTree = iInstanceMapTrees.find(pMapId);

		// Create StaticMapTree if this is the first tile for this mapId
		if (instanceTree == iInstanceMapTrees.end())
		{
			std::string mapFileName = getMapFileName(pMapId);

			StaticMapTree* newTree = new StaticMapTree(pMapId, basePath);
			if (!newTree->InitMap(mapFileName, this))
			{
				delete newTree;
				return false;
			}

			instanceTree = iInstanceMapTrees.insert({ pMapId, newTree }).first;
		}

		if (!instanceTree->second)
		{
			return false;
		}

		bool result = instanceTree->second->LoadMapTile(tileX, tileY, this);

		return result;
	}

	/**
	 * @brief Unloads a map.
	 *
	 * @param pMapId The map ID.
	 */
	void VMapManager2::unloadMap(unsigned int pMapId)
	{
		InstanceTreeMap::iterator instanceTree = iInstanceMapTrees.find(pMapId);
		if (instanceTree != iInstanceMapTrees.end())
		{
			instanceTree->second->UnloadMap(this);
			if (instanceTree->second->numLoadedTiles() == 0)
			{
				delete instanceTree->second;
				iInstanceMapTrees.erase(pMapId);
			}
		}
	}

	/**
	 * @brief Unloads a specific map tile.
	 *
	 * @param pMapId The map ID.
	 * @param x The x-coordinate of the tile.
	 * @param y The y-coordinate of the tile.
	 */
	void VMapManager2::unloadMap(unsigned int pMapId, int x, int y)
	{
		InstanceTreeMap::iterator instanceTree = iInstanceMapTrees.find(pMapId);
		if (instanceTree != iInstanceMapTrees.end())
		{
			instanceTree->second->UnloadMapTile(x, y, this);
			if (instanceTree->second->numLoadedTiles() == 0)
			{
				delete instanceTree->second;
				iInstanceMapTrees.erase(pMapId);
			}
		}
	}

	/**
	 * @brief Checks if there is a line of sight between two points.
	 *
	 * @param pMapId The map ID.
	 * @param x1 The x-coordinate of the first point.
	 * @param y1 The y-coordinate of the first point.
	 * @param z1 The z-coordinate of the first point.
	 * @param x2 The x-coordinate of the second point.
	 * @param y2 The y-coordinate of the second point.
	 * @param z2 The z-coordinate of the second point.
	 * @return bool True if there is a line of sight, false otherwise.
	 */
	bool VMapManager2::isInLineOfSight(unsigned int pMapId, float x1, float y1, float z1, float x2, float y2, float z2, bool ignoreM2Model)
	{
		if (!isLineOfSightCalcEnabled()) return true;
		bool result = true;
		InstanceTreeMap::iterator instanceTree = iInstanceMapTrees.find(pMapId);
		if (instanceTree != iInstanceMapTrees.end())
		{
			Vec3 pos1 = convertPositionToInternalRep(x1, y1, z1);
			Vec3 pos2 = convertPositionToInternalRep(x2, y2, z2);
			if (pos1 != pos2)
				result = instanceTree->second->isInLineOfSight(pos1, pos2, ignoreM2Model);
		}
		return result;
	}

	/**
	 * @brief Gets the hit position of an object in the line of sight.
	 *
	 * @param pMapId The map ID.
	 * @param x1 The x-coordinate of the first point.
	 * @param y1 The y-coordinate of the first point.
	 * @param z1 The z-coordinate of the first point.
	 * @param x2 The x-coordinate of the second point.
	 * @param y2 The y-coordinate of the second point.
	 * @param z2 The z-coordinate of the second point.
	 * @param rx The x-coordinate of the hit position.
	 * @param ry The y-coordinate of the hit position.
	 * @param rz The z-coordinate of the hit position.
	 * @param pModifyDist The distance to modify the hit position.
	 * @return bool True if an object was hit, false otherwise.
	 */
	bool VMapManager2::getObjectHitPos(unsigned int mapId,
		float x1, float y1, float z1,
		float x2, float y2, float z2,
		float& outX, float& outY, float& outZ,
		float modifyDist)
	{
		outX = x2;
		outY = y2;
		outZ = z2;

		float tempX = x2, tempY = y2, tempZ = z2;
		bool resultStatic = false, resultDynamic = false;

		if (!isLineOfSightCalcEnabled() || (IsVMAPDisabledForPtr && IsVMAPDisabledForPtr(mapId, VMAP_DISABLE_LOS)))
			return false;

		auto instanceTree = iInstanceMapTrees.find(mapId);
		if (instanceTree != iInstanceMapTrees.end() && instanceTree->second)
		{
			Vec3 pos1 = convertPositionToInternalRep(x1, y1, z1);
			Vec3 pos2 = convertPositionToInternalRep(x2, y2, z2);
			Vec3 resultPos;

			if (instanceTree->second->getObjectHitPos(pos1, pos2, resultPos, modifyDist))
			{
				Vec3 finalPos = convertPositionToInternalRep(resultPos.x, resultPos.y, resultPos.z);
				tempX = finalPos.x;
				tempY = finalPos.y;
				tempZ = finalPos.z;
				outX = tempX;
				outY = tempY;
				outZ = tempZ;
				resultStatic = true;
			}
		}

		// Check dynamic tree, using updated dest from static if applicable
		if (_dynamicTree.getObjectHitPos(x1, y1, z1, outX, outY, outZ, tempX, tempY, tempZ, modifyDist))
		{
			outX = tempX;
			outY = tempY;
			outZ = tempZ;
			resultDynamic = true;
		}

		return resultStatic || resultDynamic;
	}

	/**
	 * @brief Gets the height at a specific position.
	 *
	 * @param pMapId The map ID.
	 * @param x The x-coordinate of the position.
	 * @param y The y-coordinate of the position.
	 * @param z The z-coordinate of the position.
	 * @param maxSearchDist The maximum search distance.
	 * @return float The height at the position, or VMAP_INVALID_HEIGHT_VALUE if no height is available.
	 */
	float VMapManager2::getHeight(unsigned int pMapId, float x, float y, float z, float maxSearchDist)
	{
		float height = VMAP_INVALID_HEIGHT_VALUE;           // no height
		if (isHeightCalcEnabled())
		{
			InstanceTreeMap::iterator instanceTree = iInstanceMapTrees.find(pMapId);
			if (instanceTree != iInstanceMapTrees.end())
			{
				Vec3 pos = convertPositionToInternalRep(x, y, z);
				height = instanceTree->second->getHeight(pos, maxSearchDist);
				if (!(height < finf()))
				{
					height = VMAP_INVALID_HEIGHT_VALUE;     // no height
				}
			}
		}
		return height;
	}

	/**
	 * @brief Gets area information at a specific position.
	 *
	 * @param pMapId The map ID.
	 * @param x The x-coordinate of the position.
	 * @param y The y-coordinate of the position.
	 * @param z The z-coordinate of the position.
	 * @param flags The area flags.
	 * @param adtId The ADT ID.
	 * @param rootId The root ID.
	 * @param groupId The group ID.
	 * @return bool True if area information was retrieved, false otherwise.
	 */
	bool VMapManager2::getAreaInfo(unsigned int pMapId, float x, float y, float& z, unsigned int& flags, int& adtId, int& rootId, int& groupId) const
	{
		bool result = false;
		if (!IsVMAPDisabledForPtr(pMapId, VMAP_DISABLE_AREAFLAG))
		{
			InstanceTreeMap::const_iterator instanceTree = iInstanceMapTrees.find(pMapId);
			if (instanceTree != iInstanceMapTrees.end())
			{
				Vec3 pos = convertPositionToInternalRep(x, y, z);
				result = instanceTree->second->getAreaInfo(pos, flags, adtId, rootId, groupId);
				// z is not touched by convertPositionToMangosRep(), so just copy
				z = pos.z;
			}
		}
		return result;
	}

	/**
	 * @brief Gets the liquid level at a specific position.
	 *
	 * @param pMapId The map ID.
	 * @param x The x-coordinate of the position.
	 * @param y The y-coordinate of the position.
	 * @param z The z-coordinate of the position.
	 * @param ReqLiquidType The required liquid type.
	 * @param level The liquid level.
	 * @param floor The floor level.
	 * @param type The liquid type.
	 * @return bool True if the liquid level was retrieved, false otherwise.
	 */
	bool VMapManager2::GetLiquidLevel(unsigned int mapId, float x, float y, float z, uint8_t reqLiquidType, float& level, float& floor, unsigned int& type) const
	{
		if (!IsVMAPDisabledForPtr(mapId, VMAP_DISABLE_LIQUIDSTATUS))
		{
			auto instanceTree = iInstanceMapTrees.find(mapId);
			if (instanceTree != iInstanceMapTrees.end())
			{
				LocationInfo info;
				Vec3 pos = convertPositionToInternalRep(x, y, z);
				if (instanceTree->second->GetLocationInfo(pos, info))
				{
					floor = info.ground_Z;
					type = info.hitModel->GetLiquidType();

					if (reqLiquidType && !(type & reqLiquidType))
					{
						std::cout << "[VMAP][Liquid] Liquid type mismatch. Required: " << (int)reqLiquidType << ", Found: " << type << std::endl;
						return false;
					}

					if (info.hitInstance->GetLiquidLevel(pos, info, level))
					{
						return true;
					}

					std::cout << "[VMAP][Liquid] GetLiquidLevel failed." << std::endl;
				}
				else
				{
					std::cout << "[VMAP][Liquid] GetLocationInfo failed." << std::endl;
				}
			}
			else
			{
				std::cout << "[VMAP][Liquid] No tree found for mapId=" << mapId << std::endl;
			}
		}

		return false;
	}

	static VMOFormat DetectVMOFormat(const std::string& filepath)
	{
		std::ifstream file(filepath, std::ios::binary);
		if (!file)
			return VMOFormat::Unknown;

		char chunkID[5] = { 0 };
		file.read(chunkID, 4);

		if (std::strncmp(chunkID, "GRP ", 4) == 0 || std::strncmp(chunkID, "MAIN", 4) == 0)
			return VMOFormat::WMO;

		if (std::strncmp(chunkID, "MD20", 4) == 0 || std::strncmp(chunkID, "MOGP", 4) == 0)
			return VMOFormat::M2;

		return VMOFormat::Unknown;
	}
	/**
	 * @brief Acquires a model instance.
	 *
	 * @param basepath The base path to the model files.
	 * @param filename The name of the model file.
	 * @param flags The flags for the model.
	 * @return Model* The acquired model instance.
	 */
	WorldModel* VMapManager2::acquireUniversalModelInstance(const std::string& basepath, const std::string& rawFilename, unsigned int flags)
	{
		std::string modelKey = getModelKey(rawFilename);

		auto it = iLoadedModelFiles.find(modelKey);
		if (it != iLoadedModelFiles.end()) {
			it->second.incRefCount();
			return it->second.getModel();
		}

		fs::path base = fs::path(basepath);
		std::vector<std::string> candidates = {
			modelKey + ".wmo.vmo",
			modelKey + ".m2.vmo",
			modelKey + ".vmo"
		};

		std::string foundPath;
		VMOType foundType = VMO_Unknown;
		for (const std::string& c : candidates) {
			fs::path tryPath = base / c;
			if (fs::exists(tryPath)) {
				VMOType t = detectVMOType(tryPath.string());
				if (t != VMO_Unknown) {
					foundPath = tryPath.string();
					foundType = t;
					break;
				}
			}
		}

		if (foundPath.empty() || foundType == VMO_Unknown) {
			return nullptr;
		}

		std::unique_ptr<WorldModel> model = std::make_unique<WorldModel>();
		model->Flags = flags;
		if (!model->ReadFile(foundPath)) {
			std::cerr << "[VMAP][Universal][Error] Failed to parse model: " << foundPath << std::endl;
			return nullptr;
		}
		iLoadedModelFiles[modelKey].setModel(model.release());
		iLoadedModelFiles[modelKey].incRefCount();
		return iLoadedModelFiles[modelKey].getModel();
	}

	/**
	 * @brief Releases a model instance.
	 *
	 * @param filename The name of the model file.
	 */
	void VMapManager2::releaseModelInstance(const std::string& filename)
	{
		ModelFileMap::iterator model = iLoadedModelFiles.find(filename);
		if (model == iLoadedModelFiles.end())
		{
			ERROR_LOG("VMapManager2: trying to unload non-loaded file '%s'!", filename.c_str());
			return;
		}
		if (model->second.decRefCount() == 0)
		{
			DEBUG_FILTER_LOG(LOG_FILTER_MAP_LOADING, "VMapManager2: unloading file '%s'", filename.c_str());
			delete model->second.getModel();
			iLoadedModelFiles.erase(model);
		}
	}

	/**
	 * @brief Checks if a map exists.
	 *
	 * @param pBasePath The base path to the map files.
	 * @param pMapId The map ID.
	 * @param x The x-coordinate of the tile.
	 * @param y The y-coordinate of the tile.
	 * @return bool True if the map exists, false otherwise.
	 */
	bool VMapManager2::existsMap(const char* pBasePath, unsigned int pMapId, int x, int y)
	{
		return StaticMapTree::CanLoadMap(std::string(pBasePath), pMapId, x, y);
	}

	void VMapManager2::buildModelFileIndex(const std::string& basePath) {
		if (_modelFilesIndexed.count(basePath))
			return; // Already indexed

		_basePath = basePath;
		_modelFileIndex.clear();

		for (const auto& entry : fs::directory_iterator(basePath)) {
			if (!fs::is_regular_file(entry.path())) continue;
			std::string fname = entry.path().filename().string();
			std::string base = fs::path(fname).stem().string();
			_modelFileIndex[base].push_back(fname);
		}
		_modelFilesIndexed.insert(basePath);
		std::cout << "[VMAP][Flat] Indexed " << _modelFileIndex.size() << " unique basenames in: " << basePath << std::endl;
	}

	WorldModel* VMapManager2::loadModelByReference(const std::string& ref, unsigned int flags) {
		std::string basename = fs::path(ref).stem().string();

		// Prevent infinite recursion
		if (_visitedRefs.count(basename)) return iLoadedModelFiles.count(basename) ? iLoadedModelFiles[basename].getModel() : nullptr;
		_visitedRefs.insert(basename);

		// Check already loaded
		if (iLoadedModelFiles.count(basename)) {
			return iLoadedModelFiles[basename].getModel();
		}
		// Search for any file with matching basename
		auto it = _modelFileIndex.find(basename);
		if (it == _modelFileIndex.end()) {
			std::cerr << "[VMAP][Flat][Error] Could not find model for reference: " << ref << std::endl;
			return nullptr;
		}
		for (const auto& candidate : it->second) {
			std::string fullPath = (fs::path(_basePath) / candidate).string();
			VMOType t = detectVMOType(fullPath);
			if (t != VMO_Unknown) {
				std::unique_ptr<WorldModel> model = std::make_unique<WorldModel>();
				model->Flags = flags;
				if (model->ReadFile(fullPath)) {
					iLoadedModelFiles[basename].setModel(model.release());
					iLoadedModelFiles[basename].incRefCount();
					std::cout << "[VMAP][Flat] Loaded model: " << fullPath << std::endl;
					loadSubmodelsRecursively(iLoadedModelFiles[basename].getModel(), flags);
					return iLoadedModelFiles[basename].getModel();
				}
			}
		}
		std::cerr << "[VMAP][Flat][Error] No valid model loaded for reference: " << ref << std::endl;
		return nullptr;
	}

} // namespace VMAP