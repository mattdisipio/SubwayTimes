# Subway Times App

## Overview
This is a small app that uses the [MTA Service Status .NET Wrapper](https://github.com/cheesemacfly/MTAServiceStatus/) package to parse and display when subways become delayed or on time for the NY Subway system. This should reflect what is seen on the [MTA Service status](http://alert.mta.info/) page.

## Running
This is a .net core console app. There is an executable that you can run. Upon initialization, the app will fire off a background thread that polls the MTA service status api every 30 seconds and updates the user on what trains have become delayed or are on time.

## Commands
_uptime [line]_
  
  * When entered, this command will give you the uptime for the current line (A, H, 1, etc.)

_status [line]_ 

  * When entered, this command will give you the status for the current line. Currently, this will tell you whether the line is delayed or on time.

_info_
  
  * When entered, this command will give you all the different commands you can run in the app.

_exit_

* When entered, this command wil exit the application.

## Room for Improvement
This app was written as a one off and was more focused on functionality than providing an enterprise level solution. The first thing I would add to this project would be unit tests. In a production level environment, I unit test all my code but felt it was unnecessary for this exercies. Unit testing this app would help developers better catch errors that come back from using the MTA service status .net wrapper.

Another improvement I would make is adding an actual alerting service that sends a user an email when a train is delayed. This would allow a user to not hae to physically watch the app and instead be alerted whenever a line they cared about was delayed.

## Tech Used
This is a .net core console app. It uses the .net dependency injection framework to work, and could easily be swapped out for a service that returns more/less data if that application were to be expanded. Using dependency injection would allow for easy unit test of any passed in services. There are unit tests for the MTA Status .net wrapper, where a bulk of the data manipulation is done coming back from the mta service.