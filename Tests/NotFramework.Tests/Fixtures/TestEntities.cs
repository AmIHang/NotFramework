using Not.Core.Model;
using Not.Core.Model.Localization;
using Not.Core.Model.Metadata;
using Not.Core.Model.Metadata.Property;
using Not.Core.Service;

namespace Not.Core.Tests.Fixtures;

// --- Domain entities ---
// PropertyInfo objects must be static fields on the entity class so that
// CommonEntityTableMappingStrategy.BuildProperties() can find them via reflection.

public class Person : BusinessObject
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public LocalizedString Title { get; set; } = new();

    public static readonly PersonClassInfo ClassInfo = new();
    public static readonly StringPropertyInfo<Person> NameInfo = new("Name");
    public static readonly IntPropertyInfo<Person> AgeInfo = new("Age") { IsRequired = true };
    public static readonly LocalizedStringPropertyInfo<Person> TitleInfo = new("Title");
}

public class PersonClassInfo : ClassInfo<Person>
{
    public override int CID => 100;
    public override string TableMappingStrategy => "TPH";
}

// Employee is a derivation of Person (not a direct BusinessObject child)
public class Employee : Person
{
    public string Department { get; set; } = "";

    // 'new' hides the inherited Person.ClassInfo static field
    public static new readonly EmployeeClassInfo ClassInfo = new();
    public static readonly StringPropertyInfo<Employee> DepartmentInfo = new("Department");
}

public class EmployeeClassInfo : ClassInfo<Employee>
{
    public override int CID => 101;
    public override string TableMappingStrategy => "TPH";
}

// --- Test service for Session service-resolution tests ---

public interface IGreetingService : IService
{
    string Greet();
}

public class GreetingService : IGreetingService
{
    public string Greet() => "Hello from GreetingService";
}
