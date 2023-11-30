using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiTutorial.Controllers;
using WebApiTutorial.Data;
using WebApiTutorial.Dtos;
using WebApiTutorial.Models;
using WebApiTutorial.Services;

namespace WebApiTutorial.Api.Tests;

// Test db functionality here

public class PollServiceTests {
    private IMapper _mapper;
    public PollServiceTests() {
        var mC = new MapperConfiguration(cfg => {
            cfg.AddProfile(new AutoMapperProfile());
        });
        _mapper = new Mapper(mC);
    }

    private int _pollCount = 10;

    private async Task<DataContext> GetDataContext() {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var ctx = new DataContext(options);
        ctx.Database.EnsureCreated();

        for (int i = 0; i < _pollCount; i++) {
            ctx.Polls.Add(new Poll() {
                Title = "Poll title " + (i + 1).ToString(),
                Text = "This is the text of the poll with id " + (i + 1).ToString(),
                Options = new() {
                    new() {
                        Text = "Option 1"
                    },
                    new() {
                        Text = "Option 2"
                    }
                }
            });
        }
        await ctx.SaveChangesAsync();

        return ctx;
    }


    [Fact]
    public async void PollService_GetAll_ReturnsSuccess() {
        // Arrange
        var ctx = await GetDataContext();
        var pollService = new PollService(_mapper, ctx);

        // Act
        var result = await pollService.GetAll();

        // Assert
        result.Should().NotBeNull();
        result.Count().Should().Be(_pollCount);
    }

    [Fact]
    public async void PollService_ById_ReturnsSuccess() {
        // Arrange
        var id = 1;
        var ctx = await GetDataContext();
        var pollService = new PollService(_mapper, ctx);

        // Act
        var result = await pollService.ByID(id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<GetPollDto>();
    }

    [Fact]
    public async void PollService_ById_ThrowsException() {
        // Arrange
        var id = 0;
        var ctx = await GetDataContext();
        var pollService = new PollService(_mapper, ctx);

        // Act
        var act = () => pollService.ByID(id);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async void PollService_Add_ReturnsSuccess() {
        // Arrange
        var poll = A.Fake<AddPollDto>();
        var ctx = await GetDataContext();

        var pollService = new PollService(_mapper, ctx);
        var prevCount = ctx.Polls.Count();

        // Act
        var result = await pollService.Add(poll);

        // Assert
        prevCount.Should().NotBe(0);
        result.Count().Should().NotBe(prevCount);
    }

}