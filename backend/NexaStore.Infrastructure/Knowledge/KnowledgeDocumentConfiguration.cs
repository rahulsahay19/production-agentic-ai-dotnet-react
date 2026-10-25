using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NexaStore.Infrastructure.Knowledge
{
    public sealed class KnowledgeDocumentConfiguration : IEntityTypeConfiguration<KnowledgeDocument>
    {
        public void Configure(EntityTypeBuilder<KnowledgeDocument> builder)
        {
            builder.ToTable("knowledge_documents");
            builder.HasKey(document => document.Id);
            builder.Property(document => document.SourcePath).HasMaxLength(500).IsRequired();
            builder.Property(document => document.Title).HasMaxLength(250).IsRequired();
            builder.Property(document => document.Category).HasMaxLength(100).IsRequired();
            builder.Property(document => document.ContentHash).HasMaxLength(64).IsRequired();
            builder.Property(document => document.EmbeddingModel).HasMaxLength(100).IsRequired();
            builder.HasIndex(document => document.SourcePath).IsUnique();
            builder.HasMany(document => document.Chunks)
                .WithOne(chunk => chunk.Document)
                .HasForeignKey(chunk => chunk.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
