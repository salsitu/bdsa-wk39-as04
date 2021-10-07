using System.Collections.Generic;
using System.Collections.Immutable;
using Assignment4.Core;
using System.Linq;

namespace Assignment4.Entities
{
    public class TagRepository : ITagRepository
    {
        private KanbanContext _kanbanContext;

        public TagRepository(KanbanContext kanbanContext)
        {
            _kanbanContext = kanbanContext;
        }

        public (Response Response, int TagId) Create(TagCreateDTO tag)
        {
            _kanbanContext.Tags.Add(new Tag
            {
                Name = tag.Name
            });
            _kanbanContext.SaveChanges();
            return (Response.Created, _kanbanContext.Tags.Last().Id);
        }

        public Response Delete(int tagId, bool force = false)
        {
            var tag = _kanbanContext.Tags.SingleOrDefault(x => x.Id == tagId);
            if (tag.Tasks.Count > 0 && force)
            {
                _kanbanContext.Tags.Remove(tag);
            } 
            else return Response.Conflict;

            _kanbanContext.SaveChanges();

            return Response.Deleted;
        }

        public TagDTO Read(int tagId)
        {
            var tag = _kanbanContext.Tags.SingleOrDefault(x => x.Id == tagId);
            return new TagDTO(tag.Id, tag.Name);
        }

        public IReadOnlyCollection<TagDTO> ReadAll()
        {
            return _kanbanContext.Tags.Select(x => new TagDTO(x.Id,x.Name)).ToImmutableList();
        }

        public Response Update(TagUpdateDTO tag)
        {
            
            throw new System.NotImplementedException();
        }
    }
}