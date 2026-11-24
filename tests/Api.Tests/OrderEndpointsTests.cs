using System.Net;
using System.Net.Http.Json;
using System.Text;
using Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Tests;

public sealed class OrderEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrderEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostOrder_ValidRequest_Returns201Created_AndGetByLocationReturns200()
    {
        // Arrange
        var request = new SaveOrderRequest(
            "CUST-TEST-01",
            null,
            [new SaveOrderLineRequest("P-100", "Цемент М400", 250m, 4)]);

        // Act
        var postResponse = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert: 201 Created + Location
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        Assert.NotNull(postResponse.Headers.Location);

        var created = await postResponse.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(created);
        Assert.Equal("CUST-TEST-01", created.CustomerId);
        Assert.Equal(1000m, created.Total);

        // Act: GET за отриманим Location
        var getResponse = await _client.GetAsync(postResponse.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
    }

    [Fact]
    public async Task PostOrder_InvalidRequest_Returns400ValidationProblem()
    {
        // Arrange: порожній CustomerId та порожній масив Lines
        var invalidRequest = new SaveOrderRequest("", null, []);

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", invalidRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        Assert.Contains("CustomerId", body);
        Assert.Contains("Lines", body);
        Assert.DoesNotContain("Exception", body);
    }

    [Fact]
    public async Task PostOrder_MalformedJsonType_Returns400WithoutStackTrace()
    {
        // Arrange: "price": "abc" (порушення типу JSON)
        using var content = new StringContent(
            """{"customerId":"CUST-1","lines":[{"productId":"P1","productName":"Товар","price":"abc","quantity":1}]}""",
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/orders", content);

        // Assert: має бути 400, а не 500
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("System.", body);
        Assert.DoesNotContain("Exception", body);
    }

    [Fact]
    public async Task PutAndDeleteOrder_FullCrudCycle_AfterDeleteGetReturns404()
    {
        // 1. Create
        var createReq = new SaveOrderRequest(
            "CUST-CRUD",
            null,
            [new SaveOrderLineRequest("P-1", "Клавіатура", 1500m, 1)]);

        var postRes = await _client.PostAsJsonAsync("/api/orders", createReq);
        var created = await postRes.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(created);

        // 2. Update (PUT)
        var updateReq = new SaveOrderRequest(
            "CUST-CRUD-UPD",
            null,
            [new SaveOrderLineRequest("P-1", "Клавіатура Pro", 2000m, 2)]);

        var putRes = await _client.PutAsJsonAsync($"/api/orders/{created.Id}", updateReq);
        Assert.Equal(HttpStatusCode.OK, putRes.StatusCode);
        var updated = await putRes.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(updated);
        Assert.Equal(4000m, updated.Total);

        // 3. Domain endpoint /total
        var totalRes = await _client.GetAsync($"/api/orders/{created.Id}/total");
        Assert.Equal(HttpStatusCode.OK, totalRes.StatusCode);
        var totalDto = await totalRes.Content.ReadFromJsonAsync<OrderTotalResponse>();
        Assert.NotNull(totalDto);
        Assert.Equal(4000m, totalDto.Total);

        // 4. Delete
        var delRes = await _client.DeleteAsync($"/api/orders/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delRes.StatusCode);

        // 5. GET after DELETE -> 404 Not Found
        var getAfterDel = await _client.GetAsync($"/api/orders/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDel.StatusCode);
    }

    [Fact]
    public async Task GetOrders_WithPaginationAndSearch_ReturnsPagedResult()
    {
        // Act
        var response = await _client.GetAsync("/api/orders?page=1&pageSize=1&search=Цемент");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paged = await response.Content.ReadFromJsonAsync<PagedOrdersResponse>();
        Assert.NotNull(paged);
        Assert.True(paged.Total >= 1);
        Assert.Single(paged.Items);
        Assert.Equal(1, paged.Page);
        Assert.Equal(1, paged.PageSize);
    }

    [Fact]
    public async Task ConfirmOrder_WhenAlreadyConfirmed_Returns409Conflict()
    {
        // Arrange: створюємо нове замовлення
        var req = new SaveOrderRequest(
            "CUST-CONF",
            null,
            [new SaveOrderLineRequest("P-9", "Монітор", 10000m, 1)]);
        var postRes = await _client.PostAsJsonAsync("/api/orders", req);
        var created = await postRes.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(created);

        // 1-ше підтвердження -> 200 OK
        var firstConfirm = await _client.PostAsync($"/api/orders/{created.Id}/confirm", null);
        Assert.Equal(HttpStatusCode.OK, firstConfirm.StatusCode);

        // 2-ге підтвердження (порушення доменного інваріанта) -> DomainException -> 409 Conflict
        var secondConfirm = await _client.PostAsync($"/api/orders/{created.Id}/confirm", null);
        Assert.Equal(HttpStatusCode.Conflict, secondConfirm.StatusCode);
    }
}
