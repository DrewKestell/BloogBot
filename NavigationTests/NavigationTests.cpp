#include "..\Navigation\Navigation.h"
#include "CppUnitTest.h"
#include <windows.h>
#include <string>

using namespace Microsoft::VisualStudio::CppUnitTestFramework;
using namespace std;

namespace NavigationTests 
{
    EXTERN_C IMAGE_DOS_HEADER __ImageBase;

    typedef XYZ*(__cdecl *f_calculatePath)(unsigned int, XYZ, XYZ, bool, int*);
    
    TEST_CLASS(NavigationTests)
    {
    public:
        TEST_METHOD(CorrectlyBuildsNavigationPath)
        { 
            const auto path = GetNavigationDllPath();
            HINSTANCE hGetProcIDDLL = LoadLibrary(path.c_str());

            if (hGetProcIDDLL == nullptr)
            {
                DWORD error = GetLastError();
                return;
            }

            f_calculatePath calculatePathFunc = (f_calculatePath)GetProcAddress(hGetProcIDDLL, "CalculatePath");

            int length = 0;
            XYZ start = XYZ{ -614.7, -4335.4, 40.4 };
            XYZ end = XYZ{ -590.2, -4206.1, 38.7 };
            XYZ* navPath = calculatePathFunc(1, start, end, false, &length);

            Assert::IsTrue(length > 2);
        }

        TEST_METHOD(UnreachableDestination)
        {
            const auto path = GetNavigationDllPath();
            HINSTANCE hGetProcIDDLL = LoadLibrary(path.c_str());

            if (hGetProcIDDLL == nullptr)
            {
                DWORD error = GetLastError();
                return;
            }

            f_calculatePath calculatePathFunc = (f_calculatePath)GetProcAddress(hGetProcIDDLL, "CalculatePath");

            int length = 0;
            XYZ start = XYZ{ -614.7, -4335.4, 40.4 };
            XYZ end = XYZ{ -623.1, -4357.4, 41.1 };
            XYZ* navPath = calculatePathFunc(1, start, end, false, &length);

            Assert::IsTrue(length > 2);
        }
    private:
        std::string GetNavigationDllPath()
        {
            WCHAR dllPath[MAX_PATH] = { 0 };
            GetModuleFileNameW((HINSTANCE)&__ImageBase, dllPath, _countof(dllPath));
            wstring ws(dllPath);
            string path(ws.begin(), ws.end());
            path = path.substr(0, path.find_last_of("\\"));
            path = path.append("\\Navigation.dll");

            return path;
        }
    };
}