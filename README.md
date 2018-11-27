# Olive Microservice Explorer

A typical solution may have tens of microservice projects. During development, you usually will have a root folder for the entire solution, with a sub-folder per microservice project inside it. 

Unlike traditional model where a large solution can still be a single Visual Studio solution (with multiple projects inside it), in Microservices, each microservice project will be an independant Visual Studio solution, with a separate GIT repository, access permissions, etc. This can make it hard to see the big picture in one view.

This utility will solve that problem, by providing a high level management tool. You can think of it as a complementory tool to Visual Studio.

# How does it work?

When first running it, you should point it to the root folder of the solution.
It will then go through all sub-folders to determine if it's an Olive microservice. If it is, then it will generate a row on the UI.

![screenshot](Resources/Screenshot.JPG)

# Features

- The first column, will show the current state of the service and whether or not it's running locally. 
  - Red means it's off, and green means it's running. 
  - You can click the button to toggle the state. So to run a service locally, you can just press this button.
- The port value is read from the `Website\Properties\LaunchSettings.json` file.
- The Open icon (Chrome) is a shortcut to launch a browser to view tht service. If the service isn't running, it will be disabled.
  - Usually you only need to use this for the `Hub` service, because all other services will be accessible via Hub.
- The *Git* column shows you how many new commits exist on the repository (from other developers) which you haven't pulled yet.
  - When you click on it, it will automatically pull the latest changes (where there is no conflict).
- The *Nuget* column shows how many of the nuget packages in that service are outdated.
  - You can click to update them from here automatically.
- The Visual Studio icon will load that service in a new Visual Studio.
  - But if it's already open, it will just bring the VS window to the top.
- The *folder* icon will open a new explorer window to the source of that service.
- The *gear* icon will do a full build (compilation) on that service. 
- The *debug* column will show you a log of events and errors for that service.

## Automatic Nuget update
In the main menu, you have the `Nuget` item with two options:
- *Update all*: It will update nuget packages for all services in the list.
- *Auto update*: It will periodically check for nuget updates and automatically update the packages.

## New Microservice creation
Under `File` menu you have the `New Microservice` item. It will open a dialog asking you to input a couple of fields:
- *Service Name*: This is a short name for the new microservice. Do not add any solution related prefix or suffix to it.
- *Git Repository Url*: This is the repository for the new service. You should have created a blank repo beforehand.

When you click `Create` it will generate a full Visual Studio solution in the root folder of your project, ready for your development!
