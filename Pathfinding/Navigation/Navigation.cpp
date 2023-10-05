#include "Navigation.h"
#include "MoveMap.h"
#include "PathFinder.h"
#include <vector>

using namespace std;

Navigation* Navigation::s_singletonInstance = NULL;
bool loaded = false;

int zeroX[] = { 22, 22, 23, 23, 23, 23, 23, 23, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 33, 33, 33, 33, 33, 33, 33, 33, 33, 33, 33, 33, 33, 33, 33, 33, 34, 34, 34, 34, 34, 34, 34, 34, 34, 34, 34, 34, 34, 34, 34, 34, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 38, 38, 38, 38, 38, 38, 38, 38, 38, 38, 38, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 41, 41, 41, 41, 41, 41, 41, 41, 41, 41, 41, 41, 41, 41, 41, 41, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 53, 53, 53, 53, 53, 53, 53, 53, 53, 53, 53, 53, 53, 53, 53, 54, 54, 54, 54, 54, 54, 54, 54, 54, 54, 54, 54, 54, 54, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 55, 56, 56, 56, 56, 56, 56, 56, 56, 56, 56, 57, 57, 57, 57, 57, 57, 57, 58, 58, 58, 58, 58, 58, 59, 59, 59, 59, 59, 59, 60, 60, 60, 60, 60, 61, 61, 61, 61, 29, 29, 29, 29, 29, 30, 30, 30, 30, 30, 31, 31, 31, 31, 31, 32, 32, 32, 33, 33, 33, 34, 34, 34, 35, 35, 31, 32, 32, 32, 32, 32, 33, 33, 33, 29, 29, 30, 30, 31, 31, 31, 31, 32, 32 };
int zeroY[] = { 39, 40, 36, 37, 38, 39, 40, 41, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 25, 26, 32, 33, 34, 35, 36, 37, 38, 39, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 29, 30, 31, 32, 33, 34, 35, 29, 30, 31, 32, 33, 34, 29, 30, 31, 32, 33, 34, 29, 30, 31, 32, 33, 30, 31, 32, 33, 30, 31, 32, 33, 34, 30, 31, 32, 33, 34, 30, 31, 32, 33, 34, 31, 32, 33, 31, 32, 33, 31, 32, 33, 32, 33, 31, 31, 32, 30, 31, 32, 30, 31, 32, 32, 33, 31, 32, 31, 32, 31, 32, 31, 32 };

int oneX[] = { 00, 01, 01, 01, 02, 9, 9, 9, 9, 9, 9, 10, 10, 10, 10, 10, 10, 10, 11, 11, 11, 11, 11, 11, 11, 11, 12, 12, 12, 12, 12, 12, 12, 12, 12, 13, 13, 13, 13, 13, 13, 13, 13, 13, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 15, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 17, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 18, 19, 19, 19, 19, 19, 19, 19, 19, 19, 19, 19, 19, 19, 19, 19, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 21, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 22, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 23, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 24, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 26, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 27, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 29, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 32, 33, 33, 33, 33, 33, 33, 33, 33, 33, 33, 33, 33, 33, 33, 33, 33, 33, 33, 33, 34, 34, 34, 34, 34, 34, 34, 34, 34, 34, 34, 34, 34, 34, 34, 34, 34, 34, 34, 34, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 35, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 36, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 38, 38, 38, 38, 38, 38, 38, 38, 38, 38, 38, 38, 38, 38, 38, 38, 38, 38, 38, 38, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 40, 41, 41, 41, 41, 41, 41, 41, 41, 41, 41, 41, 41, 41, 41, 41, 41, 41, 41, 41, 41, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 42, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 44, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 45, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 46, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 47, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 49, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 51, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 52, 53, 53, 53, 53, 54, 54, 54, 54, 55, 55, 00, 00, 00, 00, 00, 00, 01, 01, 01, 01, 01, 01, 02, 02, 02, 02, 02, 02, 46, 46, 46, 46, 47, 47, 47, 47, 48, 48, 48, 49, 49, 49 };
int oneY[] = { 01, 00, 01, 02, 01, 27, 28, 29, 30, 31, 32, 27, 28, 29, 30, 31, 32, 33, 26, 27, 28, 29, 30, 31, 32, 33, 25, 26, 27, 28, 29, 30, 31, 32, 33, 25, 26, 27, 28, 29, 30, 31, 32, 33, 25, 26, 27, 28, 29, 30, 31, 32, 33, 38, 39, 40, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 44, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 44, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 44, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 44, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 26, 27, 28, 29, 30, 31, 32, 35, 36, 37, 38, 39, 40, 41, 42, 26, 27, 28, 29, 30, 31, 32, 35, 36, 37, 38, 39, 40, 41, 27, 28, 29, 30, 31, 37, 38, 39, 40, 41, 42, 39, 40, 41, 42, 26, 39, 40, 41, 40, 41, 00, 01, 02, 61, 62, 63, 00, 01, 02, 61, 62, 63, 00, 01, 02, 61, 62, 63, 27, 28, 29, 30, 27, 28, 29, 30, 27, 28, 29, 27, 28, 29 };

