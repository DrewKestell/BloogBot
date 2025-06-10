#pragma once

#include "Vec3Ray.h"

namespace VMAP {
	/**
	 * @brief Structure to hold area information.
	 */
	struct AreaInfo
	{
		/**
		 * @brief Default constructor for AreaInfo.
		 */
		AreaInfo() : result(false), ground_Z(-finf()), flags(0), adtId(0), rootId(0), groupId(0) {}
		bool result; /**< Flag indicating if the area information is valid. */
		float ground_Z; /**< Ground Z coordinate. */
		unsigned int flags; /**< Area flags. */
		int adtId; /**< ADT ID. */
		int rootId; /**< Root ID. */
		int groupId; /**< Group ID. */
	};

	class ModelInstance;
	class GroupModel;

	/**
	 * @brief Structure to hold location information.
	 */
	struct LocationInfo
	{
		LocationInfo() : hitInstance(nullptr), hitModel(nullptr), ground_Z(-finf()) {};
		const ModelInstance* hitInstance;
		const GroupModel* hitModel;
		float ground_Z;
	};
}