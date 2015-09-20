# This is a comment.  Comments are allowed anywhere EXCEPT inside the [[ViewModel]], 
# [[View]], and [[CodeBehind]] sections, which contain T4 templates.  The '#' must be
# at column 0.  
#
# This file can contain multiple templates, or multiple files may be used. As soon as the 
# [[Template]] section is encountered, a new template is started.
#
# Section must appear in order, and all but [[Field]] must be present:
# [[Template]]
# [[Field]] (optional / multiple allowed)
# [[ViewModel]]
# [[View]]
# [[CodeBehind]]
#
# In property values (after ':'), use pipe for line continuation.  Comment lines aren't 
# allowed until the value ends.

# [[Template]] Section
# ====================
#
# Platforms: 'Any' or a comma delimited combination of: WPF, Silverlight, Xamarin, or 
#     WinRT.  For Universal apps, use WinRT.
# Framework: 'None' or the requisite MVVM framework such as Prism, MVVM Light, or Caliburn.
# Name + Language must be unique.
# Language: C# or VB.
# Description: Required.
# Tags: Optional, a comma separated list of tags for searching.

[[Template]]
Platforms: WPF
Framework: None
Name: Simple Window
Description: This is a multi-line |
             description.  This is line two |
             and this is on line three.
Language: C#
Tags: Simple,Window

# [[Field]] Section(s)
# ====================
#
# Zero or more [[Field]] sections are allowed.  They represent
# data for which the user is prompted before scaffolding.  They are 
# passed into the T4 sections so you can perform logic based on user
# preferences.
#
# Name: For fields, must be a valid C# identifier.
# Type: Required.  One of: TextBox, ComboBox, or CheckBox.
# Description: Optional for fields.
#
# If TextBox, set MultiLine: True to allow multiple lines.
#
# If ComboBox, Choices must be set (pipe delimited list).  Set Open: True
# to allow free-form text.
#
# If CheckBox, the Default property must specified, and must be 
# set to True or False.

[[Field]]
Name: MyStringField
Type: TextBox
MultiLine: True
Default: Whatever |
         Whatever 2 |
		 Whatever 3
Prompt: Prompt 2
Description: This is a multi-line |
             description.  This is line two |
             and this is on line three.

[[Field]]
Name: MyCheckBoxField
Type: CheckBox
Default: true
Prompt: This is a check box

[[Field]]
Name: MyComboBoxField
Type: ComboBox
Default: Item 2
Prompt: Prompt 4
Description: This is a multi-line |
             description.  This is line two |
             and this is on line three.
Choices: Item 1|Item 2|Item 3|Item 4
Open: false

# The [[ViewModel]], [[View]], and [[CodeBehind]] section each contain a
# T4 template.  They are passed predefined field values, and your fields
# defined in the [[Field]] section(s).
#
# '#' style Comments are not allowed from here on, until another
# [[Template]] is started.

[[ViewModel]]
// <copyright file="$classname$" company="My Company Name">
// Copyright (c) 2015 All Rights Reserved
// <author>Chris Bordeman</author>
// </copyright>

[[View]]
<#@ template debug="false" hostspecific="false" language="C#" #>
<#@ output extension=".xaml" #>
<#@ assembly name="System.Core" #>
<#@ import namespace="System.Linq" #>
<#@ import namespace="System.Text" #>
<#@ import namespace="System.Collections.Generic" #>
<#@ property processor="PropertyProcessor" name="ViewFullName" #>
<#@ property processor="PropertyProcessor" name="ViewNamespace" #>
<#@ property processor="PropertyProcessor" name="ViewModelName" #>
<#@ property processor="PropertyProcessor" name="Title" #>
<Window x:Class="$(ViewFullName)"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:local="clr-namespace:$(ViewNamespace)"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:vm="clr-namespace:$(ViewModelNamespace)"
        Title="$(Title)"
        Padding="10"
        d:DataContext="{d:DesignInstance vm:$(ViewModelName),
                                         d:IsDesignTimeCreatable=False}"
        d:Height="300"
        d:Width="300"
        mc:Ignorable="d">
    <Grid>
        <Grid.ColumnDefinitions>
            <!--  Labels  -->
            <ColumnDefinition Width="Auto" />
            <!--  Fields  -->
            <ColumnDefinition />
            <!--  Field Buttons  -->
            <ColumnDefinition Width="Auto" />
        </Grid.ColumnDefinitions>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />

            <!--  Bottom Button Row  -->
            <RowDefinition />
        </Grid.RowDefinitions>
		


        <!--  Bottom Button Row  -->
        <StackPanel Margin="0,10,0,0"
                    HorizontalAlignment="Right"
                    Orientation="Horizontal">
            <Button Command="{Binding }" />
            <Button Margin="5,0,0,0" 
			        Command="{Binding }" />
        </StackPanel>
    </Grid>
</Window>

[[CodeBehind]]
// <copyright file="$classname$" company="My Company Name">
// Copyright (c) 2015 All Rights Reserved
// <author>Chris Bordeman</author>
// </copyright>
