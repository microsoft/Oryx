This project contains tests which validate the `build image` on the machine where the image is built.  

## How to run these tests?
Since this is a .NET Core test project, you can use Visual Studio or CLI to run these tests.  
But since these tests depend on the built image on the machine, you need to remember to always build the image first so  
that you are testing against the latest built image.

An easy way to both build the image and run tests would be to follow this flow:  
- Run the `build/testBuildImages.sh` script. This script first builds the `build image` and then runs tests.  
- If you see any test failures and want to debug you can Visual Studio in that case and needn't worry about testing  
  against an older `build image`.
