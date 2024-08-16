# AvnIdentityManager

A self-contained .NET 8 manager for ASP.NET Core Identity SQL databases.

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