bool zeroLoaded = false;
bool oneLoaded = false;

Navigation* Navigation::GetInstance()
{
	if (s_singletonInstance == NULL)
		s_singletonInstance = new Navigation();
	return s_singletonInstance;
}

void Navigation::Initialize()
{
	dtAllocSetCustom(dtCustomAlloc, dtCustomFree);
	lastMapId = -1;
}

void Navigation::Release()
{
	MMAP::MMapFactory::createOrGetMMapManager()->~MMapManager();
}

void Navigation::FreePathArr(XYZ* pathArr)
{
	delete[] pathArr;
}

XYZ* Navigation::CalculatePath(unsigned int mapId, XYZ start, XYZ end, bool useStraightPath, int* length)
{
	MMAP::MMapManager* manager = MMAP::MMapFactory::createOrGetMMapManager();
	if (mapId != lastMapId)
	{
		// ToDo: Check if unload was successful. Problem => first map won't be loaded
		manager->unloadMap(lastMapId);
	}

	int gridX = (int)(32 - start.X / SIZE_OF_GRIDS);
	int gridY = (int)(32 - start.Y / SIZE_OF_GRIDS);
	if (!manager->loadMap(mapId, gridX, gridY))
	{
		*length = 2;
		return new XYZ[2]{ start, end };
	}


	if (mapId == 0) {
		if (!manager->hasLoadedMap(mapId, zeroX[0], zeroY[0])){
			for (int i = 0; i < 558 /*size*/; i++) {
				manager->loadMap(mapId, zeroX[i], zeroY[i]);
			}
		}
	}
	else if (mapId == 1) {
		if (!manager->hasLoadedMap(mapId, oneX[0], oneY[0])) {
			for (int i = 0; i < 810 /*size*/; i++) {
				manager->loadMap(mapId, oneX[i], oneY[i]);
			}
		}
	}
	else {
		manager->loadMap(mapId, gridX + 0, gridY - 1);
		manager->loadMap(mapId, gridX + 1, gridY - 1);
		manager->loadMap(mapId, gridX + 1, gridY + 0);
		manager->loadMap(mapId, gridX + 1, gridY + 1);
		manager->loadMap(mapId, gridX + 0, gridY + 1);
		manager->loadMap(mapId, gridX - 1, gridY + 1);
		manager->loadMap(mapId, gridX - 1, gridY + 0);
		manager->loadMap(mapId, gridX - 1, gridY - 1);
	}
	// Load all surrounding map tiles
	/*
	manager->loadMap(mapId, gridX + 0, gridY - 1);
	manager->loadMap(mapId, gridX + 1, gridY - 1);
	manager->loadMap(mapId, gridX + 1, gridY + 0);
	manager->loadMap(mapId, gridX + 1, gridY + 1);
	manager->loadMap(mapId, gridX + 0, gridY + 1);
	manager->loadMap(mapId, gridX - 1, gridY + 1);
	manager->loadMap(mapId, gridX - 1, gridY + 0);
	manager->loadMap(mapId, gridX - 1, gridY - 1);
	*/



	// ToDo: Remove instanceId
	PathFinder pathFinder(mapId, 1);
	pathFinder.setUseStrightPath(!useStraightPath);

	if (!pathFinder.calculate(start.X, start.Y, start.Z, end.X, end.Y, end.Z))
	{
		*length = 2;
		return new XYZ[2]{ start, end };
	}

	PointsArray pointPath = pathFinder.getPath();

	if (pointPath.size() == 0)
	{
		*length = 2;
		return new XYZ[2]{ start, end };
	}
	*length = pointPath.size();
	XYZ* pathArr = new XYZ[pointPath.size()];

	for (unsigned int i = 0; i < pointPath.size(); i++)
	{

		pathArr[i].X = pointPath[i].x;
		pathArr[i].Y = pointPath[i].y;
		pathArr[i].Z = pointPath[i].z;
	}
	return pathArr;

}

