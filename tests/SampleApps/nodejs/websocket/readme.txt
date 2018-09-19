AntaresCmd.exe CreateWebSite jlawsub jlawspace %sitename% /computeMode:Dedicated /serverFarm:LinuxServerFarm

AntaresCmd.exe UpdateWebSiteConfig jlawsub jlawspace %sitename%  /publishingPassword:iis6dfu /nodeVersion:4.4.7 /appCommandLine:index.js

>> git push should install express and socket.io.
