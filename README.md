# Mvvm-Tools

A Visual Studio extension to help move between view/viewmodel and other MVVM XAML tasks, for any project type.  The latest versions of MVVM Tools support VS 2015 only.  It is available in the Tools > Extension Manager and at: https://visualstudiogallery.msdn.microsoft.com/978ed555-9f0d-44a2-884c-9084844ac469

To create an account and submit your own MVVM view+view model T4 templates, please visit http://www.mvvmtools.net.

Copyright 2015 Chris Bordeman

LICENSE

This project is released under the terms of the latest version of the Apache License, at http://www.apache.org/licenses/.

CURRENT FEATURES

1.  Framework agnostic.  MVVM Tools is NOT an MVVM framework.  You can use any MVVM framework, such as Prism, Caliburn, MvvmCross, or others.
2.  Automatically switch between a view and its viewmodel by pressing Ctrl+E, Ctrl+V (configurable). Think 'Ctrl+Editor+View/Viewmodel.
3.  Options to specify, based on the selected project, the location of views and view models.  This may be separate projects and viewmodels.  View suffixes are configurable, and view model suffixes can be set based on the selected project.
4.  Options are set on the solution and can be changed per-project as to facilitate different MVVM paradigms per application module.
5.  Scaffold new view and view model based on a set of MVVM (view+vm) templates maintained by the community (see http://www.mvvmtools.net).
6.  Teams can store a set of MVVM templates in a folder of their choosing, shared on the network or source controlled.

FUTURE FEATURES

1.  The ability to extract a view model or view from the view or view model automatically.  This is great for existing code you want to convert to MVVM quickly.