using DataEntities;

namespace Store.Services;

public class CustomerService
{
    private readonly ILogger<CustomerService> _logger;
    private int _currentCustomerId = 1; // Default to first customer

    // Sample customers for demo purposes. In a production app, these would be
    // fetched from a Customer API. The data here mirrors the database initialization
    // in Products.Models.DbInitializer to maintain consistency for the demo.
    private readonly List<Customer> _sampleCustomers = new()
    {
        new Customer { Id = 1, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@example.com", Phone = "555-0101", MembershipTier = MembershipTier.Gold },
        new Customer { Id = 2, FirstName = "Bob", LastName = "Smith", Email = "bob.smith@example.com", Phone = "555-0102", MembershipTier = MembershipTier.Silver },
        new Customer { Id = 3, FirstName = "Carol", LastName = "Williams", Email = "carol.williams@example.com", Phone = "555-0103", MembershipTier = MembershipTier.Normal },
        new Customer { Id = 4, FirstName = "David", LastName = "Brown", Email = "david.brown@example.com", Phone = "555-0104", MembershipTier = MembershipTier.Normal },
    };

    public CustomerService(ILogger<CustomerService> logger)
    {
        _logger = logger;
    }

    public Customer GetCurrentCustomer()
    {
        var customer = _sampleCustomers.FirstOrDefault(c => c.Id == _currentCustomerId);
        return customer ?? _sampleCustomers.First();
    }

    public void SetCurrentCustomer(int customerId)
    {
        if (_sampleCustomers.Any(c => c.Id == customerId))
        {
            _currentCustomerId = customerId;
            _logger.LogInformation("Current customer changed to ID: {CustomerId}", customerId);
        }
        else
        {
            _logger.LogWarning("Customer with ID {CustomerId} not found", customerId);
        }
    }

    public List<Customer> GetAllCustomers()
    {
        return _sampleCustomers;
    }
}
