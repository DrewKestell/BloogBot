#ifndef NAVIGATION_H
#define NAVIGATION_H

#include <vector>

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
	static const int ERROR = -1;
	static const int ERROR_LOAD_MAP = -2;
	static const int ERROR_PATH_CALCULATION = -3;

public:
	static Navigation* GetInstance();
	void Initialize();
	void Release();
	XYZ* CalculatePath(unsigned int mapId, XYZ start, XYZ end, bool useStraightPath, int* parLength);
	XYZ* CalculatePath(unsigned int mapId, XYZ start, int startZoneID, XYZ end, int endZoneID, bool useStraightPath, int* length);
	bool GetCrossZonePath(int startZoneID, int endZoneID, std::vector<XYZ>* posList);
	void GetPath(XYZ* path, int length);
	void FreePathArr(XYZ* pathArr);

private:
	static Navigation* s_singletonInstance;
	unsigned int lastMapId;
	XYZ* currentPath;
};

#endif
