# This is a comment.  The '#' must appear in column 0.
#
# This file can contain multiple templates, or multiple files may be used. Each time the
# [[Template]] section is encountered, a new template is started.
#
# The Template section must come first, followed by one or more Field sections, then the 
# rest of the sections, in any order.
# [[Template]]
# [[Field]] (optional / multiple allowed)
# [[View]]                  | View = XAML, which is identical wither C# or VB.
# [[ViewModel-(Language)]]  | (Language) is either CSharp or VisualBasic.  All four permutations
# [[CodeBehind-(Language)]] | may speficied in one template, or just two for C# or VB.
#
# In property values (after ':'), use pipe for line continuation.  Comment lines aren't 
# allowed until the value ends.

# [[Template]] Section
# ====================
#
# Platforms: 'All' or a comma separated list of: WPF, Silverlight, Xamarin, or WinRT.  
#            For Universal apps, use WinRT.
# Framework: 'None' or the requisite MVVM framework such as Prism, MVVM Cross, MVVM Light, 
#            or Caliburn.Micro.
# Form Factors: 'All' or a comma separated list of: Phone, Tablet, or Desktop.
# Language: C# or VB.
# Description: Required.  It's encouraged to provide details about the expected structure
#              of the project(s), and urls to any relevant sources and prerequesites.
# Tags: Optional, a comma separated list of tags for searching.

[[Template]]
Platforms: WPF
Framework: None
Form Factors: All
Name: Simple Window
Description: This is a single line comment.
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
# If ComboBox or ComboBoxOpen, Choices must be set (pipe delimited list).
#
# If CheckBox, the Default property must be True or False.

[[Field]]
Name: MyStringField
Type: TextBox
Default: Whatever |
         Whatever 2 |
		 Whatever 3
Prompt: Prompt 2
Description: This is a multi-line |
             description.  This is line two |
             and this is on line three.  You should avoid continuing the line unless you want a hard break.  Your text will be automatically wrapped in any case.

[[Field]]
Name: MyCheckBoxField
Type: CheckBox
Default: True
Prompt: This is a check box

[[Field]]
Name: MyComboBoxField
# Type 'ComboBox' requires the Choices property.  Type 'ComboBoxOpen' avoids
# checking that Default is present in the list of Choices.
Type: ComboBoxOpen
Default: Item X
Prompt: Prompt 4
Description: This is a multi-line |
             description.  This is line two |
             and this is on line three.
Choices: Item 1|Item 2|Item 3|Item 4|Note that choices|can't be multi-line.


# T4 Sections
# ===========
#
# The [[ViewModel-(Language)]], [[View]], and [[CodeBehind-(Language)]] 
# section each contain a T4 template.  They are passed predefined field 
# values, and your fields defined in the [[Field]] section(s).
#
# '#' style Comments are not allowed from here on, until another
# [[Template]] is started.  The '#' character would muck up the T4.

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

[[CodeBehind-CSharp]]
// <copyright file="$classname$" company="My Company Name">
// Copyright (c) 2015 All Rights Reserved
// <author>Chris Bordeman</author>
// </copyright>

[[ViewModel-CSharp]]
// <copyright file="$classname$" company="My Company Name">
// Copyright (c) 2015 All Rights Reserved
// <author>Chris Bordeman</author>
// </copyright>
