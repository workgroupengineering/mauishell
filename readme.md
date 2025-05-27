# Shiny MAUI

Work In Progress

## Shell

Inspired entirely by Prism - there is nothing here that is an original idea.  I wanted to build this myself and
build it around Shell so I could understand the inner workings of Shell.

### Features/Roadmap
* [x] Registration
* [ ] ServiceScope per Page
* [ ] Shell XAML Integration(?)
* [ ] Navigation Service
  * [ ] Events…?
  * [ ] To Commands
  * [x] Goto(string uri, args)
  * [ ] GotoByViewModel
  * [ ] GoBack(args)
  * [ ] Pop To Root
  * [ ] Set Root
  * [ ] Modals/Tabs
* [x] Auto ViewModel Push on to page
* [ ] Source Generation
  * [ ] UseShinyShellGenerated
* [ ] ViewModel lifecycle
  * [ ] Initialize?
  * [x] OnAppearing/OnDisappearing
  * [x] Navigation Confirmation
  * [x] Disposable/Destroy
  * [ ] OnNavigatedTo(args) and direction of navigation(?)
  * [ ] OnNavigatedFrom() - direction pop, uri from where?

### Setup
* Install Nuget
* MauiProgram - UseShinyShell

### Navigator
Shiny.INavigator 

### ViewModel Lifecycle

* IDisposable - to permanently destroy any hooks
* IPageLifecycleAware
* INavigationConfirmation
* Receive arguments on your ViewModel by implementing - IQueryAttributable

## Goals
* Must be AOT compliant