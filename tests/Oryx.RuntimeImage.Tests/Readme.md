This project contains tests which validate the runtime images on the machine where the images are built.  

## How to run these tests?
Since this is a .NET Core test project, you can use Visual Studio or CLI to run these tests.  
But since these tests depend on the built images on the machine, you need to remember to always build the images first  
so that you are testing against the latest built images.

An easy way to both build the images and run tests would be to follow this flow:  
- Run the `build/test-runtimeimages.sh` script. This script first builds the runtime images and then runs tests.  
- If you see any test failures and want to debug you can Visual Studio in that case and needn't worry about testing  
  against an older images.
