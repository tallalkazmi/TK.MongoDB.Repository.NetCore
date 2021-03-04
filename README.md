# TK.MongoDB.Repository.NetCore
[![Nuget](https://img.shields.io/nuget/v/TK.MongoDB.Repository.NetCore)](https://www.nuget.org/packages/TK.MongoDB.Repository.NetCore) 
[![Nuget](https://img.shields.io/nuget/dt/TK.MongoDB.Repository.NetCore)](https://www.nuget.org/packages/TK.MongoDB.Repository.NetCore)
![Azure DevOps builds](https://img.shields.io/azure-devops/build/tallalkazmi/79c589e2-20be-4ad6-9b5a-90be5ddc7916/5) 
![Azure DevOps tests](https://img.shields.io/azure-devops/tests/tallalkazmi/79c589e2-20be-4ad6-9b5a-90be5ddc7916/5) 
![Azure DevOps releases](https://img.shields.io/azure-devops/release/tallalkazmi/79c589e2-20be-4ad6-9b5a-90be5ddc7916/4/4)

Repository pattern implementation of MongoDB collections using LINQ with optional Dependency Tracking in .NET Standard 2.0.

## Usage

#### Connection

Connection string should be provided by using `Connection` class. Inject this class in the following manner if you are using DI:

```c#
services.AddTransient(x => new Connection(Configuration.GetConnectionString("DefaultConnection")));
```

#### Settings

1. Global collection settings can be applied by using `Settings.Configure<T>` where `T` is collection model implementing `IDbModel`. This method accepts:

   - Connection (for connection string)
   - Expire After (for *ExpiryAfterSeconds* index)
   - Create Collection Options ()

Use this method with a specific collection model, make sure this method is called once for each application run.


2. If you intend to use **Dependency Tracking**, you can specify commands to <u>not</u> track by setting the `Settings.NotTrackedCommands`, by default the following commands are not not tracked:

   - isMaster
   - buildInfo
   - getLastError
   - saslStart
   - saslContinue
   - listIndexes

#### Collections

Create a collection model implementing `IDbModel` interface for use in repository. The name of this model will be used as collection name in MongoDB. For example:

```c#
public class Activity : IDbModel
{
    public ObjectId Id { get; set; }
    public bool Deleted { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime? UpdationDate { get; set; }
    public string Name { get; set; }
}
```

#### Dependency Tracking

To use dependency tracking implement the `IDependencyTracker` interface to meet your needs. For example:

```c#
public class DependencyTracker : IDependencyTracker
{
	public void Dependency(string name, string description, bool success, TimeSpan duration)
    {
    	Console.WriteLine($"{name}-{description}-{success}-{duration}");
    }
}
```

#### Examples & Tests

Refer to **TK.MongoDB.Test** project for all Unit Tests and examples.