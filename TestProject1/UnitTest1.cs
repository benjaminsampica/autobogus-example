using Soenneker.Utils.AutoBogus;
using Soenneker.Utils.AutoBogus.Config;
using Soenneker.Utils.AutoBogus.Context;
using Soenneker.Utils.AutoBogus.Override;

namespace TestProject1
{
    public class UnitTest1
    {
        [Fact]
        public void GivenIGenerateAOrder_ThenCreatesACustomerWithNoPayment()
        {
            var order = FakeFactory.CreateFakeOrder();

            Assert.NotNull(order.Customer);
            Assert.Null(order.Payment);
        }

        [Fact]
        public void GivenIGenerateACustomer_ThenDoesNotCreateAnyOrders()
        {
            var customer = FakeFactory.CreateFakeCustomer();

            Assert.Empty(customer.Orders);
        }
    }
}

public static class FakeFactory
{
    public static AutoFakerConfig _config = new()
    {
        DefaultTimezoneOffset = DateTimeOffset.UtcNow.Offset,
        Overrides = [
            new FakeCustomerOverride(),
            new FakeOrderOverride(),
            new FakeOrderPaymentOverride(),
        ],
        RecursiveDepth = 0
    };
    public static AutoFaker Faker { get; set; } = new(_config);

    public static Order CreateFakeOrder() => Faker.Generate<Order>();
    public static Customer CreateFakeCustomer() => Faker.Generate<Customer>();

    extension(Customer customer)
    {
        public Customer WithOrder(Action<Order>? configure = null)
        {
            var order = CreateFakeOrder();

            configure?.Invoke(order);

            customer.Orders.Add(order);

            return customer;
        }
    }
}

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public OrderPayment? Payment { get; set; }
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
}

public class FakeOrderOverride : AutoFakerOverride<Order>
{
    /// Trying to make this equivalent to.
    /// new AutoFaker<Order>(_config)
    ///     .Ignore(o => o.Id)
    ///     .Ignore(o => o.CustomerId)
    ///     .Ignore(o => o.Payment)
    ///     .RuleFor(o => o.Customer, FakeFactory.CreateFakeCustomer())
    ///     .Generate()
    public override void Generate(AutoFakerOverrideContext context)
    {
        var entity = (context.Instance as Order)!;

        entity.Id = default;
        entity.Customer = FakeFactory.CreateFakeCustomer(); // Orders cannot exist in the domain without a customer on them.
        entity.CustomerId = default;
        entity.Payment = null;
    }
}

public class OrderPayment
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
}

public class FakeOrderPaymentOverride : AutoFakerOverride<OrderPayment>
{
    /// Trying to make this equivalent to.
    /// new AutoFaker<OrderPayment>(_config)
    ///     .Ignore(op => op.Id)
    ///     .Ignore(op => op.OrderId)
    ///     .Ignore(op => op.Id)
    ///     .RuleFor(op => op.Order, FakeFactory.CreateFakeOrder())
    ///     .Generate()
    public override void Generate(AutoFakerOverrideContext context)
    {
        var entity = (context.Instance as OrderPayment)!;

        entity.Id = default;
        entity.Order = FakeFactory.CreateFakeOrder(); // Order payments cannot exist in the domain without a order on them.
        entity.OrderId = default;
    }
}

public class Customer
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public List<Order> Orders { get; set; } = [];
}

public class FakeCustomerOverride : AutoFakerOverride<Customer>
{
    /// Trying to make this equivalent to.
    /// new AutoFaker<Order>(_config)
    ///     .Ignore(o => o.Id)
    ///     .Ignore(o => o.Orders)
    ///     .Generate()
    public override void Generate(AutoFakerOverrideContext context)
    {
        var entity = (context.Instance as Customer)!;
        entity.Id = default;
        entity.Orders = []; // Customers can exist in the domain without any orders.
    }
}