namespace DemoLib;

/// <summary>
/// Represents the possible states of an order in the system.
/// </summary>
public enum OrderStatus
{
    Draft,
    Submitted,
    Validated,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Returned,
    Refunded
}

/// <summary>
/// Represents a single item in an order.
/// </summary>
public record OrderItem(string ProductId, string ProductName, int Quantity, decimal UnitPrice)
{
    public decimal TotalPrice => Quantity * UnitPrice;
}

/// <summary>
/// Represents an order with items, status tracking, and validation.
/// Implements a state machine pattern for order lifecycle management.
/// </summary>
public class Order
{
    private readonly List<OrderItem> _items = new();
    private readonly List<StatusTransition> _statusHistory = new();

    public string OrderId { get; }
    public string CustomerId { get; }
    public DateTime CreatedAt { get; }
    public OrderStatus Status { get; private set; }
    public string? ShippingAddress { get; set; }
    public string? Notes { get; set; }
    public decimal DiscountPercent { get; set; }

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyList<StatusTransition> StatusHistory => _statusHistory.AsReadOnly();

    public decimal Subtotal => _items.Sum(i => i.TotalPrice);
    public decimal DiscountAmount => Subtotal * (DiscountPercent / 100m);
    public decimal Total => Subtotal - DiscountAmount;

    public Order(string orderId, string customerId)
    {
        ArgumentNullException.ThrowIfNull(orderId);
        ArgumentNullException.ThrowIfNull(customerId);
        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentException("Order ID cannot be empty.", nameof(orderId));
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Customer ID cannot be empty.", nameof(customerId));

        OrderId = orderId;
        CustomerId = customerId;
        CreatedAt = DateTime.UtcNow;
        Status = OrderStatus.Draft;
    }

    public void AddItem(OrderItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Can only add items to draft orders.");
        if (item.Quantity <= 0)
            throw new ArgumentException("Quantity must be positive.", nameof(item));
        if (item.UnitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(item));
        _items.Add(item);
    }

    public bool RemoveItem(string productId)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Can only remove items from draft orders.");
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        return item != null && _items.Remove(item);
    }

    public void UpdateStatus(OrderStatus newStatus, string reason = "")
    {
        ValidateTransition(Status, newStatus);
        var transition = new StatusTransition(Status, newStatus, DateTime.UtcNow, reason);
        _statusHistory.Add(transition);
        Status = newStatus;
    }

    private static void ValidateTransition(OrderStatus from, OrderStatus to)
    {
        var allowedTransitions = new Dictionary<OrderStatus, OrderStatus[]>
        {
            [OrderStatus.Draft] = [OrderStatus.Submitted, OrderStatus.Cancelled],
            [OrderStatus.Submitted] = [OrderStatus.Validated, OrderStatus.Cancelled],
            [OrderStatus.Validated] = [OrderStatus.Processing, OrderStatus.Cancelled],
            [OrderStatus.Processing] = [OrderStatus.Shipped, OrderStatus.Cancelled],
            [OrderStatus.Shipped] = [OrderStatus.Delivered, OrderStatus.Returned],
            [OrderStatus.Delivered] = [OrderStatus.Returned],
            [OrderStatus.Returned] = [OrderStatus.Refunded],
            [OrderStatus.Cancelled] = Array.Empty<OrderStatus>(),
            [OrderStatus.Refunded] = Array.Empty<OrderStatus>(),
        };

        if (!allowedTransitions.TryGetValue(from, out var allowed) || !allowed.Contains(to))
            throw new InvalidOperationException(
                $"Cannot transition from {from} to {to}.");
    }
}

/// <summary>
/// Records a status transition event.
/// </summary>
public record StatusTransition(OrderStatus From, OrderStatus To, DateTime Timestamp, string Reason);

/// <summary>
/// Processes and validates orders, applying business rules.
/// </summary>
public class OrderProcessor
{
    private readonly decimal _minimumOrderAmount;
    private readonly int _maximumItemsPerOrder;
    private readonly decimal _maxDiscountPercent;

