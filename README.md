# CompanionMR

Windows Mixed Reality application that showcases the [Companion](https://github.com/LibCompanion/libCompanion/) object recognition framework on a HoloLens device.

Demo video:

[![Video: Companion on Microsoft HoloLens](http://img.youtube.com/vi/d8Utp1bLp0Y/0.jpg)](https://www.youtube.com/watch?v=d8Utp1bLp0Y)

Some assets are not included into the source code because of copyright restrictions. However, you can sideload [the demo app](/releases/latest) onto your device for an unrestricted experience. Please make sure to use the [CompanionAPI](https://github.com/LibCompanion/CompanionAPI) Web Service to provide AR information for the artworks when the application is started for the first time. Please refer to the [official documentation](https://developer.microsoft.com/en-us/windows/mixed-reality/using_the_windows_device_portal#apps) by Microsoft to learn how to use the Device Portal for sideloading.

## Dependencies

* Companion for WinRT/UWP: [CompanionWinRT](https://github.com/LibCompanion/CompanionWinRT)
* OpenCV 3 for WinRT/UWP: [opencvWinRT](https://github.com/LibCompanion/opencvWinRT/)
* A RESTful web service that manages the data library for CompanionMR: [CompanionAPI](https://github.com/LibCompanion/CompanionAPI)
* HoloToolkit: [MixedRealityToolkit-Unity](https://github.com/Microsoft/MixedRealityToolkit-Unity)
> All HoloToolkit code alterations are described in [HoloToolkit_Diff.md](Assets/HoloToolkit/HoloToolkit_Diff.md).

## Building CompanionMR

To provide AR information for the artworks, CompanionMR needs to have access to the [CompanionAPI](https://github.com/LibCompanion/CompanionAPI) web service when the application is started for the first time. Before building the project the host IP has to be altered accordingly to your network settings. It can be changed as a parameter of the "HoloLensCamera" GameObject:

![Web Service URL](https://libcompanion.github.io/CompanionDoc/images/example/webservice.jpg "Web Service URL")

1. Simply use Unity to build CompanionMR. The currently used version is `2017.1.2p1`. Make sure to switch to UWP as the target platform and to activate the `Unity C# Projects` option.
3. Open the generated project file `CompanionMR.sln` in Visual Studio.
4. Please refer to the [official documentation](https://developer.microsoft.com/en-us/windows/mixed-reality/using_visual_studio) by Microsoft to learn how to deploy the app on your HoloLens device.

## License

```
CompanionMR is a Windows Mixed Reality example project for Companion.
Copyright (C) 2018 Dimitri Kotlovsky, Andreas Sekulski

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
```

### Additional licenses

Please refer to the [LICENSE.md](/LICENSE.md) file for additional license information applicable to the HoloToolkit source code and the OpenCV binary files.
