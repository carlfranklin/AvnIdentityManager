# AvnIdentityManager

A self-contained .NET 9 manager for ASP.NET Core Identity SQL databases.

**Actions:**

- Create a new Auth database via connection string
- Change the working Auth database on the fly by passing a new connection string
- Get a list of users (with a filter)
- Get a list of roles
- Create a new user
- Create a new role
- Delete a user
- Delete a role
- Edit a user
- Change the user's password without requiring the existing password

> :point_up: This REPO is an update of the .NET 7 [IdentityManagerLibrary](https://github.com/carlfranklin/IdentityManagerLibrary). Unlike that library, AvnIdentiityManager is self-contained. The host app does not have to have any Identity configuration. Also, you can now switch Auth databases on the fly by supplying a different connection string.

The **AvnIdentityManager** is a self-contained class library for managing an ASP.NET Core Identity SQL database.

The **AvnIdentityManagerBlazor8ServerDemo** project, is a .NET 8 Blazor Web App in Global Server interactivity mode that shows some sample UI for using the library. 

:point_up: You can use the demo app for your own Auth databases. You will need to:

- Change the connection string in *appsettings.json*
- To test multiple databases, set the two connection strings in the demo code.
- The demo lets you create a new database in LocalDB with a unique name, and then use it.

The **AvnIdentityManagerConsoleDemo** is a console app that uses the AvnIdentityManager library to perform all the functions in the library.