    public OrderProcessor(decimal minimumOrderAmount = 10m, int maximumItemsPerOrder = 100, decimal maxDiscountPercent = 50m)
    {
        if (minimumOrderAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(minimumOrderAmount), "Minimum order amount cannot be negative.");
        if (maximumItemsPerOrder <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximumItemsPerOrder), "Maximum items must be positive.");
        if (maxDiscountPercent < 0 || maxDiscountPercent > 100)
            throw new ArgumentOutOfRangeException(nameof(maxDiscountPercent), "Discount must be between 0 and 100.");

        _minimumOrderAmount = minimumOrderAmount;
        _maximumItemsPerOrder = maximumItemsPerOrder;
        _maxDiscountPercent = maxDiscountPercent;
    }

    /// <summary>
    /// Validates an order against business rules.
    /// Returns a list of validation errors (empty if valid).
    /// </summary>
    public List<string> ValidateOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        var errors = new List<string>();

        if (!order.Items.Any())
            errors.Add("Order must contain at least one item.");

        if (order.Items.Count > _maximumItemsPerOrder)
            errors.Add($"Order cannot contain more than {_maximumItemsPerOrder} items.");

        if (order.Subtotal < _minimumOrderAmount)
            errors.Add($"Order subtotal must be at least {_minimumOrderAmount:C}.");

        if (order.DiscountPercent > _maxDiscountPercent)
            errors.Add($"Discount cannot exceed {_maxDiscountPercent}%.");

        if (order.DiscountPercent < 0)
            errors.Add("Discount cannot be negative.");

        if (string.IsNullOrWhiteSpace(order.ShippingAddress))
            errors.Add("Shipping address is required.");

        var duplicateProducts = order.Items
            .GroupBy(i => i.ProductId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateProducts.Any())
            errors.Add($"Duplicate products found: {string.Join(", ", duplicateProducts)}.");

        return errors;
    }

    /// <summary>
    /// Submits an order after validation. Throws if validation fails.
    /// </summary>
    public void SubmitOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        var errors = ValidateOrder(order);
        if (errors.Any())
            throw new InvalidOperationException(
                $"Order validation failed: {string.Join("; ", errors)}");

        order.UpdateStatus(OrderStatus.Submitted, "Order submitted for processing.");
    }

    /// <summary>
    /// Applies a bulk discount based on order total.
    /// </summary>
    public decimal CalculateBulkDiscount(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        var total = order.Subtotal;
        return total switch
        {
            >= 10000m => 15m,
            >= 5000m => 10m,
            >= 1000m => 5m,
            >= 500m => 2m,
            _ => 0m
        };
    }

    /// <summary>
    /// Calculates the shipping cost based on item count and total weight.
    /// </summary>
    public decimal CalculateShippingCost(Order order, decimal totalWeightKg)
    {
        ArgumentNullException.ThrowIfNull(order);
        if (totalWeightKg < 0)
            throw new ArgumentOutOfRangeException(nameof(totalWeightKg), "Weight cannot be negative.");

        // Free shipping for large orders
        if (order.Subtotal >= 5000m)
            return 0m;

        var baseCost = 5.99m;
        var perKgCost = 2.50m;
        var perItemCost = 0.50m;

        return baseCost + (totalWeightKg * perKgCost) + (order.Items.Count * perItemCost);
    }

    /// <summary>
    /// Generates an order summary string.
    /// </summary>
    public string GenerateOrderSummary(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        var lines = new List<string>
        {
            $"Order: {order.OrderId}",
            $"Customer: {order.CustomerId}",
            $"Status: {order.Status}",
            $"Items: {order.Items.Count}",
            "---"
        };

        foreach (var item in order.Items)
        {
            lines.Add($"  {item.ProductName} x{item.Quantity} @ {item.UnitPrice:C} = {item.TotalPrice:C}");
        }

        lines.Add("---");
        lines.Add($"Subtotal: {order.Subtotal:C}");
        if (order.DiscountPercent > 0)
            lines.Add($"Discount ({order.DiscountPercent}%): -{order.DiscountAmount:C}");
        lines.Add($"Total: {order.Total:C}");

        return string.Join(Environment.NewLine, lines);
    }
}
