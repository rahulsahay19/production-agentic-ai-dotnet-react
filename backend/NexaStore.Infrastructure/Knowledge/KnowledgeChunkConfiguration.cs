using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NexaStore.Infrastructure.Knowledge
{
    public sealed class KnowledgeChunkConfiguration : IEntityTypeConfiguration<KnowledgeChunk>
    {
        public void Configure(EntityTypeBuilder<KnowledgeChunk> builder)
        {
            builder.ToTable("knowledge_chunks");
            builder.HasKey(chunk => chunk.Id);
            builder.Property(chunk => chunk.Content).IsRequired();
            builder.Property(chunk => chunk.Embedding).HasColumnType("vector(1536)").IsRequired();
            builder.HasIndex(chunk => new { chunk.DocumentId, chunk.ChunkIndex }).IsUnique();
            builder.HasIndex(chunk => chunk.Embedding)
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops");
        }
    }
}
