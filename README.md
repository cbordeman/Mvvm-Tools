# Mvvm-Tools

A Visual Studio extension to help move between view/viewmodel and other MVVM XAML tasks, for any project type.  The latest versions of MVVM Tools support VS 2015 only.  It is available in the Tools > Extension Manager and at: https://visualstudiogallery.msdn.microsoft.com/978ed555-9f0d-44a2-884c-9084844ac469

MVVM TOOLS IS NOT YET ANOTHER MVVM FRAMEWORK.  IT IS FRAMEWORK AGNOSTIC.

Copyright 2015 Chris Bordeman

LICENSE

This project is released under the terms of the GNU Lesser General Public License.

CURRENT FEATURES

1.  Framework agnostic.  MVVM Tools is NOT an MVVM framework.  You can use any MVVM framework, such as Prism, Caliburn, MvvmCross, or others.
2.  Automatically switch between a view and its viewmodel by pressing Ctrl+E, Ctrl+V (configurable). Think 'Ctrl+Editor+View/Viewmodel.

FUTURE FEATURES

1.  I currently use an algorithm which scans all .cs and .vb files for matching types, but that won't always work. So I want to add a way for the developer to customize the process of locating views and viewmodels.
2.  Ability to scaffold a view and view model using a set  of online templates that all can contribute to. 
3.  The ability to extract a view model or view from the view or view model automatically.
