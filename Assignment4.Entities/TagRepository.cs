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
            var entity = new Tag { Name = tag.Name };
            if (_kanbanContext.Tags.SingleOrDefault(x => x.Name == entity.Name) != null) return (Response.Conflict,0);
            _kanbanContext.Tags.Add(entity);
            _kanbanContext.SaveChanges();
            return (Response.Created, entity.Id);
        }

        public Response Delete(int tagId, bool force = false)
        {
            var tag = _kanbanContext.Tags.SingleOrDefault(x => x.Id == tagId);
            if (tag == null) return Response.NotFound;

            if (tag.Tasks != null && tag.Tasks.Count > 0)
            {
                if (force) _kanbanContext.Tags.Remove(tag);
                else return Response.Conflict;
            }
            else
            {
                _kanbanContext.Tags.Remove(tag);
            }

            _kanbanContext.SaveChanges();
            return Response.Deleted;
        }

        public TagDTO Read(int tagId)
        {
            var tag = _kanbanContext.Tags.SingleOrDefault(x => x.Id == tagId);
            return tag == null ? null : new TagDTO(tag.Id, tag.Name);
        }

        public IReadOnlyCollection<TagDTO> ReadAll()
        {
            return _kanbanContext.Tags.Select(x => new TagDTO(x.Id, x.Name)).ToImmutableList();
        }

        public Response Update(TagUpdateDTO tag)
        {
            var t = _kanbanContext.Tags.FirstOrDefault(x => x.Id == tag.Id);
            if (t == null) return Response.NotFound;

            t.Name = tag.Name;

            _kanbanContext.SaveChanges();
            return Response.Updated;
        }
    }
}