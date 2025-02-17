#ifndef NAVIGATION_H
#define NAVIGATION_H

#include "MoveMap.h"
#include <vector>
#include <string>

class XYZ
{
public:
	float X;
	float Y;
	float Z;

	XYZ()
	{
		X = 0;
		Y = 0; 
		Z = 0;
	}

	XYZ(double X, double Y, double Z)
	{
		this->X = (float)X;
		this->Y = (float)Y;
		this->Z = (float)Z;
	}
};

class Navigation
{
public:
	static Navigation* GetInstance();
	void Initialize();
	void Release();
	XYZ* CalculatePath(unsigned int mapId, XYZ start, XYZ end, bool straightPath, int* length);
	void FreePathArr(XYZ* length);
    std::string GetMmapsPath();

private:
	void InitializeMapsForContinent(MMAP::MMapManager* manager, unsigned int mapId);
	static Navigation* s_singletonInstance;
	XYZ* currentPath;
};

#endif
