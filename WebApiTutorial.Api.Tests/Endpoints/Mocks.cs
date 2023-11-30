using System.Net.Http.Json;
using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WebApiTutorial;
using WebApiTutorial.Dtos;
using WebApiTutorial.Models;
using WebApiTutorial.Services;

class UserServiceMock : IUserService
{
    public List<User> Users { get; set; } = new();

    private IMapper _mapper;


    public UserServiceMock(IMapper mapper) {
        _mapper = mapper;
    }

    public async Task<GetUserDto> ByUsername(string username)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<GetUserDto>> GetAll()
    {
        return Users.Select(user => _mapper.Map<GetUserDto>(user));
    }

    public async Task<string> Login(UserDto user)
    {
        throw new NotImplementedException();
    }

    public async Task<GetUserDto> Register(UserDto user)
    {
        throw new NotImplementedException();
    }
}

class PollServiceMock : IPollService
{
    private IMapper _mapper;
    public List<Poll> Polls { get; set; } = new();

    private IUserService _userService;

    public PollServiceMock(IMapper mapper, IUserService userService) {
        // var mC = new MapperConfiguration(cfg => {
        //     cfg.AddProfile(new AutoMapperProfile());
        // });
        // _mapper = new Mapper(mC);
        _mapper = mapper;

        _userService = userService;

        for (int i = 0; i < 10; i++) {
            var poll = new Poll() {
                ID = i+1,
                Title = "Poll" + (i+1),
                Text = "Poll text" + (i+1),
                Options = new() {
                    new() {
                        Text = "Option1",
                    },
                    new() {
                        Text = "Option2",
                    }
                }
            };

            Polls.Add(poll);
        }
    }
    public async Task<IEnumerable<GetPollDto>> Add(AddPollDto poll)
    {
        Polls.Add(_mapper.Map<Poll>(poll));
        return (IEnumerable<GetPollDto>)Polls;
    }

    public async Task<GetPollDto> ByID(int id)
    {
        var result = Polls.SingleOrDefault(poll => poll.ID == id) ?? throw new Exception();
        return _mapper.Map<GetPollDto>(result);
    }

    public async Task<IEnumerable<GetPollDto>> GetAll()
    {
        return Polls.Select(poll => _mapper.Map<GetPollDto>(poll));
    }

    public async Task<GetPollDto> Update(UpdatePollDto updatedPoll)
    {
        var poll = Polls.SingleOrDefault(poll => poll.ID == updatedPoll.ID) ?? throw new Exception();
        poll.Text = updatedPoll.Text;
        poll.Title = updatedPoll.Title;
        // TODO options?
        return _mapper.Map<GetPollDto>(poll);
    }

    public async Task<GetPollDto> VoteFor(string username, int pollID, string option)
    {
        var poll = Polls.SingleOrDefault(poll => poll.ID == pollID) ?? throw new Exception();
        foreach (var o in poll.Options) {
            var user = o.VotedUsers.FirstOrDefault(u => u.Username == username);
            if (user != null) throw new Exception();
        }
        var op = poll.Options.FirstOrDefault(po => po.Text == option) ?? throw new Exception("");
        var voter = await _userService.ByUsername(username);
        op.VotedUsers.Add(_mapper.Map<User>(voter));
        return _mapper.Map<GetPollDto>(poll);
    }
}