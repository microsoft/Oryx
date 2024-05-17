The `all` folder here represents a typical .NET Core installation directory structure where all SDK versions are put under the `sdk` folder and the `dotnet.exe` muxer chooses the appropriate SDK for an app.

The `sdks` folder structure here separates different SDK versions into their own separate folders, so a dotnet muxer does not know about existence of other SDK versions.