class NavNode
{
public:
	NavNode(int zoneID, int nextZoneID, XYZ pos)
	{
		m_zoneID = zoneID;
		m_nextZoneID = nextZoneID;
		m_pos = pos;
	}

	int m_zoneID;
	int m_nextZoneID;
	XYZ m_pos;
};

NavNode* ExitNodeList[] =
{
	new NavNode(1519, 12, XYZ(-9075.45, 425.84, 93.06)),		//Stormwind City to Elwynn Forest

	new NavNode(12, 1519, XYZ(-9081.21, 421.57, 92.62)),  		//Elwynn Forest to Stormwind City
	new NavNode(12, 40, XYZ(-9800.62, 831.53, 29.03)),			//Elwynn Forest to Westfall
	new NavNode(12, 44, XYZ(-9609.13, -1727.33, 56.63)), 		//Elwynn Forest to Redridge Mountains

	new NavNode(40, 12, XYZ(-9806.91, 839.33, 29.14)),  		//Westfall to Elwynn Forest
	new NavNode(40, 10, XYZ(-10865.25, 635.26, 31.35)), 		//Westfall to Duskwood
	new NavNode(40, 1581, XYZ(-11073.68,1526.48,43.22)), 		//Westfall to Deadmines

	new NavNode(1581, 40, XYZ(-11077.34,1526.26,43.28)), 		//Deadmines to Westfall

	new NavNode(10, 33, XYZ(-11298.08, -367.99, 65.39)), 		//Duskwood to Stranglethorn Vale
	new NavNode(10, 40, XYZ(-10865.00, 631.23, 31.35)), 		//Duskwood to Westfall
	new NavNode(10, 44, XYZ(-9901.68, -1707.74, 25.92)),  		//Duskwood to Redridge Mountains
	new NavNode(10, 41, XYZ(-10465.60, -1628.12, 73.97)),  		//Duskwood to Deadwind Pass

	new NavNode(41, 10, XYZ(-10465.93, -1636.52, 74.81)), 		//Deadwind Pass to Duskwood
	new NavNode(41, 8, XYZ(-10551.51, -2332.61, 88.44)),  		//Deadwind Pass to Swamp of Sorrows

	new NavNode(8, 41, XYZ(-10551.29, -2334.27, 88.27)), 		//Swamp of Sorrows to Deadwind Pass
	new NavNode(8, 4, XYZ(-10664.28, -2983.31, 33.29)), 		//Swamp of Sorrows to Blasted Lands
	new NavNode(8, 1477, XYZ(-10491.32,-3813.58,18.70)), 		//Swamp of Sorrows to Temple of Atal Makkar

	new NavNode(1477, 8, XYZ(-10491.32,-3813.58,18.70)), 		//Temple of Atal Makkar to Swamp of Sorrows

	new NavNode(4, 8, XYZ(-10669.21, -2983.21, 34.26)), 		//Blasted Lands to Swamp of Sorrows

	new NavNode(33, 10, XYZ(-11303.57, -370.27, 65.58)), 		//Stranglethorn Vale to Duskwood

	new NavNode(44, 12, XYZ(-9610.49, -1751.36, 55.98)), 		//Redridge Mountains to Elwynn Forest
	new NavNode(44, 10, XYZ(-9892.05, -1712.44, 26.10)), 		//Redridge Mountains to Duskwood
	new NavNode(44, 46, XYZ(-8676.10, -2578.88, 132.67)),		//(Redridge Mountains to Burning Steppes

	new NavNode(46, 44, XYZ(-8637.84, -2577.26, 132.53)),  		//Burning Steppes to Redridge Mountains
	new NavNode(46, 25, XYZ(-8014.39, -1199.16, 146.23)), 		//Burning Steppes to Blackrock Mountain

	new NavNode(25, 46, XYZ(-8013.95, -1193.61, 147.72)), 		//Blackrock Mountain to Burning Steppes
	new NavNode(25, 51, XYZ(-7352.99, -1097.91, 277.84)),		//Blackrock Mountain to Searing Gorge

	new NavNode(51, 25, XYZ(-7349.18, -1095.52, 277.07)),		//Searing Gorge to Blackrock Mountain
	new NavNode(51, 3, XYZ(-6945.80, -2065.85, 282.48)),		//Searing Gorge to Badlands
	//new NavNode(51, 38, XYZ(-6035.83, -2486.57, 310.98)),		//Searing Gorge to Thelsamr <-- NEED KEY (ally)

	new NavNode(3, 51, XYZ(-6944.57, -2071.13, 282.48)), 		//Badlands to Searing Gorge
	new NavNode(3, 38, XYZ(-6002.87, -3301.63, 264.97)), 		//Badlands to Dun Morogh
	new NavNode(3, 1337,  XYZ(-6090.83,-3188.60,255.54)), 		//Badlands to Uldaman

	new NavNode(1337, 3, XYZ(-6091.29,-3183.26,256.18)), 		//Uldaman to Badlands

	new NavNode(38, 51, XYZ(-6031.35, -2493.00, 309.94)), 		//Thelsamar to Searing Gorge
	new NavNode(38, 1, XYZ(-5563.60, -2447.03, 401.24)), 		//Thelsamar to Dun Morogh
	new NavNode(38, 3, XYZ(-5996.43, -3300.10, 266.27)), 		//Thelsamar to Badlands
	new NavNode(38, 1, XYZ(-4894.50, -2357.33, 409.24)), 		//Thelsamar to Dun Morogh
	new NavNode(38, 11, XYZ(-4492.62, -2690.74, 266.96)), 		//Thelsamar to Wetlands

	new NavNode(1537, 1, XYZ(-5020.90, -835.45, 496.98)),  		//Ironforge to Dun Morogh

	new NavNode(1, 38, XYZ(-5561.87, -2444.92, 400.36)),  		//Dun Morogh to Thelsamar
	new NavNode(1, 38, XYZ(-4896.25, -2354.23, 408.62)), 		//Dun Morogh to Thelsamar
	new NavNode(1, 1537, XYZ(-5023.68, -830.81, 495.32)),		//Dun Morogh to Ironforge

	new NavNode(11, 38, XYZ(-4485.75, -2690.33, 266.05)),  		//Wetlands to Thelsamar
	new NavNode(11, 45, XYZ(-2368.41, -2503.12, 88.34)), 		//Wetlands to Arathi Highlands

	new NavNode(45, 11, XYZ(-2365.17, -2503.07, 88.34)),  		//Arathi Highlands to Wetlands
	new NavNode(45, 267, XYZ(-834.47, -1579.77, 54.31)), 		//Arathi Highlands to Southshore

	new NavNode(267, 45, XYZ(-832.99, -1577.83, 54.23)), 		//Southshore to Arathi Highlands
	new NavNode(267, 47, XYZ(-136.11, -1810.54, 123.28)), 		//Southshore to The Hinterlands
	new NavNode(267, 36, XYZ(198.90, -673.12, 106.27)), 		//Southshore to Alterac Mountains
	new NavNode(267, 36, XYZ(-170.09, 99.90, 62.80)), 			//Southshore to Alterac Mountains
	new NavNode(267, 130, XYZ(-586.16, 630.87, 84.50)), 		//Southshore to Silverpine Forest

	new NavNode(130, 267, XYZ(-582.45, 637.28, 85.23)), 		//Silverpine Forest to Southshore
	new NavNode(130, 85, XYZ(1531.21, 601.35, 46.32)), 			//Silverpine Forest to Tirisfal Glades

	new NavNode(85, 130, XYZ(1536.19, 598.80, 45.67)), 			//Tirisfal Glades to Silverpine Forest
	new NavNode(85, 1497, XYZ(1879.12, 238.24, 59.68)), 		//Tirisfal Glades to Undercity
	new NavNode(85, 28, XYZ(1716.93, -797.62, 57.29)), 			//Tirisfal Glades to Western Plaguelands
	new NavNode(85, 796, XYZ(2819.11,-700.28,137.10)), 			//Tirisfal Glades to Scarlet Monastery

	new NavNode(796, 85, XYZ(2822.28,-698.92,137.70)), 			//Scarlet Monastery to Tirisfal Glades

	new NavNode(28, 85, XYZ(1717.10, -800.25, 57.44)), 			//Western Plaguelands to Tirisfal Glades
	new NavNode(28, 36, XYZ(869.28, -1478.27, 64.72)), 			//Western Plaguelands to Alterac Mountains
	new NavNode(28, 139, XYZ(1925.27, -2597.22, 61.77)), 		//Western Plaguelands to Eastern Plaguelands
	new NavNode(28, 2057, XYZ(1244.17,-2591.18,90.19)), 		//Western Plaguelands to Scholomance

	new NavNode(2057, 28, XYZ(1247.66,-2589.43,91.42)), 		//Scholomance to Western Plaguelands

	new NavNode(139, 28, XYZ(1925.20, -2600.74, 62.24)), 		//Eastern Plaguelands to Western Plaguelands
	new NavNode(139, 2017, XYZ(3279.77,-3380.57,142.00)),		//(Eastern Plaguelands to Stratholme)

	new NavNode(2017, 139, XYZ(3283.48,-3380.22,141.78)),		//(Stratholme to Eastern Plaguelands)

	new NavNode(1497, 85, XYZ(1872.94, 238.50, 62.28)), 		//Undercity to Tirisfal Glades

	new NavNode(36, 267, XYZ(201.30, -671.17, 107.07)), 		//Alterac Mountains to Southshore
	new NavNode(36, 267, XYZ(-165.58, 105.92, 62.47)), 			//Alterac Mountains to Southshore
	new NavNode(36, 28, XYZ(863.87, -1478.56, 65.43)), 			//Alterac Mountains to Western Plaguelands

	new NavNode(47, 267, XYZ(-131.76, -1813.79, 124.48)),  		//The Hinterlands to Southshore

	//
	new NavNode(440, 1941, XYZ(-8174.92, -4730.57, 31.95)),		//Tanaris to Caverns of Time
	new NavNode(1941, 440, XYZ(-8174.56, -4740.33, 33.83)),		//Caverns of Time to Tanaris

	new NavNode(490, 440, XYZ(-8231.25, -2082.57, -100.87)),	//Un'Goro Crater to Tanaris
	new NavNode(440, 490, XYZ(-8244.02, -2075.10, -94.53)),		//Tanaris to Un'Goro Crater

	new NavNode(440, 400, XYZ(-6835.47, -3756.47, 27.90)),		//Tanaris to Thousand Needles
	new NavNode(400, 440, XYZ(-6830.29, -3755.37, 25.32)),		//Thousand Needles to Tanaris

	new NavNode(400, 357, XYZ(-4282.64, -771.42, -48.47)),		//Thousand Needles to Feralas
	new NavNode(357, 400, XYZ(-4277.26, -756.11, -41.81)),		//Feralas to Thousand Needles

	new NavNode(357, 2557, XYZ(-4460.85, 1330.88, 125.44)),		//Feralas to Dire Maul
	new NavNode(2557, 357, XYZ(-4454.81, 1330.75, 126.00)),		//Dire Maul to Feralas

	new NavNode(357, 405, XYZ(-2468.82, 2318.44, 117.19)),		//Feralas to Desolace
	new NavNode(405, 357, XYZ(-2463.72, 2324.10, 117.06)),		//Desolace to Feralas

	new NavNode(405, 2100, XYZ(-1420.95, 2889.15, 132.89)),		//Desolace to Maraudon
	new NavNode(2100, 405, XYZ(-1421.61, 2896.22, 134.23)),		//Maraudon to Desolace

	new NavNode(405, 406, XYZ(364.10, 1801.91, 51.83)),			//Desolace to Stonetalon Mountains
	new NavNode(406, 405, XYZ(368.53, 1800.15, 49.98)),			//Stonetalon Mountains to Desolace

	new NavNode(406, 17, XYZ(-248.45, -827.46, 8.28)),			//Stonetalon Mountains to The Barrens
	new NavNode(17, 406, XYZ(-255.21, -837.33, 8.31)),			//The Barrens to Stonetalon Mountains

	new NavNode(17, 331, XYZ(1330.39, -2256.26, 91.05)),        //The Barrens to Ashenvale
	new NavNode(331, 17, XYZ(1343.17, -2256.89, 90.22)),		//Ashenvale to The Barrens

	new NavNode(331, 148, XYZ(4130.68, 36.59, 21.59)),			//Ashenvale to Darkshore
	new NavNode(148, 331, XYZ(4135.33, 39.07, 22.51)),			//Darkshore to Ashenvale

	new NavNode(331, 361, XYZ(3563.26, -1514.20, 168.36)),      //Ashenvale to Felwood
	new NavNode(361, 331, XYZ(3569.16, -1514.55, 168.69)),      //Felwood to Ashenvale

	new NavNode(361, 618, XYZ(6905.95, -2295.13, 590.19)),      //Felwood to Winterspring
	new NavNode(618, 361, XYZ(6903.23, -2298.72, 589.10)),      //Winterspring to Felwood

	new NavNode(16, 331, XYZ(2808.95, -3801.96, 86.87)),        //Azshara to Ashenvale
	new NavNode(331, 16, XYZ(2811.66, -3798.19, 87.48)),		//Ashenvale to Azshara

	new NavNode(17, 14, XYZ(316.05, -3764.44, 37.98)),			//The Barrens to Durotar
	new NavNode(14, 17, XYZ(316.28, -3767.25, 37.73)),			//Durotar to The Barrens

	new NavNode(14, 1637, XYZ(1381.97, -4369.37, 26.02)),       //Durotar to Orgrimmar
	new NavNode(1637, 14, XYZ(1387.07, -4368.68, 27.02)),		//Orgrimmar to Durotar

	new NavNode(1637, 17, XYZ(1649.09, -3842.04, 52.70)),		//Orgrimmar to The Barrens
	new NavNode(17, 1637, XYZ(1646.36, -3839.94, 51.17)),       //The Barrens to Orgrimmar

	new NavNode(17, 15, XYZ(-3684.90, -2458.86, 80.47)),		//The Barrens to Dustwallow Marsh
	new NavNode(15, 17, XYZ(-3684.71, -2474.22, 78.03)),		//Dustwallow Marsh to The Barrens

	new NavNode(15, 2159, XYZ(-4685.43, -3711.52, 46.93)),      //Dustwallow Marsh to Onyxia's Lair
	new NavNode(2159, 15, XYZ(-4687.44, -3713.10, 47.57)),		//Onyxia's Lair to Dustwallow Marsh

	new NavNode(490, 1377, XYZ(-6224.03, -467.24, -62.10)),     //Un'Goro Crater to Silithus
	new NavNode(1377, 490, XYZ(-6230.45, -458.60, -55.85)),     //Silithus to Un'Goro Crater

	new NavNode(1377, 3478, XYZ(-8198.46, 1517.24, 4.37)),      //Silithus to Gates of Ahn'Qiraj
	new NavNode(3478, 1377, XYZ(-8202.66, 1540.76, 4.35)),      //Gates of Ahn'Qiraj to Silithus

	new NavNode(361, 493, XYZ(7373.08, -2206.33, 535.89)),      //Felwood to Moonglade
	new NavNode(493, 361, XYZ(7375.47, -2206.15, 535.41)),      //Moonglade to Felwood

	new NavNode(331, 406, XYZ(1940.29, -739.35, 114.42)),       //Ashenvale to Stonetalon Mountains
	new NavNode(406, 331, XYZ(1934.67, -736.56, 114.45)),       //Stonetalon Mountains to Ashenvale

	new NavNode(17, 215, XYZ(-2345.76, -1535.61, 52.83)),       //The Barrens to Mulgore
	new NavNode(215, 17, XYZ(-2345.42, -1531.41, 51.64)),		//Mulgore to The Barrens

	new NavNode(1638, 215, XYZ(-1399.31, 120.36, 17.62)),       //Thunder Bluff to Mulgore
	new NavNode(215, 1638, XYZ(-1401.92, 117.14, 17.02)),       //Mulgore to Thunder Bluff

	new NavNode(17, 722, XYZ(-4678.88, -2367.88, 86.28)),       //The Barrens to Razorfen Down
	new NavNode(722, 17, XYZ(-4670.66, -2370.79, 85.50)),       //Razorfen Downs to The Barrens

	new NavNode(17, 400, XYZ(-4829.89, -2213.80, 83.93)),       //The Barrens to Thousand Needles
	new NavNode(400, 17, XYZ(-4834.88, -2208.40, 83.60)),       //Thousand Needles to The Barrens

	new NavNode(17, 491, XYZ(-4474.51, -1681.95, 80.43)),       //The Barrens to Razorfen Kraul
	new NavNode(491, 17, XYZ(-4472.47, -1679.75, 80.82)),       //Razorfen Kraul to The Barrens
};

