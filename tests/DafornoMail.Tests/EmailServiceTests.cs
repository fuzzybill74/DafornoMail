using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DafornoMail.Core.Interfaces.Repositories;
using DafornoMail.Core.Interfaces.Services;
using DafornoMail.Core.Models;
using DafornoMail.Infrastructure.Email.Providers;
using DafornoMail.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class EmailServiceTests
{
    private static EmailService CreateService(
        Mock<IEmailAccountRepository>? accountRepo = null,
        Mock<IEmailFolderRepository>? folderRepo = null,
        Mock<IEmailMessageRepository>? messageRepo = null,
        Mock<IKeyVaultService>? keyVault = null,
        Mock<IServiceProvider>? provider = null)
    {
        accountRepo ??= new Mock<IEmailAccountRepository>();
        folderRepo ??= new Mock<IEmailFolderRepository>();
        messageRepo ??= new Mock<IEmailMessageRepository>();
        keyVault ??= new Mock<IKeyVaultService>();
        provider ??= new Mock<IServiceProvider>();
        var logger = Mock.Of<ILogger<EmailService>>();
        return new EmailService(
            accountRepo.Object,
            folderRepo.Object,
            messageRepo.Object,
            keyVault.Object,
            logger,
            provider.Object);
    }

    [Fact]
    public async Task GetAccountAsync_ReturnsAccount()
    {
        var accountId = Guid.NewGuid();
        var account = new EmailAccount { Id = accountId, Email = "test@example.com" };
        var repo = new Mock<IEmailAccountRepository>();
        repo.Setup(r => r.GetByIdAsync(accountId)).ReturnsAsync(account);

        var service = CreateService(accountRepo: repo);
        var result = await service.GetAccountAsync(accountId);
        Assert.Equal(accountId, result.Id);
    }

    [Fact]
    public async Task GetFoldersAsync_NoRefresh_UsesRepository()
    {
        var accountId = Guid.NewGuid();
        var folders = new List<EmailFolder> { new EmailFolder { Id = Guid.NewGuid(), Name = "Inbox" } };
        var folderRepo = new Mock<IEmailFolderRepository>();
        folderRepo.Setup(r => r.GetByAccountIdAsync(accountId)).ReturnsAsync(folders);

        var service = CreateService(folderRepo: folderRepo);
        var result = await service.GetFoldersAsync(accountId);
        Assert.Equal(folders, result);
    }

    [Fact]
    public async Task GetFoldersAsync_ForceRefresh_UsesProvider()
    {
        var accountId = Guid.NewGuid();
        var account = new EmailAccount { Id = accountId, Email = "a@b.com", Provider = "gmail", KeyVaultSecretName = "sec" };
        var accountRepo = new Mock<IEmailAccountRepository>();
        accountRepo.Setup(r => r.GetByIdAsync(accountId)).ReturnsAsync(account);

        var keyVault = new Mock<IKeyVaultService>();
        keyVault.Setup(k => k.GetSecretAsync(account.KeyVaultSecretName)).ReturnsAsync("pwd");

        var providerMock = new Mock<GmailProvider>(Mock.Of<ILogger<GmailProvider>>()) { CallBase = false };
        providerMock.Setup(p => p.ConnectAsync(account.Email, "pwd", false, null)).ReturnsAsync(true);
        providerMock.Setup(p => p.GetFoldersAsync()).ReturnsAsync(new List<EmailFolder>());

        var sp = new Mock<IServiceProvider>();
        sp.Setup(s => s.GetService(typeof(GmailProvider))).Returns(providerMock.Object);

        var service = CreateService(accountRepo, keyVault: keyVault, provider: sp);
        await service.GetFoldersAsync(accountId, true);

        providerMock.Verify(p => p.ConnectAsync(account.Email, "pwd", false, null), Times.Once);
    }

    [Fact]
    public async Task GetMessagesAsync_UsesSearch()
    {
        var accountId = Guid.NewGuid();
        var messages = new List<EmailMessage>();
        var repo = new Mock<IEmailMessageRepository>();
        repo.Setup(r => r.SearchAsync(accountId, "q", 1, 50, null)).ReturnsAsync(messages);

        var service = CreateService(messageRepo: repo);
        var result = await service.GetMessagesAsync(accountId, searchQuery: "q");
        Assert.Equal(messages, result);
    }

    [Fact]
    public async Task GetMessagesAsync_ByFolder()
    {
        var accountId = Guid.NewGuid();
        var folderId = Guid.NewGuid();
        var messages = new List<EmailMessage>();
        var repo = new Mock<IEmailMessageRepository>();
        repo.Setup(r => r.GetByFolderAsync(folderId, 2, 10, "Date", true)).ReturnsAsync(messages);

        var service = CreateService(messageRepo: repo);
        var result = await service.GetMessagesAsync(accountId, folderId, page: 2, pageSize: 10);
        Assert.Equal(messages, result);
    }

    [Fact]
    public async Task GetMessagesAsync_ByAccount()
    {
        var accountId = Guid.NewGuid();
        var messages = new List<EmailMessage>();
        var repo = new Mock<IEmailMessageRepository>();
        repo.Setup(r => r.GetByAccountIdAsync(accountId, 1, 50, "Date", true)).ReturnsAsync(messages);

        var service = CreateService(messageRepo: repo);
        var result = await service.GetMessagesAsync(accountId);
        Assert.Equal(messages, result);
    }
}
