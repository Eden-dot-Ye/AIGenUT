using NUnit.Framework;

namespace DemoLib.Tests;

[TestFixture]
public class OrderProcessorTest
{
	private OrderProcessor _processor = null!;

	[SetUp]
	public void SetUp()
	{
		_processor = new OrderProcessor();
	}

	[Test]
	public void TestOrderCreation_WhenValidIds_ThenDraftStatus()
	{
		var order = new Order("ORD-001", "CUST-001");
		Assert.That(order.Status, Is.EqualTo(OrderStatus.Draft));
	}

	[Test]
	public void TestOrderCreation_WhenNullOrderId_ThenThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => new Order(null!, "CUST-001"));
	}

	[Test]
	public void TestOrderCreation_WhenEmptyCustomerId_ThenThrowsArgumentException()
	{
		Assert.Throws<ArgumentException>(() => new Order("ORD-001", "  "));
	}

	[Test]
	public void TestAddItem_WhenDraftOrder_ThenItemAdded()
	{
		var order = new Order("ORD-001", "CUST-001");
		order.AddItem(new OrderItem("P1", "Widget", 2, 10.00m));
		Assert.That(order.Items.Count, Is.EqualTo(1));
	}

	[Test]
	public void TestAddItem_WhenNotDraft_ThenThrowsInvalidOperationException()
	{
		var order = CreateSubmittableOrder();
		_processor.SubmitOrder(order);
		Assert.Throws<InvalidOperationException>(() =>
			order.AddItem(new OrderItem("P2", "Gadget", 1, 5.00m)));
	}

	[Test]
	public void TestAddItem_WhenZeroQuantity_ThenThrowsArgumentException()
	{
		var order = new Order("ORD-001", "CUST-001");
		Assert.Throws<ArgumentException>(() =>
			order.AddItem(new OrderItem("P1", "Widget", 0, 10.00m)));
	}

	[Test]
	public void TestAddItem_WhenNegativePrice_ThenThrowsArgumentException()
	{
		var order = new Order("ORD-001", "CUST-001");
		Assert.Throws<ArgumentException>(() =>
			order.AddItem(new OrderItem("P1", "Widget", 1, -5.00m)));
	}

	[Test]
	public void TestRemoveItem_WhenDraftOrder_ThenItemRemoved()
	{
		var order = new Order("ORD-001", "CUST-001");
		order.AddItem(new OrderItem("P1", "Widget", 2, 10.00m));
		var removed = order.RemoveItem("P1");
		Assert.That(removed, Is.True);
		Assert.That(order.Items.Count, Is.EqualTo(0));
	}

	[Test]
	public void TestRemoveItem_WhenProductNotFound_ThenReturnsFalse()
	{
		var order = new Order("ORD-001", "CUST-001");
		var removed = order.RemoveItem("NONEXISTENT");
		Assert.That(removed, Is.False);
	}

	[Test]
	public void TestUpdateStatus_WhenValidTransition_ThenStatusChanges()
	{
		var order = new Order("ORD-001", "CUST-001");
		order.UpdateStatus(OrderStatus.Submitted);
		Assert.That(order.Status, Is.EqualTo(OrderStatus.Submitted));
	}

	[Test]
	public void TestUpdateStatus_WhenInvalidTransition_ThenThrowsInvalidOperationException()
	{
		var order = new Order("ORD-001", "CUST-001");
		Assert.Throws<InvalidOperationException>(() =>
			order.UpdateStatus(OrderStatus.Shipped));
	}

	[Test]
	public void TestUpdateStatus_WhenCancelled_ThenNoFurtherTransitionsAllowed()
	{
		var order = new Order("ORD-001", "CUST-001");
		order.UpdateStatus(OrderStatus.Cancelled);
		Assert.Throws<InvalidOperationException>(() =>
			order.UpdateStatus(OrderStatus.Draft));
	}

	[Test]
	public void TestUpdateStatus_WhenTransitioned_ThenHistoryRecorded()
	{
		var order = new Order("ORD-001", "CUST-001");
		order.UpdateStatus(OrderStatus.Submitted, "Customer submitted");
		Assert.That(order.StatusHistory.Count, Is.EqualTo(1));
		Assert.That(order.StatusHistory[0].From, Is.EqualTo(OrderStatus.Draft));
		Assert.That(order.StatusHistory[0].To, Is.EqualTo(OrderStatus.Submitted));
	}

	[Test]
	public void TestValidateOrder_WhenNoItems_ThenReturnsError()
	{
		var order = new Order("ORD-001", "CUST-001");
		order.ShippingAddress = "123 Main St";
		var errors = _processor.ValidateOrder(order);
		Assert.That(errors, Does.Contain("Order must contain at least one item."));
	}

	[Test]
	public void TestValidateOrder_WhenNoShippingAddress_ThenReturnsError()
	{
		var order = new Order("ORD-001", "CUST-001");
		order.AddItem(new OrderItem("P1", "Widget", 1, 15.00m));
		var errors = _processor.ValidateOrder(order);
		Assert.That(errors, Does.Contain("Shipping address is required."));
	}

	[Test]
	public void TestValidateOrder_WhenValidOrder_ThenNoErrors()
	{
		var order = CreateSubmittableOrder();
		var errors = _processor.ValidateOrder(order);
		Assert.That(errors, Is.Empty);
	}

	[Test]
	public void TestValidateOrder_WhenDuplicateProducts_ThenReturnsError()
	{
		var order = new Order("ORD-001", "CUST-001");
		order.AddItem(new OrderItem("P1", "Widget", 1, 10.00m));
		order.AddItem(new OrderItem("P1", "Widget", 2, 10.00m));
		order.ShippingAddress = "123 Main St";
		var errors = _processor.ValidateOrder(order);
		Assert.That(errors, Has.Some.Contains("Duplicate products"));
	}

	[Test]
	public void TestSubmitOrder_WhenValid_ThenStatusChangesToSubmitted()
	{
		var order = CreateSubmittableOrder();
		_processor.SubmitOrder(order);
		Assert.That(order.Status, Is.EqualTo(OrderStatus.Submitted));
	}

	[Test]
	public void TestSubmitOrder_WhenInvalid_ThenThrowsInvalidOperationException()
	{
		var order = new Order("ORD-001", "CUST-001");
		Assert.Throws<InvalidOperationException>(() => _processor.SubmitOrder(order));
	}

	[Test]
	public void TestCalculateBulkDiscount_WhenBelow500_ThenZeroDiscount()
	{
		var order = new Order("ORD-001", "CUST-001");
		order.AddItem(new OrderItem("P1", "Widget", 1, 100.00m));
		Assert.That(_processor.CalculateBulkDiscount(order), Is.EqualTo(0m));
	}

	[Test]
	public void TestCalculateBulkDiscount_WhenAbove500_ThenTwoPercent()
	{
		var order = new Order("ORD-001", "CUST-001");
		order.AddItem(new OrderItem("P1", "Widget", 1, 500.00m));
		Assert.That(_processor.CalculateBulkDiscount(order), Is.EqualTo(2m));
	}

	[Test]
	public void TestCalculateBulkDiscount_WhenAbove1000_ThenFivePercent()
	{
		var order = new Order("ORD-001", "CUST-001");
		order.AddItem(new OrderItem("P1", "Widget", 1, 1000.00m));
		Assert.That(_processor.CalculateBulkDiscount(order), Is.EqualTo(5m));
	}

	[Test]
	public void TestCalculateBulkDiscount_WhenAbove5000_ThenTenPercent()
	{
		var order = new Order("ORD-001", "CUST-001");
		order.AddItem(new OrderItem("P1", "Widget", 1, 5000.00m));
		Assert.That(_processor.CalculateBulkDiscount(order), Is.EqualTo(10m));
	}

	[Test]
	public void TestCalculateBulkDiscount_WhenAbove10000_ThenFifteenPercent()
	{
		var order = new Order("ORD-001", "CUST-001");
		order.AddItem(new OrderItem("P1", "Widget", 1, 10000.00m));
		Assert.That(_processor.CalculateBulkDiscount(order), Is.EqualTo(15m));
	}

	[Test]
	public void TestCalculateShippingCost_WhenSmallOrder_ThenCalculatesCorrectly()
	{
		var order = new Order("ORD-001", "CUST-001");
		order.AddItem(new OrderItem("P1", "Widget", 1, 50.00m));
		// baseCost(5.99) + weight(10 * 2.50) + items(1 * 0.50) = 5.99 + 25 + 0.50 = 31.49
		var result = _processor.CalculateShippingCost(order, 10m);
		Assert.That(result, Is.EqualTo(31.49m));
	}

	[Test]
	public void TestCalculateShippingCost_WhenLargeOrder_ThenFreeShipping()
	{
		var order = new Order("ORD-001", "CUST-001");
		order.AddItem(new OrderItem("P1", "Widget", 1, 5000.00m));
		Assert.That(_processor.CalculateShippingCost(order, 100m), Is.EqualTo(0m));
	}

	[Test]
	public void TestCalculateShippingCost_WhenNegativeWeight_ThenThrows()
	{
		var order = new Order("ORD-001", "CUST-001");
		order.AddItem(new OrderItem("P1", "Widget", 1, 50.00m));
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			_processor.CalculateShippingCost(order, -1m));
	}

	[Test]
	public void TestGenerateOrderSummary_WhenOrderHasItems_ThenContainsOrderInfo()
	{
		var order = new Order("ORD-001", "CUST-001");
		order.AddItem(new OrderItem("P1", "Widget", 2, 10.00m));
		var summary = _processor.GenerateOrderSummary(order);
		Assert.That(summary, Does.Contain("Order: ORD-001"));
		Assert.That(summary, Does.Contain("Customer: CUST-001"));
		Assert.That(summary, Does.Contain("Widget"));
	}

	[Test]
	public void TestOrderSubtotal_WhenMultipleItems_ThenSumsCorrectly()
	{
		var order = new Order("ORD-001", "CUST-001");
		order.AddItem(new OrderItem("P1", "Widget", 2, 10.00m));
		order.AddItem(new OrderItem("P2", "Gadget", 1, 25.00m));
		Assert.That(order.Subtotal, Is.EqualTo(45.00m));
	}

	[Test]
	public void TestOrderTotal_WhenDiscountApplied_ThenReducesTotal()
	{
		var order = new Order("ORD-001", "CUST-001");
		order.AddItem(new OrderItem("P1", "Widget", 10, 10.00m));
		order.DiscountPercent = 10m;
		Assert.That(order.Total, Is.EqualTo(90.00m));
	}

	private Order CreateSubmittableOrder()
	{
		var order = new Order("ORD-001", "CUST-001");
		order.AddItem(new OrderItem("P1", "Widget", 1, 15.00m));
		order.ShippingAddress = "123 Main St";
		return order;
	}
}
