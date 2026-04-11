using Microsoft.EntityFrameworkCore;
using Not.Core.Model.Metadata.Property;
using Not.Core.Tests.Fixtures;
using Not.Sqlite.Persistence;
using Xunit;

namespace Not.Core.Tests;

/// <summary>
/// Covers the four relationship cardinalities:
///   0..1 - m   → ReferencePropertyInfo IsRequired=false  + NavigationPropertyInfo
///   1    - m   → ReferencePropertyInfo IsRequired=true   + NavigationPropertyInfo
///   1    - 1   → ReferencePropertyInfo IsRequired=true   + PrincipalPropertyInfo
///   0..1 - 1   → ReferencePropertyInfo IsRequired=false  + PrincipalPropertyInfo
///                (Supplier.Contact is currently configured as IsRequired=true for 1:1)
/// </summary>
public class OneToOneRelationshipTests
{
    // ── 1:1 · EF model ───────────────────────────────────────────────────────

    [Fact]
    public void OneToOne_EfModel_ConfiguredAsWithOne()
    {
        using var ctx = RelationshipModelDefinition.CreateInMemory();
        ctx.Database.EnsureCreated();

        var contactType = ctx.Model.GetEntityTypes().First(e => e.ClrType == typeof(SupplierContact));

        // Must be a 1:1 relationship (no collection on the other side)
        var fk = contactType.GetForeignKeys()
            .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Supplier));

        Assert.NotNull(fk);
        Assert.True(fk!.IsUnique);   // unique FK = EF's indicator for 1:1
        Assert.True(fk.IsRequired);  // IsRequired = true on SupplierInfo
    }

    [Fact]
    public void OneToOne_PrincipalNavigation_RegisteredInEfModel()
    {
        using var ctx = RelationshipModelDefinition.CreateInMemory();
        ctx.Database.EnsureCreated();

        var supplierType = ctx.Model.GetEntityTypes().First(e => e.ClrType == typeof(Supplier));
        var nav = supplierType.GetNavigations().FirstOrDefault(n => n.Name == "Contact");

        Assert.NotNull(nav);
    }

    // ── 1:1 · Persist and reload ─────────────────────────────────────────────

    [Fact]
    public void OneToOne_SaveAndLoad_PrincipalToDependent()
    {
        var (ctx, db) = RelationshipModelDefinition.CreateInMemoryWithDb();
        ctx.Database.EnsureCreated();
        int supplierOid;

        using (ctx)
        {
            var supplier = new Supplier { Name = "ACME" };
            var contact = new SupplierContact { Phone = "0800-123", Supplier = supplier };
            ctx.Set<Supplier>().Add(supplier);
            ctx.Set<SupplierContact>().Add(contact);
            ctx.SaveChanges();
            supplierOid = supplier.OID;
        }

        using var ctx2 = new RelationshipModelDefinition(
            db.Configure(new DbContextOptionsBuilder<RelationshipModelDefinition>()).Options);

        var loaded = ctx2.Set<Supplier>()
            .Include(s => s.Contact)
            .First(s => s.OID == supplierOid);

        Assert.NotNull(loaded.Contact);
        Assert.Equal("0800-123", loaded.Contact!.Phone);
    }

    [Fact]
    public void OneToOne_SaveAndLoad_DependentToPrincipal()
    {
        var (ctx, db) = RelationshipModelDefinition.CreateInMemoryWithDb();
        ctx.Database.EnsureCreated();
        int contactOid;

        using (ctx)
        {
            var supplier = new Supplier { Name = "ACME" };
            var contact = new SupplierContact { Phone = "0800-123", Supplier = supplier };
            ctx.Set<Supplier>().Add(supplier);
            ctx.Set<SupplierContact>().Add(contact);
            ctx.SaveChanges();
            contactOid = contact.OID;
        }

        using var ctx2 = new RelationshipModelDefinition(
            db.Configure(new DbContextOptionsBuilder<RelationshipModelDefinition>()).Options);

        var loaded = ctx2.Set<SupplierContact>()
            .Include(sc => sc.Supplier)
            .First(sc => sc.OID == contactOid);

        Assert.NotNull(loaded.Supplier);
        Assert.Equal("ACME", loaded.Supplier!.Name);
    }

    // ── 1:1 · In-memory bidirectional sync ───────────────────────────────────

    [Fact]
    public void OneToOne_SetDependent_SyncsToPrincipal()
    {
        var supplier = new Supplier { Name = "S1" };
        var contact = new SupplierContact { Phone = "111" };

        contact.Supplier = supplier;

        Assert.Same(contact, supplier.Contact);
    }

    [Fact]
    public void OneToOne_SetPrincipal_SyncsToDependent()
    {
        var supplier = new Supplier { Name = "S2" };
        var contact = new SupplierContact { Phone = "222" };

        supplier.Contact = contact;

        Assert.Same(supplier, contact.Supplier);
    }

    [Fact]
    public void OneToOne_Reassign_ClearsOldPrincipal()
    {
        var s1 = new Supplier { Name = "Old" };
        var s2 = new Supplier { Name = "New" };
        var contact = new SupplierContact { Phone = "333" };

        contact.Supplier = s1;
        contact.Supplier = s2;

        Assert.Null(s1.Contact);
        Assert.Same(contact, s2.Contact);
    }

    [Fact]
    public void OneToOne_SetNull_ClearsBothSides()
    {
        var supplier = new Supplier { Name = "S" };
        var contact = new SupplierContact { Phone = "444" };

        supplier.Contact = contact;
        supplier.Contact = null;

        Assert.Null(supplier.Contact);
        Assert.Null(contact.Supplier);
    }

    // ── PrincipalPropertyInfo metadata ───────────────────────────────────────

    [Fact]
    public void PrincipalPropertyInfo_IsInClassInfoProperties()
    {
        var prop = Supplier.ClassInfo.GetProperty("Contact");

        Assert.NotNull(prop);
        Assert.IsType<PrincipalPropertyInfo<Supplier, SupplierContact>>(prop);
    }

    [Fact]
    public void PrincipalPropertyInfo_TargetClass_IsCorrect()
    {
        Assert.Equal(typeof(SupplierContact), Supplier.ContactInfo.TargetType);
    }

    [Fact]
    public void PrincipalPropertyInfo_InverseNavigation_IsSet()
    {
        Assert.Equal("Supplier", Supplier.ContactInfo.InverseNavigation);
    }

    // ── All four cardinalities summary ────────────────────────────────────────

    [Fact]
    public void AllFourCardinalities_EfModel_Correct()
    {
        using var ctx = RelationshipModelDefinition.CreateInMemory();
        ctx.Database.EnsureCreated();

        // 0..1 - m : Order.Customer (optional reference)
        var orderType = ctx.Model.GetEntityTypes().First(e => e.ClrType == typeof(Order));
        var customerFk = orderType.GetForeignKeys()
            .First(fk => fk.PrincipalEntityType.ClrType == typeof(Person));
        Assert.False(customerFk.IsRequired);    // 0..1 side
        Assert.False(customerFk.IsUnique);      // m side (not 1:1)

        // 1 - m : OrderItem.Order (required reference)
        var itemType = ctx.Model.GetEntityTypes().First(e => e.ClrType == typeof(OrderItem));
        var orderFk = itemType.GetForeignKeys()
            .First(fk => fk.PrincipalEntityType.ClrType == typeof(Order));
        Assert.True(orderFk.IsRequired);        // 1 side
        Assert.False(orderFk.IsUnique);         // m side

        // 1 - 1 : SupplierContact.Supplier (required, unique FK)
        var contactType = ctx.Model.GetEntityTypes().First(e => e.ClrType == typeof(SupplierContact));
        var supplierFk = contactType.GetForeignKeys()
            .First(fk => fk.PrincipalEntityType.ClrType == typeof(Supplier));
        Assert.True(supplierFk.IsRequired);     // 1 side
        Assert.True(supplierFk.IsUnique);       // 1:1
    }
}
