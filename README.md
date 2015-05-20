# Mvvm-Tools

A Visual Studio extension to help move between view/viewmodel and other MVVM XAML tasks, for any project type.  Supports VS2010, VS2012, VS2013, and VS2015.  Available in the VS Extension manager and at: https://visualstudiogallery.msdn.microsoft.com/978ed555-9f0d-44a2-884c-9084844ac469

Copyright 2015 Chris Bordeman

LICENSE

This project is released under the terms of the GNU Lesser General Public License.

CURRENT FEATURES

1.  Automatically switch between a view and its viewmodel by pressing Ctrl+E, Ctrl+V (configurable). Think 'Ctrl+Editor+View/Viewmodel.

GOALS

1.  I currently use a pretty good algorithm which scans all .cs and .vb files for matching types, but that won't work for all projects. So I want to add a way for the developer to customize the process of locating views and viewmodels.
2.  Want the ability to add a viewmodel from the view automatically, and link it up to the view using Prism's ViewModelLocator or another method as requested.
3.  More...
