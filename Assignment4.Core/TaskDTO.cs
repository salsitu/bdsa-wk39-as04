using System.Collections.Generic;

namespace Assignment4.Core
{
    public record TaskDTO(int Id, string Title, string Description, int? AssignedTo, IReadOnlyCollection<string> Tags, State State)
    {
        public int Id { get; init; } = Id;
        public string Title { get; init; } = Title;
        public string Description { get; init; } = Description;
        public int? AssignedToId { get; init; } = AssignedTo;
        public IReadOnlyCollection<string> Tags { get; init; } = Tags;
        public State State { get; init; } = State;
    }
}