bool Navigation::GetCrossZonePath(int startZoneID, int endZoneID, std::vector<XYZ>* posList)
{
	if(startZoneID == endZoneID)
	{
		return false;
	}

	int num = sizeof(ExitNodeList) / sizeof(PVOID);
	vector<vector<NavNode*>*>* vList = new vector<vector<NavNode*>*>;
	vector<vector<NavNode*>*>* vListTemp = new vector<vector<NavNode*>*>;

	for (int i = 0; i < num; i++)
	{
		if (ExitNodeList[i]->m_zoneID == startZoneID)
		{
			if (ExitNodeList[i]->m_nextZoneID == endZoneID)
			{
				for (int x = 0; x < num; x++)
				{
					if (ExitNodeList[x]->m_zoneID == ExitNodeList[i]->m_nextZoneID &&
						ExitNodeList[x]->m_nextZoneID == ExitNodeList[i]->m_zoneID)
					{
						posList->push_back(ExitNodeList[x]->m_pos);
						break;
					}
				}

				vList->clear();
				delete vList;

				vListTemp->clear();
				delete vListTemp;

				return true;
			}

			vector<NavNode*>* zoneList = new vector<NavNode*>;
			zoneList->push_back(ExitNodeList[i]);

			vList->push_back(zoneList);

			//printf("Exit Node %d->%d\n", list[i]->m_zoneID, list[i]->m_nextZoneID);
		}
	}

	while (vList && vList->size())
	{
		for (int i = 0; i < (int)vList->size(); i++)
		{
			for (int x = 0; x < num; x++)
			{
				vector<NavNode*>* zoneList = vList->at(i);
				NavNode* node = zoneList->at(zoneList->size() - 1);
				if (ExitNodeList[x]->m_zoneID == node->m_nextZoneID && ExitNodeList[x]->m_nextZoneID == node->m_zoneID)
				{
					for (int j = 0; j < num; j++)
					{
						if (ExitNodeList[j]->m_zoneID == ExitNodeList[x]->m_zoneID && ExitNodeList[j]->m_nextZoneID != node->m_zoneID)
						{
							bool found = false;
							for (int q = 0; q < (int)zoneList->size(); q++)
							{
								NavNode* q_ = zoneList->at(q);
								if (ExitNodeList[j]->m_nextZoneID == q_->m_zoneID)
								{
									found = true;
									break;
								}
							}

							if (found)
							{
								continue;
							}

							vector<NavNode*>* NewZoneList = new vector<NavNode*>;

							//printf("%d] ", ii);

							for (int q = 0; q < (int)zoneList->size(); q++)
							{
								NewZoneList->push_back(zoneList->at(q));

								//printf("%d ", zoneList->at(q)->m_nextZoneID);
							}
							NewZoneList->push_back(ExitNodeList[j]);

							//printf("%d\n", list[j]->m_nextZoneID);

							vListTemp->push_back(NewZoneList);

							if (ExitNodeList[j]->m_nextZoneID == endZoneID)
							{
								for (int q = 0; q < (int)NewZoneList->size(); q++)
								{
									posList->push_back(NewZoneList->at(q)->m_pos);
								}

								//for (int q = 0; q < NewZoneList->size(); q++)
								//{
								//	printf("%s, ", WoWClass::GetZoneText(NewZoneList->at(q)->m_nextZoneID));
								//}
								//printf("\n");

								vList->clear();
								delete vList;

								vListTemp->clear();
								delete vListTemp;

								return true;
							}
						}
					}
				}
			}
		}

		vList->clear();
		delete vList;

		vList = vListTemp;
		vListTemp = new vector<vector<NavNode*>*>;
	}

	//printf("[ERROR] COULD NOT FIND, (contact admins with info [%d -> %d] )!!\n", startZoneID, endZoneID);

	return false;
}

