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

// --- Relationship test entities ---

public class Order : BusinessObject
{
    public string Name { get; set; } = "";
    public Person? Customer { get; set; }
    public ICollection<OrderItem> Items { get; }

    public Order()
    {
        Items = new BidirectionalCollection<Order, OrderItem>(
            this,
            oi => oi.Order,
            (oi, o) => oi.Order = o);
    }

    public static readonly OrderClassInfo ClassInfo = new();
    public static readonly StringPropertyInfo<Order> NameInfo = new("Name");
    public static readonly ReferencePropertyInfo<Order, Person> CustomerInfo = new("Customer") { IsRequired = false };
    public static readonly NavigationPropertyInfo<Order, OrderItem> ItemsInfo = new("Items", "OrderId", inverseNavigation: "Order");
}

public class OrderClassInfo : ClassInfo<Order>
{
    public override int CID => 110;
    public override string TableMappingStrategy => "TPH";
}

public class OrderItem : BusinessObject
{
    public string Description { get; set; } = "";

    private Order? _order;
    public Order? Order
    {
        get => _order;
        set => SetReference(this, ref _order, value, o => o.Items);
    }

    public static readonly OrderItemClassInfo ClassInfo = new();
    public static readonly StringPropertyInfo<OrderItem> DescriptionInfo = new("Description");
    public static readonly ReferencePropertyInfo<OrderItem, Order> OrderInfo = new("Order", inverseNavigation: "Items") { IsRequired = true };
}

public class OrderItemClassInfo : ClassInfo<OrderItem>
{
    public override int CID => 111;
    public override string TableMappingStrategy => "TPH";
}

// Vehicle / Car — for TPH + ReferencePropertyInfo test (TC-002-06)

public class Vehicle : BusinessObject
{
    public Person? Owner { get; set; }

    public static readonly VehicleClassInfo ClassInfo = new();
    public static readonly ReferencePropertyInfo<Vehicle, Person> OwnerInfo = new("Owner") { IsRequired = false };
}

public class VehicleClassInfo : ClassInfo<Vehicle>
{
    public override int CID => 120;
    public override string TableMappingStrategy => "TPH";
}

public class Car : Vehicle
{
    public static new readonly CarClassInfo ClassInfo = new();
}

public class CarClassInfo : ClassInfo<Car>
{
    public override int CID => 121;
    public override string TableMappingStrategy => "TPH";
}

// --- 1:1 and 0..1:1 test entities ---
//
// Supplier (principal, no FK)  0..1 ──── 1  SupplierContact (dependent, has FK)
//
// Supplier.Contact  → PrincipalPropertyInfo  (no FK column, EF config skipped)
// SupplierContact.Supplier → ReferencePropertyInfo (has FK, configures HasOne.WithOne)

public class Supplier : BusinessObject
{
    public string Name { get; set; } = "";

    private SupplierContact? _contact;
    public SupplierContact? Contact
    {
        get => _contact;
        set => SetOneToOne(this, ref _contact, value, sc => sc.Supplier, (sc, s) => sc.Supplier = s);
    }

    public static readonly SupplierClassInfo ClassInfo = new();
    public static readonly StringPropertyInfo<Supplier> NameInfo = new("Name");
    // Principal side — no FK column, EF config is done by SupplierContact.SupplierInfo
    public static readonly PrincipalPropertyInfo<Supplier, SupplierContact> ContactInfo =
        new("Contact", inverseNavigation: "Supplier");
}

public class SupplierClassInfo : ClassInfo<Supplier>
{
    public override int CID => 140;
    public override string TableMappingStrategy => "TPH";
}

public class SupplierContact : BusinessObject
{
    public string Phone { get; set; } = "";

    private Supplier? _supplier;
    public Supplier? Supplier
    {
        get => _supplier;
        set => SetOneToOne(this, ref _supplier, value, s => s.Contact, (s, sc) => s.Contact = sc);
    }

    public static readonly SupplierContactClassInfo ClassInfo = new();
    public static readonly StringPropertyInfo<SupplierContact> PhoneInfo = new("Phone");
    // Dependent side — has FK, configures HasOne.WithOne.HasForeignKey(SupplierContact)
    // IsRequired = true  → 1:1  (SupplierContact must always have a Supplier)
    // IsRequired = false → 0..1:1 (SupplierContact may exist without a Supplier)
    public static readonly ReferencePropertyInfo<SupplierContact, Supplier> SupplierInfo =
        new("Supplier", inverseNavigation: "Contact") { IsRequired = true };
}

public class SupplierContactClassInfo : ClassInfo<SupplierContact>
{
    public override int CID => 141;
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
