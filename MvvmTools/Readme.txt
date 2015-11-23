The MvvmTools.csproj project is the startup project.

Some settings are not included in the project file, so they are not source controlled.  

1) To debug, you'll need to put this in MvvmTools | Project Properties | Debug

   Start External Program: C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe
   (might vary on your PC, and '14.0' will be greater if your VS is > VS2015)

   Command line arguments: /rootsuffix Exp

