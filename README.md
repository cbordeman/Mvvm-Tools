# Mvvm-Tools
A Visual Studio extension to help move between view/viewmodel and other MVVM XAML tasks, for any project type.

By Chris Bordeman

CURRENT FEATURES

1.  Automatically switch between a view and its viewmodel by pressing Ctrl+E, Ctrl+V (configurable).  Think 'Ctrl+Editor/View/Viewmodel.'

GOALS

A.  Currently supports VS2015, but doesn't use Roslyn so should be able to port back to 2010, 2012, and 2013.
B.  I currently use a pretty strong algorithm which scans all .cs and .vb files for matching types, but that won't work for all projects.  So, I want to add a way for the user to take a file's list of classes and return the view or viewmodel type.  Perhaps in a small class implementing an interface...
C.  Ability to add a viewmodel from the view automatically.
D.  More features.
