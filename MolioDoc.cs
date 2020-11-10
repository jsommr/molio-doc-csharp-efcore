﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.IO;
using System.Text;

namespace MolioDocEFCore
{
    public partial class MolioDoc : DbContext
    {
        readonly DbConnection connection;

        public MolioDoc(DbConnection connection) : base()
        {
            this.connection = connection;
        }

        public DbSet<BygningsdelsbeskrivelseSection> BygningsdelsbeskrivelseSections { get; set; }

        public DbSet<Bygningsdelsbeskrivelse> Bygningsdelsbeskrivelser { get; set; }

        public DbSet<Attachment> Attachments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite(connection);

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Bygningsdelsbeskrivelse>()
                .Property(b => b.BygningsdelsbeskrivelseGuid)
                .HasConversion(new GuidToBytesConverter());

            builder.Entity<Bygningsdelsbeskrivelse>()
                .Property(b => b.BasisbeskrivelseVersionGuid)
                .HasConversion(new GuidToBytesConverter());

            builder.Entity<BygningsdelsbeskrivelseSection>()
                .Property(b => b.MolioSectionGuid)
                .HasConversion(new GuidToBytesConverter());
        }
    }


    [Table("bygningsdelsbeskrivelse_section")]
    public partial class BygningsdelsbeskrivelseSection
    {
        [Key, Column("bygningsdelsbeskrivelse_section_id")]
        public int BygningsdelsbeskrivelseSectionId { get; set; }

        [Column("bygningsdelsbeskrivelse_id")]
        public int BygningsdelsbeskrivelseId { get; set; }

        public Bygningsdelsbeskrivelse Bygningsdelsbeskrivelse { get; set; }

        [Column("section_no")]
        public int SectionNo { get; set; }

        public string Heading { get; set; }

        public string Text { get; set; } = "";

        [Column("molio_section_guid")]
        public Guid MolioSectionGuid { get; set; }

        [Column("parent_id")]
        public int? ParentId { get; set; }

        public BygningsdelsbeskrivelseSection Parent { get; set; }

        public List<BygningsdelsbeskrivelseSectionAttachment> BygningsdelsbeskrivelseSectionAttachments { get; set; } = new List<BygningsdelsbeskrivelseSectionAttachment>();

        public BygningsdelsbeskrivelseSection() { }

        public BygningsdelsbeskrivelseSection(int sectionNo, string heading, string text = "")
        {
            SectionNo = sectionNo;
            Heading = heading;
            Text = text;
        }

        public BygningsdelsbeskrivelseSection(BygningsdelsbeskrivelseSection parent, int sectionNo, string heading, string text = "")
            : this(sectionNo, heading, text)
        {
            Parent = parent;
        }

        public BygningsdelsbeskrivelseSectionAttachment Attach(Attachment attachment)
        {
            var attachmentRel = new BygningsdelsbeskrivelseSectionAttachment(attachment);
            BygningsdelsbeskrivelseSectionAttachments.Add(attachmentRel);
            return attachmentRel;
        }
    }

    [Table("Bygningsdelsbeskrivelse")]
    public partial class Bygningsdelsbeskrivelse
    {
        [Key, Column("bygningsdelsbeskrivelse_id")]
        public int BygningsdelsbeskrivelseId { get; set; }

        [Column("bygningsdelsbeskrivelse_guid")]
        public Guid BygningsdelsbeskrivelseGuid { get; set; }

        public string Name { get; set; }

        [Column("basisbeskrivelse_version_guid")]
        public Guid BasisbeskrivelseVersionGuid { get; set; }

        public List<BygningsdelsbeskrivelseSection> Sections { get; set; } = new List<BygningsdelsbeskrivelseSection>();
    }

    [Table("attachment")]
    public partial class Attachment
    {
        [Key, Column("attachment_id")]
        public int AttachmentId { get; set; }

        public string Name { get; set; }

        [Column("mime_type")]
        public string MimeType { get; set; }

        public byte[] Content { get; set; }

        public List<BygningsdelsbeskrivelseSectionAttachment> BygningsdelsbeskrivelseSectionAttachments { get; set; } = new List<BygningsdelsbeskrivelseSectionAttachment>();

        public static Attachment Json(string name, string content) =>
            new Attachment
            {
                Name = name,
                MimeType = "application/json",
                Content = Encoding.UTF8.GetBytes(content)
            };

        public static Attachment Pdf(string name, Stream stream) =>
            new Attachment
            {
                Name = name,
                MimeType = "application/pdf",
                Content = StreamToBytes(stream)
            };

        static byte[] StreamToBytes(Stream stream)
        {
            using (var memory = new MemoryStream())
            {
                stream.CopyTo(memory);
                return memory.ToArray();
            }
        }
    }

    [Table("bygningsdelsbeskrivelse_section_attachment")]
    public partial class BygningsdelsbeskrivelseSectionAttachment
    {
        [Key, Column("bygningsdelsbeskrivelse_section_attachment_id")]
        public int BygningsdelsbeskrivelseSectionAttachmentId { get; set; }

        [Column("attachment_id")]
        public int AttachmentId { get; set; }

        public Attachment Attachment { get; set; }

        [Column("bygningsdelsbeskrivelse_section_id")]
        public int BygningsdelsbeskrivelseSectionId { get; set; }
        public BygningsdelsbeskrivelseSection BygningsdelsbeskrivelseSection { get; set; }

        public BygningsdelsbeskrivelseSectionAttachment() { }

        public BygningsdelsbeskrivelseSectionAttachment(Attachment attachment)
        {
            Attachment = attachment;
        }
    }
}
