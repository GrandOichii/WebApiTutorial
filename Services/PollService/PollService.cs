using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace WebApiTutorial.Services;

public class PollService : IPollService
{
    private IMapper _mapper;
    private DataContext _ctx;

    public PollService(IMapper mapper, DataContext ctx) {
        _mapper = mapper;
        _ctx = ctx;
    }

    public async Task<IEnumerable<GetPollDto>> Add([FromBody] AddPollDto poll)
    {
        await _ctx.AddAsync(_mapper.Map<Poll>(poll));
        await _ctx.SaveChangesAsync();
        return await GetAll();
    }

    public async Task<GetPollDto> ByID(int id)
    {
        var poll = await _ctx.Polls
                                .Include(poll => poll.Options)
                                    .ThenInclude(pollOption => pollOption.VotedUsers)
                                .FirstAsync(poll => poll.ID == id);
        return _mapper.Map<GetPollDto>(poll);
    }

    public async Task<IEnumerable<GetPollDto>> GetAll()
    {
        var list = await _ctx.Polls
            .Include(poll => poll.Options)
                .ThenInclude(pollOption => pollOption.VotedUsers)
            .ToListAsync();
        return list
            .Select(poll => _mapper.Map<GetPollDto>(poll));
    }

    public async Task<GetPollDto> Update(UpdatePollDto updatedPoll)
    {
        var poll = Global.Instance.Polls.First(poll => poll.ID == updatedPoll.ID);
        poll.Text = updatedPoll.Text;
        poll.Title = updatedPoll.Title;
        return await ByID(updatedPoll.ID);
    }

    public async Task<GetPollDto> VoteFor(string username, int pollID, string option)
    {
        var poll = await _ctx.Polls
                            .Include(poll => poll.Options)
                            .FirstAsync(poll => poll.ID == pollID);
        foreach (var o in poll.Options) {
            if (o.Text != option) continue;

            var me = o.VotedUsers.Find(user => user.Username == username);
            if (me != null) {
                throw new Exception(username + " has already voted in poll " + pollID);
            }

            var newVoter = await _ctx.Users.FirstAsync(user => user.Username == username);
            o.VotedUsers.Add(newVoter);
            _ctx.SaveChanges();
            return await ByID(pollID);
        }
        throw new Exception("Can't find poll with ID " + pollID);
    }
}