XYZ* Navigation::CalculatePath(unsigned int mapId, XYZ start, int startZoneID, XYZ end, int endZoneID, bool useStraightPath, int* length)
{
	vector<XYZ>* path = new vector<XYZ>;
	vector<XYZ> posList;
	bool found = GetCrossZonePath(startZoneID, endZoneID, &posList);

	if (found)
	{
		int loopSize = posList.size();

		int _length = 0;
		XYZ* temp = NULL;

		temp = CalculatePath(mapId, start, posList[0], true, &_length);
		for (int i = 0; i < _length; i++)
		{
			path->push_back(temp[i]);
		}
		delete temp;

		for(int x = 0; x < loopSize - 1; x++)
		{
			temp = CalculatePath(mapId, posList[x], posList[x + 1], true, &_length);
			for (int i = 0; i < _length; i++)
			{
				path->push_back(temp[i]);
			}
			delete temp;
		}

		temp = CalculatePath(mapId, posList[loopSize-1], end, true, &_length);
		for (int i = 0; i < _length; i++)
		{
			path->push_back(temp[i]);
		}
		delete temp;

		XYZ* pathArr = new XYZ[(*path).size()];
		int length = (*path).size();

		for (int i = 0; i < length; i++)
		{
			pathArr[i].X = (*path)[i].X;
			pathArr[i].Y = (*path)[i].Y;
			pathArr[i].Z = (*path)[i].Z;
		}

		delete path;

		return pathArr;
	}
	else
	{
		XYZ* temp = NULL;

		temp = CalculatePath(mapId, start, end, true, length);

		return temp;
	}

	return NULL;
}


void Navigation::GetPath(XYZ* path, int length)
{
	for (int i = 0; i < length; i++)
	{
		path[i] = currentPath[i];
	}
	delete[] currentPath;
}
