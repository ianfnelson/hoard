using Hoard.Api.Controllers;
using Hoard.Core.Application;
using Hoard.Core.Application.Accounts;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Hoard.Test.Unit.Api.Controllers;

public class AccountsControllerTests
{
    private readonly Mock<IMediator> _mediator = new();
    private readonly AccountsController _controller;

    public AccountsControllerTests()
    {
        _controller = new AccountsController(_mediator.Object);
    }

    [Fact]
    public async Task GetList_ReturnsOkWithAccounts()
    {
        var expected = new List<AccountSummaryDto>();
        _mediator
            .Setup(m => m.QueryAsync<GetAccountsQuery, List<AccountSummaryDto>>(
                It.IsAny<GetAccountsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.GetList(new GetAccountsQuery(), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task Get_ExistingAccount_ReturnsOk()
    {
        var expected = new AccountDetailDto { Name = "ISA", Colour = "#FF0000", Portfolios = [] };
        _mediator
            .Setup(m => m.QueryAsync<GetAccountQuery, AccountDetailDto?>(
                It.Is<GetAccountQuery>(q => q.AccountId == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _controller.Get(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task Get_NonExistentAccount_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.QueryAsync<GetAccountQuery, AccountDetailDto?>(
                It.IsAny<GetAccountQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountDetailDto?)null);

        var result = await _controller.Get(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }
}
